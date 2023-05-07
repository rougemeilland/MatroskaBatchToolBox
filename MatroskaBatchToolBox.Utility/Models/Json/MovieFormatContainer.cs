using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MatroskaBatchToolBox.Utility.Models.Json
{
    public class MovieFormatContainer
    {
        public MovieFormatContainer()
        {
            FilePath = string.Empty;
            FormatName = string.Empty;
            FormatLongName = string.Empty;
            StartTime = string.Empty;
            Duration = string.Empty;
            Size = string.Empty;
            BitRate = string.Empty;
        }

        [JsonPropertyName("filename")]
        public string FilePath { get; set; }

        [JsonPropertyName("nb_streams")]
        public int StreamsCount { get; set; }

        [JsonPropertyName("nb_programs")]
        public int ProgramsCount { get; set; }

        [JsonPropertyName("format_name")]
        public string FormatName { get; set; }

        [JsonPropertyName("format_long_name")]
        public string FormatLongName { get; set; }

        [JsonPropertyName("start_time")]
        public string StartTime { get; set; }

        [JsonPropertyName("duration")]
        public string Duration { get; set; }

        [JsonPropertyName("size")]
        public string Size { get; set; }

        [JsonPropertyName("bit_rate")]
        public string? BitRate { get; set; }

        [JsonPropertyName("probe_score")]
        public int ProbeScore { get; set; }

        [JsonPropertyName("tags")]
        public Dictionary<string, JsonElement>? Tags { get; set; }
    }
}
