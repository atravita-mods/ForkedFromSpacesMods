using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using SpaceShared;
using StardewModdingAPI;

namespace JsonAssets.Data
{
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = DiagnosticMessages.IsPublicApi)]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.IsPublicApi)]
    [DebuggerDisplay("name = {Name}")]
    public class ShirtData : ClothingData
    {
        /*********
        ** Accessors
        *********/
        [JsonIgnore]
        public IRawTextureData TextureMaleColor { get; set; }

        [JsonIgnore]
        public IRawTextureData TextureFemaleColor { get; set; }
    }
}
