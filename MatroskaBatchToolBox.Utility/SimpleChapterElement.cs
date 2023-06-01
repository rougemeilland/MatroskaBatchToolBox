using System;

namespace MatroskaBatchToolBox.Utility
{
    public class SimpleChapterElement
    {
        public static readonly TimeSpan DefaultMaximumDuration;
        public static readonly TimeSpan DefaultMinimumDuration;

        protected const long _timeBaseDenominator = 1000000000;

        static SimpleChapterElement()
        {
            DefaultMaximumDuration = TimeSpan.FromDays(7);
            DefaultMinimumDuration = TimeSpan.FromMilliseconds(100);
        }

        public SimpleChapterElement(TimeSpan startTime, TimeSpan endTime, string title)
        {
            StartTime = startTime;
            EndTime = endTime;
            Duration = endTime - startTime;
            Title = title;
        }

        public TimeSpan StartTime { get; }
        public TimeSpan EndTime { get; }
        public TimeSpan Duration { get; }
        public string Title { get; }
    }
}
