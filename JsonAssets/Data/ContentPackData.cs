using System.Collections.Generic;
using System.Diagnostics;

namespace JsonAssets.Data
{
    [DebuggerDisplay("name = {Name}")]
    public class ContentPackData
    {
        /*********
        ** Accessors
        *********/
        public string Name { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public string Author { get; set; }
        public IList<string> UpdateKeys { get; set; } = new List<string>();
    }
}
