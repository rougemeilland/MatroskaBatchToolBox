using System.Collections.Generic;
using Utility;

namespace ChapterConverter
{
    internal interface IChapterFormatter
    {
        IEnumerable<SimpleChapterElement> Parse(string rawText);
        string Render(IEnumerable<SimpleChapterElement> chapters);
    }
}
