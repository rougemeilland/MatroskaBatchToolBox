using Utility.Models.Json;

namespace Utility
{
    public class AudioStreamInfo
        : StreamInfo
    {
        public AudioStreamInfo(MovieStreamInfoContainer stream, int indexWithinAudioStream)
            : base(stream)
        {
            IndexWithinAudioStream = indexWithinAudioStream;
        }

        public int IndexWithinAudioStream { get; }
    }
}
