using System;
using System.Collections.Generic;

namespace Utility.Movie
{
    public class ChapterFilterParameter
    {
        public ChapterFilterParameter()
        {
            From = TimeSpan.Zero;
            To = TimeSpan.MaxValue;
            Titles = new Dictionary<int, string>();
            KeepEmptyChapter = false;
            MinimumDuration = SimpleChapterElement.DefaultMinimumDuration;
            WarningMessageReporter = message => { };
        }

        public TimeSpan From { get; set; }
        public TimeSpan To { get; set; }
        public IDictionary<int, string> Titles { get; set; }
        public bool KeepEmptyChapter { get; set; }
        public TimeSpan MinimumDuration { get; set; }
        public Action<string> WarningMessageReporter { get; set; }
    }
}
