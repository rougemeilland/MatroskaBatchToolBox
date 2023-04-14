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
        }

        public FileInfo File { get; set; }
        public int StreamsCount { get; set; }
        public int ProgramsCount { get; set; }
        public string FormatName { get; set; }
        public string? FormatLongName { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? Duration { get; set; }
        public long? Size { get; set; }
        public int? BitRate { get; set; }
        public int ProbeScore { get; set; }
    }
}
