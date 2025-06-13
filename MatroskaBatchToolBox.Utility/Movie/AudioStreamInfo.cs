using System;
using MatroskaBatchToolBox.Utility.Models.Json;
using Palmtree.Numerics;

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
            Channels = stream.Channels ?? throw new ApplicationException("\"channels\" property not found.");
            SampleFormat = ParseSampleFormat(stream.SampleFormat ?? "unknown");
            BitRate = stream.BitRate is not null && stream.BitRate.TryParse(out int bitRateValue) ? bitRateValue : null;
        }

        public int IndexWithinAudioStream { get; }
        public string? ChannelLayout { get; }
        public int Channels { get; }
        public AudioSampleFormat SampleFormat { get; }
        public int? BitRate { get; }
        private static AudioSampleFormat ParseSampleFormat(string s)
            => s.ToUpperInvariant() switch
            {
                "UNKNOWN" => AudioSampleFormat.Unknown,
                "U8" => AudioSampleFormat.U8,
                "S16" => AudioSampleFormat.S16,
                "S32" => AudioSampleFormat.S32,
                "S64" => AudioSampleFormat.S64,
                "FLT" => AudioSampleFormat.FLT,
                "DBL" => AudioSampleFormat.DBL,
                "U8P" => AudioSampleFormat.U8P,
                "S16P" => AudioSampleFormat.S16P,
                "S32P" => AudioSampleFormat.S32P,
                "S64P" => AudioSampleFormat.S64P,
                "FLTP" => AudioSampleFormat.FLTP,
                "DBLP" => AudioSampleFormat.DBLP,
                _ => throw new ApplicationException($"Not supported audio sample format: \"{s}\""),
            };
    }
}
