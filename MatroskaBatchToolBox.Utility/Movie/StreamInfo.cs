using System;
using MatroskaBatchToolBox.Utility.Models.Json;
using Palmtree;

namespace MatroskaBatchToolBox.Utility.Movie
{
    public abstract class StreamInfo
    {
        internal StreamInfo(MovieStreamInfoContainer stream)
        {
            Index =
                stream.Index
                ?? throw new Exception("The \"index\" property of the stream information is undefined.");
            CodecName =
                stream.CodecName
                ?? throw new Exception("The \"codec_name\" property of the stream information is undefined.");
            CodecLongName =
                stream.CodecLongName
                ?? throw new Exception("The \"codec_long_name\" property of the stream information is undefined.");
            BitsPerRawSample =
                stream.BitsPerRawSample is not null && stream.BitsPerRawSample.TryParse(out int bitsPerRawSample) ? bitsPerRawSample : null;
            Disposition = new StreamDisposition(stream.Disposition);
            Tags = new StreamTags(stream.Tags);
        }

        public int Index { get; }
        public string CodecName { get; }
        public string CodecLongName { get; }
        public int? BitsPerRawSample { get; }
        public StreamDisposition Disposition { get; }
        public StreamTags Tags { get; }
    }
}
