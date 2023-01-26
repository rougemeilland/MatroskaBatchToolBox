namespace MatroskaBatchToolBox.Model.json
{
    public class MovieStreamInfoContainer
    {
        public MovieStreamInfoContainer()
        {
            index = null;
            codec_name = null;
            codec_long_name = null;
            codec_type = null;
            tags = new MovieStreamTagsContainer();
        }

        public int? index { get; set; }
        public string? codec_name { get; set; }
        public string? codec_long_name { get; set; }
        public string? codec_type { get; set; }
        public MovieStreamTagsContainer tags { get; set; }
    }
}
