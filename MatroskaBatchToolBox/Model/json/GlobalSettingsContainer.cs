using System.Text.Json.Serialization;

namespace MatroskaBatchToolBox.Model.Json
{
    public class GlobalSettingsContainer
    {
        public GlobalSettingsContainer()
        {
            FFmpegNormalizeCommandFilePath = null;
            FFmpegVideoEncoder = null;
            FFmpegLibx264EncoderOption = null;
            FFmpegLibx265EncoderOption = null;
            FFmpegLibaomAV1EncoderOption = null;
            FFmpegOption = null;
            DeleteChapters = null;
            DeleteImageVideoStream = null;
            AllowMultipleVideoStreams = null;
            CalculateVMAFScore = null;
            DegreeOfParallelism = null;
        }

        [JsonPropertyName("ffmpeg_normalize_command_file_path")]
        public string? FFmpegNormalizeCommandFilePath { get; set; }

        [JsonPropertyName("ffmpeg_video_encoder")]
        public string? FFmpegVideoEncoder { get; set; }

        [JsonPropertyName("ffmpeg_libx264_encoder_option")]
        public string? FFmpegLibx264EncoderOption { get; set; }

        [JsonPropertyName("ffmpeg_libx265_encoder_option")]
        public string? FFmpegLibx265EncoderOption { get; set; }

        [JsonPropertyName("ffmpeg_libaom-av1_encoder_option")]
        public string? FFmpegLibaomAV1EncoderOption { get; set; }

        [JsonPropertyName("ffmpeg_option")]
        public string? FFmpegOption { get; set; }

        [JsonPropertyName("delete_chapters")]
        public bool? DeleteChapters { get; set; }

        [JsonPropertyName("delete_metadata")]
        public bool? DeleteMetadata { get; set; }

        [JsonPropertyName("delete_image_video_stream")]
        public bool? DeleteImageVideoStream { get; set; }

        [JsonPropertyName("allow_multiple_vodeo_streams")]
        public bool? AllowMultipleVideoStreams { get; set; }

        [JsonPropertyName("cropping")]
        public string? Cropping { get; set; }

        [JsonPropertyName("trimming")]
        public string? Trimming { get; set; }

        [JsonPropertyName("calculate_vmaf_score")]
        public bool? CalculateVMAFScore { get; set; }

        [JsonPropertyName("degree_of_parallelism")]
        public int? DegreeOfParallelism { get; set; }
    }
}
