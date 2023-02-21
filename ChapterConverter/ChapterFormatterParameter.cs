using System;

namespace ChapterConverter
{
    internal class ChapterFormatterParameter
    {
        private Action<string> _warningMessageReporter;

        public ChapterFormatterParameter(TimeSpan maximumDuration, string? ffMetadataEncoder, Action<string> wainingMessageReporter)
        {
            MaximumDuration = maximumDuration;
            FFMetadataEncoder = ffMetadataEncoder;
            _warningMessageReporter = wainingMessageReporter;
        }

        public TimeSpan MaximumDuration { get; }
        public string? FFMetadataEncoder { get; }

        public void ReportWarningMessage(string warningMessage) => _warningMessageReporter(warningMessage);
    }
}
