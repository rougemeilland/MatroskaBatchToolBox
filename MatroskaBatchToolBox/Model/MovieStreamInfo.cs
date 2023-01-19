namespace MatroskaBatchToolBox.Model
{
    public class MovieStreamInfo
    {
        public MovieStreamInfo()
        {
            index = null;
            codec_name = null;
            codec_long_name = null;
            codec_type = null;
        }

        public int? index { get; set; }
        public string? codec_name { get; set; }
        public string? codec_long_name { get; set; }
        public string? codec_type { get; set; }
    }
}
