using MatroskaBatchToolBox.Model.Json;

namespace MatroskaBatchToolBox.Model
{
    internal class VideoStreamInfo
        : StreamInfo
    {
        public VideoStreamInfo(MovieStreamInfoContainer stream, int indexWithinVideoStream)
            : base(stream)
        {
            IndexWithinVideoStream = indexWithinVideoStream;
        }

        public int IndexWithinVideoStream { get; }
    }
}
