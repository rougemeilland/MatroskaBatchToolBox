using MatroskaBatchToolBox.Model.json;

namespace MatroskaBatchToolBox.Model
{
    internal class StreamTags
    {
        public StreamTags(MovieStreamTagsContainer? tags)
        {
            Title = tags?.title;
            Language = tags?.language;
        }

        public string? Title { get; }

        public string? Language { get; }
    }
}
