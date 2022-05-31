using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace JsonAssets.Patches
{
    /// <summary>Applies Harmony patches to <see cref="GiantCrop"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class GiantCropPatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<GiantCrop>(nameof(GiantCrop.draw)),
                prefix: this.GetHarmonyMethod(nameof(Before_Draw))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call before <see cref="GiantCrop.draw"/>.</summary>
        private static bool Before_Draw(GiantCrop __instance, SpriteBatch spriteBatch, Vector2 tileLocation, double ___shakeTimer)
        {
            try
            {
                if (__instance.parentSheetIndex.Value >= Mod.StartingObjectId)
                {
                    Texture2D tex = Mod.instance.Crops.Single(cd => Mod.instance.ResolveObjectId(cd.Product) == __instance.parentSheetIndex.Value).GiantTexture.Value;
                    spriteBatch.Draw(tex, Game1.GlobalToLocal(Game1.viewport, tileLocation * 64f - new Vector2(___shakeTimer > 0.0 ? (float)(Math.Sin(2.0 * Math.PI / ___shakeTimer) * 2.0) : 0.0f, 64f)), null, Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, (float)((tileLocation.Y + 2.0) * 64.0 / 10000.0));
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Failed in {nameof(GiantCropPatcher)}.{nameof(Before_Draw)}:\n{ex}");
            }
            return true;
        }
    }
}
