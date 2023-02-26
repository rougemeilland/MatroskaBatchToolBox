using System.Text.Json.Serialization;

namespace Utility.Models.Json
{
    public class MovieChapterTagContainer
    {
        public MovieChapterTagContainer()
        {
            Title = null;
        }

        [JsonPropertyName("title")]
        public string? Title { get; set; }
    }
}
