using MatroskaBatchToolBox.Model.Json;
using System;

namespace MatroskaBatchToolBox.Model
{
    internal class VideoStreamInfo
        : StreamInfo
    {
        public VideoStreamInfo(MovieStreamInfoContainer stream, int indexWithinVideoStream)
            : base(stream)
        {
            IndexWithinVideoStream = indexWithinVideoStream;
            Width = stream.Width ?? throw new Exception($"Video stream #{indexWithinVideoStream} has no \"width\" property.");
            Height = stream.Height ?? throw new Exception($"Video stream #{indexWithinVideoStream} has no \"height\" property.");
            DisplayAspectRatio = stream.DisplayAspectRatio ?? throw new Exception($"Video stream #{indexWithinVideoStream} has no \"display_aspect_ratio\" property.");
            Resolution = $"{Width}x{Height}";
        }

        public int IndexWithinVideoStream { get; }
        public int Width { get; }
        public int Height { get; }
        public string Resolution { get; }
        public string DisplayAspectRatio { get; }
    }
}
