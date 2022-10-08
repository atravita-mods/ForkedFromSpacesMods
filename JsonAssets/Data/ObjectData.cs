using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Text;
using JsonAssets.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using SpaceShared;
using SObject = StardewValley.Object;
using StardewModdingAPI;
using JsonAssets.Framework.Internal;

namespace JsonAssets.Data
{
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = DiagnosticMessages.IsPublicApi)]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.IsPublicApi)]
    [DebuggerDisplay("name = {Name}, id = {Id}")]
    public class ObjectData : DataNeedsIdWithTexture, ITranslatableItem
    {
        /*********
        ** Accessors
        *********/
        [JsonIgnore]
        public IRawTextureData TextureColor { get; set; }

        /// <inheritdoc />
        public string Description { get; set; }
        public ObjectCategory Category { get; set; }
        public string CategoryTextOverride { get; set; }
        public Color CategoryColorOverride { get; set; } = new(0, 0, 0, 0);
        public bool IsColored { get; set; } = false;

        public int Price { get; set; }

        public bool CanTrash { get; set; } = true;
        public bool CanSell { get; set; } = true;
        public bool CanBeGifted { get; set; } = true;

        public bool HideFromShippingCollection { get; set; } = false;

        public ObjectRecipe Recipe { get; set; }

        public int Edibility { get; set; } = SObject.inedible;
        public bool EdibleIsDrink { get; set; } = false;
        public ObjectFoodBuffs EdibleBuffs { get; set; } = new();

        public bool CanPurchase { get; set; } = false;
        public int PurchasePrice { get; set; }
        public string PurchaseFrom { get; set; } = "Pierre";
        public IList<string> PurchaseRequirements { get; set; } = new List<string>();
        public IList<PurchaseData> AdditionalPurchaseData { get; set; } = new List<PurchaseData>();

        public ObjectGiftTastes GiftTastes { get; set; } = new();

        /// <inheritdoc />
        public Dictionary<string, string> NameLocalization { get; set; } = new();

        /// <inheritdoc />
        public Dictionary<string, string> DescriptionLocalization { get; set; } = new();

        /// <inheritdoc />
        public string TranslationKey { get; set; }

        public List<string> ContextTags { get; set; } = new();

        // A list of IDs that match rings for JA.
        [JsonIgnore]
        internal static HashSet<int> TrackedRings { get; } = new();

        /*********
        ** Public methods
        *********/
        public int GetObjectId()
        {
            return this.Id;
        }

        internal string GetObjectInformation()
        {
            if (this.Edibility != SObject.inedible)
            {
                StringBuilder sb = StringBuilderCache.Acquire();
                sb.Append($"{this.Name}/{this.Price}/{this.Edibility}/")
                .Append(this.Category == ObjectCategory.Artifact ? "Arch" : $"{this.Category} {this.Category:D}")
                .Append($"/{this.LocalizedName()}/{this.LocalizedDescription()}/")
                .Append(this.EdibleIsDrink ? "drink" : "food").Append('/')
                .Append($"{this.EdibleBuffs.Farming} {this.EdibleBuffs.Fishing} {this.EdibleBuffs.Mining} 0 {this.EdibleBuffs.Luck} {this.EdibleBuffs.Foraging} 0 {this.EdibleBuffs.MaxStamina} {this.EdibleBuffs.MagnetRadius} {this.EdibleBuffs.Speed} {this.EdibleBuffs.Defense} {this.EdibleBuffs.Attack}/{this.EdibleBuffs.Duration}");
                return StringBuilderCache.GetStringAndRelease(sb);
            }
            else
            {
                return $"{this.Name}/{this.Price}/{this.Edibility}/{(this.Category == ObjectCategory.Artifact ? "Arch" : $"Basic {this.Category:D}")}/{this.LocalizedName()}/{this.LocalizedDescription()}";
            }
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Normalize the model after it's deserialized.</summary>
        /// <param name="context">The deserialization context.</param>
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            this.EdibleBuffs ??= new();
            this.PurchaseRequirements ??= new List<string>();
            this.AdditionalPurchaseData ??= new List<PurchaseData>();
            this.GiftTastes ??= new();
            this.NameLocalization ??= new();
            this.DescriptionLocalization ??= new();
            this.ContextTags ??= new();

            this.PurchaseRequirements.FilterNulls();
            this.AdditionalPurchaseData.FilterNulls();
            this.ContextTags.FilterNulls();
        }
    }
}
