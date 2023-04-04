using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MatroskaBatchToolBox.Utility.Interprocess;
using MatroskaBatchToolBox.Utility.Movie;

namespace ChapterConverter
{
    internal class MovieChapterFormatter
         : ChapterFormatter
    {
        public MovieChapterFormatter(ChapterFormatterParameter parameter)
            : base(parameter)
        {
        }

        protected override IEnumerable<InternalChapterElement> Parse(string inputFilePath)
        {
            var inputFile = new FileInfo(inputFilePath);
            var movieInfo =
                Command.GetMovieInformation(
                    null,
                    inputFile,
                    MovieInformationType.Chapters,
                    (level, message) =>
                    {
                        if (level != "INFO")
                            Parameter.ReportWarningMessage(message);
                    });
            return
                movieInfo.Chapters
                .Select((chapter, index) => new InternalChapterElement($"#{index}", chapter.StartTime, chapter.EndTime, chapter.Title));
        }

        protected override string Render(IEnumerable<InternalChapterElement> chapters)
            => throw new NotSupportedException($"It is not possible to output in \"movie\" format.");
    }
}
