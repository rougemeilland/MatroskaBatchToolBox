using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Utility;
using Utility.Movie;

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
            var (commandResult, movieInfo) =
                Command.GetMovieInformation(
                    GetFfprobeProgramFile(),
                    inputFile,
                    MovieInformationType.Chapters,
                    (level, message) =>
                    {
                        if (level != "INFO")
                            Parameter.ReportWarningMessage(message);
                    });
            return
                commandResult != CommandResult.Completed
                ? throw new Exception("Failed to execute \"ffprobe\".")
                : movieInfo is null
                ? throw new Exception($"The video file information could not be acquired successfully.: \"{inputFile.FullName}\"")
                : movieInfo.Chapters
                .Select((chapter, index) => new InternalChapterElement($"#{index}", chapter.StartTime, chapter.EndTime, chapter.Title));
        }

        protected override string Render(IEnumerable<InternalChapterElement> chapters)
            => throw new NotSupportedException($"It is not possible to output in \"movie\" format.");

        private static FileInfo GetFfprobeProgramFile()
        {
            var baseDirectoryPath = Path.GetDirectoryName(typeof(MovieChapterFormatter).Assembly.Location) ?? ".";
            var ffprobeProgramFile = new FileInfo(Path.Combine(baseDirectoryPath, "ffprobe"));
            if (ffprobeProgramFile.Exists)
                return ffprobeProgramFile;
            ffprobeProgramFile = new FileInfo(Path.Combine(baseDirectoryPath, "ffprobe.exe"));
            return
                ffprobeProgramFile.Exists
                ? ffprobeProgramFile
                : throw new Exception($"Cannot find the executable file of \"ffprobe\" under the directory \"{baseDirectoryPath}\".");
        }
    }
}
