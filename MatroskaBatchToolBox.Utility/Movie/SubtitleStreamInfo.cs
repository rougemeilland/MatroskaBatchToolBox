using MatroskaBatchToolBox.Utility.Models.Json;

namespace MatroskaBatchToolBox.Utility.Movie
{
    public class SubtitleStreamInfo
        : StreamInfo
    {
        internal SubtitleStreamInfo(MovieStreamInfoContainer stream, int indexWithinSubtitleStream)
            : base(stream)
        {
            IndexWithinSubtitleStream = indexWithinSubtitleStream;
        }

        public int IndexWithinSubtitleStream { get; }
    }
}
