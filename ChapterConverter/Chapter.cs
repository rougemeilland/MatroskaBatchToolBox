using System;

namespace ChapterConverter
{
    internal class Chapter
    {
        public Chapter(TimeSpan startTime, TimeSpan endTime, string title)
        {
            StartTime = startTime;
            EndTime= endTime;
            Duration = endTime - startTime;
            Title = title;
        }

        public TimeSpan StartTime { get; }
        public TimeSpan EndTime { get; }
        public TimeSpan Duration { get; }
        public string Title { get; }
    }
}
