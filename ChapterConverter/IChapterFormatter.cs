using System.Collections.Generic;

namespace ChapterConverter
{
    internal interface IChapterFormatter
    {
        IEnumerable<Chapter> Parse(string rawText);
        string Render(IEnumerable<Chapter> chapters);
    }
}
