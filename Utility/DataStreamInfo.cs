using Utility.Models.Json;

namespace Utility
{
    public class DataStreamInfo
        : StreamInfo
    {
        internal DataStreamInfo(MovieStreamInfoContainer stream, int indexWithinDataStream)
            : base(stream)
        {
            IndexWithinDataStream = indexWithinDataStream;
        }

        public int IndexWithinDataStream { get; }
    }
}
