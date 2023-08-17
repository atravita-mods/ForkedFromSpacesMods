using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Monsters;

namespace CustomNPCFixes
{
    internal class Mod : StardewModdingAPI.Mod
    {
        public override void Entry(IModHelper helper)
        {
            Log.Monitor = this.Monitor;

            this.Helper.Events.GameLoop.SaveCreated += this.DoNpcFixes;
            this.Helper.Events.GameLoop.SaveLoaded += this.DoNpcFixes;

            this.Helper.Events.GameLoop.DayStarted += this.OnDayStart;
        }

        [EventPriority(EventPriority.Low)]
        // See comments in doNpcFixes. This handles conditional spawning.
        public void OnDayStart(object sender, DayStartedEventArgs e)
        {
            if (Context.IsMainPlayer)
            {
                SpawnNpcs();
                FixSchedules();
            }
        }

        [EventPriority(EventPriority.Low)]
        public void DoNpcFixes(object sender, EventArgs args)
        {
            if (!Context.IsMainPlayer)
                return;

            // This needs to be called again so that custom NPCs spawn in locations added after the original call
            //Game1.fixProblems();
            SpawnNpcs();

            // Similarly, this needs to be called again so that pathing works.
            NPC.populateRoutesFromLocationToLocationList();

            // Schedules for new NPCs don't work the first time.
            FixSchedules();
        }

        class NpcEqualityChecker : IEqualityComparer<NPC>
        {
            public bool Equals(NPC x, NPC y)
            {
                return x.Name == y.Name;
            }

            public int GetHashCode([DisallowNull] NPC obj)
            {
                return obj.Name.GetHashCode();
            }
        }

        private void SpawnNpcs()
        {
            List<NPC> allCharacters = Utility.getPooledList();
            Utility.getAllCharacters(allCharacters);

            var dispos = Game1.content.Load<Dictionary<string, string>>("Data\\NPCDispositions");
            Dictionary<string, NPC> chars = new();

            // Utility.getAllCharacters for some reason also checks inside farm buildings
            // Where, uh, NPCs aren't even supposed to be.
            foreach (GameLocation loc in Game1.locations)
                foreach (NPC npc in loc.characters)
                    if (npc.isVillager())
                        chars[npc.Name] = npc;

            foreach (var (name, dispo) in dispos)
            {
                if ((name == "Kent" && Game1.year <= 1) || (name == "Leo" && !Game1.MasterPlayer.hasOrWillReceiveMail("addedParrotBoy")))
                    continue;
                if (chars.ContainsKey(name))
                    continue;
                try
                {
                    string[] defaultpos = dispo.Split('/')[10].Split(' ');
                    GameLocation map = Game1.getLocationFromName(defaultpos[0]);
                    if (map is null)
                    {
                        Log.Warn($"{name} has a dispo entry for map {defaultpos[0]} which could not be found!");
                        continue;
                    }
                    map.addCharacter(
                        new NPC(
                            sprite: new AnimatedSprite("Characters\\" + NPC.getTextureNameForCharacter(name), 0, 16, 32),
                            position: new Vector2(int.Parse(defaultpos[1]), int.Parse(defaultpos[2])) * 64f,
                            defaultMap: defaultpos[0],
                            facingDir: 0,
                            name: name,
                            schedule: null,
                            portrait: Game1.content.Load<Texture2D>("Portraits\\" + NPC.getTextureNameForCharacter(name)),
                            eventActor: false));
                    Log.Trace($"Adding {name} to {defaultpos[0]}");
                }
                catch (Exception ex)
                {
                    Log.Info($"Issue adding NPC {name}: {ex}");
                }
            }

            foreach (var (name, npc) in chars)
            {
                if (npc.datable.Value && npc.getSpouse() is null
                    && (npc.DefaultMap.Contains("cabin", StringComparison.OrdinalIgnoreCase) || npc.DefaultMap.Equals("Farmhouse", StringComparison.OrdinalIgnoreCase)))
                {
                    var defaultmap = dispos[name].Split('/')[10].AsSpan();
                    int index = defaultmap.IndexOf(' ');
                    if (index > 0 && !npc.DefaultMap.AsSpan().Equals(defaultmap[..index], StringComparison.OrdinalIgnoreCase))
                    {
                        Log.Trace($"Fixing {name} who was improperly divorced and left stranded");
                        npc.PerformDivorce();
                    }
                }
            }
        }

        private static void FixSchedules()
        {
            // Utility.getAllCharacters for some reason also checks inside farm buildings
            // Where, uh, NPCs aren't even supposed to be.
            foreach (GameLocation loc in Game1.locations)
            {
                foreach (NPC npc in loc.characters)
                {
                    if (npc.Schedule is null && npc.isVillager())
                    {
                        try
                        {
                            npc.Schedule = npc.getSchedule(Game1.dayOfMonth);
                            npc.checkSchedule(Game1.timeOfDay);
                            Log.Trace($"Scheduling {npc.Name}.");
                        }
                        catch (Exception e)
                        {
                            Log.Error("Exception doing schedule for NPC " + npc.Name + ": " + e);
                        }
                    }
                }
            }
        }
    }
}
