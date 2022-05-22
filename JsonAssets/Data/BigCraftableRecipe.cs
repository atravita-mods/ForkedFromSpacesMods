using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using JsonAssets.Framework;
using StardewValley;

namespace JsonAssets.Data
{
    public class BigCraftableRecipe
    {
        /*********
        ** Accessors
        *********/
        public string SkillUnlockName { get; set; } = null;
        public int SkillUnlockLevel { get; set; } = -1;

        public int ResultCount { get; set; } = 1;
        public IList<BigCraftableIngredient> Ingredients { get; set; } = new List<BigCraftableIngredient>();

        public bool IsDefault { get; set; } = false;
        public bool CanPurchase { get; set; } = false;
        public int PurchasePrice { get; set; }
        public string PurchaseFrom { get; set; } = "Gus";
        public IList<string> PurchaseRequirements { get; set; } = new List<string>();
        public IList<PurchaseData> AdditionalPurchaseData { get; set; } = new List<PurchaseData>();


        /*********
        ** Public methods
        *********/
        internal StringBuilder GetRecipeString(BigCraftableData parent)
        {
            StringBuilder str = new();
            foreach (var ingredient in this.Ingredients)
                str.Append(Mod.instance.ResolveObjectId(ingredient.Object) + " " + ingredient.Count + " ");
            str.Remove(str.Length-1, 1);
            str.Append($"/what is this for?/{parent.Id} {this.ResultCount}/true/");
            if (this.SkillUnlockName?.Length > 0 && this.SkillUnlockLevel > 0)
                str.Append(this.SkillUnlockName + " " + this.SkillUnlockLevel);
            else
                str.Append("null");
            if (LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.en)
                str.Append("/" + parent.LocalizedName());
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
            this.Ingredients ??= new List<BigCraftableIngredient>();
            this.PurchaseRequirements ??= new List<string>();
            this.AdditionalPurchaseData ??= new List<PurchaseData>();

            this.Ingredients.FilterNulls();
            this.PurchaseRequirements.FilterNulls();
            this.AdditionalPurchaseData.FilterNulls();
        }
    }
}
