using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MatroskaBatchToolBox.Utility;
using MatroskaBatchToolBox.Utility.Interprocess;
using MatroskaBatchToolBox.Utility.Movie;
using Palmtree;
using Palmtree.IO;

namespace MovieMetadataEditor
{
    public static partial class Program
    {
        [Flags]
        private enum MetadataType
        {
            /// <summary>
            /// 対象メタデータなし
            /// </summary>
            None = 0,

            /// <summary>
            /// 最低限のメタデータ
            /// </summary>
            /// <remarks>
            /// <list type="bullet">
            /// <item>ストリームのtitle</item>
            /// <item>ストリームのlanguage</item>
            /// <item>ストリームのencoder</item>
            /// </list>
            /// </remarks>
            MinimumMetadata = 1 << 0,

            /// <summary>
            /// チャプターのメタデータ (title)
            /// </summary>
            ChapterMetadata = 1 << 1,

            /// <summary>
            /// その他のメタデータ
            /// </summary>
            OtherMetadata = 1 << 2,

            /// <summary>
            /// すべてのメタデータ
            /// </summary>
            All = MinimumMetadata | ChapterMetadata | OtherMetadata,
        }

        private class StreamComparer
            : IEqualityComparer<(string streamType, int streamIndex)>
        {
            bool IEqualityComparer<(string streamType, int streamIndex)>.Equals((string streamType, int streamIndex) x, (string streamType, int streamIndex) y)
                => string.Equals(x.streamType, y.streamType, StringComparison.OrdinalIgnoreCase) &&
                    x.streamIndex == y.streamIndex;

            int IEqualityComparer<(string streamType, int streamIndex)>.GetHashCode((string streamType, int streamIndex) obj)
                => HashCode.Combine(obj.streamIndex, obj.streamIndex);
        }

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
                StreamMetadata =
                    options
                    .Where(option => option.OptionType == OptionType.StreamMetadata)
                    .Select(option => (streamType: (string)option.OptionParameter[1], streamIndex: (int)option.OptionParameter[2], metadataName: (string)option.OptionParameter[3], metadataValue: (string)option.OptionParameter[4]))
                    .GroupBy(item => (item.streamType, item.streamIndex))
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(item => (item.metadataName, item.metadataValue)),
                        new StreamComparer());
                StreamDisposition =
                    options
                    .Where(option => option.OptionType == OptionType.StreamDisposition)
                    .ToDictionary(
                        option => (streamType: (string)option.OptionParameter[1], streamIndex: (int)option.OptionParameter[2]),
                        option => (IEnumerable<(string name, bool value)>)option.OptionParameter[3],
                        new StreamComparer());
                MetadataToClear = MetadataType.None;
                if (options.Any(option => option.OptionType == OptionType.ClearMetadata))
                    MetadataToClear |= MetadataType.All & ~(MetadataType.MinimumMetadata | MetadataType.ChapterMetadata);
                if (options.Any(option => option.OptionType == OptionType.ClearChapterMetadata))
                    MetadataToClear |= MetadataType.ChapterMetadata;
                if (options.Any(option => option.OptionType == OptionType.ClearAllMetadata))
                    MetadataToClear |= MetadataType.All;
                ClearDisposition = options.Any(option => option.OptionType == OptionType.ClearDisposition);
                ClearChapters = options.Any(option => option.OptionType == OptionType.ClearChapters);
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
            public IDictionary<(string streamType, int streamIndex), IEnumerable<(string metadataName, string metadataValue)>> StreamMetadata { get; }
            public IDictionary<(string streamType, int streamIndex), IEnumerable<(string dispositionName, bool dispositionValue)>> StreamDisposition { get; }
            public MetadataType MetadataToClear { get; }
            public bool ClearDisposition { get; }
            public bool ClearChapters { get; }
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
                        : throw new InvalidCommandOptionException($"The time specified with the \"-to\" option is less than the time specified with the \"-ss\" option.\r\nCheck the \"-ss\" option value and \"-to\" option value.: -ss {from.FormatTime(TimeFormatType.LongFormat, 3)} -to {toValue.Value.FormatTime(TimeFormatType.LongFormat, 3)}");
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

        private const string _metadataNameTitle = "TITLE";
        private const string _metadataNameLanguage = "LANGUAGE";
        private const string _metadataNameEncoder = "ENCODER";
        private const string _metadataNameDuration = "DURATION";
        private const string _dispositionNameForced = "FORCED";
        private const string _dispositionNameDefault = "DEFAULT";
        private static readonly string _thisProgramName;
        private static readonly FileInfo _ffmpegCommandFile;
        private static readonly Regex _chapterTitleOptionNamePattern;
        private static readonly Regex _streamOptionNamePattern;
        private static readonly Regex _streamOptionValuePattern;
        private static readonly Regex _dispositionOptionNamePattern;
        private static readonly IEnumerable<CommandOptionDefinition<OptionType>> _optionDefinitions;

        static Program()
        {
            _thisProgramName = Path.GetFileNameWithoutExtension(typeof(Program).Assembly.Location);
            _ffmpegCommandFile = new FileInfo(ProcessUtility.WhereIs("ffmpeg") ?? throw new FileNotFoundException("ffmpeg command is not installed."));
            _chapterTitleOptionNamePattern = new Regex(@"^(-tt|--chapter_title):(?<chapterIndex>\d+)$", RegexOptions.Compiled);
            _streamOptionNamePattern = new Regex(@"^(-s|--stream_metadata):(?<streamType>[vasdt]):(?<streamIndex>\d+)$", RegexOptions.Compiled);
            _streamOptionValuePattern = new Regex(@"^(?<metadataName>[a-zA-Z0-9_]+)=(?<metadataValue>.*)$", RegexOptions.Compiled);
            _dispositionOptionNamePattern = new Regex(@"^(-d|--stream_disposition):(?<streamType>[vasdt]):(?<streamIndex>\d+)$", RegexOptions.Compiled);
            _optionDefinitions = GetOptionDefinitions();
        }

        public static int Main(string[] args)
        {
            // このプロセスでは Ctrl+C を無視する。
            TinyConsole.CancelKeyPress += (sender, e) => e.Cancel = true;

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
                    // 入力の準備をする (入力元が標準入力である場合は一時ファイルにコピーする)
                    var (inputFormat, inputFile, temporaryInputFile) = GetInputMovieFile(commandOptions);

                    // 出力の準備をする (出力先が標準出力である場合は一時ファイルを作成する)
                    var (outputFormat, outputFile, temporaryOutputFile) = GetOutputMovieFile(commandOptions);

                    try
                    {
                        var movieInformation = GetMovieInformation(commandOptions, inputFormat, inputFile);

                        if (commandOptions.Verbose)
                        {
                            PrintInformationMessage("Start movie conversion.");
                            PrintInformationMessage($"  Input file format: {inputFormat}");
                            PrintInformationMessage($"  Input file format: \"{inputFile.FullName}\"");
                            if (outputFormat is not null)
                                PrintInformationMessage($"  Output file format: {outputFormat}");
                            PrintInformationMessage($"  Output file format: \"{outputFile.FullName}\"");
                        }

                        EditMetadata(
                            commandOptions,
                            movieInformation,
                            inputFormat,
                            inputFile,
                            outputFormat,
                            outputFile,
                            commandOptions.IsForceMode || temporaryOutputFile is not null);

                        if (commandOptions.Verbose)
                            PrintInformationMessage("Movie conversion finished.");

                        if (temporaryOutputFile is not null)
                        {
                            using var inStream = new FileStream(temporaryOutputFile.FullName, FileMode.Open, FileAccess.Read, FileShare.None);
                            using var outStream = Console.OpenStandardOutput();
                            if (commandOptions.Verbose)
                                PrintInformationMessage($"Copying from temporary file to standard output.: \"{temporaryOutputFile.FullName}\"");

                            CopyStream(inStream, outStream, inStream.Length);

                            if (commandOptions.Verbose)
                                PrintInformationMessage("Copy finished.");
                        }

                        return 0;
                    }
                    finally
                    {
                        if (temporaryInputFile is not null)
                        {
                            try
                            {
                                File.Delete(temporaryInputFile.FullName);
                                if (commandOptions.Verbose)
                                    PrintInformationMessage($"Temporary file is deleted.: \"{temporaryInputFile.FullName}\"");
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
                                if (commandOptions.Verbose)
                                    PrintInformationMessage($"Temporary file is deleted.: \"{temporaryOutputFile.FullName}\"");
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
                for (var e = ex as Exception; e is not null; e = e.InnerException)
                    PrintErrorMessage(e.Message);
                return null;
            }
        }

        private static (string? inputFormat, FileInfo inputFile, FileInfo? inputTemporaryFile) GetInputMovieFile(CommandParameter commandParameters)
        {

            if (commandParameters.Input is not null)
            {
                if (commandParameters.Verbose)
                    PrintInformationMessage($"Input file path: \"{commandParameters.Input}\"");
                return (commandParameters.InputFormat, new FileInfo(commandParameters.Input), null);
            }
            else
            {
                var temporaryInputFile = new FileInfo(Path.GetTempFileName());
                if (commandParameters.Verbose)
                {
                    PrintInformationMessage("Read from standard input.");
                    PrintInformationMessage($"Temprary file is created.: \"{temporaryInputFile.FullName}\"");
                }

                try
                {
                    using var inputStream = TinyConsole.OpenStandardInput();
                    using var outputStream = new FileStream(temporaryInputFile.FullName, FileMode.Create, FileAccess.Write, FileShare.None);
                    {
                        if (commandParameters.Verbose)
                            PrintInformationMessage($"Copying from standard input to temporary file.: \"{temporaryInputFile.FullName}\"");

                        CopyStream(inputStream, outputStream);

                        if (commandParameters.Verbose)
                            PrintInformationMessage("Copy finished.");
                    }

                    return (commandParameters.InputFormat, temporaryInputFile, temporaryInputFile);
                }
                catch (Exception ex)
                {
                    try
                    {
                        File.Delete(temporaryInputFile.FullName);
                        if (commandParameters.Verbose)
                            PrintInformationMessage($"Temporary file is deleted.: \"{temporaryInputFile.FullName}\"");
                    }
                    catch (Exception)
                    {
                    }

                    throw new Exception("An error occurred while preparing the input file.", ex);
                }
            }
        }

        private static (string? outputFormat, FileInfo outputFile, FileInfo? outputTemporaryFile) GetOutputMovieFile(CommandParameter commandParameters)
        {

            if (commandParameters.Output is not null)
            {
                if (commandParameters.Verbose)
                    PrintInformationMessage($"Output file path: \"{commandParameters.Output}\"");
                return (commandParameters.OutputFormat, new FileInfo(commandParameters.Output), null);
            }
            else
            {
                var temporaryOutputFile = new FileInfo(Path.GetTempFileName());
                if (commandParameters.Verbose)
                {
                    PrintInformationMessage("Write to standard output.");
                    PrintInformationMessage($"Temprary file is created.: \"{temporaryOutputFile.FullName}\"");
                }

                return (commandParameters.OutputFormat, temporaryOutputFile, temporaryOutputFile);
            }
        }

        private static MovieInformation GetMovieInformation(CommandParameter commandParameters, string? inputFormat, FileInfo inputFile)
        {
            if (commandParameters.Verbose)
                PrintInformationMessage($"Probe movie information.: \"{inputFile.FullName}\"");
            try
            {
                return
                    Command.GetMovieInformation(
                        inputFormat,
                        inputFile,
                        MovieInformationType.Chapters | MovieInformationType.Streams | MovieInformationType.Format,
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

        private static void EditMetadata(CommandParameter commandOptions, MovieInformation movieInformation, string? inputFormat, FileInfo inputFile, string? outputFormat, FileInfo outputFile, bool doOverWrite)
        {
            var streams =
                movieInformation.VideoStreams
                .Select(stream => new
                {
                    streamTypeSymbol = "v",
                    index = stream.IndexWithinVideoStream,
                    tags = stream.Tags,
                    disposition = stream.Disposition,
                })
                .Concat(
                    movieInformation.AudioStreams
                    .Select(stream => new
                    {
                        streamTypeSymbol = "a",
                        index = stream.IndexWithinAudioStream,
                        tags = stream.Tags,
                        disposition = stream.Disposition,
                    }))
                .Concat(
                    movieInformation.SubtitleStreams
                    .Select(stream => new
                    {
                        streamTypeSymbol = "s",
                        index = stream.IndexWithinSubtitleStream,
                        tags = stream.Tags,
                        disposition = stream.Disposition,
                    }))
                .Concat(
                    movieInformation.DataStreams
                    .Select(stream => new
                    {
                        streamTypeSymbol = "d",
                        index = stream.IndexWithinDataStream,
                        tags = stream.Tags,
                        disposition = stream.Disposition,
                    }))
                .Concat(
                    movieInformation.AttachmentStreams
                    .Select(stream => new
                    {
                        streamTypeSymbol = "t",
                        index = stream.IndexWithinAttachmentStream,
                        tags = stream.Tags,
                        disposition = stream.Disposition,
                    }))
                .Select(stream =>
                {
                    var streamTags = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    foreach (var tagName in stream.tags.EnumerateTagNames())
                    {
                        streamTags[tagName] =
                            tagName.ToUpperInvariant() switch
                            {
                                _metadataNameTitle or _metadataNameLanguage => (commandOptions.MetadataToClear & MetadataType.MinimumMetadata) != MetadataType.None ? "" : (stream.tags[tagName] ?? ""),
                                _ => (commandOptions.MetadataToClear & MetadataType.OtherMetadata) != MetadataType.None ? "" : (stream.tags[tagName] ?? ""),
                            };
                    }

                    if (commandOptions.StreamMetadata.TryGetValue((stream.streamTypeSymbol, stream.index), out var specifiedMetadataList))
                    {
                        foreach (var (metadataName, metadataValue) in specifiedMetadataList)
                            streamTags[metadataName] = metadataValue;
                    }

                    // encoder および duration のメタデータは ffmpeg により自動的に設定されるため、再設定対象から除外する。
                    _ = streamTags.Remove(_metadataNameEncoder);
                    _ = streamTags.Remove(_metadataNameDuration);

                    var streamDispositions = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
                    foreach (var (dispositionName, dispositionValue) in stream.disposition.EnumerateDispositions())
                    {
                        streamDispositions[dispositionName] =
                            dispositionName.ToUpperInvariant() switch
                            {
                                _dispositionNameForced or _dispositionNameDefault => !commandOptions.ClearDisposition && dispositionValue,
                                _ => dispositionValue,
                            };
                    }

                    if (commandOptions.StreamDisposition.TryGetValue((stream.streamTypeSymbol, stream.index), out var specifiedDispositionList))
                    {
                        foreach (var (dispositionName, dispositionValue) in specifiedDispositionList)
                            streamDispositions[dispositionName] = dispositionValue;
                    }

                    return new
                    {
                        stream.streamTypeSymbol,
                        stream.index,
                        tags = streamTags,
                        disposition = streamDispositions,
                        originalTags = stream.tags,
                        originalDisPosition = stream.disposition,
                    };
                })
                .ToList();

            var metadataFilePath = (string?)null;
            try
            {
                var ffmpegCommandParameters =
                    new List<string>
                    {
                        "-hide_banner"
                    };
                if (doOverWrite)
                    ffmpegCommandParameters.Add("-y");
                if (inputFormat is not null)
                    ffmpegCommandParameters.Add($"-f {inputFormat.CommandLineArgumentEncode()}");
                ffmpegCommandParameters.Add($"-i {inputFile.FullName.CommandLineArgumentEncode()}");
                if (!commandOptions.ClearChapters)
                {
                    metadataFilePath = Path.GetTempFileName();
                    if (commandOptions.Verbose)
                        PrintInformationMessage($"Temprary file is created.: \"{metadataFilePath}\"");
                    File.WriteAllText(metadataFilePath, GetFfmetadataText(commandOptions, movieInformation.Chapters), Encoding.UTF8);
                    ffmpegCommandParameters.Add($"-f ffmetadata -i {metadataFilePath.CommandLineArgumentEncode()}");
                }

                ffmpegCommandParameters.Add("-c copy -map 0");

                var formatTags = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var tagName in movieInformation.Format.Tags.EnumerateTagNames())
                {
                    formatTags[tagName] =
                        tagName.ToUpperInvariant() switch
                        {
                            _ => (commandOptions.MetadataToClear & MetadataType.OtherMetadata) != MetadataType.None ? "" : movieInformation.Format.Tags[tagName] ?? "",
                        };
                }

                // これらのメタデータは ffmpeg により自動的に設定されるため、再設定対象から除外する。
                _ = formatTags.Remove(_metadataNameEncoder);
                _ = formatTags.Remove(_metadataNameDuration);

                foreach (var tag in formatTags)
                {
                    if (tag.Value != (movieInformation.Format.Tags[tag.Key] ?? ""))
                        ffmpegCommandParameters.Add($"-metadata {tag.Key.CommandLineArgumentEncode()}={tag.Value.CommandLineArgumentEncode()}");
                }

                foreach (var stream in streams)
                {
                    foreach (var tag in stream.tags)
                    {
                        if (tag.Value != (stream.originalTags[tag.Key] ?? ""))
                            ffmpegCommandParameters.Add($"-metadata:s:{stream.streamTypeSymbol}:{stream.index} {tag.Key.CommandLineArgumentEncode()}={tag.Value.CommandLineArgumentEncode()}");
                    }

                    var dispositionSpecs = new List<string>();
                    foreach (var disposition in stream.disposition)
                    {
                        // default だけは 明示的に設定しないと必ず true になってしまう模様であるため、必ず設定する。
                        if (string.Equals(disposition.Key, _dispositionNameDefault, StringComparison.OrdinalIgnoreCase) || disposition.Value != stream.originalDisPosition[disposition.Key])
                            dispositionSpecs.Add($"{(disposition.Value ? "+" : "-")}{disposition.Key.CommandLineArgumentEncode()}");
                    }

                    if (dispositionSpecs.Any())
                        ffmpegCommandParameters.Add($"-disposition:{stream.streamTypeSymbol}:{stream.index} {string.Concat(dispositionSpecs)}");
                }

                if (commandOptions.ClearChapters)
                    ffmpegCommandParameters.Add("-map_chapters -1");
                else
                    ffmpegCommandParameters.Add("-map_chapters 1");
                if (outputFormat is not null)
                    ffmpegCommandParameters.Add($"-f {outputFormat.CommandLineArgumentEncode()}");
                ffmpegCommandParameters.Add($"{outputFile.FullName.CommandLineArgumentEncode()}");
                var ffmpegCommandLineText = string.Join(" ", ffmpegCommandParameters);
                if (commandOptions.Verbose)
                    PrintInformationMessage($"Execute: ffmpeg {ffmpegCommandLineText}");

                var exitCode =
                    Command.ExecuteCommand(
                        _ffmpegCommandFile,
                        ffmpegCommandLineText,
                        Encoding.UTF8,
                        null,
                        null,
                        null,
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
                        null);
                if (exitCode != 0)
                    throw new Exception($"An error occurred in the \"ffmpeg\" command. : exit-code={exitCode}");
            }
            finally
            {
                if (metadataFilePath is not null)
                {
                    try
                    {
                        File.Delete(metadataFilePath);
                        if (commandOptions.Verbose)
                            PrintInformationMessage($"Temporary file is deleted.: \"{metadataFilePath}\"");
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        private static string GetFfmetadataText(CommandParameter commandParameters, IEnumerable<ChapterInfo> chapters)
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
            if (commandParameters.ChapterTimes is not null)
            {
                // チャプターの時間が即値で指定されている場合
                return
                    commandParameters.ChapterTimes
                    .ToSimpleChapterElements(commandParameters.MaximumDuration, PrintWarningMessage)
                    .ChapterFilter(filterParameter)
                    .ToMetadataString();
            }
            else if ((commandParameters.MetadataToClear & MetadataType.ChapterMetadata) != MetadataType.None)
            {
                // チャプターのメタデータ(タイトル)の消去の指定がされている場合
                return
                    chapters
                    .Select(chapter => new SimpleChapterElement(chapter.StartTime, chapter.EndTime, ""))
                    .ChapterFilter(filterParameter)
                    .ToMetadataString();
            }
            else
            {
                return
                    chapters
                    .ChapterFilter(filterParameter)
                    .ToMetadataString();
            }
        }

        private static void CopyStream(Stream instream, Stream outstream)
        {
            try
            {
                var state = 0;
                TinyConsole.CursorVisible = ConsoleCursorVisiblity.Invisible;
                instream.CopyTo(
                    outstream,
                    new Progress<long>(CopiedLength =>
                    {
                        var stateSymbol =
                            state switch
                            {
                                1 => "＼",
                                2 => "│",
                                3 => "／",
                                _ => "─",
                            };
                        TinyConsole.Error.Write($"Copying... {stateSymbol}");
                        state = (state + 1) % 4;
                        try
                        {
                            TinyConsole.Erase(ConsoleEraseMode.FromCursorToEndOfLine);
                        }
                        catch (Exception)
                        {
                        }

                        TinyConsole.Write("\r");
                    }));
            }
            finally
            {
                TinyConsole.ResetColor();
                TinyConsole.CursorVisible = ConsoleCursorVisiblity.NormalMode;
                TinyConsole.Error.Write("\r");
                try
                {
                    TinyConsole.Erase(ConsoleEraseMode.FromCursorToEndOfLine);
                }
                catch (Exception)
                {
                }
            }
        }

        private static void CopyStream(Stream instream, Stream outstream, long copyLength)
        {
            try
            {
                TinyConsole.CursorVisible = ConsoleCursorVisiblity.Invisible;
                instream.CopyTo(
                    outstream,
                    new Progress<long>(CopiedLength =>
                    {
                        var progress = (double)CopiedLength / copyLength;
                        TinyConsole.Error.Write($"Copying... {progress * 100:F2}%");
                        var totalcolumns = 40;
                        TinyConsole.BackgroundColor = ConsoleColor.Black;
                        var copiedColumnLength = (int)Math.Round(progress * totalcolumns + 0.5);
                        TinyConsole.Error.Write(new string(' ', copiedColumnLength));
                        TinyConsole.ResetColor();
                        TinyConsole.Error.Write(new string('-', totalcolumns - copiedColumnLength));
                        try
                        {
                            TinyConsole.Erase(ConsoleEraseMode.FromCursorToEndOfLine);
                        }
                        catch (Exception)
                        {
                        }

                        TinyConsole.Write("\r");
                    }));
            }
            finally
            {
                TinyConsole.ResetColor();
                TinyConsole.CursorVisible = ConsoleCursorVisiblity.NormalMode;
                TinyConsole.Error.Write("\r");
                try
                {
                    TinyConsole.Erase(ConsoleEraseMode.FromCursorToEndOfLine);
                }
                catch (Exception)
                {
                }
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
        {
            TinyConsole.ForegroundColor = ConsoleColor.Cyan;
            TinyConsole.Write($"{programName}:INFORMATION:");
            TinyConsole.ResetColor();
            TinyConsole.Write($" {message}");
            try
            {
                TinyConsole.Erase(ConsoleEraseMode.FromCursorToEndOfLine);
            }
            catch (InvalidOperationException)
            {
            }

            TinyConsole.WriteLine();
        }

        private static void PrintWarningMessage(string message)
            => PrintWarningMessage(_thisProgramName, message);

        private static void PrintWarningMessage(string programName, string message)
        {
            TinyConsole.ForegroundColor = ConsoleColor.Yellow;
            TinyConsole.Write($"{programName}:WARNING: {message}");
            TinyConsole.ResetColor();
            try
            {
                TinyConsole.Erase(ConsoleEraseMode.FromCursorToEndOfLine);
            }
            catch (InvalidOperationException)
            {
            }

            TinyConsole.WriteLine();
        }

        private static void PrintErrorMessage(string message)
            => PrintErrorMessage(_thisProgramName, message);

        private static void PrintErrorMessage(string programName, string message)
        {
            TinyConsole.ForegroundColor = ConsoleColor.Red;
            TinyConsole.Write($"{programName}:ERROR: {message}");
            TinyConsole.ResetColor();
            try
            {
                TinyConsole.Erase(ConsoleEraseMode.FromCursorToEndOfLine);
            }
            catch (InvalidOperationException)
            {
            }

            TinyConsole.WriteLine();
        }
    }
}
