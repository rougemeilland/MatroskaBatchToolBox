using System;

namespace ChapterConverter
{
    internal class Chapter
    {
        public Chapter(TimeSpan startTime, TimeSpan endTime, string title)
        {
            StartTime = startTime;
            EndTime= endTime;
            Title = title;
        }

        public TimeSpan StartTime { get; }
        public TimeSpan EndTime { get; }
        public string Title { get; }
    }
}
