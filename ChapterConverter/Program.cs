using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MatroskaBatchToolBox.Utility;
using MatroskaBatchToolBox.Utility.Movie;
using Palmtree;

namespace ChapterConverter
{
    public class Program
    {
        private enum ActionMode
        {
            NotSpecified,
            Abort,
            Help,
            Convert,
        }

        private enum ChapterFormat
        {
            NotSpecified,
            Csv,
            Ffmetadata,
            Fflog,
            ChapterList,
            Immediate,
            FfprobeCsv,
            FfprobeFlat,
            FfprobeJson,
            Movie,
        }

        private class CommandLineOptions
        {
            public CommandLineOptions()
            {
                ActionMode = ActionMode.NotSpecified;
                InputFormat = ChapterFormat.NotSpecified;
                InputFilePath = null;
                OutputFormat = ChapterFormat.NotSpecified;
                OutputFilePath = null;
                Force = false;
                From = TimeSpan.Zero;
                To = SimpleChapterElement.DefaultMaximumDuration;
                Titles = new Dictionary<int, string>();
                MaximumDuration = SimpleChapterElement.DefaultMinimumDuration;
                KeepEmptyChapter = false;
            }

            public ActionMode ActionMode { get; set; }
            public ChapterFormat InputFormat { get; set; }
            public string? InputFilePath { get; set; }
            public ChapterFormat OutputFormat { get; set; }
            public string? OutputFilePath { get; set; }
            public bool Force { get; set; }
            public TimeSpan MaximumDuration { get; set; }
            public TimeSpan From { get; set; }
            public TimeSpan To { get; set; }
            public IDictionary<int, string> Titles { get; set; }
            public TimeSpan MinimumDuration { get; set; }
            public bool KeepEmptyChapter { get; set; }
        }

        private const string _fileFormatCsv = "csv";
        private const string _fileFormatFfmetadata = "ffmetadata";
        private const string _fileFormatFfllog = "fflog";
        private const string _fileFormatChapterList = "chapter_list";
        private const string _fileFormatImmediate = "immediate";
        private const string _fileFormatFfprobeCsv = "ffprobe_csv";
        private const string _fileFormatFfprobeFlat = "ffprobe_flat";
        private const string _fileFormatFfprobeJson = "ffprobe_json";
        private const string _fileFormatMovie = "movie";
        private static readonly object _consoleLockObject;
        private static readonly string _thisProgramName;
        private static readonly Regex _titleOptionPattern;

        static Program()
        {
            _consoleLockObject = new object();
            _thisProgramName = Path.GetFileNameWithoutExtension(typeof(Program).Assembly.Location);
            _titleOptionPattern = new Regex(@"^(-tt|--set_title):(?<chapterNumber>\d+)$");
        }

        public static int Main(string[] args)
        {
            var options = ParseOptions(args);
            if (options.ActionMode == ActionMode.Help)
            {
                HelpAction();
                return 0;
            }
            else if (options.ActionMode == ActionMode.Convert)
            {
                if (ConvertAction(options))
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine("OK");
#endif
                    return 0;
                }
                else
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine("NG");
#endif
                    return 1;
                }
            }
            else
            {
                PrintErrorMessage($"Specify the \"-help\" option to see how to use {_thisProgramName}.");
                return 1;
            }
        }

        private static void HelpAction()
        {
            var helpMessageTextLines = new[]
            {
                $"Usage: {_thisProgramName} <option1> <option2> ... <optionN>",
                $"",
                $"  * You can specify the options in any order. However, the \"-help\" option cannot be specified together with other options.",
                $"",
                $"  [Options]",
                $"    -help",
                $"        Display this help text.",
                $"",
                $"    --input_format <input format>  or  -if <input format>",
                $"      (Required) Specifies the format of the input file.",
                $"      The file formats that can be used are described later.",
                $"",
                $"    --output_format <output format>  or  -of <output format>",
                $"      (Required) Specifies the format of the output file.",
                $"      The file formats that can be used are described later.",
                $"",
                $"    --input <input file path>  or  -i <input file path>",
                $"      (Optional) Specifies the path name of the file from which to read the conversion source data.",
                $"      If this option is omitted, the source data is read from standard input.",
                $"",
                $"    --output <output file path>  or  -i <output file path>",
                $"      (Optional) Specifies the path name of the file that outputs the conversion results.",
                $"      If this option is omitted, the conversion result is written to standard output instead.",
                $"",
                $"    --force  or  -f",
                $"      (Optional) Specifies to overwrite the destination file if it already exists.",
                $"      If this option is omitted and the output destination file exists, an error will occur.",
                $"",
                $"    -ss <start time>",
                $"      (Optional) When trimming the input chapter, specify the trimming start time.",
                $"      If this option is not specified, the input chapter will be output from the beginning.",
                $"      The end time can be specified in hour-minute-second format (hh:mm.sss.sss) or second format (sssss.sss).",
                $"",
                $"    -to <end time>",
                $"      (Optional) When trimming the input chapter, specify the trimming end time.",
                $"      If this option is not specified, the input chapter will be output until the end.",
                $"      The end time can be specified in hour-minute-second format (hh:mm.sss.sss) or second format (sssss.sss).",
                $"      This option cannot be specified with the \"-t\" option.",
                $"",
                $"    -t <duration time>",
                $"      (Optional) When trimming the input chapter, specify the trimming length time.",
                $"      If this option is not specified, the input chapter will be output until the end.",
                $"      The end time can be specified in hour-minute-second format (hh:mm.sss.sss) or second format (sssss.sss).",
                $"      This option cannot be specified with the \"-to\" option.",
                $"",
                $"    -tt:<chapter number> <chapter title>  or  --set_title:<chapter number> <chapter title>",
                $"      (Optional) Change the title of the specified chapter.",
                $"      Chapter numbers are integers starting from 0.",
                $"      * Note that if trimming is done, the chapter number of the trimming result will be applied.",
                $"      * Don't forget to enclose the title in double quotes if the chapter title contains spaces.",
                $"",
                $"    --keep_empty_chapter",
                $"      (Optional) Causes zero-length chapters to be output as-is.",
                $"      By default chapconv automatically removes zero length chapters.",
                $"",
                $"    --maximum_duration <duratuon time>",
                $"      (Optional) Specifies the time to apply instead of the end time of the last chapter if it is unknown.",
                $"      The value for this option can be specified in hour-minute-second format (hh:mm.ss.sss) or second format (sssss.sss).",
                $"      The default value for this option is 168:00:00.000 (7 days).",
                $"      Normally you do not need to change the value of this option.",
                $"",
                $"      * commentary",
                $"        Some chapter file formats only describe the chapter start time, not the end time.",
                $"        The value of this option is used when converting from formats that do not describe an end time to formats that require an end time. (Example: conversion from \"chapter_list\" format to \"ffmetadata\" format, etc.)",
                $"        chapconv applies the next chapter's start time as the chapter's end time.",
                $"        And apply the value of this option as the end time of the last chapter.",
                $"",
                $"    --minimum_duration <duratuon time>",
                $"      (Optional) Specifies the minimum valid chapter length.",
                $"      The default is 0.01 (10 ms).",
                $"      Chapters shorter than this value are automatically merged with the chapters before and after them.",
                $"",
                $"      * Commentary",
                $"        In general, chapters that are too short to be visible are meaningless.",
                $"        So {_thisProgramName} will automatically merge chapters shorter than the value specified in this option with the chapter before or after it.",
                $"",
                $"        More specifically, if the first chapter is too short, merge it with the second chapter.",
                $"        Also, if the second and subsequent chapters are too short, they are combined with the previous chapter.",
                $"",
                $"        Titles before merging are carried over to the merged chapters.",
                $"        However, if both chapters had titles before the merge, one title will be lost and a warning message will be displayed.",
                $"",
                $"        If you want to suppress these operations, specify the \"--minimum_duration 0\" option.",
                $"",
                $"  [File formats]",
                $"    {_fileFormatFfmetadata}:",
                $"      Same format as ffmpeg metadata.",
                $"      An encoder must also be specified if this format is used in the output.",
                $"        Example: --ffencoder \"Lavf59.27.100\"",
                $"",
                $"    {_fileFormatFfllog}:",
                $"      This is the format displayed in the execution log of ffmpeg/ffprobe.",
                $"      For example:",
                $"",
                $"        Chapters:",
                $"          Chapter #0:0: start 0.000000, end 311.033000",
                $"            Metadata:",
                $"              title    : Chapter.1 Opening",
                $"          Chapter #0:1: start 311.033000, end 672.433000",
                $"            Metadata:",
                $"              title    : Chapter.2 And it happened",
                $"          ...",
                $"",
                $"      * Character encoding is UTF-8.",
                $"      * Chapters are read from the first found \"Chapters:\" in the input data.",
                $"        And reading ends when data other than chapters is found.",
                $"      * This format can only be specified on input.",
                $"",
                $"    {_fileFormatChapterList}:",
                $"      Text in the following format:",
                $"",
                $"        CHAPTER000=00:00:00.000",
                $"        CHAPTER000NAME=Chapter.1 Opening",
                $"        CHAPTER001=00:05:11.033",
                $"        CHAPTER001NAME=Chapter.2 And it happened",
                $"        ...",
                $"        CHAPTERnnn=00:29:51.953",
                $"        CHAPTERnnnNAME=Chapter.n Ending",
                $"",
                $"      * Character encoding is UTF-8.",
                $"      * The time format is <hour>:<minute>:<second>.",
                $"        You can also use fractions for seconds.",
                $"        Note that <hour> and <minute> cannot be omitted.",
                $"",
                $"    {_fileFormatCsv}:",
                $"      It's a very simple CSV format.",
                $"      Each line contains information for one chapter.",
                $"      Each row consists of two columns, each separated by a TAB code.",
                $"      The first column represents the chapter start time and the second column represents the chapter title.",
                $"",
                $"      * Character encoding is UTF-8.",
                $"      * The start time can be expressed in hour-minute-second format (eg 00:23:15.952) or seconds format (eg 1823.555).",
                $"",
                $"    {_fileFormatImmediate}:",
                $"      This is the format for specifying the chapter start time directly with a command parameter.",
                $"      This format can only be specified as an input format.",
                $"      When specifying this format, the \"-i\" or \"--input\" option must specify a comma-separated list of chapter start times instead of input file pathnames.",
                $"      Each chapter start time is specified in hour, minute, second format (eg 00:12:34.567) or second format (eg 1234.567).",
                $"      Also, if a chapter start time is prefixed with a plus (+) sign, it represents the time added to the previous chapter start time.",
                $"        Examples: Here are some example specifications. Any specification is completely equivalent.",
                $"",
                $"          -if immediate -i 0,101.835,211.144,316.115",
                $"          -if immediate -i 0,0:01:41.835,0:03:31.144,0:05:16.115",
                $"          -if immediate -i 0,+101.835,+109.309,+104.971",
                $"          -if immediate -i 0,+0:01:41.835,+0:01:49.309,+0:01:44.971",
                $"",
                $"      * Chapter titles cannot be specified in the \"immediate\" format. To specify the chapter title, add the \"-tt\" option or \"--set_title\" option.",
                $"",
                $"    {_fileFormatFfprobeCsv}:",
                $"      This format is equivalent to the data obtained by specifying the \"-print_format csv\" option to the \"ffprobe\" command.",
                $"",
                $"    {_fileFormatFfprobeFlat}:",
                $"      This format is equivalent to the data obtained by specifying the \"-print_format flat\" option to the \"ffprobe\" command.",
                $"",
                $"    {_fileFormatFfprobeJson}:",
                $"      This format is equivalent to the data obtained by specifying the \"-print_format json\" option to the \"ffprobe\" command.",
                $"",
                $"    {_fileFormatMovie}:",
                $"      This format means that the data is a movie file.",
                $"      This format can only be specified on input.",
                $"      If you specify this format, you must specify the path name of the movie file in the \"-i\" option or \"-input\" option.",
                $"",
                $"      * The specified video file path name extension must be appropriate for the type of movie file.",
                $"          For example: extensions such as \".mp4\" and \".mkv\" are allowed, but extensions such as \".tmp\" are not.",
                $"",
            };
            foreach (var message in helpMessageTextLines)
                Console.WriteLine(message);
        }

        private static bool ConvertAction(CommandLineOptions options)
        {
            var inputRawText = ReadRawText(options.InputFormat, options.InputFilePath);
            if (inputRawText is null)
                return false;

            var formatterParameter = new ChapterFormatterParameter(options.MaximumDuration, PrintWarningMessage);

            var inputChapterFormatter =
                options.InputFormat switch
                {
                    ChapterFormat.Csv => new CsvChapterFormatter(formatterParameter),
                    ChapterFormat.Ffmetadata => new FfmetadataChapterFormatter(formatterParameter),
                    ChapterFormat.Fflog => new FflogChapterFormatter(formatterParameter),
                    ChapterFormat.ChapterList => new ChapterListChapterFormatter(formatterParameter),
                    ChapterFormat.Immediate => new ImmediateChapterFormatter(formatterParameter),
                    ChapterFormat.FfprobeCsv => new FfprobeCsvChapterFormatter(formatterParameter),
                    ChapterFormat.FfprobeFlat => new FfprobeFlatChapterFormatter(formatterParameter),
                    ChapterFormat.FfprobeJson => new FfprobeJsonChapterFormatter(formatterParameter),
                    ChapterFormat.Movie => new MovieChapterFormatter(formatterParameter),
                    _ => (IChapterFormatter?)null,
                };
            if (inputChapterFormatter is null)
            {
                PrintErrorMessage($"Not supported format for intput: {options.InputFormat}");
                return false;
            }

            var chapters = ParseChapters(inputChapterFormatter, inputRawText);
            if (chapters is null)
                return false;

            var updatedChapters = ChapterFilter(chapters, options);
            if (updatedChapters is null)
                return false;

            var outputChapterFormatter =
                options.OutputFormat switch
                {
                    ChapterFormat.Csv => new CsvChapterFormatter(formatterParameter),
                    ChapterFormat.Ffmetadata => new FfmetadataChapterFormatter(formatterParameter),
                    ChapterFormat.ChapterList => new ChapterListChapterFormatter(formatterParameter),
                    ChapterFormat.FfprobeCsv => new FfprobeCsvChapterFormatter(formatterParameter),
                    ChapterFormat.FfprobeFlat => new FfprobeFlatChapterFormatter(formatterParameter),
                    ChapterFormat.FfprobeJson => new FfprobeJsonChapterFormatter(formatterParameter),
                    _ => (IChapterFormatter?)null,
                };
            if (outputChapterFormatter is null)
            {
                PrintErrorMessage($"Not supported format for output: {options.OutputFormat}");
                return false;
            }

            var outputRawText = RenderChapters(outputChapterFormatter, updatedChapters);
            return
                outputRawText is not null &&
                WriteRawText(options.OutputFilePath, outputRawText, options.Force);
        }

        private static IEnumerable<SimpleChapterElement>? ChapterFilter(IEnumerable<SimpleChapterElement> chapters, CommandLineOptions options)
        {
            try
            {
                return
                    chapters.ChapterFilter(
                        new ChapterFilterParameter
                        {
                            From = options.From,
                            To = options.To,
                            Titles = options.Titles,
                            MinimumDuration = options.MinimumDuration,
                            KeepEmptyChapter = options.KeepEmptyChapter,
                            WarningMessageReporter = PrintWarningMessage,
                        });
            }
            catch (Exception ex)
            {
                PrintErrorMessage(ex.Message);
                return null;
            }
        }

        private static string? ReadRawText(ChapterFormat inputFormat, string? inputFilePath)
        {
            try
            {
                if (inputFormat.IsAnyOf(ChapterFormat.Immediate, ChapterFormat.Movie))
                {
                    Validation.Assert(inputFilePath is not null, "inputFilePath is not null");
                    return inputFilePath;
                }
                else
                {
                    using var reader =
                        inputFilePath is null
                        ? new StreamReader(Console.OpenStandardInput(), Encoding.UTF8)
                        : new StreamReader(inputFilePath, Encoding.UTF8);
                    return reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                PrintErrorMessage(ex.Message);
                return null;
            }
        }

        private static bool WriteRawText(string? outputFilePath, string? outputRawText, bool force)
        {
            try
            {
                var utf8EncodingWithoutBOM = new UTF8Encoding(false);
                using var writer =
                    outputFilePath is null
                    ? new StreamWriter(Console.OpenStandardOutput(), utf8EncodingWithoutBOM)
                    : new StreamWriter(new FileStream(outputFilePath, force ? FileMode.Create : FileMode.CreateNew, FileAccess.Write, FileShare.None), utf8EncodingWithoutBOM);
                writer.Write(outputRawText);
                return true;
            }
            catch (Exception ex)
            {
                PrintErrorMessage(ex.Message);
                return false;
            }
        }

        private static IEnumerable<SimpleChapterElement>? ParseChapters(IChapterFormatter inputChapterFormatter, string inputRawText)
        {
            try
            {
                return inputChapterFormatter.Parse(inputRawText).ToList();
            }
            catch (Exception ex)
            {
                for (var exception = ex; exception is not null; exception = exception.InnerException)
                    PrintErrorMessage(exception.Message);
                return null;
            }
        }

        private static string? RenderChapters(IChapterFormatter outputChapterFormatter, IEnumerable<SimpleChapterElement> chapters)
        {
            try
            {
                return outputChapterFormatter.Render(chapters);
            }
            catch (Exception ex)
            {
                for (var exception = ex; exception is not null; exception = exception.InnerException)
                    PrintErrorMessage(exception.Message);
                return null;
            }
        }

        private static CommandLineOptions ParseOptions(string[] args)
        {
            var actionMode = ActionMode.NotSpecified;
            var inputFormat = ChapterFormat.NotSpecified;
            var inputFilePath = (string?)null;
            var outputFormat = ChapterFormat.NotSpecified;
            var outputFilePath = (string?)null;
            var force = (bool?)null;
            var maximumDuration = (TimeSpan?)null;
            var defaultReturnValue = new CommandLineOptions { ActionMode = ActionMode.Abort };
            var from = (TimeSpan?)null;
            var to = (TimeSpan?)null;
            var t = (TimeSpan?)null;
            var titles = new Dictionary<int, string>();
            var minimumDuration = (TimeSpan?)null;
            var keepEmptyChapter = (bool?)null;
            for (var index = 0; index < args.Length; index++)
            {
                switch (args[index])
                {
                    case "-help":
                        if (actionMode == ActionMode.Help)
                        {
                            PrintErrorMessage("The '-help' option is specified more than once.");
                            return defaultReturnValue;
                        }

                        if (actionMode == ActionMode.Convert)
                        {
                            PrintErrorMessage("The '-help' option must be specified alone.");
                            return defaultReturnValue;
                        }

                        actionMode = ActionMode.Help;
                        break;
                    case "-if":
                    case "--input_format":
                        if (actionMode == ActionMode.Help)
                        {
                            PrintErrorMessage("The '-help' option is specified more than once.");
                            return defaultReturnValue;
                        }

                        if (inputFormat != ChapterFormat.NotSpecified)
                        {
                            PrintErrorMessage("The \"-if\" option or \"--input_format\" option is specified more than once.");
                            return defaultReturnValue;
                        }

                        if (index + 1 >= args.Length)
                        {
                            PrintErrorMessage("The value of the \"-if\" option or \"--input_format\" option is not specified.");
                            return defaultReturnValue;
                        }

                        ++index;
                        {
                            var optionValue =
                                args[index] switch
                                {
                                    _fileFormatChapterList => ChapterFormat.ChapterList,
                                    _fileFormatCsv => ChapterFormat.Csv,
                                    _fileFormatFfllog => ChapterFormat.Fflog,
                                    _fileFormatFfmetadata => ChapterFormat.Ffmetadata,
                                    _fileFormatImmediate => ChapterFormat.Immediate,
                                    _fileFormatFfprobeCsv => ChapterFormat.FfprobeCsv,
                                    _fileFormatFfprobeFlat => ChapterFormat.FfprobeFlat,
                                    _fileFormatFfprobeJson => ChapterFormat.FfprobeJson,
                                    _fileFormatMovie => ChapterFormat.Movie,
                                    _ => ChapterFormat.NotSpecified,
                                };
                            if (optionValue == ChapterFormat.NotSpecified)
                            {
                                PrintErrorMessage($"The value of the \"-if\" option or \"--input_format\" option is an unsupported value.: \"{args[index]}\"");
                                return defaultReturnValue;
                            }

                            inputFormat = optionValue;
                        }

                        actionMode = ActionMode.Convert;
                        break;
                    case "-i":
                    case "--input":
                        if (actionMode == ActionMode.Help)
                        {
                            PrintErrorMessage("The '-help' option is specified more than once.");
                            return defaultReturnValue;
                        }

                        if (inputFilePath is not null)
                        {
                            PrintErrorMessage("The \"-i\" option or \"--input\" option is specified more than once.");
                            return defaultReturnValue;
                        }

                        if (index + 1 >= args.Length)
                        {
                            PrintErrorMessage("The value of the \"-i\" option or \"--input\" option is not specified.");
                            return defaultReturnValue;
                        }

                        ++index;
                        inputFilePath = args[index];
                        actionMode = ActionMode.Convert;
                        break;
                    case "-of":
                    case "--output_format":
                        if (actionMode == ActionMode.Help)
                        {
                            PrintErrorMessage("The '-help' option is specified more than once.");
                            return defaultReturnValue;
                        }

                        if (outputFormat != ChapterFormat.NotSpecified)
                        {
                            PrintErrorMessage("The \"-of\" option or \"--output_format\" option is specified more than once.");
                            return defaultReturnValue;
                        }

                        if (index + 1 >= args.Length)
                        {
                            PrintErrorMessage("The value of the \"-if\" option or \"--input_format\" option is not specified.");
                            return defaultReturnValue;
                        }

                        ++index;
                        {
                            var optionValue =
                                args[index] switch
                                {
                                    _fileFormatChapterList => ChapterFormat.ChapterList,
                                    _fileFormatCsv => ChapterFormat.Csv,
                                    _fileFormatFfllog => ChapterFormat.Fflog,
                                    _fileFormatFfmetadata => ChapterFormat.Ffmetadata,
                                    _fileFormatFfprobeCsv => ChapterFormat.FfprobeCsv,
                                    _fileFormatFfprobeFlat => ChapterFormat.FfprobeFlat,
                                    _fileFormatFfprobeJson => ChapterFormat.FfprobeJson,
                                    _ => ChapterFormat.NotSpecified,
                                };
                            if (optionValue == ChapterFormat.NotSpecified)
                            {
                                PrintErrorMessage($"The value of the \"-of\" option or \"--output_format\" option is an unsupported value.: \"{args[index]}\"");
                                return defaultReturnValue;
                            }

                            outputFormat = optionValue;
                        }

                        actionMode = ActionMode.Convert;
                        break;
                    case "-o":
                    case "--output":
                        if (actionMode == ActionMode.Help)
                        {
                            PrintErrorMessage("The '-help' option is specified more than once.");
                            return defaultReturnValue;
                        }

                        if (outputFilePath is not null)
                        {
                            PrintErrorMessage("The \"-o\" option or \"--output\" option is specified more than once.");
                            return defaultReturnValue;
                        }

                        if (index + 1 >= args.Length)
                        {
                            PrintErrorMessage("The value of the \"-o\" option or \"--output\" option is not specified.");
                            return defaultReturnValue;
                        }

                        ++index;
                        outputFilePath = args[index];
                        actionMode = ActionMode.Convert;
                        break;
                    case "-f":
                    case "--force":
                        if (actionMode == ActionMode.Help)
                        {
                            PrintErrorMessage("The '-help' option is specified more than once.");
                            return defaultReturnValue;
                        }

                        if (force is not null)
                        {
                            PrintErrorMessage("The \"-f\" option or \"--force\" option is specified more than once.");
                            return defaultReturnValue;
                        }

                        force = true;
                        actionMode = ActionMode.Convert;
                        break;
                    case "--maximum_duration":
                        if (actionMode == ActionMode.Help)
                        {
                            PrintErrorMessage("The '-help' option is specified more than once.");
                            return defaultReturnValue;
                        }

                        if (maximumDuration is not null)
                        {
                            PrintErrorMessage("The \"--maximum_duration\" option is specified more than once.");
                            return defaultReturnValue;
                        }

                        if (index + 1 >= args.Length)
                        {
                            PrintErrorMessage("The value of the \"--maximum_duration\" option is not specified.");
                            return defaultReturnValue;
                        }

                        ++index;
                        {
                            if (!args[index].TryParse(false, out TimeSpan maximumDurationValue))
                            {
                                PrintErrorMessage($"The format of the value of the \"--maximum_duration\" option is incorrect. The values for these options must be in hour-minute-second format (eg 00:12:34.567) or seconds format (eg 1234.567).: \"{args[index]}\"");
                                return defaultReturnValue;
                            }

                            maximumDuration = maximumDurationValue;
                        }

                        actionMode = ActionMode.Convert;
                        break;
                    case "-ss":
                        if (actionMode == ActionMode.Help)
                        {
                            PrintErrorMessage("The '-help' option is specified more than once.");
                            return defaultReturnValue;
                        }

                        if (from is not null)
                        {
                            PrintErrorMessage("The \"-ss\" option is specified more than once.");
                            return defaultReturnValue;
                        }

                        if (index + 1 >= args.Length)
                        {
                            PrintErrorMessage("The value of the \"-ss\" option is not specified.");
                            return defaultReturnValue;
                        }

                        ++index;
                        {
                            if (!args[index].TryParse(false, out TimeSpan time))
                            {
                                PrintErrorMessage($"The format of the value of the \"--ss\" option is incorrect. The values for these options must be in hour-minute-second format (eg 00:12:34.567) or seconds format (eg 1234.567).: \"{args[index]}\"");
                                return defaultReturnValue;
                            }

                            from = time;
                        }

                        actionMode = ActionMode.Convert;
                        break;
                    case "-to":
                        if (actionMode == ActionMode.Help)
                        {
                            PrintErrorMessage("The '-help' option is specified more than once.");
                            return defaultReturnValue;
                        }

                        if (to is not null)
                        {
                            PrintErrorMessage("The \"-to\" option is specified more than once.");
                            return defaultReturnValue;
                        }

                        if (index + 1 >= args.Length)
                        {
                            PrintErrorMessage("The value of the \"-to\" option is not specified.");
                            return defaultReturnValue;
                        }

                        ++index;
                        {
                            if (!args[index].TryParse(false, out TimeSpan time))
                            {
                                PrintErrorMessage($"The format of the value of the \"--to\" option is incorrect. The values for these options must be in hour-minute-second format (eg 00:12:34.567) or seconds format (eg 1234.567).: \"{args[index]}\"");
                                return defaultReturnValue;
                            }

                            to = time;
                        }

                        actionMode = ActionMode.Convert;
                        break;
                    case "-t":
                        if (actionMode == ActionMode.Help)
                        {
                            PrintErrorMessage("The '-help' option is specified more than once.");
                            return defaultReturnValue;
                        }

                        if (t is not null)
                        {
                            PrintErrorMessage("The \"-to\" option is specified more than once.");
                            return defaultReturnValue;
                        }

                        if (index + 1 >= args.Length)
                        {
                            PrintErrorMessage("The value of the \"-to\" option is not specified.");
                            return defaultReturnValue;
                        }

                        ++index;
                        {
                            if (!args[index].TryParse(false, out TimeSpan time))
                            {
                                PrintErrorMessage($"The format of the value of the \"--to\" option is incorrect. The values for these options must be in hour-minute-second format (eg 00:12:34.567) or seconds format (eg 1234.567).: \"{args[index]}\"");
                                return defaultReturnValue;
                            }

                            t = time;
                        }

                        actionMode = ActionMode.Convert;
                        break;
                    case "--minimum_duration":
                        if (actionMode == ActionMode.Help)
                        {
                            PrintErrorMessage("The '-help' option is specified more than once.");
                            return defaultReturnValue;
                        }

                        if (maximumDuration is not null)
                        {
                            PrintErrorMessage("The \"--minimum_duration\" option is specified more than once.");
                            return defaultReturnValue;
                        }

                        if (index + 1 >= args.Length)
                        {
                            PrintErrorMessage("The value of the \"--minimum_duration\" option is not specified.");
                            return defaultReturnValue;
                        }

                        ++index;
                        {
                            if (!args[index].TryParse(false, out TimeSpan minimumDurationValue))
                            {
                                PrintErrorMessage($"The format of the value of the \"--minimum_duration\" option is incorrect. The values for these options must be in hour-minute-second format (eg 00:12:34.567) or seconds format (eg 1234.567).: \"{args[index]}\"");
                                return defaultReturnValue;
                            }

                            minimumDuration = minimumDurationValue;
                        }

                        actionMode = ActionMode.Convert;
                        break;
                    case "--keep_empty_chapter":
                        if (actionMode == ActionMode.Help)
                        {
                            PrintErrorMessage("The '-help' option is specified more than once.");
                            return defaultReturnValue;
                        }

                        if (keepEmptyChapter is not null)
                        {
                            PrintErrorMessage("The \"--keep_empty_chapter\" option is specified more than once.");
                            return defaultReturnValue;
                        }

                        keepEmptyChapter = true;
                        actionMode = ActionMode.Convert;
                        break;
                    default:
                    {
                        var match = _titleOptionPattern.Match(args[index]);
                        if (match.Success)
                        {
                            if (actionMode == ActionMode.Help)
                            {
                                PrintErrorMessage("The '-help' option is specified more than once.");
                                return defaultReturnValue;
                            }

                            var chapterNumber = match.Groups["chapterNumber"].Value.ParseAsInt32();
                            if (index + 1 >= args.Length)
                            {
                                PrintErrorMessage($"The value of the \"-tt:{chapterNumber}\" option or \"--set_title\" option is not specified.");
                                return defaultReturnValue;
                            }

                            ++index;
                            var chapterName = args[index];
                            if (titles.ContainsKey(chapterNumber))
                            {
                                PrintErrorMessage($"The \"-tt:{chapterNumber}\" option or \"--set_title:{chapterNumber}\" option is specified more than once.");
                                return defaultReturnValue;
                            }

                            titles.Add(chapterNumber, chapterName);
                        }
                        else
                        {
                            PrintErrorMessage($"Incorrect option specified. : \"{args[index]}\"");
                            return defaultReturnValue;
                        }

                        actionMode = ActionMode.Convert;
                        break;
                    }
                }
            }

            if (actionMode == ActionMode.NotSpecified)
            {
                PrintErrorMessage("No arguments specified.");
                return defaultReturnValue;
            }

            if (actionMode == ActionMode.Help)
                return new CommandLineOptions { ActionMode = ActionMode.Help };

            if (inputFormat == ChapterFormat.NotSpecified)
            {
                PrintErrorMessage($"Input file format is not specified. Please specify the \"--input_format\" option.");
                return defaultReturnValue;
            }

            if (outputFormat == ChapterFormat.NotSpecified)
            {
                PrintErrorMessage($"Output file format is not specified. Please specify the \"--output_format\" option.");
                return defaultReturnValue;
            }

            if (inputFormat is ChapterFormat.Immediate or ChapterFormat.Movie)
            {
                if (inputFilePath is null)
                {
                    PrintErrorMessage($"Neither the \"-i\" option nor the \"--input\" option is specified even though \"{inputFormat switch { ChapterFormat.Immediate => _fileFormatImmediate, ChapterFormat.Movie => _fileFormatMovie, _ => "???", }}\" is specified for the input format. Specify a list of chapter start times with the \"-i\" or \"--input\" option.");
                    return defaultReturnValue;
                }
            }
            else
            {
                if (inputFilePath is not null && !File.Exists(inputFilePath))
                {
                    PrintErrorMessage($"Input file does not exist.: \"{inputFilePath}\"");
                    return defaultReturnValue;
                }
            }

            if (outputFilePath is not null && !(force ?? false) && File.Exists(outputFilePath))
            {
                PrintErrorMessage($"Output file already exists. Specify the \"-f\" option or \"--force\" option to allow overwriting of the output file.: \"{outputFilePath}\"");
                return defaultReturnValue;
            }

            var actualMaximumDuration = maximumDuration ?? SimpleChapterElement.DefaultMaximumDuration;

            if (to is not null && t is not null)
            {
                PrintErrorMessage("\"-to\" option and \"-t\" option cannot be specified at once.");
                return defaultReturnValue;
            }

            try
            {
                var (actualFrom, actualTo) = GetTrimmingRange(from, to, t);
                if (actualFrom >= actualMaximumDuration)
                {
                    PrintErrorMessage("The value of the \"-ss\" option is too large. Consider changing the value of the \"--maximum_duration\" option or \"-ss\" option.");
                    return defaultReturnValue;
                }

                return
                    new CommandLineOptions
                    {
                        ActionMode = actionMode,
                        InputFormat = inputFormat,
                        InputFilePath = inputFilePath,
                        OutputFormat = outputFormat,
                        OutputFilePath = outputFilePath,
                        Force = force ?? false,
                        MaximumDuration = actualMaximumDuration,
                        From = actualFrom,
                        To = actualTo,
                        Titles = titles,
                        MinimumDuration = minimumDuration ?? SimpleChapterElement.DefaultMinimumDuration,
                        KeepEmptyChapter = keepEmptyChapter ?? false,
                    };
            }
            catch (Exception ex)
            {
                PrintErrorMessage(ex.Message);
                return defaultReturnValue;
            }
        }

        private static (TimeSpan from, TimeSpan to) GetTrimmingRange(TimeSpan? ssValue, TimeSpan? toValue, TimeSpan? tValue)
        {
            if (toValue is not null && tValue is not null)
                throw new Exception("\"-to\" option and \"-t\" option cannot be specified at once.");

            Validation.Assert(ssValue is null || ssValue.Value >= TimeSpan.Zero, "ssValue is null || ssValue.Value >= TimeSpan.Zero");
            Validation.Assert(toValue is null || toValue.Value >= TimeSpan.Zero, "toValue is null || toValue.Value >= TimeSpan.Zero");
            Validation.Assert(tValue is null || tValue.Value >= TimeSpan.Zero, "tValue is null || tValue.Value >= TimeSpan.Zero");

            var from = ssValue ?? TimeSpan.Zero;
            var to =
                tValue is null
                ? toValue ?? TimeSpan.MaxValue
                : from > TimeSpan.MaxValue - tValue.Value
                ? TimeSpan.MaxValue
                : from + tValue.Value;
            return
                to >= from
                ? (from, to)
                : throw new Exception("The value of the \"-to\" option is smaller than the value of the \"-ss\" option. Consider changing the value of the \"-to\" option or \"-ss\" option.");
        }

        private static void PrintWarningMessage(string message)
        {
            lock (_consoleLockObject)
            {
                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Error.WriteLine($"{_thisProgramName}: WARNING: {message}");
                Console.ForegroundColor = color;
            }
        }

        private static void PrintErrorMessage(string message)
        {
            lock (_consoleLockObject)
            {
                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine($"{_thisProgramName}: ERROR: {message}");
                Console.ForegroundColor = color;
            }
        }
    }
}
