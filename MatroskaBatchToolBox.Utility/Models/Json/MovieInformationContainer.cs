using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MatroskaBatchToolBox.Utility.Models.Json
{
    public class MovieInformationContainer
    {
        [JsonPropertyName("format")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public MovieFormatContainer? Format { get; set; }

        [JsonPropertyName("streams")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<MovieStreamInfoContainer>? Streams { get; set; }

        [JsonPropertyName("chapters")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<MovieChapterContainer>? Chapters { get; set; }
    }
}
