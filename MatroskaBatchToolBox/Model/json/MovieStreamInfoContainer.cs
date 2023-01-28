using System.Text.Json.Serialization;

namespace MatroskaBatchToolBox.Model.Json
{
    public class MovieStreamInfoContainer
    {
        public MovieStreamInfoContainer()
        {
            Index = null;
            codecName = null;
            CodecLongName = null;
            CodecType = null;
            Tags = new MovieStreamTagsContainer();
        }

        [JsonPropertyName("index")]
        public int? Index { get; set; }

        [JsonPropertyName("codec_name")]
        public string? codecName { get; set; }

        [JsonPropertyName("codec_long_name")]
        public string? CodecLongName { get; set; }

        [JsonPropertyName("codec_type")]
        public string? CodecType { get; set; }

        [JsonPropertyName("tags")]
        public MovieStreamTagsContainer Tags { get; set; }

        [JsonPropertyName("width")]
        public int? Width { get; set; }

        [JsonPropertyName("height")]
        public int? Height { get; set; }

        [JsonPropertyName("display_aspect_ratio")]
        public string? DisplayAspectRatio { get; set; }
    }
}
