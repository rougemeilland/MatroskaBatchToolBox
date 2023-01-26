namespace MatroskaBatchToolBox.Model.json
{
    public class LocalSettingsContainer
    {
        public LocalSettingsContainer()
        {
            FFmpegVideoEncoder = null;
            FFmpegLibx264EncoderOption = null;
            FFmpegLibx265EncoderOption = null;
            FFmpegLibaomAV1EncoderOption = null;
            CalculateVMAFScore = null;
        }

        public string? FFmpegVideoEncoder { get; set; }
        public string? FFmpegLibx264EncoderOption { get; set; }
        public string? FFmpegLibx265EncoderOption { get; set; }
        public string? FFmpegLibaomAV1EncoderOption { get; set; }
        public string? FFmpegOption { get; set; }
        public bool? CalculateVMAFScore { get; set; }
    }
}
