using System.Text.Json.Serialization;

namespace MatroskaBatchToolBox.Utility.Models.Json
{
    public class MovieStreamDispositionContainer
    {
        [JsonPropertyName("default")]
        public int Default { get; set; }

        [JsonPropertyName("forced")]
        public int Forced { get; set; }

        [JsonPropertyName("attached_pic")]
        public int AttachedPicture { get; set; }
    }
}
