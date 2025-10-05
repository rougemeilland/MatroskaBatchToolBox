using System;
using System.Linq;
using MatroskaBatchToolBox.Utility;
using MatroskaBatchToolBox.Utility.Interprocess;
using MatroskaBatchToolBox.Utility.Movie;
using Palmtree;
using Palmtree.IO;
using Palmtree.IO.Console;

namespace CheckDuration
{
    internal sealed class Program
    {
        private static readonly TimeSpan _deltaDuration = TimeSpan.FromSeconds(1);

        private static int Main(string[] args)
        {
            var explicitlySpecifiedDuration = (TimeSpan?)null;
            var inputFile = (FilePath?)null;
            var verbose = false;
            for (var index = 0; index < args.Length; ++index)
            {
                var arg = args[index];
                if (arg is "-i" or "--input")
                {
                    if (index + 1 >= args.Length)
                    {
                        TinyConsole.WriteLog(LogCategory.Error, "\"--input_file\" requires a additional parameter.");
                        return 1;
                    }

                    if (inputFile is not null)
                    {
                        TinyConsole.WriteLog(LogCategory.Error, "The \"--input\" option is specified more than once.");
                        return 1;
                    }

                    var filePath = GetFilePath(args[index + 1]);
                    if (filePath is null)
                    {
                        TinyConsole.WriteLog(LogCategory.Error, $"The path name specified in the \"--input\" option is invalid.: \"{args[index + 1]}\"");
                        return 1;
                    }

                    if (!filePath.Exists)
                    {
                        TinyConsole.WriteLog(LogCategory.Error, $"The file specified in the \"--input\" option does not exist.: \"{args[index + 1]}\"");
                        return 1;
                    }

                    inputFile = filePath;
                    ++index;
                }
                else if (arg is "-t" or "--duration")
                {
                    if (index + 1 >= args.Length)
                    {
                        TinyConsole.WriteLog(LogCategory.Error, "\"--duration\" requires a additional parameter.");
                        return 1;
                    }

                    if (explicitlySpecifiedDuration is not null)
                    {
                        TinyConsole.WriteLog(LogCategory.Error, "The \"--duration\" option is specified more than once.");
                        return 1;
                    }

                    if (!args[index + 1].TryParse(TimeParsingMode.LazyMode, out var duration))
                    {
                        TinyConsole.WriteLog(LogCategory.Error, $"The time format specified in the \"--duration\" option is invalid.: \"{args[index + 1]}\"");
                        return 1;
                    }

                    explicitlySpecifiedDuration = duration;
                    ++index;
                }
                else if (arg is "-v" or "--verbose")
                {
                    if (verbose == true)
                    {
                        TinyConsole.WriteLog(LogCategory.Error, "The \"--verbose\" option is specified more than once.");
                        return 1;
                    }

                    verbose = true;
                }
                else if (arg == "-help")
                {
                    if (args.Length != 1)
                    {
                        TinyConsole.WriteLog(LogCategory.Error, "The \"-help\" option cannot be specified with any other options.");
                        return 1;
                    }

                    var helpMessage = new[]
                    {
                        "This command checks whether the specified movie file has been recorded to the end, and if it has not been recorded to the end, it renames the file with the prefix \".incomplete.…\".",
                        "By default, if chapters are set, the video length is inferred from the end time of the last chapter.",
                        "Optionally, you can explicitly specify the video length using command options.",
                        "",
                        "Usage:",
                        "  chkdur <options>",
                        "",
                        "Options:",
                        "  -i <input file path name>  or  --input_file <input file path name>  (Required)",
                        "    Specifies the path name of the input movie file.",
                        "",
                        "  -t <duration time>  or  --duration <duration time>  (Optional)",
                        "    Specifies the path name of the input movie file.",
                        "",
                        "  -v  or  --verbose  (Optional)",
                        "    Outputs additional informational messages during execution.",
                        "",
                        "  -help",
                        "    Outputs a help message.",
                    };

                    foreach (var line in helpMessage)
                        TinyConsole.Out.WriteLine(line);

                    return 0;
                }
                else
                {
                    TinyConsole.WriteLog(LogCategory.Error, $"Unsupported option.: \"{arg}\"");
                    return 1;
                }
            }

            if (inputFile is null)
            {
                TinyConsole.WriteLog(LogCategory.Error, "The input file path name is not specified.");
                return 1;
            }

            try
            {
                var info = GetMovieInformation(null, inputFile, verbose);
                if (explicitlySpecifiedDuration is not null)
                {
                    if (ValidateMovieDuration(info, explicitlySpecifiedDuration.Value - TimeSpan.FromMilliseconds(1)) == true)
                        return 0;

                    RenameBadMovieFile(inputFile);
                    return 2;
                }

                var chapters = info.Chapters.ToArray();
                if (chapters.Length > 0)
                {
                    if (ValidateMovieDuration(info, chapters[^1].EndTime - TimeSpan.FromMilliseconds(1)) == true)
                        return 0;

                    RenameBadMovieFile(inputFile);
                    return 2;
                }

                return 0;
            }
            catch (Exception ex)
            {
                TinyConsole.WriteLog(ex);
                return 1;
            }
        }

        private static FilePath? GetFilePath(string path)
        {
            try
            {
                return new FilePath(path);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static MovieInformation GetMovieInformation(string? inputFormat, FilePath inputFile, bool _verbose)
        {
            if (_verbose)
                TinyConsole.WriteLog(LogCategory.Information, $"Probe movie information.: \"{inputFile.FullName}\"");
            try
            {
                return
                    Command.GetMovieInformation(
                        inputFormat,
                        inputFile,
                        MovieInformationType.Chapters | MovieInformationType.Streams | MovieInformationType.Format,
                        (level, message) =>
                        {
                            if (_verbose)
                                TinyConsole.WriteLog("ffprobe", level, message);
                        });
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Failed to get movie information.", ex);
            }
        }

        private static bool? ValidateMovieDuration(MovieInformation movieInfo, TimeSpan expectedDuration)
            => movieInfo.Format.Duration is null
                ? null
                : long.Abs(movieInfo.Format.Duration.Value.Ticks - expectedDuration.Ticks) <= _deltaDuration.Ticks;

        private static void RenameBadMovieFile(FilePath movieFilePath)
        {
            var baseDirectory = movieFilePath.Directory;
            var originalFileName = movieFilePath.NameWithoutExtension;
            var extension = movieFilePath.Extension;
            for (var count = 0 ; ; ++count)
            {
                var newFileePath = baseDirectory.GetFile($".incomplete.{originalFileName}{(count <= 0 ? "" : $".{count + 1}")}{extension}");
                if (!newFileePath.Exists)
                {
                    try
                    {
                        movieFilePath.MoveTo(newFileePath);
                        return;
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }
    }
}
