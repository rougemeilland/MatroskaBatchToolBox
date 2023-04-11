using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MatroskaBatchToolBox.Utility;
using MatroskaBatchToolBox.Utility.Interprocess;
using MatroskaBatchToolBox.Utility.Movie;
using Palmtree;

namespace MovieChapterEditor
{
    public static partial class Program
    {
        private class CommandParameter
        {
            private CommandParameter(IEnumerable<CommandOption<OptionType>> options)
            {
                InputFormat = options.SingleOrNone(option => option.OptionType == OptionType.InputFormat)?.OptionParameter[1] as string;
                Input = options.SingleOrNone(option => option.OptionType == OptionType.Input)?.OptionParameter[1] as string;
                OutputFormat = options.SingleOrNone(option => option.OptionType == OptionType.OutputFormat)?.OptionParameter[1] as string;
                Output = options.SingleOrNone(option => option.OptionType == OptionType.Output)?.OptionParameter[1] as string;
                IsForceMode = options.Any(option => option.OptionType == OptionType.Force);
                ChapterTimes = options.SingleOrNone(option => option.OptionType == OptionType.ChapterTimes)?.OptionParameter[1] as IEnumerable<TimeSpan>;
                ChapterTitles = options.Where(option => option.OptionType == OptionType.ChapterTitle).ToDictionary(option => (int)option.OptionParameter[1], option => (string)option.OptionParameter[2]);
                MaximumDuration = options.SingleOrNone(option => option.OptionType == OptionType.MaximumDuration)?.OptionParameter[1] as TimeSpan? ?? SimpleChapterElement.DefaultMaximumDuration;
                MinimumDuration = options.SingleOrNone(option => option.OptionType == OptionType.MinimumDuration)?.OptionParameter[1] as TimeSpan? ?? SimpleChapterElement.DefaultMinimumDuration;
                (From, To) = GetTrimmingRange(
                    options.SingleOrNone(option => option.OptionType == OptionType.FromForTrimming)?.OptionParameter[1] as TimeSpan?,
                    options.SingleOrNone(option => option.OptionType == OptionType.ToForTrimming)?.OptionParameter[1] as TimeSpan?,
                    options.SingleOrNone(option => option.OptionType == OptionType.DurationForTrimming)?.OptionParameter[1] as TimeSpan?);
                KeepEmptyChapter = options.Any(option => option.OptionType == OptionType.KeepEemptyChapter);
                Verbose = options.Any(option => option.OptionType == OptionType.Verbose);
                IsHelpMode = options.Any(option => option.OptionType == OptionType.Help);
            }

            public string? InputFormat { get; }
            public string? Input { get; }
            public string? OutputFormat { get; }
            public string? Output { get; }
            public bool IsForceMode { get; }
            public IEnumerable<TimeSpan>? ChapterTimes { get; }
            public IDictionary<int, string> ChapterTitles { get; }
            public TimeSpan MaximumDuration { get; }
            public TimeSpan MinimumDuration { get; }
            public TimeSpan From { get; }
            public TimeSpan To { get; }
            public bool KeepEmptyChapter { get; }
            public bool Verbose { get; }
            public bool IsHelpMode { get; }

            public static CommandParameter Parse(IEnumerable<CommandOptionDefinition<OptionType>> optionDefinitions, string[] args)
                => new(optionDefinitions.ParseCommandArguments(args.AsReadOnlyArray()));

            private static (TimeSpan from, TimeSpan to) GetTrimmingRange(TimeSpan? ssValue, TimeSpan? toValue, TimeSpan? tValue)
            {
                Validation.Assert(toValue is null || tValue is null, "toValue is null || tValue is null");
                Validation.Assert(ssValue is null || ssValue.Value >= TimeSpan.Zero, "ssValue is null || ssValue.Value >= TimeSpan.Zero");
                Validation.Assert(toValue is null || toValue.Value >= TimeSpan.Zero, "toValue is null || toValue.Value >= TimeSpan.Zero");
                Validation.Assert(tValue is null || tValue.Value >= TimeSpan.Zero, "tValue is null || tValue.Value >= TimeSpan.Zero");

                var from = ssValue ?? TimeSpan.Zero;
                if (toValue is not null)
                {
                    return
                        from <= toValue.Value
                        ? (from, toValue.Value)
                        : throw new InvalidCommandOptionException($"The time specified with the \"-to\" option is less than the time specified with the \"-ss\" option.\r\nCheck the \"-ss\" option value and \"-to\" option value.: -ss {from.FormatTime(3)} -to {toValue.Value.FormatTime(3)}");
                }
                else
                {
                    return
                        (from,
                            tValue is null
                            ? TimeSpan.MaxValue
                            : from <= TimeSpan.MaxValue - tValue.Value
                            ? from + tValue.Value
                            : TimeSpan.MaxValue);
                }
            }
        }

        private static readonly string _thisProgramName;
        private static readonly Regex _setTitleOptionNamePattern;
        private static readonly IEnumerable<CommandOptionDefinition<OptionType>> _optionDefinitions;

        static Program()
        {
            _thisProgramName = Path.GetFileNameWithoutExtension(typeof(Program).Assembly.Location);
            _setTitleOptionNamePattern = new Regex(@"^(-tt|--set_title):(?<chapterIndex>\d+)$", RegexOptions.Compiled);
            _optionDefinitions = GetOptionDefinitions();
        }

        public static int Main(string[] args)
        {
            try
            {
                var commandOptions = ParseCommandOptions(args);
                if (commandOptions is null)
                    return 1;

                if (commandOptions.Input is not null)
                {
                    try
                    {
                        if (!File.Exists(commandOptions.Input))
                        {
                            PrintErrorMessage($"The input file specified by the \"--input\" option does not exist.: \"{commandOptions.Input}\"");
                            return 1;
                        }
                    }
                    catch (Exception ex)
                    {
                        PrintErrorMessage($"Failed to access the input file specified by the \"--input\" option.: \"{commandOptions.Input}\"");
                        PrintExceptionMessage(ex);
                        return 1;
                    }
                }

                if (commandOptions.Output is not null)
                {
                    try
                    {
                        var outputFile = new FileInfo(commandOptions.Output);
                        var parentDirectory = outputFile.Directory;
                        if (parentDirectory is null || !parentDirectory.Exists)
                        {
                            PrintErrorMessage($"The output file directory specified with the \"--output\" option is invalid or does not exist.: \"{commandOptions.Output}\"");
                            return 1;
                        }

                        if (!commandOptions.IsForceMode && outputFile.Exists)
                        {
                            PrintErrorMessage($"The output file specified with the \"--output\" option already exists.: \"{commandOptions.Output}\"");
                            return 1;
                        }
                    }
                    catch (Exception ex)
                    {
                        PrintErrorMessage($"Failed to access the output file specified by the \"--output\" option.: \"{commandOptions.Output}\"");
                        PrintExceptionMessage(ex);
                        return 1;
                    }
                }

                if (commandOptions.IsHelpMode)
                {
                    PrintHelpMessage();
                    return 0;
                }

                if (commandOptions.Verbose)
                    PrintInformationMessage("Start processing.");
                try
                {
                    var (inputFile, temporaryInputFile) = GetInputMovieFile(commandOptions);
                    var (outputFile, temporaryOutputFile) = GetOutputMovieFile(commandOptions);
                    try
                    {
                        var movieInformation = GetMovieInformation(commandOptions, commandOptions.InputFormat, inputFile);
                        var ffmpegCommandParameters =
                            new List<string>
                            {
                                "-hide_banner"
                            };
                        if (commandOptions.IsForceMode || temporaryOutputFile is not null)
                            ffmpegCommandParameters.Add("-y");
                        if (commandOptions.InputFormat is not null)
                            ffmpegCommandParameters.Add($"-f {commandOptions.InputFormat}");
                        ffmpegCommandParameters.Add($"-i \"{inputFile.FullName}\"");
                        ffmpegCommandParameters.Add("-f ffmetadata -i -");
                        ffmpegCommandParameters.Add("-c copy -map 0");
                        foreach (var stream in movieInformation.VideoStreams)
                            ffmpegCommandParameters.Add($"-disposition:v:{stream.IndexWithinVideoStream} {(stream.Disposition.Default ? "+" : "-")}default{(stream.Disposition.Forced ? "+" : "-")}forced");
                        foreach (var stream in movieInformation.AudioStreams)
                            ffmpegCommandParameters.Add($"-disposition:a:{stream.IndexWithinAudioStream} {(stream.Disposition.Default ? "+" : "-")}default{(stream.Disposition.Forced ? "+" : "-")}forced");
                        foreach (var stream in movieInformation.SubtitleStreams)
                            ffmpegCommandParameters.Add($"-disposition:s:{stream.IndexWithinSubtitleStream} {(stream.Disposition.Default ? "+" : "-")}default{(stream.Disposition.Forced ? "+" : "-")}forced");
                        ffmpegCommandParameters.Add("-map_chapters 1");
                        if (commandOptions.OutputFormat is not null)
                            ffmpegCommandParameters.Add($"-f {commandOptions.OutputFormat}");
                        ffmpegCommandParameters.Add($"\"{outputFile.FullName}\"");
                        using var inMetadataReader = GetInputMetadataStream(commandOptions, movieInformation.Chapters);
                        var exitCode =
                            Command.ExecuteFfmpeg(
                                string.Join(" ", ffmpegCommandParameters),
                                Command.GetTextInputRedirector(inMetadataReader.ReadLine),
                                null,
                                TinyConsole.Error.WriteLine,
                                (level, message) =>
                                {
                                    switch (level)
                                    {
                                        case "WARNING":
                                            PrintWarningMessage(message);
                                            break;
                                        case "ERROR":
                                            PrintErrorMessage(message);
                                            break;
                                        default:
                                            // NOP
                                            break;
                                    }
                                },
                                new Progress<double>(_ => { }));
                        if (temporaryOutputFile is not null)
                        {
                            using var inStream = new FileStream(temporaryOutputFile.FullName, FileMode.Open, FileAccess.Read, FileShare.None);
                            using var outStream = Console.OpenStandardOutput();
                            if (commandOptions.Verbose)
                                PrintInformationMessage($"Copy from temporary file to standard output.: {temporaryOutputFile.FullName}");

                            CopyStream(inStream, outStream);

                            if (commandOptions.Verbose)
                                PrintInformationMessage("Detecting the end of temporary file, end the copy.");
                        }

                        return
                            exitCode == 0
                            ? 0
                            : throw new Exception($"An error occurred in the \"ffmpeg\" command. : exit-code={exitCode}");
                    }
                    finally
                    {
                        if (temporaryInputFile is not null)
                        {
                            try
                            {
                                File.Delete(temporaryInputFile.FullName);
                            }
                            catch (Exception)
                            {
                            }
                        }

                        if (temporaryOutputFile is not null)
                        {
                            try
                            {
                                File.Delete(temporaryOutputFile.FullName);
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    PrintExceptionMessage(ex);
                    return 1;
                }
            }
            finally
            {
                TinyConsole.ResetColor();
            }
        }

        private static CommandParameter? ParseCommandOptions(string[] args)
        {
            try
            {
                return CommandParameter.Parse(_optionDefinitions, args);
            }
            catch (InvalidCommandOptionException ex)
            {
                PrintErrorMessage(ex.Message);
                return null;
            }
        }

        private static (FileInfo inputFilePath, FileInfo? inputTemporaryFilePath) GetInputMovieFile(CommandParameter commandParameters)
        {

            if (commandParameters.Input is not null)
            {
                if (commandParameters.Verbose)
                    PrintInformationMessage($"Input file path: \"{commandParameters.Input}\"");
                return (new FileInfo(commandParameters.Input), null);
            }
            else
            {
                if (commandParameters.Verbose)
                    PrintInformationMessage("Read from standard input.");
                var temporaryInputFile = new FileInfo(Path.GetTempFileName());
                try
                {
                    using var inputStream = TinyConsole.OpenStandardInput();
                    using var outputStream = new FileStream(temporaryInputFile.FullName, FileMode.Create, FileAccess.Write, FileShare.None);
                    {
                        if (commandParameters.Verbose)
                            PrintInformationMessage($"Copy from standard input to a temporary file.: {temporaryInputFile.FullName}");

                        CopyStream(inputStream, outputStream);

                        if (commandParameters.Verbose)
                            PrintInformationMessage("Detecting the end of standard input, end the copy.");
                    }

                    return (temporaryInputFile, temporaryInputFile);
                }
                catch (Exception ex)
                {
                    try
                    {
                        File.Delete(temporaryInputFile.FullName);
                    }
                    catch (Exception)
                    {
                    }

                    throw new Exception("An error occurred while preparing the input file.", ex);
                }
            }
        }

        private static (FileInfo outputFilePath, FileInfo? outputTemporaryFilePath) GetOutputMovieFile(CommandParameter commandParameters)
        {

            if (commandParameters.Output is not null)
            {
                if (commandParameters.Verbose)
                    PrintInformationMessage($"Output file path: \"{commandParameters.Output}\"");
                return (new FileInfo(commandParameters.Output), null);
            }
            else
            {
                if (commandParameters.Verbose)
                    PrintInformationMessage("Write to standard output.");
                var temporaryOutputFile = new FileInfo(Path.GetTempFileName());
                return (temporaryOutputFile, temporaryOutputFile);
            }
        }

        private static TextReader GetInputMetadataStream(CommandParameter commandParameters, IEnumerable<ChapterInfo> chapters)
        {
            var filterParameter = new ChapterFilterParameter
            {
                From = commandParameters.From,
                KeepEmptyChapter = commandParameters.KeepEmptyChapter,
                MinimumDuration = commandParameters.MinimumDuration,
                Titles = commandParameters.ChapterTitles,
                To = commandParameters.To,
                WarningMessageReporter = PrintWarningMessage,
            };
            return
                new StringReader(
                    (commandParameters.ChapterTimes is not null
                        ? commandParameters.ChapterTimes
                            .ToSimpleChapterElements(commandParameters.MaximumDuration, PrintWarningMessage)
                            .ChapterFilter(filterParameter)
                        : chapters
                            .ChapterFilter(filterParameter))
                    .ToMetadataString());
        }

        private static MovieInformation GetMovieInformation(CommandParameter commandParameters, string? inputFormat, FileInfo inputFile)
        {
            if (commandParameters.Verbose)
                PrintInformationMessage("Probe movie information.");
            try
            {
                return
                    Command.GetMovieInformation(
                        inputFormat,
                        inputFile,
                        MovieInformationType.Chapters,
                        (level, message) =>
                        {
                            switch (level)
                            {
                                case "WARNING":
                                    PrintWarningMessage("ffprobe", message);
                                    break;
                                case "ERROR":
                                    PrintWarningMessage("ffprobe", message);
                                    break;
                                default:
                                    break;
                            }
                        });
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to get movie information.", ex);
            }
        }

        private static void CopyStream(Stream inputStream, Stream outputStream)
        {
            var buffer = new byte[10240];
            while (true)
            {
                var length = inputStream.Read(buffer);
                if (length <= 0)
                    break;
                outputStream.Write(buffer, 0, length);
            }
        }

        private static void PrintHelpMessage()
        {
            var textLines =
                new[]
                {
#if false // この部分はREADME などで書く
                    $"[About {_thisCommandName}]",
                    $"  The {_thisCommandName} command edits the chapters of the input movie file and outputs them as a new movie file.",
                    $"  You can specify a file path name for the input/output of a movie file, or you can use the standard input/output.",
                    "",
                    "[What can be edited with chapedit]",
                    "  * You can replace the chapter times with the specified times.",
                    "  * You can change/delete the title of the specified chapter.",
                    "",
#endif
                    "[Usage]",
                    $"{_thisProgramName} <option list>",
                    $"  For \"<option list>\", specify the necessary options from the list below. You can arrange them in any order.",
                    "",
                    "[Options]",
                }
                .Concat(
                    _optionDefinitions
                    .OrderBy(optionDefinition => optionDefinition.OptionType)
                    .SelectMany(optionDefinition =>
                        optionDefinition.GetHelpDescriptionTextLines()
                        .Select(lineText =>
                            string.IsNullOrEmpty(lineText)
                            ? lineText
                            : $"    {lineText}")
                        .Prepend($"  * {optionDefinition.GetHelpSyntaxText()}")
                        .Append("")));
            foreach (var lineText in textLines)
                TinyConsole.Out.WriteLine(lineText);
        }

        private static void PrintExceptionMessage(Exception ex)
        {
            for (var exception = ex; exception != null; exception = exception.InnerException)
                PrintErrorMessage(exception.Message);
        }

        private static void PrintInformationMessage(string message)
            => PrintInformationMessage(_thisProgramName, message);

        private static void PrintInformationMessage(string programName, string message)
            => TinyConsole.WriteLine($"{programName}:INFORMATION: {message}");

        private static void PrintWarningMessage(string message)
            => PrintWarningMessage(_thisProgramName, message);

        private static void PrintWarningMessage(string programName, string message)
        {
            TinyConsole.ForegroundColor = ConsoleColor.Yellow;
            TinyConsole.WriteLine($"{programName}:WARNING: {message}");
            TinyConsole.ResetColor();
        }

        private static void PrintErrorMessage(string message)
            => PrintErrorMessage(_thisProgramName, message);

        private static void PrintErrorMessage(string programName, string message)
        {
            TinyConsole.ForegroundColor = ConsoleColor.Red;
            TinyConsole.WriteLine($"{programName}:ERROR: {message}");
            TinyConsole.ResetColor();
        }
    }
}
