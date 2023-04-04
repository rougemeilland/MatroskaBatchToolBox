using System.Text.Json.Serialization;

namespace MatroskaBatchToolBox.Utility.Models.Json
{
    public class MovieChapterTagContainer
    {
        [JsonPropertyName("title")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Title { get; set; }
    }
}
