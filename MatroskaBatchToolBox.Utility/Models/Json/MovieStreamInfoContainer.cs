using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MatroskaBatchToolBox.Utility.Models.Json
{
    public class MovieStreamInfoContainer
    {
        public MovieStreamInfoContainer()
            => Disposition = new MovieStreamDispositionContainer();

        [JsonPropertyName("index")]
        public int? Index { get; set; }

        [JsonPropertyName("codec_name")]
        public string? CodecName { get; set; }

        [JsonPropertyName("codec_long_name")]
        public string? CodecLongName { get; set; }

        [JsonPropertyName("codec_type")]
        public string? CodecType { get; set; }

        [JsonPropertyName("disposition")]
        public MovieStreamDispositionContainer Disposition { get; set; }

        [JsonPropertyName("tags")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, JsonElement>? Tags { get; set; }

        #region for video stream

        [JsonPropertyName("width")]
        public int? Width { get; set; }

        [JsonPropertyName("height")]
        public int? Height { get; set; }

        [JsonPropertyName("display_aspect_ratio")]
        public string? DisplayAspectRatio { get; set; }

        #endregion

        #region for audio stream

        [JsonPropertyName("sample_fmt")]
        public string? SampleFormat { get; set; }

        [JsonPropertyName("sample_rate")]
        public string? SampleRate { get; set; }

        [JsonPropertyName("channels")]
        public int? Channels { get; set; }

        [JsonPropertyName("channel_layout")]
        public string? ChannelLayout { get; set; }

        [JsonPropertyName("bits_per_sample")]
        public int? BitsPerSample { get; set; }

        [JsonPropertyName("initial_padding")]
        public int? InitialPadding { get; set; }

        [JsonPropertyName("r_frame_rate")]
        public string? RFrameRate { get; set; }

        [JsonPropertyName("avg_frame_rate")]
        public string? AverageFrameRate { get; set; }

        [JsonPropertyName("time_base")]
        public string? TimeBase { get; set; }

        [JsonPropertyName("start_pts")]
        public int? StartPts { get; set; }

        [JsonPropertyName("start_time")]
        public string? StartTime { get; set; }

        [JsonPropertyName("bit_rate")]
        public string? BitRate { get; set; }

        #endregion
    }
}
