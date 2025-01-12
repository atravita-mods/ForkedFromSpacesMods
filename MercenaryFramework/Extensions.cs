using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Netcode;
using StardewValley;

namespace MercenaryFramework
{
    public static class FarmerCurrentMercs
    {
        private static ConditionalWeakTable<Farmer, NetObjectList<Mercenary>> Data = new();

        public static NetObjectList<Mercenary> GetCurrentMercenaries(this Farmer farmer)
        {
            return Data.GetOrCreateValue(farmer);
        }
    }

    // farmer prop - merc persistent data

    [HarmonyPatch(typeof(Farmer), "farmerInit")]
    public static class FarmerInitAddNetFieldsPatch
    {
        public static void Postfix(Farmer __instance)
        {
            __instance.NetFields.AddFields(__instance.GetCurrentMercenaries());
        }
    }

    public static class Extensions
    {
        public static bool IsAlreadyMercenary(this NPC npc)
        {
            foreach (var player in Game1.getOnlineFarmers())
            {
                foreach (var merc in player.GetCurrentMercenaries())
                {
                    if (merc.CorrespondingNpc == npc.Name)
                        return true;
                }
            }
            return false;
        }
    }
}
