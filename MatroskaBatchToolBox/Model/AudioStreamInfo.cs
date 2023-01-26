using MatroskaBatchToolBox.Model.Json;

namespace MatroskaBatchToolBox.Model
{
    internal class AudioStreamInfo
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
