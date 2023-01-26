namespace MatroskaBatchToolBox.Model.json
{
    public class GlobalSettingsContainer
    {
        public GlobalSettingsContainer()
        {
            FFmpegNormalizeCommandPath = null;
            FFmpegVideoEncoder = null;
            FFmpegLibx264EncoderOption = null;
            FFmpegLibx265EncoderOption = null;
            FFmpegLibaomAV1EncoderOption = null;
            FFmpegOption = null;
            CalculateVMAFScore = null;
            DegreeOfParallelism = null;
        }

        public string? FFmpegNormalizeCommandPath { get; set; }
        public string? FFmpegVideoEncoder { get; set; }
        public string? FFmpegLibx264EncoderOption { get; set; }
        public string? FFmpegLibx265EncoderOption { get; set; }
        public string? FFmpegLibaomAV1EncoderOption { get; set; }
        public string? FFmpegOption { get; set; }
        public bool? CalculateVMAFScore { get; set; }
        public int? DegreeOfParallelism { get; set; }
    }
}
