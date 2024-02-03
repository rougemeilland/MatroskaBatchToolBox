using System.Text.Json.Serialization;

namespace MatroskaBatchToolBox.Model.Json
{
    internal class GlobalSettingsContainer
    {
        [JsonPropertyName("ffmpeg_normalize_command_file_path")]
        public string? FfmpegNormalizeCommandFilePath { get; set; }

        [JsonPropertyName("ffmpeg_video_encoder")]
        public string? FfmpegVideoEncoder { get; set; }

        [JsonPropertyName("ffmpeg_libx264_encoder_option")]
        public string? FfmpegLibx264EncoderOption { get; set; }

        [JsonPropertyName("ffmpeg_libx265_encoder_option")]
        public string? FfmpegLibx265EncoderOption { get; set; }

        [JsonPropertyName("ffmpeg_libaom-av1_encoder_option")]
        public string? FfmpegLibaomAv1EncoderOption { get; set; }

        [JsonPropertyName("ffmpeg_option")]
        public string? FfmpegOption { get; set; }

        [JsonPropertyName("delete_chapters")]
        public bool? DeleteChapters { get; set; }

        [JsonPropertyName("do_not_keep_chapter_titles")]
        public bool? KeepChapterTitles { get; }

        [JsonPropertyName("delete_metadata")]
        public bool? DeleteMetadata { get; set; }

        [JsonPropertyName("delete_image_video_stream")]
        public bool? DeleteImageVideoStream { get; set; }

        [JsonPropertyName("allow_multiple_vodeo_streams")]
        public bool? AllowMultipleVideoStreams { get; set; }

        [JsonPropertyName("behavior_for_data_streams")]
        public string? BehaviorForDataStreams { get; set; }

        [JsonPropertyName("behavior_for_attachment_streams")]
        public string? BehaviorForAttachmentStreams { get; set; }

        [JsonPropertyName("default_video_language")]
        public string? DefaultVideoLanguage { get; set; }

        [JsonPropertyName("default_audio_language")]
        public string? DefaultAudioLanguage { get; set; }

        [JsonPropertyName("reset_forced_stream")]
        public bool? ResetForcedStream { get; set; }

        [JsonPropertyName("reset_default_stream")]
        public bool? ResetDefaultStream { get; set; }

        [JsonPropertyName("calculate_vmaf_score")]
        public bool? CalculateVmafScore { get; set; }

        [JsonPropertyName("degree_of_parallelism")]
        public int? DegreeOfParallelism { get; set; }
    }
}
