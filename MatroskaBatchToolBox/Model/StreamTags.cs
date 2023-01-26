using MatroskaBatchToolBox.Model.Json;

namespace MatroskaBatchToolBox.Model
{
    internal class StreamTags
    {
        public StreamTags(MovieStreamTagsContainer? tags)
        {
            Title = tags?.Title;
            Language = tags?.Language;
        }

        public string? Title { get; }

        public string? Language { get; }
    }
}
