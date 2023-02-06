using MatroskaBatchToolBox.Model.Json;
using System;

namespace MatroskaBatchToolBox.Model
{
    internal class VideoStreamInfo
        : StreamInfo
    {
        private const string _mpngVideoStreamName = "png";
        private const string _mjpegVideoStreamName = "mjpeg";

        public VideoStreamInfo(MovieStreamInfoContainer stream, int indexWithinVideoStream)
            : base(stream)
        {
            IndexWithinVideoStream = indexWithinVideoStream;
            Width = stream.Width ?? throw new Exception($"Video stream #{indexWithinVideoStream} has no \"width\" property.");
            Height = stream.Height ?? throw new Exception($"Video stream #{indexWithinVideoStream} has no \"height\" property.");
            DisplayAspectRatio = stream.DisplayAspectRatio;
            Resolution = $"{Width}x{Height}";
            IsImageVideoStream =
                string.Equals(stream.CodecName, _mpngVideoStreamName, StringComparison.InvariantCulture) ||
                string.Equals(stream.CodecName, _mjpegVideoStreamName, StringComparison.InvariantCulture);
        }

        public int IndexWithinVideoStream { get; }
        public int Width { get; }
        public int Height { get; }
        public string Resolution { get; }
        public string? DisplayAspectRatio { get; }
        public bool IsImageVideoStream { get; }
    }
}
