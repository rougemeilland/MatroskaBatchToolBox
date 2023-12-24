using MatroskaBatchToolBox.Utility.Models.Json;

namespace MatroskaBatchToolBox.Utility.Movie
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
