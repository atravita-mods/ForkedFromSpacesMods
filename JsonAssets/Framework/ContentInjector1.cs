using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JsonAssets.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceCore;
using SpaceCore.AssetManagers.Models;
using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Crafting;

namespace JsonAssets.Framework
{
    internal static class ContentInjector1
    {
        private static readonly Dictionary<string, Action<IAssetData>> Files = new(StringComparer.OrdinalIgnoreCase);

        private static readonly Dictionary<int, FenceData> FenceIndexes = new();

        /// <summary>
        /// Call after assigning IDs. Populate the content injector's dictionary
        /// with only the assets that need editing.
        /// </summary>
        /// <param name="helper">Game content helper.</param>
        internal static void Initialize(IGameContentHelper helper)
        {
            lock (Files)
            {
                Files.Clear();
                if (Mod.instance.Objects.Count > 0 || Mod.instance.Boots.Count > 0)
                { // boots are objects too.
                    Files[helper.ParseAssetName(@"Data\ObjectInformation").BaseName] = InjectDataObjectInformation;
                    Files[helper.ParseAssetName(@"Data\ObjectContextTags").BaseName] = InjectDataObjectContextTags;
                    Files[helper.ParseAssetName(@"Data\CookingRecipes").BaseName] = InjectDataCookingRecipes;
                    Files[helper.ParseAssetName(@"Maps\springobjects").BaseName] = InjectMapsSpringobjects;
                }
                if (Mod.instance.Objects.Count > 0 || Mod.instance.Boots.Count > 0 || Mod.instance.BigCraftables.Count > 0)
                {
                    Files[helper.ParseAssetName(@"Data\CraftingRecipes").BaseName] = InjectDataCraftingRecipes;
                }
                if (Mod.instance.BigCraftables.Count > 0)
                {
                    Files[helper.ParseAssetName(@"Data\BigCraftablesInformation").BaseName] = InjectDataBigCraftablesInformation;
                    Files[helper.ParseAssetName(@"TileSheets\Craftables").BaseName] = InjectTileSheetsCraftables;
                }
                if (Mod.instance.Crops.Count > 0)
                {
                    Files[helper.ParseAssetName(@"Data\Crops").BaseName] = InjectDataCrops;
                    Files[helper.ParseAssetName(@"TileSheets\crops").BaseName] = InjectTileSheetsCrops;
                }
                if (Mod.instance.FruitTrees.Count > 0)
                {
                    Files[helper.ParseAssetName(@"Data\fruitTrees").BaseName] = InjectDataFruitTrees;
                    Files[helper.ParseAssetName(@"TileSheets\fruitTrees").BaseName] = InjectTileSheetsFruitTrees;
                }
                if (Mod.instance.Hats.Count > 0)
                {
                    Files[helper.ParseAssetName(@"Data\hats").BaseName] = InjectDataHats;
                    Files[helper.ParseAssetName(@"Characters\Farmer\hats").BaseName] = InjectCharactersFarmerHats;
                }
                if (Mod.instance.Weapons.Count > 0)
                {
                    Files[helper.ParseAssetName(@"Data\weapons").BaseName] = InjectDataWeapons;
                    Files[helper.ParseAssetName(@"TileSheets\weapons").BaseName] = InjectTileSheetsWeapons;
                }
                if (Mod.instance.Shirts.Count > 0 || Mod.instance.Pants.Count > 0 )
                {
                    Files[helper.ParseAssetName(@"Data\ClothingInformation").BaseName] = InjectDataClothingInformation;
                }
                if (Mod.instance.Shirts.Count > 0)
                {
                    Files[helper.ParseAssetName(@"Characters\Farmer\shirts").BaseName] = InjectCharactersFarmerShirts;
                }
                if (Mod.instance.Pants.Count > 0)
                {
                    Files[helper.ParseAssetName(@"Characters\Farmer\pants").BaseName] = InjectCharactersFarmerPants;
                }
                if (Mod.instance.Tailoring.Count > 0)
                {
                    Files[helper.ParseAssetName(@"Data\TailoringRecipes").BaseName] = InjectDataTailoringRecipes;
                }
                if (Mod.instance.Boots.Count > 0)
                {
                    Files[helper.ParseAssetName(@"Data\Boots").BaseName] = InjectDataBoots;
                    Files[helper.ParseAssetName(@"Characters\Farmer\shoeColors").BaseName] = InjectCharactersFarmerShoeColors;
                }

                Log.Trace($"Content Injector 1 initialized with {Files.Count} assets.");
            }

            lock (FenceIndexes)
            {
                FenceIndexes.Clear();
                foreach (FenceData fence in Mod.instance.Fences)
                    if (fence?.CorrespondingObject?.GetObjectId() is int index)
                        FenceIndexes[index] = fence;
            }
        }

        internal static void Clear()
        {
            lock (Files)
            {
                Files.Clear();
            }
            lock (FenceIndexes)
            {
                FenceIndexes.Clear();
            }
        }

        public static void InvalidateUsed()
        {
            Mod.instance.Helper.GameContent.InvalidateCache(asset => Files.ContainsKey(asset.NameWithoutLocale.BaseName));
        }

        public static void OnAssetRequested(AssetRequestedEventArgs e)
        {
            if (!Mod.instance.DidInit)
                return;
            if (e.NameWithoutLocale.StartsWith(@"LooseSprites\Fence")
                && int.TryParse(e.NameWithoutLocale.BaseName[@"LooseSprites\Fence".Length..], out int index) && FenceIndexes.ContainsKey(index))
                e.LoadFrom(() => FenceIndexes[index].Texture, AssetLoadPriority.Low);
            else if (Files.TryGetValue(e.NameWithoutLocale.BaseName, out var injector))
                e.Edit(injector, (AssetEditPriority)int.MinValue); // insist on editing first.
        }

        #region data
        private static void InjectDataObjectInformation(IAssetData asset)
        {
#warning - crosscheck boots?
            var data = asset.AsDictionary<int, string>().Data;
            foreach (var obj in Mod.instance.Objects)
            {
                try
                {
                    string objinfo = obj.GetObjectInformation().ToString();
                    if (Log.IsVerbose)
                        Log.Trace( $"Injecting to objects: {obj.GetObjectId()}: {objinfo}");
                    if (!data.TryAdd(obj.GetObjectId(), objinfo))
                        Log.Error($"Object {obj.GetObjectId()} is a duplicate???");
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting object information for {obj.Name}: {e}");
                }
            }
        }
        private static void InjectDataObjectContextTags(IAssetData asset)
        {
            var data = asset.AsDictionary<string, string>().Data;
            foreach (var obj in Mod.instance.Objects)
            {
                try
                {
                    string tags = string.Join(", ", obj.ContextTags);
                    if (Log.IsVerbose)
                        Log.Trace($"Injecting to object context tags: {obj.Name}: {tags}");
                    if (!data.TryGetValue(obj.Name, out string prevTags) || string.IsNullOrWhiteSpace(prevTags))
                        data[obj.Name] = tags;
                    else
                        data[obj.Name] += (", " + tags);
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting object context tags for {obj.Name}: {e}");
                }
            }
        }
        private static void InjectDataCrops(IAssetData asset)
        {
            var data = asset.AsDictionary<int, string>().Data;
            foreach (var crop in Mod.instance.Crops)
            {
                try
                {
                    string cropinfo = crop.GetCropInformation().ToString();
                    if (Log.IsVerbose)
                        Log.Trace($"Injecting to crops: {crop.GetSeedId()}: {cropinfo}");
                    if (!data.TryAdd(crop.GetSeedId(), cropinfo))
                        Log.Error($"Crop {crop.GetSeedId()} already exists!");
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting crop for {crop.Name}: {e}");
                }
            }
        }
        private static void InjectDataFruitTrees(IAssetData asset)
        {
            var data = asset.AsDictionary<int, string>().Data;
            foreach (var fruitTree in Mod.instance.FruitTrees)
            {
                try
                {
                    string treeinfo = fruitTree.GetFruitTreeInformation();
                    if (Log.IsVerbose)
                        Log.Trace($"Injecting to fruit trees: {fruitTree.GetSaplingId()}: {treeinfo}");
                    if (!data.TryAdd(fruitTree.GetSaplingId(), treeinfo))
                        Log.Error($"Fruit tree {fruitTree.Name} is a duplicate?");
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting fruit tree for {fruitTree.Name}: {e}");
                }
            }
        }
        private static void InjectDataCookingRecipes(IAssetData asset)
        {
            var data = asset.AsDictionary<string, string>().Data;
            foreach (var obj in Mod.instance.Objects)
            {
                try
                {
                    if (obj.Recipe == null || obj.Category != ObjectCategory.Cooking)
                        continue;
                    string recipestring = obj.Recipe.GetRecipeString(obj).ToString();
                    if (Log.IsVerbose)
                        Log.Trace($"Injecting to cooking recipes: {obj.Name}: {recipestring}");
                    if (!data.TryAdd(obj.Name, recipestring))
                        Log.Error($"Recipe for {obj.Name} already seems to exist?");
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting cooking recipe for {obj.Name}: {e}");
                }
            }
        }
        private static void InjectDataCraftingRecipes(IAssetData asset)
        {
            var data = asset.AsDictionary<string, string>().Data;
            foreach (var obj in Mod.instance.Objects)
            {
                try
                {
                    if (obj.Recipe == null || obj.Category == ObjectCategory.Cooking)
                        continue;
                    string recipestring = obj.Recipe.GetRecipeString(obj).ToString();
                    if (Log.IsVerbose)
                        Log.Trace( $"Injecting to crafting recipes: {obj.Name}: {recipestring}");
                    if (!data.TryAdd(obj.Name, recipestring))
                        Log.Error($"Recipe for {obj.Name} already seems to exist?");
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting crafting recipe for {obj.Name}: {e}");
                }
            }
            foreach (var big in Mod.instance.BigCraftables)
            {
                try
                {
                    if (big.Recipe == null)
                        continue;
                    string recipestring = big.Recipe.GetRecipeString(big).ToString();
                    if (Log.IsVerbose)
                        Log.Trace($"Injecting to crafting recipes: {big.Name}: {recipestring}");
                    if (!data.TryAdd(big.Name, recipestring))
                        Log.Error($"Recipe for {big.Name} already seems to exist?");
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting crafting recipe for {big.Name}: {e}");
                }
            }
        }
        private static void InjectDataBigCraftablesInformation(IAssetData asset)
        {
            var data = asset.AsDictionary<int, string>().Data;
            foreach (var big in Mod.instance.BigCraftables)
            {
                try
                {
                    string bigcraftableinfo = big.GetCraftableInformation();
                    if (Log.IsVerbose)
                        Log.Trace($"Injecting to big craftables: {big.GetCraftableId()}: {bigcraftableinfo}");
                    if (!data.TryAdd(big.GetCraftableId(), big.GetCraftableInformation()))
                        Log.Error($"{big.Name} already seems to exist!");
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting object information for {big.Name}: {e}");
                }
            }
        }
        private static void InjectDataHats(IAssetData asset)
        {
            var data = asset.AsDictionary<int, string>().Data;
            foreach (var hat in Mod.instance.Hats)
            {
                try
                {
                    string hatinfo = hat.GetHatInformation();
                    if (Log.IsVerbose)
                        Log.Trace($"Injecting to hats: {hat.GetHatId()}: {hatinfo}");
                    if (!data.TryAdd(hat.GetHatId(), hat.GetHatInformation()))
                        Log.Error($"Hat {hat.GetHatId()} appears to be a duplicate???");
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting hat information for {hat.Name}: {e}");
                }
            }
        }
        private static void InjectDataWeapons(IAssetData asset)
        {
            var data = asset.AsDictionary<int, string>().Data;
            foreach (var weapon in Mod.instance.Weapons)
            {
                try
                {
                    string weaponData = weapon.GetWeaponInformation();
                    if (Log.IsVerbose)
                        Log.Trace($"Injecting to weapons: {weapon.GetWeaponId()}: {weaponData}");
                    if (!data.TryAdd(weapon.GetWeaponId(), weaponData))
                        Log.Error($"{weapon.GetWeaponId()} appears to be a duplicate?");
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting weapon information for {weapon.Name}: {e}");
                }
            }
        }
        private static void InjectDataClothingInformation(IAssetData asset)
        {
            var data = asset.AsDictionary<int, string>().Data;
            foreach (var shirt in Mod.instance.Shirts)
            {
                try
                {
                    if (Log.IsVerbose)
                        Log.Trace($"Injecting to clothing information: {shirt.GetClothingId()}: {shirt.GetClothingInformation()}");
                    if (!data.TryAdd(shirt.GetClothingId(), shirt.GetClothingInformation()))
                        Log.Error($"Shirt {shirt.GetClothingId()} appears to be a duplicate?");
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting clothing information for {shirt.Name}: {e}");
                }
            }
            foreach (var pants in Mod.instance.Pants)
            {
                try
                {
                    if (Log.IsVerbose)
                        Log.Trace($"Injecting to clothing information: {pants.GetClothingId()}: {pants.GetClothingInformation()}");
                    if (!data.TryAdd(pants.GetClothingId(), pants.GetClothingInformation()))
                        Log.Error($"Pants {pants.GetClothingId()} appears to be a duplicate?");
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting clothing information for {pants.Name}: {e}");
                }
            }
        }
        private static void InjectDataTailoringRecipes(IAssetData asset)
        {
            var data = asset.GetData<List<TailorItemRecipe>>();
            foreach (var recipe in Mod.instance.Tailoring)
            {
                try
                {
                    if (Log.IsVerbose)
                        Log.Trace($"Injecting to tailoring recipe: {recipe.ToGameData()}");
                    data.Add(recipe.ToGameData());
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting tailoring recipe: {e}");
                }
            }
        }
        private static void InjectDataBoots(IAssetData asset)
        {
            var data = asset.AsDictionary<int, string>().Data;
            foreach (var boots in Mod.instance.Boots)
            {
                try
                {
                    if (Log.IsVerbose)
                        Log.Trace($"Injecting to boots: {boots.GetObjectId()}: {boots.GetBootsInformation()}");
                    if (!data.TryAdd(boots.GetObjectId(), boots.GetBootsInformation()))
                        Log.Error($"Boots {boots.Name} appear to be a duplicate?");
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting boots information for {boots.Name}: {e}");
                }
            }
        }
        #endregion

        #region tilesheets
        private static void InjectMapsSpringobjects(IAssetData asset)
        {
            if (Mod.instance.Objects.Count == 0 && Mod.instance.Boots.Count == 0)
                return;

            var tex= asset.AsImage();

            int size = tex.Data.Width * TileSheetExtensions.MAXTILESHEETHEIGHT;
            Color[] initial = ArrayPool<Color>.Shared.Rent(size);

            Rectangle startpoint = ContentInjector1.ObjectRect(Mod.StartingObjectId);
            SortedList<int, RawDataRented> scratch = new();
            SortedList<int, int> maxYs = new();

            if (startpoint.Y < tex.Data.Height)
                tex.Data.GetData(initial, tex.Data.Width * tex.Data.Height, tex.Data.Width * (tex.Data.Height - startpoint.Y));

            scratch[0] = new(initial, tex.Data.Width, TileSheetExtensions.MAXTILESHEETHEIGHT);
            maxYs[0] = tex.Data.Height;

            foreach (var obj in Mod.instance.Objects)
            {
                try
                {
                    var rect = ContentInjector1.ObjectRect(obj.GetObjectId());
                    var target = TileSheetExtensions.GetAdjustedTileSheetTarget(asset.NameWithoutLocale.BaseName, rect);
                    int ts = target.TileSheet;
                    obj.Tilesheet = asset.NameWithoutLocale.BaseName.GetTilesheetName(ts);
                    obj.TilesheetX = rect.X;
                    obj.TilesheetY = target.Y;

                    Rectangle patchLoc = new(rect.X, target.Y, rect.Width, rect.Height);

                    RawDataRented rented = GetScratchBuffer(scratch, ts, asset.NameWithoutLocale.BaseName, size, tex.Data.Width);

                    if (Log.IsVerbose)
                        Log.Trace($"Injecting {obj.Name} sprites @ {rect}");
                    rented.PatchImage(obj.Texture, null, patchLoc);

                    int maxY;

                    if (obj.IsColored)
                    {
                        var coloredRect = ContentInjector1.ObjectRect(obj.GetObjectId() + 1);
                        var coloredTarget = TileSheetExtensions.GetAdjustedTileSheetTarget(asset.NameWithoutLocale.BaseName, coloredRect);
                        int coloredTS = coloredTarget.TileSheet;
                        if (coloredTS != ts)
                        {
                            if (!maxYs.TryGetValue(ts, out maxY))
                                maxY = 0;
                            maxYs[ts] = Math.Max(maxY, patchLoc.Bottom);
                            ts = coloredTS;
                            rented = GetScratchBuffer(scratch, ts, asset.NameWithoutLocale.BaseName, size, tex.Data.Width);
                        }

                        patchLoc = new Rectangle(coloredRect.X, coloredTarget.Y, coloredRect.Width, coloredRect.Height);
                        Log.Verbose(() => $"Injecting {obj.Name} color sprites @ {coloredRect}");
                        rented.PatchImage(obj.TextureColor, null, patchLoc);
                    }

                    if (!maxYs.TryGetValue(ts, out maxY))
                        maxY = 0;
                    maxYs[ts] = Math.Max(maxY, patchLoc.Bottom);
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting sprite for {obj.Name}: {e}");
                }
            }

            foreach (var boots in Mod.instance.Boots)
            {
                try
                {
                    var rect = ContentInjector1.ObjectRect(boots.GetObjectId());
                    var target = TileSheetExtensions.GetAdjustedTileSheetTarget(asset.NameWithoutLocale.BaseName, rect);
                    int ts = target.TileSheet;
                    boots.Tilesheet = asset.NameWithoutLocale.BaseName.GetTilesheetName(ts);
                    boots.TilesheetX = rect.X;
                    boots.TilesheetY = target.Y;

                    Rectangle patchLoc = new(rect.X, target.Y, rect.Width, rect.Height);

                    RawDataRented rented = GetScratchBuffer(scratch, ts, asset.NameWithoutLocale.BaseName, size, tex.Data.Width);

                    if (Log.IsVerbose)
                        Log.Trace($"Injecting {boots.Name} sprites @ {rect}");
                    rented.PatchImage(boots.Texture, null, patchLoc);

                    if (!maxYs.TryGetValue(ts, out int maxY))
                        maxY = 0;

                    maxYs[ts] = Math.Max(maxY, patchLoc.Bottom);
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting sprite for {boots.Name}: {e}");
                }
            }

            // extend spritesheet
            int newHeight = scratch.Count > 1 ? TileSheetExtensions.MAXTILESHEETHEIGHT : maxYs[0];
            if (tex.ExtendImage(tex.Data.Width, newHeight))
                Log.Trace($"SpringObjects are now ({tex.Data.Width}, {tex.Data.Height})");

            int currentY = 0;
            foreach (var (index, data) in scratch)
            {
                Log.DebugOnlyLog($"Patching into {index}th extended tilesheet for objects");
                data.Shrink(data.Width, maxYs[index]);
                Rectangle sourceRect;
                Rectangle extendedRect;

                if (index == 0)
                {
                    sourceRect = new(0, startpoint.Y, data.Width, maxYs[index] - startpoint.Y);
                    extendedRect = sourceRect;
                }
                else
                {
                    sourceRect = new(0, 0, data.Width, maxYs[index]);
                    extendedRect = new(0, currentY, tex.Data.Width, maxYs[index]);
                }
                tex.PatchExtendedTileSheet(data, sourceRect, extendedRect);

                currentY += maxYs[index];
            }

            // dispose scratch (returns buffers)
            foreach (RawDataRented rented in scratch.Values)
                rented.Dispose();
        }

        private static void InjectTileSheetsCrops(IAssetData asset)
        {
            if (Mod.instance.Crops.Count == 0)
                return;

            IAssetDataForImage tex = asset.AsImage();

            int size = tex.Data.Width * TileSheetExtensions.MAXTILESHEETHEIGHT;
            Color[] initial = ArrayPool<Color>.Shared.Rent(size);

            Rectangle startpoint = ContentInjector1.CropRect(Mod.StartingCropId);
            SortedList<int, RawDataRented> scratch = new();
            SortedList<int, int> maxYs = new();

            if (startpoint.Y < tex.Data.Height)
                tex.Data.GetData(initial, 0, tex.Data.Width * tex.Data.Height);
            scratch[0] = new(initial, tex.Data.Width, TileSheetExtensions.MAXTILESHEETHEIGHT);
            maxYs[0] = tex.Data.Height;

            // Patch in data for each crop.
            foreach (var crop in Mod.instance.Crops)
            {
                try
                {
                    var rect = ContentInjector1.CropRect(crop.GetCropSpriteIndex());
                    var target = TileSheetExtensions.GetAdjustedTileSheetTarget(asset.NameWithoutLocale.BaseName, rect);
                    int ts = target.TileSheet;
                    crop.Tilesheet = asset.NameWithoutLocale.BaseName.GetTilesheetName(ts);
                    crop.TilesheetX = rect.X;
                    crop.TilesheetY = target.Y;

                    Rectangle patchLoc = new(rect.X, target.Y, rect.Width, rect.Height);

                    RawDataRented rented = GetScratchBuffer(scratch, ts, asset.NameWithoutLocale.BaseName, size, tex.Data.Width);

                    if (Log.IsVerbose)
                        Log.Trace($"Injecting {crop.Name} crop images @ {rect}");
                    rented.PatchImage(crop.Texture, null, patchLoc);
                    if (!maxYs.TryGetValue(ts, out int maxY))
                        maxY = 0;

                    maxYs[ts] = Math.Max(maxY, patchLoc.Bottom);
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting crop sprite for {crop.Name}: {e}");
                }
            }

            int newHeight = scratch.Count > 1 ? TileSheetExtensions.MAXTILESHEETHEIGHT : maxYs[0];
            if (tex.ExtendImage(tex.Data.Width, newHeight))
                Log.Trace($"Crops are now ({tex.Data.Width}, {tex.Data.Height})");

            int currentY = 0;
            foreach (var (index, data) in scratch)
            {
                Log.DebugOnlyLog($"Patching into {index}th extended tilesheet for crops");
                data.Shrink(data.Width, maxYs[index]);
                Rectangle sourceRect;
                Rectangle extendedRect;

                if (index == 0)
                {
                    sourceRect = new(0, startpoint.Y, data.Width, maxYs[index] - startpoint.Y);
                    extendedRect = sourceRect;
                }
                else
                {
                    sourceRect = new(0, 0, data.Width, maxYs[index]);
                    extendedRect = new(0, currentY, tex.Data.Width, maxYs[index]);
                }
                tex.PatchExtendedTileSheet(data, sourceRect, extendedRect);

                currentY += maxYs[index];
            }

            // dispose scratch (returns buffers)
            foreach (RawDataRented rented in scratch.Values)
                rented.Dispose();
        }

        private static void InjectTileSheetsFruitTrees(IAssetData asset)
        {
            if (Mod.instance.FruitTrees.Count == 0)
                return;

            // setup.
            var tex = asset.AsImage();
            int size = tex.Data.Width * TileSheetExtensions.MAXTILESHEETHEIGHT;

            Rectangle startpoint = ContentInjector1.FruitTreeRect(Mod.StartingFruitTreeId);
            Color[] initial = ArrayPool<Color>.Shared.Rent(size);

            SortedList<int, RawDataRented> scratch = new();
            SortedList<int, int> maxYs = new();

            if (startpoint.Y < tex.Data.Height)
                tex.Data.GetData(initial, 0, tex.Data.Width * tex.Data.Height);
            scratch[0] = new(initial, tex.Data.Width, TileSheetExtensions.MAXTILESHEETHEIGHT);
            maxYs[0] = tex.Data.Height;

            // Patch in data for each fruit tree.
            foreach (var fruitTree in Mod.instance.FruitTrees)
            {
                try
                {
                    var rect = ContentInjector1.FruitTreeRect(fruitTree.GetFruitTreeIndex());
                    var target = TileSheetExtensions.GetAdjustedTileSheetTarget(asset.NameWithoutLocale.BaseName, rect);
                    int ts = target.TileSheet;
                    fruitTree.Tilesheet = asset.NameWithoutLocale.BaseName.GetTilesheetName(ts);
                    fruitTree.TilesheetX = rect.X;
                    fruitTree.TilesheetY = target.Y;

                    Rectangle patchLoc = new(rect.X, target.Y, rect.Width, rect.Height);

                    RawDataRented rented = GetScratchBuffer(scratch, ts, asset.NameWithoutLocale.BaseName, size, tex.Data.Width);

                    if (Log.IsVerbose)
                        Log.Trace($"Injecting {fruitTree.Name} fruit tree images @ {rect}");
                    rented.PatchImage(fruitTree.Texture, null, patchLoc);

                    if (!maxYs.TryGetValue(ts, out int maxY))
                        maxY = 0;

                    maxYs[ts] = Math.Max(maxY, patchLoc.Bottom);
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting fruit tree sprite for {fruitTree.Name}: {e}");
                }
            }

            int newHeight = scratch.Count > 1 ? TileSheetExtensions.MAXTILESHEETHEIGHT : maxYs[0];
            if (tex.ExtendImage(tex.Data.Width, newHeight))
                Log.Trace($"FruitTrees are now ({tex.Data.Width}, {tex.Data.Height})");

            int currentY = 0;
            foreach (var (index, data) in scratch)
            {
                Log.DebugOnlyLog($"Patching into {index}th extended tilesheet for fruit trees");
                data.Shrink(data.Width, maxYs[index]);
                Rectangle sourceRect;
                Rectangle extendedRect;

                if (index == 0)
                {
                    sourceRect = new(0, startpoint.Y, data.Width, maxYs[index] - startpoint.Y);
                    extendedRect = sourceRect;
                }
                else
                {
                    sourceRect = new(0, 0, data.Width, maxYs[index]);
                    extendedRect = new(0, currentY, tex.Data.Width, maxYs[index]);
                }

                tex.PatchExtendedTileSheet(data, sourceRect, extendedRect);

                currentY += maxYs[index];
            }

            // dispose scratch (returns buffers)
            foreach (RawDataRented rented in scratch.Values)
                rented.Dispose();
        }

        private static void InjectTileSheetsCraftables(IAssetData asset)
        {
            if (Mod.instance.BigCraftables.Count == 0)
                return;

            // setup.
            var tex = asset.AsImage();
            int size = tex.Data.Width * TileSheetExtensions.MAXTILESHEETHEIGHT;
            Color[] initial = ArrayPool<Color>.Shared.Rent(size);

            Rectangle startPos = ContentInjector1.BigCraftableRect(Mod.StartingBigCraftableId);

            SortedList<int, RawDataRented> scratch = new();
            SortedList<int, int> maxYs = new();

            if (startPos.Y < tex.Data.Height)
                tex.Data.GetData(initial, 0, tex.Data.Width * tex.Data.Height);
            scratch[0] = new(initial, tex.Data.Width, TileSheetExtensions.MAXTILESHEETHEIGHT);
            maxYs[0] = tex.Data.Height;

            // patch in data for each bigcraftable.
            foreach (var big in Mod.instance.BigCraftables)
            {
                try
                {
                    var rect = ContentInjector1.BigCraftableRect(big.GetCraftableId());
                    var target = TileSheetExtensions.GetAdjustedTileSheetTarget(asset.NameWithoutLocale.BaseName, rect);
                    int ts = target.TileSheet;
                    big.Tilesheet = asset.NameWithoutLocale.BaseName.GetTilesheetName(ts);
                    big.TilesheetX = rect.X;
                    big.TilesheetY = target.Y;

                    Rectangle patchLoc = new(rect.X, target.Y, rect.Width, rect.Height);

                    RawDataRented rented = GetScratchBuffer(scratch, ts, asset.NameWithoutLocale.BaseName, size, tex.Data.Width);

                    if (Log.IsVerbose)
                        Log.Trace($"Injecting {big.Name} sprites @ {rect}");
                    rented.PatchImage(big.Texture, null, patchLoc);

                    int maxY;
                    if (big.ReserveExtraIndexCount > 0)
                    {
                        for (int i = 0; i < big.ReserveExtraIndexCount; ++i)
                        {
                            var extraRect = ContentInjector1.BigCraftableRect(big.GetCraftableId() + i + 1);
                            var extraTarget = TileSheetExtensions.GetAdjustedTileSheetTarget(asset.NameWithoutLocale.BaseName, extraRect);
                            int extraTS = extraTarget.TileSheet;

                            if (extraTS != ts)
                            {
                                // update maxY here too.
                                if (!maxYs.TryGetValue(ts, out maxY))
                                    maxY = 0;
                                maxYs[ts] = Math.Max(maxY, patchLoc.Bottom);

                                // remainder of logic can refer to the new ts.
                                ts = extraTS;
                                rented = GetScratchBuffer(scratch, ts, asset.NameWithoutLocale.BaseName, size, tex.Data.Width);
                            }

                            patchLoc = new(extraRect.X, extraTarget.Y, extraRect.Width, extraRect.Height);

                            if (Log.IsVerbose)
                                Log.Trace($"Injecting {big.Name} reserved extra sprite {i + 1} @ {extraRect}");
                            rented.PatchImage(big.ExtraTextures[i], null, patchLoc);
                        }
                    }

                    if (!maxYs.TryGetValue(ts, out maxY))
                        maxY = 0;
                    maxYs[ts] = Math.Max(maxY, patchLoc.Bottom);
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting sprite for {big.Name}: {e}");
                }
            }

            int newHeight = scratch.Count > 1 ? TileSheetExtensions.MAXTILESHEETHEIGHT : maxYs[0];
            if (tex.ExtendImage(tex.Data.Width, newHeight))
                Log.Trace($"Big craftables are now ({tex.Data.Width}, {tex.Data.Height})");

            int currentY = 0;
            foreach (var (index, data) in scratch)
            {
                Log.DebugOnlyLog($"Patching into {index}th extended tilesheet for bigCraftables");
                data.Shrink(data.Width, maxYs[index]);
                Rectangle sourceRect;
                Rectangle extendedRect;

                if (index == 0)
                {
                    sourceRect = new(0, startPos.Y, data.Width, maxYs[index] - startPos.Y);
                    extendedRect = sourceRect;
                }
                else
                {
                    sourceRect = new(0, 0, data.Width, maxYs[index]);
                    extendedRect = new(0, currentY, tex.Data.Width, maxYs[index]);
                }
                tex.PatchExtendedTileSheet(data, sourceRect, extendedRect);

                currentY += maxYs[index];
            }

            // dispose scratch (returns buffers)
            foreach (RawDataRented rented in scratch.Values)
                rented.Dispose();
        }

        private static void InjectCharactersFarmerHats(IAssetData asset)
        {
            if (Mod.instance.Hats.Count == 0)
                return;

            // setup
            var tex = asset.AsImage();
            int size = tex.Data.Width * TileSheetExtensions.MAXTILESHEETHEIGHT;
            Color[] initial = ArrayPool<Color>.Shared.Rent(size);

            Rectangle startPos = ContentInjector1.HatRect(Mod.StartingHatId);

            SortedList<int, RawDataRented> scratch = new();
            SortedList<int, int> maxYs = new();

            if (startPos.Y < tex.Data.Height)
                tex.Data.GetData(initial, 0, tex.Data.Width * tex.Data.Height);
            scratch[0] = new(initial, tex.Data.Width, TileSheetExtensions.MAXTILESHEETHEIGHT);
            maxYs[0] = tex.Data.Height;

            foreach (var hat in Mod.instance.Hats)
            {
                try
                {
                    var rect = ContentInjector1.HatRect(hat.GetHatId());
                    var target = TileSheetExtensions.GetAdjustedTileSheetTarget(asset.NameWithoutLocale.BaseName, rect);
                    int ts = target.TileSheet;
                    hat.Tilesheet = asset.NameWithoutLocale.BaseName.GetTilesheetName(ts);
                    hat.TilesheetX = rect.X;
                    hat.TilesheetY = target.Y;

                    Rectangle patchLoc = new(rect.X, target.Y, rect.Width, rect.Height);

                    RawDataRented rented = GetScratchBuffer(scratch, ts, asset.NameWithoutLocale.BaseName, size, tex.Data.Width);

                    if (Log.IsVerbose)
                        Log.Verbose($"Injecting {hat.Name} sprites @ {rect}");
                    rented.PatchImage(hat.Texture, null, patchLoc);

                    if (!maxYs.TryGetValue(ts, out int maxY))
                        maxY = 0;

                    maxYs[ts] = Math.Max(maxY, patchLoc.Bottom);
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting sprite for {hat.Name}: {e}");
                }
            }

            int newHeight = scratch.Count > 1 ? TileSheetExtensions.MAXTILESHEETHEIGHT : maxYs[0];
            if (tex.ExtendImage(tex.Data.Width, newHeight))
                Log.Trace($"Hats are now ({tex.Data.Width}, {tex.Data.Height})");

            int currentY = 0;
            foreach (var (index, data) in scratch)
            {
                Log.DebugOnlyLog($"Patching into {index}th extended tilesheet for hats.");
                data.Shrink(data.Width, maxYs[index]);
                Rectangle sourceRect;
                Rectangle extendedRect;

                if (index == 0)
                {
                    sourceRect = new(0, startPos.Y, data.Width, maxYs[index] - startPos.Y);
                    extendedRect = sourceRect;
                }
                else
                {
                    sourceRect = new(0, 0, data.Width, maxYs[index]);
                    extendedRect = new(0, currentY, tex.Data.Width, maxYs[index]);
                }
                tex.PatchExtendedTileSheet(data, sourceRect, extendedRect);

                currentY += maxYs[index];
            }

            // dispose scratch (returns buffers)
            foreach (RawDataRented rented in scratch.Values)
                rented.Dispose();
        }

        private static void InjectTileSheetsWeapons(IAssetData asset)
        {
            if (Mod.instance.Weapons.Count == 0)
                return;

            // setup
            var tex = asset.AsImage();
            int size = tex.Data.Width * TileSheetExtensions.MAXTILESHEETHEIGHT;
            Color[] initial = ArrayPool<Color>.Shared.Rent(size);

            Rectangle startPos = ContentInjector1.WeaponRect(Mod.StartingWeaponId);

            if (startPos.Y < tex.Data.Height)
                tex.Data.GetData(initial, 0, tex.Data.Width * tex.Data.Height);
            RawDataRented scratch = new(initial, tex.Data.Width, TileSheetExtensions.MAXTILESHEETHEIGHT);
            int maxY = tex.Data.Height;

            foreach (var weapon in Mod.instance.Weapons)
            {
                try
                {
                    var rect = ContentInjector1.WeaponRect(weapon.GetWeaponId());

                    //int ts = 0;// TileSheetExtensions.GetAdjustedTileSheetTarget(asset.AssetName, rect).TileSheet;
                    weapon.Tilesheet = asset.NameWithoutLocale.BaseName; // + (ts == 0 ? "" : (ts + 1).ToString());
                    weapon.TilesheetX = rect.X;
                    weapon.TilesheetY = rect.Y;

                    if (Log.IsVerbose)
                        Log.Trace($"Injecting {weapon.Name} sprites @ {rect}");
                    scratch.PatchImage(weapon.Texture, null, rect);

                    maxY = Math.Max(maxY, rect.Bottom);
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting sprite for {weapon.Name}: {e}");
                }
            }

            if (tex.ExtendImage(tex.Data.Width, maxY))
                Log.Trace($"Weapons are now ({tex.Data.Width}, {tex.Data.Height})");

            Log.DebugOnlyLog($"Patching into 0th extended tilesheet for weapons.");
            scratch.Shrink(scratch.Width, maxY);
            Rectangle sourceRect = new(0, startPos.Y, scratch.Width, maxY - startPos.Y);
            tex.PatchExtendedTileSheet(scratch, sourceRect, sourceRect);

            // dispose scratch (returns buffers)
            scratch.Dispose();
        }
        private static void InjectCharactersFarmerShirts(IAssetData asset)
        {
            if (Mod.instance.Shirts.Count == 0)
                return;

            // setup
            var tex = asset.AsImage();
            int size = tex.Data.Width * TileSheetExtensions.MAXTILESHEETHEIGHT;
            Color[] initial = ArrayPool<Color>.Shared.Rent(size);

            Rectangle startPos = ContentInjector1.ShirtRectPlain(Mod.StartingShirtTextureIndex);

            SortedList<int, RawDataRented> scratch = new();
            SortedList<int, int> maxYs = new();

            if (startPos.Y < tex.Data.Height)
                tex.Data.GetData(initial, 0, tex.Data.Width * tex.Data.Height);
            scratch[0] = new(initial, tex.Data.Width, TileSheetExtensions.MAXTILESHEETHEIGHT);
            maxYs[0] = tex.Data.Height;

            foreach (var shirt in Mod.instance.Shirts)
            {
                try
                {
                    if (Log.IsVerbose)
                    {
                        List<Rectangle> rects = new(4) { ShirtRectPlain(shirt.GetMaleIndex()) };
                        if (shirt.Dyeable)
                            rects.Add(ShirtRectDye(shirt.GetMaleIndex()));
                        if (shirt.HasFemaleVariant)
                        {
                            rects.Add(ShirtRectPlain(shirt.GetFemaleIndex()));
                            if (shirt.Dyeable)
                                rects.Add(ShirtRectDye(shirt.GetFemaleIndex()));
                        }

                        Log.Trace($"Injecting {shirt.Name} sprites @ {string.Join(',', rects)}");
                    }

                    var rect = ContentInjector1.ShirtRectPlain(shirt.GetMaleIndex());
                    var target = TileSheetExtensions.GetAdjustedTileSheetTarget(asset.NameWithoutLocale.BaseName, rect);
                    int ts = target.TileSheet;

                    Rectangle patchLoc = new(rect.X, target.Y, rect.Width, rect.Height);

                    RawDataRented rented = GetScratchBuffer(scratch, ts, asset.NameWithoutLocale.BaseName, size, tex.Data.Width);

                    rented.PatchImage(shirt.TextureMale, null, patchLoc);

                    int maxY;
                    if (shirt.Dyeable)
                    {
                        var extraRect = ContentInjector1.ShirtRectDye(shirt.GetMaleIndex());
                        var extraTarget = TileSheetExtensions.GetAdjustedTileSheetTarget(asset.NameWithoutLocale.BaseName, extraRect);
                        int extraTS = extraTarget.TileSheet;

                        if (extraTS != ts)
                        {
                            if (!maxYs.TryGetValue(ts, out maxY))
                                maxY = 0;
                            maxYs[ts] = Math.Max(maxY, patchLoc.Bottom);

                            // remainder of logic can refer to the new ts.
                            ts = extraTS;
                            rented = GetScratchBuffer(scratch, ts, asset.NameWithoutLocale.BaseName, size, tex.Data.Width);
                        }

                        patchLoc = new(extraRect.X, extraTarget.Y, extraRect.Width, extraRect.Height);
                        rented.PatchImage(shirt.TextureMaleColor, null, patchLoc);
                    }
                    if (shirt.HasFemaleVariant)
                    {
                        var extraRect = ContentInjector1.ShirtRectPlain(shirt.GetFemaleIndex());
                        var extraTarget = TileSheetExtensions.GetAdjustedTileSheetTarget(asset.NameWithoutLocale.BaseName, extraRect);
                        int extraTS = extraTarget.TileSheet;

                        if (extraTS != ts)
                        {
                            if (!maxYs.TryGetValue(ts, out maxY))
                                maxY = 0;
                            maxYs[ts] = Math.Max(maxY, patchLoc.Bottom);

                            // remainder of logic can refer to the new ts.
                            ts = extraTS;
                            rented = GetScratchBuffer(scratch, ts, asset.NameWithoutLocale.BaseName, size, tex.Data.Width);
                        }

                        patchLoc = new(extraRect.X, extraTarget.Y, extraRect.Width, extraRect.Height);
                        rented.PatchImage(shirt.TextureFemale, null, patchLoc);

                        if (shirt.Dyeable)
                        {
                            extraRect = ContentInjector1.ShirtRectDye(shirt.GetFemaleIndex());
                            extraTarget = TileSheetExtensions.GetAdjustedTileSheetTarget(asset.NameWithoutLocale.BaseName, extraRect);
                            extraTS = extraTarget.TileSheet;

                            if (extraTS != ts)
                            {
                                if (!maxYs.TryGetValue(ts, out maxY))
                                    maxY = 0;
                                maxYs[ts] = Math.Max(maxY, patchLoc.Bottom);

                                // remainder of logic can refer to the new ts.
                                ts = extraTS;
                                rented = GetScratchBuffer(scratch, ts, asset.NameWithoutLocale.BaseName, size, tex.Data.Width);
                            }

                            patchLoc = new(extraRect.X, extraTarget.Y, extraRect.Width, extraRect.Height);
                            rented.PatchImage(shirt.TextureFemaleColor, null, patchLoc);
                        }
                    }
                    if (!maxYs.TryGetValue(ts, out maxY))
                        maxY = 0;
                    maxYs[ts] = Math.Max(maxY, patchLoc.Bottom);

                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting sprite for {shirt.Name}: {e}");
                }
            }

            int newHeight = scratch.Count > 1 ? TileSheetExtensions.MAXTILESHEETHEIGHT : maxYs[0];
            if (tex.ExtendImage(tex.Data.Width, newHeight))
                Log.Trace($"Shirts are now ({tex.Data.Width}, {tex.Data.Height})");

            int currentY = 0;
            foreach (var (index, data) in scratch)
            {
                Log.DebugOnlyLog($"Patching into {index}th extended tilesheet for shirts");
                data.Shrink(data.Width, maxYs[index]);
                Rectangle sourceRect;
                Rectangle extendedRect;

                if (index == 0)
                {
                    sourceRect = new(0, startPos.Y, data.Width, maxYs[index] - startPos.Y);
                    extendedRect = sourceRect;
                }
                else
                {
                    sourceRect = new(0, 0, data.Width, maxYs[index]);
                    extendedRect = new(0, currentY, tex.Data.Width, maxYs[index]);
                }
                tex.PatchExtendedTileSheet(data, sourceRect, extendedRect);

                currentY += maxYs[index];
            }

            // dispose scratch (returns buffers)
            foreach (RawDataRented rented in scratch.Values)
                rented.Dispose();
        }

        private static void InjectCharactersFarmerPants(IAssetData asset)
        {
            if (Mod.instance.Pants.Count == 0)
                return;

            // setup
            var tex = asset.AsImage();
            int size = tex.Data.Width * TileSheetExtensions.MAXTILESHEETHEIGHT;
            Color[] initial = ArrayPool<Color>.Shared.Rent(size);

            Rectangle startPos = ContentInjector1.PantsRect(Mod.StartingPantsTextureIndex);
            SortedList<int, RawDataRented> scratch = new();
            SortedList<int, int> maxYs = new();

            if (startPos.Y < tex.Data.Height)
                tex.Data.GetData(initial, 0, tex.Data.Width * tex.Data.Height);
            scratch[0] = new(initial, tex.Data.Width, TileSheetExtensions.MAXTILESHEETHEIGHT);
            maxYs[0] = tex.Data.Height;

            foreach (var pants in Mod.instance.Pants)
            {
                try
                {
                    var rect = ContentInjector1.PantsRect(pants.GetTextureIndex());
                    var target = TileSheetExtensions.GetAdjustedTileSheetTarget(asset.NameWithoutLocale.BaseName, rect);
                    int ts = target.TileSheet;

                    Rectangle patchLoc = new(rect.X, target.Y, rect.Width, rect.Height);

                    RawDataRented rented = GetScratchBuffer(scratch, ts, asset.NameWithoutLocale.BaseName, size, tex.Data.Width);

                    if (Log.IsVerbose)
                        Log.Trace($"Injecting {pants.Name} sprites @ {ContentInjector1.PantsRect(pants.GetTextureIndex())}");
                    rented.PatchImage(pants.Texture, null, patchLoc);

                    if (!maxYs.TryGetValue(ts, out int maxY))
                        maxY = 0;
                    maxYs[ts] = Math.Max(maxY, patchLoc.Bottom);
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting sprite for {pants.Name}: {e}");
                }
            }

            int newHeight = scratch.Count > 1 ? TileSheetExtensions.MAXTILESHEETHEIGHT : maxYs[0];
            if (tex.ExtendImage(tex.Data.Width, newHeight))
                Log.Trace($"Pants are now ({tex.Data.Width}, {tex.Data.Height})");

            int currentY = 0;
            foreach (var (index, data) in scratch)
            {
                Log.DebugOnlyLog($"Patching into {index}th extended tilesheet for pants");
                data.Shrink(data.Width, maxYs[index]);
                Rectangle sourceRect;
                Rectangle extendedRect;

                if (index == 0)
                {
                    sourceRect = new(0, startPos.Y, data.Width, maxYs[index] - startPos.Y);
                    extendedRect = sourceRect;
                }
                else
                {
                    sourceRect = new(0, 0, data.Width, maxYs[index]);
                    extendedRect = new(0, currentY, tex.Data.Width, maxYs[index]);
                }
                tex.PatchExtendedTileSheet(data, sourceRect, extendedRect);

                currentY += maxYs[index];
            }

            // dispose scratch (returns buffers)
            foreach (RawDataRented rented in scratch.Values)
                rented.Dispose();
        }

        private static void InjectCharactersFarmerShoeColors(IAssetData asset)
        {
            if (Mod.instance.Boots.Count == 0)
                return;

            var tex = asset.AsImage();
            int size = tex.Data.Width * TileSheetExtensions.MAXTILESHEETHEIGHT;
            Color[] initial = ArrayPool<Color>.Shared.Rent(size);

            Rectangle startPos = ContentInjector1.BootsRect(Mod.StartingBootsId);
            SortedList<int, RawDataRented> scratch = new();
            SortedList<int, int> maxYs = new();

            if (startPos.Y < tex.Data.Height)
                tex.Data.GetData(initial, 0, tex.Data.Width * tex.Data.Height);
            scratch[0] = new(initial, tex.Data.Width, TileSheetExtensions.MAXTILESHEETHEIGHT);
            maxYs[0] = tex.Data.Height;

            foreach (var boots in Mod.instance.Boots)
            {
                try
                {
                    var rect = ContentInjector1.BootsRect(boots.GetTextureIndex());
                    var target = TileSheetExtensions.GetAdjustedTileSheetTarget(asset.NameWithoutLocale.BaseName, rect);
                    int ts = target.TileSheet;

                    Rectangle patchLoc = new(rect.X, target.Y, rect.Width, rect.Height);

                    RawDataRented rented = GetScratchBuffer(scratch, ts, asset.NameWithoutLocale.BaseName, size, tex.Data.Width);

                    if (Log.IsVerbose)
                        Log.Trace($"Injecting {boots.Name} sprites @ {rect}");
                    rented.PatchImage(boots.TextureColor, null, patchLoc);

                    if (!maxYs.TryGetValue(ts, out int maxY))
                        maxY = 0;

                    maxYs[ts] = Math.Max(maxY, patchLoc.Bottom);
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting sprite for {boots.Name}: {e}");
                }
            }

            int newHeight = scratch.Count > 1 ? TileSheetExtensions.MAXTILESHEETHEIGHT : maxYs[0];
            if (tex.ExtendImage(tex.Data.Width, newHeight))
                Log.Trace($"Boots are now ({tex.Data.Width}, {tex.Data.Height})");

            int currentY = 0;
            foreach (var (index, data) in scratch)
            {
                Log.DebugOnlyLog($"Patching into {index}th extended tilesheet for boots.");
                data.Shrink(data.Width, maxYs[index]);
                Rectangle sourceRect;
                Rectangle extendedRect;

                if (index == 0)
                {
                    sourceRect = new(0, startPos.Y, data.Width, maxYs[index] - startPos.Y);
                    extendedRect = sourceRect;
                }
                else
                {
                    sourceRect = new(0, 0, data.Width, maxYs[index]);
                    extendedRect = new(0, currentY, tex.Data.Width, maxYs[index]);
                }
                tex.PatchExtendedTileSheet(data, sourceRect, extendedRect);

                currentY += maxYs[index];
            }

            // dispose scratch (returns buffers)
            foreach (RawDataRented rented in scratch.Values)
                rented.Dispose();
        }
        #endregion


        private static RawDataRented GetScratchBuffer(
            SortedList<int, RawDataRented> scratch,
            int index,
            string assetName,
            int minsize,
            int width)
        {
            if (!scratch.TryGetValue(index, out var rented))
            {
                var texture = TileSheetExtensions.GetTileSheet(assetName, index);
                var array = ArrayPool<Color>.Shared.Rent(minsize);
                texture?.GetData(array, 0, minsize);
                rented = new(array, width, TileSheetExtensions.MAXTILESHEETHEIGHT);
                scratch[index] = rented;
            }
            return rented;
        }

        private static string GetTilesheetName(this string assetName, int ts)
            => ts == 0 ? assetName : $"{assetName}{ts + 1}";

        #region rectangles
        internal static Rectangle ObjectRect(int index)
        {
            int div = Math.DivRem(index, 24, out int rem);
            return new(rem * 16, div * 16, 16, 16);
        }
        internal static Rectangle CropRect(int index)
        {
            int div = Math.DivRem(index, 2, out int rem);
            return new(rem * 128, div * 32, 128, 32);
        }
        internal static Rectangle FruitTreeRect(int index)
        {
            return new(0, index * 80, 432, 80);
        }
        internal static Rectangle BigCraftableRect(int index)
        {
            int div = Math.DivRem(index, 8, out int rem);
            return new(rem * 16, div * 32, 16, 32);
        }
        internal static Rectangle HatRect(int index)
        {
            int div = Math.DivRem(index, 12, out int rem);
            return new(rem * 20, div * 80, 20, 80);
        }
        internal static Rectangle WeaponRect(int index)
        {
            int div = Math.DivRem(index, 8, out int rem);
            return new(rem * 16, div * 16, 16, 16);
        }
        internal static Rectangle ShirtRectPlain(int index)
        {
            int div = Math.DivRem(index, 8, out int rem);
            return new(rem * 8, div * 32, 8, 32);
        }
        internal static Rectangle ShirtRectDye(int index)
        {
            var rect = ContentInjector1.ShirtRectPlain(index);
            rect.X += 16 * 8;
            return rect;
        }
        internal static Rectangle PantsRect(int index)
        {
            int div = Math.DivRem(index, 10, out int rem);
            return new(rem * 192, div * 688, 192, 688);
        }
        internal static Rectangle BootsRect(int index)
        {
            return new(0, index, 4, 1);
        }

        #endregion
    }
}
