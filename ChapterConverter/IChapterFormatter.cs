using System.Collections.Generic;
using MatroskaBatchToolBox.Utility;

namespace ChapterConverter
{
    internal interface IChapterFormatter
    {
        IEnumerable<SimpleChapterElement> Parse(string rawText);
        string Render(IEnumerable<SimpleChapterElement> chapters);
    }
}
