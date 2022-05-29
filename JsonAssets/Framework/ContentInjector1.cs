using System;
using System.Collections.Generic;
using JsonAssets.Data;
using Microsoft.Xna.Framework;
using SpaceCore;
using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley.GameData.Crafting;

namespace JsonAssets.Framework
{
    internal class ContentInjector1 : IAssetEditor, IAssetLoader
    {
        private delegate void Injector(IAssetData asset);
        private readonly Dictionary<string, Injector> Files;
        public ContentInjector1()
        {
            Func<string, string> normalize = PathUtilities.NormalizeAssetName;

            //normalize with 
            this.Files = new Dictionary<string, Injector>
            {
                {normalize("Data\\ObjectInformation"), this.InjectDataObjectInformation},
                {normalize("Data\\ObjectContextTags"), this.InjectDataObjectContextTags},
                {normalize("Data\\Crops"), this.InjectDataCrops},
                {normalize("Data\\fruitTrees"), this.InjectDataFruitTrees},
                {normalize("Data\\CookingRecipes"), this.InjectDataCookingRecipes},
                {normalize("Data\\CraftingRecipes"), this.InjectDataCraftingRecipes},
                {normalize("Data\\BigCraftablesInformation"), this.InjectDataBigCraftablesInformation},
                {normalize("Data\\hats"), this.InjectDataHats},
                {normalize("Data\\weapons"), this.InjectDataWeapons},
                {normalize("Data\\ClothingInformation"), this.InjectDataClothingInformation},
                {normalize("Data\\TailoringRecipes"), this.InjectDataTailoringRecipes},
                {normalize("Data\\Boots"), this.InjectDataBoots},
                {normalize("Maps\\springobjects"), this.InjectMapsSpringobjects},
                {normalize("TileSheets\\crops"), this.InjectTileSheetsCrops},
                {normalize("TileSheets\\fruitTrees"), this.InjectTileSheetsFruitTrees},
                {normalize("TileSheets\\Craftables"), this.InjectTileSheetsCraftables},
                {normalize("Characters\\Farmer\\hats"), this.InjectCharactersFarmerHats},
                {normalize("TileSheets\\weapons"), this.InjectTileSheetsWeapons},
                {normalize("Characters\\Farmer\\shirts"), this.InjectCharactersFarmerShirts},
                {normalize("Characters\\Farmer\\pants"), this.InjectCharactersFarmerPants},
                {normalize("Characters\\Farmer\\shoeColors"), this.InjectCharactersFarmerShoeColors}
            };
        }

        public void InvalidateUsed()
        {
            Mod.instance.Helper.GameContent.InvalidateCache(asset => this.Files.ContainsKey(asset.AssetName));
        }

        public bool CanEdit<T>(IAssetInfo asset)
        {
            return this.Files.ContainsKey(asset.AssetName);
        }

        public void Edit<T>(IAssetData asset)
        {
            if (!Mod.instance.DidInit)
                return;

            this.Files[asset.NameWithoutLocale.BaseName](asset);
        }

        private void InjectDataObjectInformation(IAssetData asset)
        {
            var data = asset.AsDictionary<int, string>().Data;
            foreach (var obj in Mod.instance.Objects)
            {
                try
                {
                    string objinfo = obj.GetObjectInformation().ToString();
                    Log.Verbose($"Injecting to objects: {obj.GetObjectId()}: {objinfo}");
                    if (!data.TryAdd(obj.GetObjectId(), objinfo))
                        Log.Error($"Object {obj.GetObjectId()} is a duplicate???");
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting object information for {obj.Name}: {e}");
                }
            }
        }
        private void InjectDataObjectContextTags(IAssetData asset)
        {
            var data = asset.AsDictionary<string, string>().Data;
            foreach (var obj in Mod.instance.Objects)
            {
                try
                {
                    string tags = string.Join(", ", obj.ContextTags);
                    Log.Verbose($"Injecting to object context tags: {obj.Name}: {tags}");
                    if (!data.TryGetValue(obj.Name, out string prevTags) || prevTags == string.Empty)
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
        private void InjectDataCrops(IAssetData asset)
        {
            var data = asset.AsDictionary<int, string>().Data;
            foreach (var crop in Mod.instance.Crops)
            {
                try
                {
                    string cropinfo = crop.GetCropInformation().ToString();
                    Log.Verbose($"Injecting to crops: {crop.GetSeedId()}: {cropinfo}");
                    if (!data.TryAdd(crop.GetSeedId(), cropinfo))
                        Log.Error($"Crop {crop.GetSeedId()} already exists!");
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting crop for {crop.Name}: {e}");
                }
            }
        }
        private void InjectDataFruitTrees(IAssetData asset)
        {
            var data = asset.AsDictionary<int, string>().Data;
            foreach (var fruitTree in Mod.instance.FruitTrees)
            {
                try
                {
                    string treeinfo = fruitTree.GetFruitTreeInformation();
                    Log.Verbose($"Injecting to fruit trees: {fruitTree.GetSaplingId()}: {treeinfo}");
                    if (!data.TryAdd(fruitTree.GetSaplingId(), treeinfo))
                        Log.Error($"Fruit tree {fruitTree.Name} is a duplicate?");
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting fruit tree for {fruitTree.Name}: {e}");
                }
            }
        }
        private void InjectDataCookingRecipes(IAssetData asset)
        {
            var data = asset.AsDictionary<string, string>().Data;
            foreach (var obj in Mod.instance.Objects)
            {
                try
                {
                    if (obj.Recipe == null)
                        continue;
                    if (obj.Category != ObjectCategory.Cooking)
                        continue;
                    string recipestring = obj.Recipe.GetRecipeString(obj).ToString();
                    Log.Verbose($"Injecting to cooking recipes: {obj.Name}: {recipestring}");
                    if (!data.TryAdd(obj.Name, recipestring))
                        Log.Error($"Recipe for {obj.Name} already seems to exist?");
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting cooking recipe for {obj.Name}: {e}");
                }
            }
        }
        private void InjectDataCraftingRecipes(IAssetData asset)
        {
            var data = asset.AsDictionary<string, string>().Data;
            foreach (var obj in Mod.instance.Objects)
            {
                try
                {
                    if (obj.Recipe == null)
                        continue;
                    if (obj.Category == ObjectCategory.Cooking)
                        continue;
                    string recipestring = obj.Recipe.GetRecipeString(obj).ToString();
                    Log.Verbose($"Injecting to crafting recipes: {obj.Name}: {recipestring}");
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
                    Log.Verbose($"Injecting to crafting recipes: {big.Name}: {recipestring}");
                    if (!data.TryAdd(big.Name, recipestring))
                        Log.Error($"Recipe for {big.Name} already seems to exist?");
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting crafting recipe for {big.Name}: {e}");
                }
            }
        }
        private void InjectDataBigCraftablesInformation(IAssetData asset)
        {
            var data = asset.AsDictionary<int, string>().Data;
            foreach (var big in Mod.instance.BigCraftables)
            {
                try
                {
                    Log.Verbose($"Injecting to big craftables: {big.GetCraftableId()}: {big.GetCraftableInformation()}");
                    if (!data.TryAdd(big.GetCraftableId(), big.GetCraftableInformation()))
                        Log.Error($"{big.Name} already seems to exist!");
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting object information for {big.Name}: {e}");
                }
            }
        }
        private void InjectDataHats(IAssetData asset)
        {
            var data = asset.AsDictionary<int, string>().Data;
            foreach (var hat in Mod.instance.Hats)
            {
                try
                {
                    Log.Verbose($"Injecting to hats: {hat.GetHatId()}: {hat.GetHatInformation()}");
                    if (!data.TryAdd(hat.GetHatId(), hat.GetHatInformation()))
                        Log.Error($"Hat {hat.GetHatId()} appears to be a duplicate???");
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting hat information for {hat.Name}: {e}");
                }
            }
        }
        private void InjectDataWeapons(IAssetData asset)
        {
            var data = asset.AsDictionary<int, string>().Data;
            foreach (var weapon in Mod.instance.Weapons)
            {
                try
                {
                    string weaponData = weapon.GetWeaponInformation();
                    Log.Verbose($"Injecting to weapons: {weapon.GetWeaponId()}: {weaponData}");
                    if (!data.TryAdd(weapon.GetWeaponId(), weaponData))
                        Log.Error($"{weapon.GetWeaponId()} appears to be a duplicate?");
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting weapon information for {weapon.Name}: {e}");
                }
            }
        }
        private void InjectDataClothingInformation(IAssetData asset)
        {
            var data = asset.AsDictionary<int, string>().Data;
            foreach (var shirt in Mod.instance.Shirts)
            {
                try
                {
                    Log.Verbose($"Injecting to clothing information: {shirt.GetClothingId()}: {shirt.GetClothingInformation()}");
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
                    Log.Verbose($"Injecting to clothing information: {pants.GetClothingId()}: {pants.GetClothingInformation()}");
                    data.Add(pants.GetClothingId(), pants.GetClothingInformation());
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting clothing information for {pants.Name}: {e}");
                }
            }
        }
        private void InjectDataTailoringRecipes(IAssetData asset)
        {
            var data = asset.GetData<List<TailorItemRecipe>>();
            foreach (var recipe in Mod.instance.Tailoring)
            {
                try
                {
                    Log.Verbose($"Injecting to tailoring recipe: {recipe.ToGameData()}");
                    data.Add(recipe.ToGameData());
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting tailoring recipe: {e}");
                }
            }
        }
        private void InjectDataBoots(IAssetData asset)
        {
            var data = asset.AsDictionary<int, string>().Data;
            foreach (var boots in Mod.instance.Boots)
            {
                try
                {
                    Log.Verbose($"Injecting to boots: {boots.GetObjectId()}: {boots.GetBootsInformation()}");
                    if (!data.TryAdd(boots.GetObjectId(), boots.GetBootsInformation()))
                        Log.Error($"Boots {boots.Name} appear to be a duplicate?");
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting boots information for {boots.Name}: {e}");
                }
            }
        }
        private void InjectMapsSpringobjects(IAssetData asset)
        {
            if (Mod.instance.Objects.Count == 0)
                return;

            var tex= asset.AsImage();
            tex.ExtendImage(tex.Data.Width, 4096);
            //Texture2D newTex = new Texture2D(Game1.graphics.GraphicsDevice, oldTex.Width, Math.Max(oldTex.Height, 4096));
            //asset.ReplaceWith(newTex);
            //asset.AsImage().PatchImage(oldTex);

            foreach (var obj in Mod.instance.Objects)
            {
                try
                {
                    Log.Verbose($"Injecting {obj.Name} sprites @ {ContentInjector1.ObjectRect(obj.GetObjectId())}");
                    tex.PatchExtendedTileSheet(obj.Texture, null, ContentInjector1.ObjectRect(obj.GetObjectId()));
                    if (obj.IsColored)
                    {
                        Log.Verbose($"Injecting {obj.Name} color sprites @ {ContentInjector1.ObjectRect(obj.GetObjectId() + 1)}");
                        tex.PatchExtendedTileSheet(obj.TextureColor, null, ContentInjector1.ObjectRect(obj.GetObjectId() + 1));
                    }

                    var rect = ContentInjector1.ObjectRect(obj.GetObjectId());
                    var target = TileSheetExtensions.GetAdjustedTileSheetTarget(asset.AssetName, rect);
                    int ts = target.TileSheet;
                    obj.Tilesheet = asset.AssetName + (ts == 0 ? "" : (ts + 1).ToString());
                    obj.TilesheetX = rect.X;
                    obj.TilesheetY = target.Y;
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
                    Log.Verbose($"Injecting {boots.Name} sprites @ {ContentInjector1.ObjectRect(boots.GetObjectId())}");
                    tex.PatchExtendedTileSheet(boots.Texture, null, ContentInjector1.ObjectRect(boots.GetObjectId()));

                    var rect = ContentInjector1.ObjectRect(boots.GetObjectId());
                    var target = TileSheetExtensions.GetAdjustedTileSheetTarget(asset.AssetName, rect);
                    int ts = target.TileSheet;
                    boots.Tilesheet = asset.AssetName + (ts == 0 ? "" : (ts + 1).ToString());
                    boots.TilesheetX = rect.X;
                    boots.TilesheetY = target.Y;
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting sprite for {boots.Name}: {e}");
                }
            }
        }

        private void InjectTileSheetsCrops(IAssetData asset)
        {
            if (Mod.instance.Crops.Count == 0)
                return;

            var tex = asset.AsImage();
            tex.ExtendImage(tex.Data.Width, 4096);

            foreach (var crop in Mod.instance.Crops)
            {
                try
                {
                    Log.Verbose($"Injecting {crop.Name} crop images @ {ContentInjector1.CropRect(crop.GetCropSpriteIndex())}");
                    tex.PatchExtendedTileSheet(crop.Texture, null, ContentInjector1.CropRect(crop.GetCropSpriteIndex()));

                    var rect = ContentInjector1.CropRect(crop.GetCropSpriteIndex());
                    var target = TileSheetExtensions.GetAdjustedTileSheetTarget(asset.AssetName, rect);
                    int ts = target.TileSheet;
                    crop.Tilesheet = asset.AssetName + (ts == 0 ? "" : (ts + 1).ToString());
                    crop.TilesheetX = rect.X;
                    crop.TilesheetY = target.Y;
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting crop sprite for {crop.Name}: {e}");
                }
            }
        }

        private void InjectTileSheetsFruitTrees(IAssetData asset)
        {
            if (Mod.instance.FruitTrees.Count == 0)
                return;

            var tex = asset.AsImage();
            tex.ExtendImage(tex.Data.Width, 4096);

            foreach (var fruitTree in Mod.instance.FruitTrees)
            {
                try
                {
                    Log.Verbose($"Injecting {fruitTree.Name} fruit tree images @ {ContentInjector1.FruitTreeRect(fruitTree.GetFruitTreeIndex())}");
                    tex.PatchExtendedTileSheet(fruitTree.Texture, null, ContentInjector1.FruitTreeRect(fruitTree.GetFruitTreeIndex()));

                    var rect = ContentInjector1.FruitTreeRect(fruitTree.GetFruitTreeIndex());
                    var target = TileSheetExtensions.GetAdjustedTileSheetTarget(asset.AssetName, rect);
                    int ts = target.TileSheet;
                    fruitTree.Tilesheet = asset.AssetName + (ts == 0 ? "" : (ts + 1).ToString());
                    fruitTree.TilesheetX = rect.X;
                    fruitTree.TilesheetY = target.Y;
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting fruit tree sprite for {fruitTree.Name}: {e}");
                }
            }
        }

        private void InjectTileSheetsCraftables(IAssetData asset)
        {
            if (Mod.instance.BigCraftables.Count == 0)
                return;

            var tex = asset.AsImage();
            tex.ExtendImage(tex.Data.Width, 4096);
            Log.Trace($"Big craftables are now ({tex.Data.Width}, {tex.Data.Height})");

            foreach (var big in Mod.instance.BigCraftables)
            {
                try
                {
                    Log.Verbose($"Injecting {big.Name} sprites @ {ContentInjector1.BigCraftableRect(big.GetCraftableId())}");
                    tex.PatchExtendedTileSheet(big.Texture, null, ContentInjector1.BigCraftableRect(big.GetCraftableId()));
                    if (big.ReserveExtraIndexCount > 0)
                    {
                        for (int i = 0; i < big.ReserveExtraIndexCount; ++i)
                        {
                            Log.Verbose($"Injecting {big.Name} reserved extra sprite {i + 1} @ {ContentInjector1.BigCraftableRect(big.GetCraftableId() + i + 1)}");
                            asset.AsImage().PatchExtendedTileSheet(big.ExtraTextures[i], null, ContentInjector1.BigCraftableRect(big.GetCraftableId() + i + 1));
                        }
                    }

                    var rect = ContentInjector1.BigCraftableRect(big.GetCraftableId());
                    int ts = TileSheetExtensions.GetAdjustedTileSheetTarget(asset.AssetName, rect).TileSheet;
                    big.Tilesheet = asset.AssetName + (ts == 0 ? "" : (ts + 1).ToString());
                    big.TilesheetX = rect.X;
                    big.TilesheetY = rect.Y;
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting sprite for {big.Name}: {e}");
                }
            }
        }

        private void InjectCharactersFarmerHats(IAssetData asset)
        {
            if (Mod.instance.Hats.Count == 0)
                return;
                
            var image = asset.AsImage();
            image.ExtendImage(image.Data.Width, 4096);
            Log.Trace($"Hats are now ({image.Data.Width}, {image.Data.Width})");

            foreach (var hat in Mod.instance.Hats)
            {
                try
                {
                    Log.Verbose($"Injecting {hat.Name} sprites @ {ContentInjector1.HatRect(hat.GetHatId())}");
                    asset.AsImage().PatchExtendedTileSheet(hat.Texture, null, ContentInjector1.HatRect(hat.GetHatId()));

                    var rect = ContentInjector1.HatRect(hat.GetHatId());
                    var target = TileSheetExtensions.GetAdjustedTileSheetTarget(asset.AssetName, rect);
                    int ts = target.TileSheet;
                    hat.Tilesheet = asset.AssetName + (ts == 0 ? "" : (ts + 1).ToString());
                    hat.TilesheetX = rect.X;
                    hat.TilesheetY = target.Y;
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting sprite for {hat.Name}: {e}");
                }
            }
        }

        private void InjectTileSheetsWeapons(IAssetData asset)
        {
            if (Mod.instance.Weapons.Count == 0)
                return;

            var tex = asset.AsImage();
            tex.ExtendImage(tex.Data.Width, 4096);
            Log.Trace($"Weapons are now ({tex.Data.Width}, {tex.Data.Height})");

            foreach (var weapon in Mod.instance.Weapons)
            {
                try
                {
                    Log.Verbose($"Injecting {weapon.Name} sprites @ {ContentInjector1.WeaponRect(weapon.GetWeaponId())}");
                    tex.PatchImage(weapon.Texture, null, ContentInjector1.WeaponRect(weapon.GetWeaponId()));

                    var rect = ContentInjector1.WeaponRect(weapon.GetWeaponId());
                    int ts = 0;// TileSheetExtensions.GetAdjustedTileSheetTarget(asset.AssetName, rect).TileSheet;
                    weapon.Tilesheet = asset.AssetName + (ts == 0 ? "" : (ts + 1).ToString());
                    weapon.TilesheetX = rect.X;
                    weapon.TilesheetY = rect.Y;
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting sprite for {weapon.Name}: {e}");
                }
            }
        }
        private void InjectCharactersFarmerShirts(IAssetData asset)
        {
            if (Mod.instance.Shirts.Count == 0)
                return;

            var oldTex = asset.AsImage().Data;
            asset.AsImage().ExtendImage(oldTex.Width, 4096);
            Log.Trace($"Shirts are now ({oldTex.Width}, {Math.Max(oldTex.Height, 4096)})");

            foreach (var shirt in Mod.instance.Shirts)
            {
                try
                {
                    if (Mod.instance.Monitor.IsVerbose)
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

                        Log.Verbose($"Injecting {shirt.Name} sprites @ {string.Join(',', rects)}");
                    }
                    asset.AsImage().PatchExtendedTileSheet(shirt.TextureMale, null, ContentInjector1.ShirtRectPlain(shirt.GetMaleIndex()));
                    if (shirt.Dyeable)
                        asset.AsImage().PatchExtendedTileSheet(shirt.TextureMaleColor, null, ContentInjector1.ShirtRectDye(shirt.GetMaleIndex()));
                    if (shirt.HasFemaleVariant)
                    {
                        asset.AsImage().PatchExtendedTileSheet(shirt.TextureFemale, null, ContentInjector1.ShirtRectPlain(shirt.GetFemaleIndex()));
                        if (shirt.Dyeable)
                            asset.AsImage().PatchExtendedTileSheet(shirt.TextureFemaleColor, null, ContentInjector1.ShirtRectDye(shirt.GetFemaleIndex()));
                    }
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting sprite for {shirt.Name}: {e}");
                }
            }
        }

        private void InjectCharactersFarmerPants(IAssetData asset)
        {
            if (Mod.instance.Pants.Count == 0)
                return;

            var tex = asset.AsImage();
            tex.ExtendImage(tex.Data.Width, 4096);
            Log.Trace($"Pants are now ({tex.Data.Width}, {tex.Data.Height})");

            foreach (var pants in Mod.instance.Pants)
            {
                try
                {
                    Log.Verbose($"Injecting {pants.Name} sprites @ {ContentInjector1.PantsRect(pants.GetTextureIndex())}");
                    tex.PatchExtendedTileSheet(pants.Texture, null, ContentInjector1.PantsRect(pants.GetTextureIndex()));
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting sprite for {pants.Name}: {e}");
                }
            }
        }

        private void InjectCharactersFarmerShoeColors(IAssetData asset)
        {
            var tex = asset.AsImage();
            tex.ExtendImage(tex.Data.Width, 4096);
            Log.Trace($"Boots are now ({tex.Data.Width}, {tex.Data.Height})");

            foreach (var boots in Mod.instance.Boots)
            {
                try
                {
                    Log.Verbose($"Injecting {boots.Name} sprites @ {ContentInjector1.BootsRect(boots.GetTextureIndex())}");
                    tex.PatchExtendedTileSheet(boots.TextureColor, null, ContentInjector1.BootsRect(boots.GetTextureIndex()));
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting sprite for {boots.Name}: {e}");
                }
            }
        }
        internal static Rectangle ObjectRect(int index)
        {
            return new(index % 24 * 16, index / 24 * 16, 16, 16);
        }
        internal static Rectangle CropRect(int index)
        {
            return new(index % 2 * 128, index / 2 * 32, 128, 32);
        }
        internal static Rectangle FruitTreeRect(int index)
        {
            return new(0, index * 80, 432, 80);
        }
        internal static Rectangle BigCraftableRect(int index)
        {
            return new(index % 8 * 16, index / 8 * 32, 16, 32);
        }
        internal static Rectangle HatRect(int index)
        {
            return new(index % 12 * 20, index / 12 * 80, 20, 80);
        }
        internal static Rectangle WeaponRect(int index)
        {
            return new(index % 8 * 16, index / 8 * 16, 16, 16);
        }
        internal static Rectangle ShirtRectPlain(int index)
        {
            return new(index % 16 * 8, index / 16 * 32, 8, 32);
        }
        internal static Rectangle ShirtRectDye(int index)
        {
            var rect = ContentInjector1.ShirtRectPlain(index);
            rect.X += 16 * 8;
            return rect;
        }
        internal static Rectangle PantsRect(int index)
        {
            return new(index % 10 * 192, index / 10 * 688, 192, 688);
        }
        internal static Rectangle BootsRect(int index)
        {
            return new(0, index, 4, 1);
        }

        public bool CanLoad<T>(IAssetInfo asset)
        {
            foreach (var fence in Mod.instance.Fences)
            {
                if (asset.AssetNameEquals("LooseSprites\\Fence" + fence.CorrespondingObject?.GetObjectId()))
                    return true;
            }
            return false;
        }

        public T Load<T>(IAssetInfo asset)
        {
            foreach (var fence in Mod.instance.Fences)
            {
                if (asset.AssetNameEquals("LooseSprites\\Fence" + fence.CorrespondingObject?.GetObjectId()))
                    return (T)(object)fence.Texture;
            }
            return default(T);
        }
    }
}
