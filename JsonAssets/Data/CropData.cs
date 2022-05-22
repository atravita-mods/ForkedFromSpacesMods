using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using JsonAssets.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using SpaceShared;

namespace JsonAssets.Data
{
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = DiagnosticMessages.IsPublicApi)]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.IsPublicApi)]
    public class CropData : DataNeedsIdWithTexture
    {
        /*********
        ** Accessors
        *********/
        [JsonIgnore]
        public Texture2D GiantTexture { get; set; }

        public object Product { get; set; }
        public string SeedName { get; set; }
        public string SeedDescription { get; set; }

        public CropType CropType { get; set; } = CropType.Normal;

        public IList<string> Seasons { get; set; } = new List<string>();
        public IList<int> Phases { get; set; } = new List<int>();
        public int RegrowthPhase { get; set; } = -1;
        public bool HarvestWithScythe { get; set; } = false;
        public bool TrellisCrop { get; set; } = false;
        public IList<Color> Colors { get; set; } = new List<Color>();
        public CropBonus Bonus { get; set; } = null;

        public IList<string> SeedPurchaseRequirements { get; set; } = new List<string>();
        public int SeedPurchasePrice { get; set; }
        public int SeedSellPrice { get; set; } = -1;
        public string SeedPurchaseFrom { get; set; } = "Pierre";
        public IList<PurchaseData> SeedAdditionalPurchaseData { get; set; } = new List<PurchaseData>();

        public Dictionary<string, string> SeedNameLocalization { get; set; } = new();
        public Dictionary<string, string> SeedDescriptionLocalization { get; set; } = new();
        public string SeedTranslationKey { get; set; }

        internal ObjectData Seed { get; set; }


        /*********
        ** Public methods
        *********/
        public int GetSeedId()
        {
            return this.Seed.Id;
        }

        public int GetCropSpriteIndex()
        {
            return this.Id;
        }

        internal StringBuilder GetCropInformation()
        {
            StringBuilder str = new();
            //str += GetProductId() + "/";
            str.AppendJoin(' ', this.Phases).Append('/')
               .AppendJoin(' ', this.Seasons).Append('/')
               .Append($"{this.GetCropSpriteIndex()}/{Mod.instance.ResolveObjectId(this.Product)}/{this.RegrowthPhase}/")
               .Append(this.HarvestWithScythe ? "1" : "0").Append('/');
            if (this.Bonus != null)
                str.Append($"true {this.Bonus.MinimumPerHarvest} {this.Bonus.MaximumPerHarvest} {this.Bonus.MaxIncreasePerFarmLevel} {this.Bonus.ExtraChance}/");
            else
                str.Append("false/");
            str.Append((this.TrellisCrop ? "true" : "false")).Append('/');
            if (this.Colors.Any())
            {
                str.Append("true ").AppendJoin(' ', this.Colors.Select(color=>$"{color.R} {color.G} {color.B}"));
            }
            else
                str.Append("false");
            return str;
        }

        /*********
        ** Private methods
        *********/
        /// <summary>Normalize the model after it's deserialized.</summary>
        /// <param name="context">The deserialization context.</param>
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            this.Seasons ??= new List<string>();
            this.Phases ??= new List<int>();
            this.Colors ??= new List<Color>();
            this.SeedPurchaseRequirements ??= new List<string>();
            this.SeedAdditionalPurchaseData ??= new List<PurchaseData>();
            this.SeedNameLocalization ??= new();
            this.SeedDescriptionLocalization ??= new();

            this.Seasons.FilterNulls();
            this.SeedPurchaseRequirements.FilterNulls();
            this.SeedAdditionalPurchaseData.FilterNulls();
        }
    }
}
