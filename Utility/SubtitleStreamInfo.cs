using Utility.Models.Json;

namespace Utility
{
    public class SubtitleStreamInfo
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
