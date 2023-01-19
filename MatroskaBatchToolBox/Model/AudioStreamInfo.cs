using System;

namespace MatroskaBatchToolBox.Model
{
    internal class AudioStreamInfo
    {
        public AudioStreamInfo(MovieStreamInfo stream)
        {
            Index = stream.index ?? throw new Exception($"The \"{nameof(stream.index)}\" property of the audio stream information is undefined.");
            CodecName = stream.codec_name ?? throw new Exception($"The \"{nameof(stream.codec_name)}\" property of the audio stream information is undefined.");
            CodecLongName = stream.codec_long_name ?? throw new Exception($"The \"{nameof(stream.codec_long_name)}\" property of the audio stream information is undefined.");
        }

        public int Index { get; set; }
        public string CodecName { get; set; }
        public string CodecLongName { get; set; }
    }
}
