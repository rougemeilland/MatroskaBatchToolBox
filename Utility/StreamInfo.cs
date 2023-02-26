using Utility.Models.Json;
using System;

namespace Utility
{
    public abstract class StreamInfo
    {
        public StreamInfo(MovieStreamInfoContainer stream)
        {
            Index =
                stream.Index
                ?? throw new Exception($"The \"{nameof(stream.Index)}\" property of the stream information is undefined.");
            CodecName =
                stream.CodecName
                ?? throw new Exception($"The \"{nameof(stream.CodecName)}\" property of the stream information is undefined.");
            CodecLongName =
                stream.CodecLongName
                ?? throw new Exception($"The \"{nameof(stream.CodecLongName)}\" property of the stream information is undefined.");
            Disposition = new StreamDisposition(stream.Disposition);
            Tags = new StreamTags(stream.Tags);
        }

        public int Index { get; }
        public string CodecName { get; }
        public string CodecLongName { get; }
        public StreamDisposition Disposition { get; }
        public StreamTags Tags { get; }
    }
}
