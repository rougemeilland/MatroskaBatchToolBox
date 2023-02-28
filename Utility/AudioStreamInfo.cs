using Utility.Models.Json;

namespace Utility
{
    public class AudioStreamInfo
        : StreamInfo
    {
        internal AudioStreamInfo(MovieStreamInfoContainer stream, int indexWithinAudioStream)
            : base(stream)
        {
            IndexWithinAudioStream = indexWithinAudioStream;
        }

        public int IndexWithinAudioStream { get; }
    }
}
