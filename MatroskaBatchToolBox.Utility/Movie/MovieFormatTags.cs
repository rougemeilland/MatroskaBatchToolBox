using System.Collections.Generic;
using System.Text.Json;

namespace MatroskaBatchToolBox.Utility.Movie
{
    public class MovieFormatTags
        : TagCollection
    {
        internal MovieFormatTags(Dictionary<string, JsonElement>? tags)
            : base(tags)
        {
        }
    }
}
