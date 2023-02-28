using Utility.Models.Json;

namespace Utility
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
