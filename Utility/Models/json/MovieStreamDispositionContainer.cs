using System.Text.Json.Serialization;

namespace Utility.Models.Json
{
    public class MovieStreamDispositionContainer
    {
        public MovieStreamDispositionContainer()
        {
            Default = 0;
            Forced = 0;
            AttachedPicture = 0;
        }

        [JsonPropertyName("default")]
        public int Default { get; set; }

        [JsonPropertyName("forced")]
        public int Forced { get; set; }

        [JsonPropertyName("attached_pic")]
        public int AttachedPicture { get; set; }
    }
}
