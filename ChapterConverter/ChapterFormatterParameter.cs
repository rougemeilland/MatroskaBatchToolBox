using System;

namespace ChapterConverter
{
    internal class ChapterFormatterParameter
    {
        private readonly Action<string> _warningMessageReporter;

        public ChapterFormatterParameter(TimeSpan maximumDuration, Action<string> wainingMessageReporter)
        {
            MaximumDuration = maximumDuration;
            _warningMessageReporter = wainingMessageReporter;
        }

        public TimeSpan MaximumDuration { get; }

        public void ReportWarningMessage(string warningMessage) => _warningMessageReporter(warningMessage);
    }
}
