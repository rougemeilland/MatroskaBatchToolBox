using System.Collections.Generic;
using Utility;

namespace ChapterConverter
{
    internal interface IChapterFormatter
    {
        IEnumerable<ChapterInfo> Parse(string rawText);
        string Render(IEnumerable<ChapterInfo> chapters);
    }
}
