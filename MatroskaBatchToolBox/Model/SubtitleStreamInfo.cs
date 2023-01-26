using MatroskaBatchToolBox.Model.Json;

namespace MatroskaBatchToolBox.Model
{
    internal class SubtitleStreamInfo
        : StreamInfo
    {
        public SubtitleStreamInfo(MovieStreamInfoContainer stream, int indexWithinSubtitleStream)
            : base(stream)
        {
            IndexWithinSubtitleStream = indexWithinSubtitleStream;
        }

        public int IndexWithinSubtitleStream { get; }
    }
}
