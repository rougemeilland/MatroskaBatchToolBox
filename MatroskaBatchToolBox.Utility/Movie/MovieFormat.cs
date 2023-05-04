using System;
using System.IO;
using MatroskaBatchToolBox.Utility.Models.Json;
using Palmtree;

namespace MatroskaBatchToolBox.Utility.Movie
{
    public class MovieFormat
    {
        internal MovieFormat(MovieFormatContainer format)
        {
            File = new FileInfo(format.FilePath);
            StreamsCount = format.StreamsCount;
            ProgramsCount = format.ProgramsCount;
            FormatName = format.FormatName;
            FormatLongName = format.FormatLongName == "unknown" ? format.FormatLongName : null;
            StartTime = format.StartTime is not null && format.StartTime.TryParse(out double startTimeValue) ? TimeSpan.FromSeconds(startTimeValue) : null;
            Duration = format.Duration is not null && format.Duration.TryParse(out double durationValue) ? TimeSpan.FromSeconds(durationValue) : null;
            Size = format.Size is not null && format.Size.TryParse(out int sizeValue) ? sizeValue : null;
            BitRate = format.BitRate is not null && format.BitRate.TryParse(out int bitRateValue) ? bitRateValue : null;
            ProbeScore = format.ProbeScore;
            Tags = new MovieFormatTags(format.Tags);
        }

        public FileInfo File { get; }
        public int StreamsCount { get; }
        public int ProgramsCount { get; }
        public string FormatName { get; }
        public string? FormatLongName { get; }
        public TimeSpan? StartTime { get; }
        public TimeSpan? Duration { get; }
        public long? Size { get; }
        public int? BitRate { get; }
        public int ProbeScore { get; }
        public MovieFormatTags Tags { get; }
    }
}
