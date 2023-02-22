using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ChapterConverter
{
    class Program
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
            CSV,
            FFMetadata,
            FFLog,
            ChapterList,
            Immediate,
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
                To = _defaultMaximumDuration;
                Titles = new Dictionary<int, string>();
                MaximumDuration = _defaultMaximumDuration;
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
        }

        private const string _fileFormat_CSV = "csv";
        private const string _fileFormat_FFMetadata = "ffmetadata";
        private const string _fileFormat_FFLog = "fflog";
        private const string _fileFormat_ChapterList = "chapter_list";
        private const string _fileFormat_Immediate = "immediate";
        private static readonly object _consoleLockObject;
        private static readonly string _thisProgramName;
        private static readonly TimeSpan _defaultMaximumDuration;
        private static readonly Regex _titleOptionPattern;

        static Program()
        {
            _consoleLockObject = new object();
            _thisProgramName =Path.GetFileNameWithoutExtension(typeof(Program).Assembly.Location);
            _defaultMaximumDuration = TimeSpan.FromDays(7);
            _titleOptionPattern = new Regex(@"^(-tt|--title):(?<chapterNumber>\d+)$");
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
                    return 0;
                else
                    return 1;
            }
            else
            {
                PrintErrorMessage($"Specify the \"-help\" option to see how to use {_thisProgramName}.");
                return 1;
            }
        }

        private static void HelpAction()
        {
            var helpMessageTextLines = new []
            {
                $"Usage: {_thisProgramName} <option1> <option2> ... <optionN>",
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
                $"    -tt:<chapter number> <chapter title>  or  --title:<chapter number> <chapter title>",
                $"      (Optional) Change the title of the specified chapter.",
                $"      Chapter numbers are integers starting from 0.",
                $"      * Note that if trimming is done, the chapter number of the trimming result will be applied.",
                $"      * Don't forget to enclose the title in double quotes if the chapter title contains spaces.",
                $"",
                $"  [File formats]",
                $"    {_fileFormat_FFMetadata}:",
                $"      Same format as ffmpeg metadata.",
                $"      An encoder must also be specified if this format is used in the output.",
                $"        Example: --ffencoder \"Lavf59.27.100\"",
                $"",
                $"    {_fileFormat_FFLog}:",
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
                $"    {_fileFormat_ChapterList}:",
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
                $"    {_fileFormat_CSV}:",
                $"      CSV format. The delimiter is TAB, the first column of each line represents the chapter start time, and the second column represents the chapter name.",
                $"",
                $"        <start-time><TAB><chapter-name>",
                $"",
                $"      * Character encoding is UTF-8.",
                $"      * Do not enclose each column in quotation marks.",
                $"      * The start time can be expressed in hour-minute-second format (eg 00:23:15.952) or seconds format (eg 1823.555).",
                $"      * Chapter names cannot contain TAB codes.",
                $"",
                $"    {_fileFormat_Immediate}:",
                $"      This is the format for specifying the chapter start time directly with a command parameter.",
                $"      This format can only be specified as an input format.",
                $"      When specifying this format, the \"-i\" or \"--input\" option must specify a comma-separated list of chapter start times instead of input file pathnames.",
                $"      Each chapter start time is specified in hour, minute, second format (hh:mm:ss.sss) or second format (ssss.sss).",
                $"      Also, if a chapter start time is prefixed with a plus (+) sign, it represents the time added to the previous chapter start time.",
                $"        Examples: Here are some example specifications. Any specification is completely equivalent.",
                $"",
                $"          -if immediate -i 0,101.835,211.144,316.115",
                $"          -if immediate -i 0,0:01:41.835,0:03:31.144,0:05:16.115",
                $"          -if immediate -i 0,+101.835,+109.309,+104.971",
                $"          -if immediate -i 0,+0:01:41.835,+0:01:49.309,+0:01:44.971",
                $"",
                $"      * Chapter titles cannot be specified in the \"immediate\" format. To specify the chapter title, add the \"-tt\" option or \"--title\" option.",
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
                    ChapterFormat.CSV => new CSVChapterFormatter(formatterParameter),
                    ChapterFormat.FFMetadata => new FFMetadataChapterFormatter(formatterParameter),
                    ChapterFormat.FFLog => new FFLogChapterFormatter(formatterParameter),
                    ChapterFormat.ChapterList => new ChapterListChapterFormatter(formatterParameter),
                    ChapterFormat.Immediate => new ImmediateChapterFormatter(formatterParameter),
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

            var updatedChapters = ChapterFilter(chapters, options.From, options.To, options.Titles);
            if (updatedChapters is null)
                return false;

            var outputChapterFormatter =
                options.OutputFormat switch
                {
                    ChapterFormat.CSV => new CSVChapterFormatter(formatterParameter),
                    ChapterFormat.FFMetadata => new FFMetadataChapterFormatter(formatterParameter),
                    ChapterFormat.ChapterList => new ChapterListChapterFormatter(formatterParameter),
                    _ => (IChapterFormatter?)null,
                };
            if (outputChapterFormatter is null)
            {
                PrintErrorMessage($"Not supported format for output: {options.OutputFormat}");
                return false;
            }

            var outputRawText = RenderChapters(outputChapterFormatter, updatedChapters);
            if (outputRawText is null)
                return false;

            if (!WriteRawText(options.OutputFilePath, outputRawText, options.Force))
                return false;

            return true;
        }

        private static string? ReadRawText(ChapterFormat inputFormat, string? inputFilePath)
        {
            try
            {
                if (inputFormat == ChapterFormat.Immediate)
                {
                    if (inputFilePath is null)
                        throw new Exception("internal error (inputFormat == ChapterFormat.Immediate && inputFilePath is null)");
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

        private static IEnumerable<Chapter>? ParseChapters(IChapterFormatter inputChapterFormatter, string inputRawText)
        {
            try
            {
                return inputChapterFormatter.Parse(inputRawText).ToList();
            }
            catch (Exception ex)
            {
                PrintErrorMessage(ex.Message);
                return null;
            }
        }

        private static string? RenderChapters(IChapterFormatter outputChapterFormatter, IEnumerable<Chapter> chapters)
        {
            try
            {
                return outputChapterFormatter.Render(chapters);
            }
            catch (Exception ex)
            {
                PrintErrorMessage(ex.Message);
                return null;
            }
        }

        private static IEnumerable<Chapter>? ChapterFilter(IEnumerable<Chapter> chapters, TimeSpan from, TimeSpan to, IDictionary<int, string> titles)
        {
            try
            {
                if (from > to)
                    throw new Exception("internal error (from > to)");
                var duration = to - from;

                var trimmedChapters =
                    chapters
                    .Select(chapter =>
                    {
                        if (chapter.StartTime > chapter.EndTime)
                            throw new Exception("internal error (chapter.StartTime > chapter.EndTime)");
                        return new
                        {
                            startTime = chapter.StartTime - from,
                            endTime = chapter.EndTime - from,
                            title = chapter.Title,
                        };
                    })
                    .Where(chapter => chapter.endTime > TimeSpan.Zero && chapter.startTime < duration && chapter.startTime < chapter.endTime)
                    .Select((chapter, chapterNumber) =>
                        new Chapter(
                            chapter.startTime > TimeSpan.Zero ? chapter.startTime : TimeSpan.Zero,
                            chapter.endTime < duration ? chapter.endTime : duration,
                            !titles.ContainsKey(chapterNumber) ? chapter.title : titles[chapterNumber]))
                    .ToList();

                var invalidTitle =
                    titles
                    .Where(item => item.Key >= trimmedChapters.Count)
                    .Select(item => new { chapterNumber = item.Key, chapterTitle = item.Value })
                    .FirstOrDefault();
                if (invalidTitle is not null)
                    throw new Exception($"A chapter title was specified with the '--title:{invalidTitle.chapterNumber} \"{invalidTitle.chapterTitle}\"' option, but there is no corresponding chapter #{invalidTitle.chapterNumber}.");

                return trimmedChapters;
            }
            catch (Exception ex)
            {
                PrintErrorMessage(ex.Message);
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
                        switch (args[index])
                        {
                            case _fileFormat_ChapterList:
                                inputFormat = ChapterFormat.ChapterList;
                                break;
                            case _fileFormat_CSV:
                                inputFormat = ChapterFormat.CSV;
                                break;
                            case _fileFormat_FFLog:
                                inputFormat = ChapterFormat.FFLog;
                                break;
                            case _fileFormat_FFMetadata:
                                inputFormat = ChapterFormat.FFMetadata;
                                break;
                            case _fileFormat_Immediate:
                                inputFormat = ChapterFormat.Immediate;
                                break;
                            default:
                                PrintErrorMessage($"The value of the \"-if\" option or \"--input_format\" option is an unsupported value.: \"{args[index]}\"");
                                return defaultReturnValue;
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
                        switch (args[index])
                        {
                            case _fileFormat_ChapterList:
                                outputFormat = ChapterFormat.ChapterList;
                                break;
                            case _fileFormat_CSV:
                                outputFormat = ChapterFormat.CSV;
                                break;
                            case _fileFormat_FFLog:
                                outputFormat = ChapterFormat.FFLog;
                                break;
                            case _fileFormat_FFMetadata:
                                outputFormat = ChapterFormat.FFMetadata;
                                break;
                            default:
                                PrintErrorMessage($"The value of the \"-of\" option or \"--output_format\" option is an unsupported value.: \"{args[index]}\"");
                                return defaultReturnValue;
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
                            if (!double.TryParse(args[index], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture.NumberFormat, out double maximumDurationValue) ||
                                maximumDurationValue <= 0)
                            {
                                PrintErrorMessage("The value of the \"--maximum_duration\" option is non-numeric, negative or zero. Specify a positive number in seconds.");
                                return defaultReturnValue;
                            }
                            maximumDuration = TimeSpan.FromSeconds(maximumDurationValue);
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
                            var time = Utility.ParseTime(args[index], false);
                            if (time is null)
                            {
                                PrintErrorMessage($"The format of the value of the \"--ss\" option is incorrect. The values for these options must be in hour-minute-second format (eg hh:mm:ss.sss or mm:ss.sss) or seconds format (ss.sss).: \"{args[index]}\"");
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
                            var time = Utility.ParseTime(args[index], false);
                            if (time is null)
                            {
                                PrintErrorMessage($"The format of the value of the \"--to\" option is incorrect. The values for these options must be in hour-minute-second format (eg hh:mm:ss.sss or mm:ss.sss) or seconds format (ss.sss).: \"{args[index]}\"");
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
                            var time = Utility.ParseTime(args[index], false);
                            if (time is null)
                            {
                                PrintErrorMessage($"The format of the value of the \"--to\" option is incorrect. The values for these options must be in hour-minute-second format (eg hh:mm:ss.sss or mm:ss.sss) or seconds format (ss.sss).: \"{args[index]}\"");
                                return defaultReturnValue;
                            }
                            t = time;
                        }
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
                                var chapterNumber = int.Parse(match.Groups["chapterNumber"].Value, NumberStyles.None, CultureInfo.InvariantCulture.NumberFormat);
                                if (index + 1 >= args.Length)
                                {
                                    PrintErrorMessage($"The value of the \"-tt:{chapterNumber}\" option or \"--title\" option is not specified.");
                                    return defaultReturnValue;
                                }
                                ++index;
                                var chapterName = args[index];
                                if (titles.ContainsKey(chapterNumber))
                                {
                                    PrintErrorMessage($"The \"-tt:{chapterNumber}\" option or \"--title:{chapterNumber}\" option is specified more than once.");
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
                return new CommandLineOptions { ActionMode= ActionMode.Help };

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
            if (inputFormat == ChapterFormat.Immediate)
            {
                if (inputFilePath is null)
                {
                    PrintErrorMessage($"Neither the \"-i\" option nor the \"--input\" option is specified even though \"immediate\" is specified for the input format. Specify a list of chapter start times with the \"-i\" or \"--input\" option.");
                    return defaultReturnValue;
                }
            }
            else
            {
                if (inputFilePath is not null&& !File.Exists(inputFilePath))
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
            if (to is not null && t is not null)
            {
                PrintErrorMessage("\"-to\" option and \"-t\" option cannot be specified at once.");
                return defaultReturnValue;
            }

            var actualMaximumDuration = maximumDuration ?? _defaultMaximumDuration;
            var actualFrom = from ?? TimeSpan.Zero;
            if (actualFrom >= actualMaximumDuration)
            {
                PrintErrorMessage("The value of the \"-ss\" option is too large. Consider changing the value of the \"--maximum_duration\" option or \"-ss\" option.");
                return defaultReturnValue;
            }
            var actualTo = t is not null ? (actualFrom + t.Value) : (to ?? actualMaximumDuration);
            if (actualTo <= actualFrom)
            {
                PrintErrorMessage("The value of the \"-to\" option is smaller than the value of the \"-ss\" option. Consider changing the value of the \"-to\" option or \"-ss\" option.");
                return defaultReturnValue;
            }
            if (actualTo > actualMaximumDuration)
                actualTo = actualMaximumDuration;
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
                };
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