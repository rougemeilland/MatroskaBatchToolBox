using MatroskaBatchToolBox.Model.Json;
using System;

namespace MatroskaBatchToolBox.Model
{
    internal abstract class StreamInfo
    {
        public StreamInfo(MovieStreamInfoContainer stream)
        {
            Index = stream.Index ?? throw new Exception($"The \"{nameof(stream.Index)}\" property of the stream information is undefined.");
            CodecName = stream.codecName ?? throw new Exception($"The \"{nameof(stream.codecName)}\" property of the stream information is undefined.");
            CodecLongName = stream.CodecLongName ?? throw new Exception($"The \"{nameof(stream.CodecLongName)}\" property of the stream information is undefined.");
            Tags = new StreamTags(stream.Tags);
        }

        public int Index { get; }
        public string CodecName { get; }
        public string CodecLongName { get; }
        public StreamTags Tags { get; }
    }
}
