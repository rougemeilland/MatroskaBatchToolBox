using MatroskaBatchToolBox.Utility.Models.Json;
using System;
using Palmtree;

namespace MatroskaBatchToolBox.Utility.Movie
{
    public class AudioStreamInfo
        : StreamInfo
    {
        internal AudioStreamInfo(MovieStreamInfoContainer stream, int indexWithinAudioStream)
            : base(stream)
        {
            IndexWithinAudioStream = indexWithinAudioStream;
            ChannelLayout = stream.ChannelLayout;
            Channels = stream.Channels ?? throw new Exception("\"channels\" property not found.");
            BitRate = stream.BitRate is not null && stream.BitRate.TryParse(out int bitRateValue) ? bitRateValue : null;
        }

        public int IndexWithinAudioStream { get; }
        public string? ChannelLayout { get; }
        public int Channels { get; }
        public int? BitRate { get; }
    }
}
