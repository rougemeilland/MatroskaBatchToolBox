﻿using System.Text.Json.Serialization;

namespace MatroskaBatchToolBox.Utility.Models.Json
{
    public class MovieChapterContainer
    {
        public MovieChapterContainer()
        {
            TimeBase = "";
            StartTime = "";
            EndTime = "";
        }

        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("time_base")]
        public string TimeBase { get; set; } // 1/1000000000 などの文字列

        [JsonPropertyName("start")]
        public long Start { get; set; }

        [JsonPropertyName("start_time")]
        public string StartTime { get; set; } // 秒単位の文字列

        [JsonPropertyName("end")]
        public long End { get; set; }

        [JsonPropertyName("end_time")]
        public string EndTime { get; set; } // 秒単位の文字列

        [JsonPropertyName("tags")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public MovieChapterTagContainer? Tags { get; set; }
    }
}
