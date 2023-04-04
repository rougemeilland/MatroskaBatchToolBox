using MatroskaBatchToolBox.Utility.Models.Json;

namespace MatroskaBatchToolBox.Utility.Movie
{
    public class AudioStreamInfo
        : StreamInfo
    {
        internal AudioStreamInfo(MovieStreamInfoContainer stream, int indexWithinAudioStream)
            : base(stream)
            => IndexWithinAudioStream = indexWithinAudioStream;

        public int IndexWithinAudioStream { get; }
    }
}
