using System;
using MatroskaBatchToolBox.Model.json;

namespace MatroskaBatchToolBox.Model
{
    internal class VideoStreamInfo
    {
        public VideoStreamInfo(MovieStreamInfoContainer stream)
        {
            Index = stream.index ?? throw new Exception($"The \"{nameof(stream.index)}\" property of the audio stream information is undefined.");
            CodecName = stream.codec_name ?? throw new Exception($"The \"{nameof(stream.codec_name)}\" property of the audio stream information is undefined.");
            CodecLongName = stream.codec_long_name ?? throw new Exception($"The \"{nameof(stream.codec_long_name)}\" property of the audio stream information is undefined.");
            Tags = new StreamTags(stream.tags);
        }

        public int Index { get; }
        public string CodecName { get; }
        public string CodecLongName { get; }
        public StreamTags Tags { get; }
    }
}
