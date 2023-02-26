using System.Text.Json.Serialization;

namespace Utility.Models.Json
{
    public class MovieStreamTagsContainer
    {
        public MovieStreamTagsContainer()
        {
            Language = null;
            Title = null;
        }

        [JsonPropertyName("language")]
        public string? Language { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }
    }
}
