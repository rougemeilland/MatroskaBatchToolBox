using System.Text.Json.Serialization;

namespace MatroskaBatchToolBox.Model.Json
{
    public class MovieStreamDispositionContainer
    {
        public MovieStreamDispositionContainer()
        {
            Default = 0;
            Forced = 0;
        }

        [JsonPropertyName("default")]
        public int Default { get; set; }

        [JsonPropertyName("forced")]
        public int Forced { get; set; }
    }
}
