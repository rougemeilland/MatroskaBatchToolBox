using System;
using Palmtree;

namespace ChapterConverter
{
    internal sealed class ChapterFormatterParameter
    {
        private readonly Action<LogCategory, string> _messageReporter;

        public ChapterFormatterParameter(TimeSpan maximumDuration, Action<LogCategory, string> messageReporter)
        {
            MaximumDuration = maximumDuration;
            _messageReporter = messageReporter;
        }

        public TimeSpan MaximumDuration { get; }

        public void ReportWarningMessage(string warningMessage) => _messageReporter(LogCategory.Warning, warningMessage);
    }
}
