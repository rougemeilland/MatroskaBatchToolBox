using System;
using MatroskaBatchToolBox.Utility.Models.Json;
using Palmtree;

namespace MatroskaBatchToolBox.Utility.Movie
{
    public class VideoStreamInfo
        : StreamInfo
    {
        private const string _mpngVideoStreamName = "png";
        private const string _mjpegVideoStreamName = "mjpeg";

        internal VideoStreamInfo(MovieStreamInfoContainer stream, int indexWithinVideoStream)
            : base(stream)
        {
            IndexWithinVideoStream = indexWithinVideoStream;
            Width =
                stream.Width
                ?? throw new Exception($"Video stream #{indexWithinVideoStream} has no \"width\" property.");
            Height =
                stream.Height
                ?? throw new Exception($"Video stream #{indexWithinVideoStream} has no \"height\" property.");
            DisplayAspectRatio = stream.DisplayAspectRatio;
            Resolution = $"{Width}x{Height}";
            IsImageVideoStream =
                stream.CodecName is not null && stream.CodecName.IsAnyOf(_mpngVideoStreamName, _mjpegVideoStreamName) ||
                Disposition.AttachedPicture;
        }

        public int IndexWithinVideoStream { get; }
        public int Width { get; }
        public int Height { get; }
        public string Resolution { get; }
        public string? DisplayAspectRatio { get; }
        public bool IsImageVideoStream { get; }
    }
}
