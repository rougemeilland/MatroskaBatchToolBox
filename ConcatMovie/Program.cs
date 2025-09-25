using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MatroskaBatchToolBox.Utility;
using MatroskaBatchToolBox.Utility.Interprocess;
using MatroskaBatchToolBox.Utility.Movie;
using Palmtree;
using Palmtree.IO;
using Palmtree.IO.Console;
using Palmtree.Linq;

namespace ConcatMovie
{
    internal static partial class Program
    {
        private sealed class StreamComparer
            : IEqualityComparer<(string streamType, int streamIndex)>
        {
            bool IEqualityComparer<(string streamType, int streamIndex)>.Equals((string streamType, int streamIndex) x, (string streamType, int streamIndex) y)
                => string.Equals(x.streamType, y.streamType, StringComparison.OrdinalIgnoreCase) &&
                    x.streamIndex == y.streamIndex;

            int IEqualityComparer<(string streamType, int streamIndex)>.GetHashCode((string streamType, int streamIndex) obj)
                => HashCode.Combine(obj.streamIndex, obj.streamIndex);
        }

        private sealed class InputFileInfo
        {
            public InputFileInfo(FilePath filePath, string? fileFormat, TimeSpan duration, MovieInformation movieInfo)
            {
                FilePath = filePath;
                FileFormat = fileFormat;
                Duration = duration;
                MovieInfo = movieInfo;
            }

            public FilePath FilePath { get; }
            public string? FileFormat { get; }
            public TimeSpan Duration { get; }
            public MovieInformation MovieInfo { get; }
        }

        private static readonly FilePath _ffmpegCommandFile = new(ProcessUtility.WhereIs("ffmpeg") ?? throw new FileNotFoundException("ffmpeg command is not installed."));

        public static int Main(string[] args)
        {
            if (TinyConsole.InputEncoding.CodePage != Encoding.UTF8.CodePage || TinyConsole.OutputEncoding.CodePage != Encoding.UTF8.CodePage)
            {
                if (OperatingSystem.IsWindows())
                    TinyConsole.WriteLog(LogCategory.Warning, "The encoding of standard input or output is not UTF8. Consider running the command \"chcp 65001\".");
                else
                    TinyConsole.WriteLog(LogCategory.Warning, "The encoding of standard input or standard output is not UTF8.");
            }

            // このプロセスでは Ctrl+C を無視する。
            TinyConsole.CancelKeyPress += (sender, e) => e.Cancel = true;

            // コマンドの入出力エンコーディングを UTF8 にする
            TinyConsole.InputEncoding = Encoding.UTF8;
            TinyConsole.OutputEncoding = Encoding.UTF8;
            TinyConsole.DefaultTextWriter = ConsoleTextWriterType.StandardError;

            try
            {
                var commandOptions = new CommandParameters(args);
                if (commandOptions.Output is not null)
                {
                    try
                    {
                        var outputFile = new FilePath(commandOptions.Output);
                        var parentDirectory = outputFile.Directory;
                        if (parentDirectory is null || !parentDirectory.Exists)
                        {
                            TinyConsole.WriteLog(LogCategory.Error, $"The output file directory specified with the \"--output\" option is invalid or does not exist.: \"{commandOptions.Output}\"");
                            return 1;
                        }

                        if (!commandOptions.IsForceMode && outputFile.Exists)
                        {
                            TinyConsole.WriteLog(LogCategory.Error, $"The output file specified with the \"--output\" option already exists.: \"{commandOptions.Output}\"");
                            return 1;
                        }
                    }
                    catch (Exception ex)
                    {
                        TinyConsole.WriteLog(LogCategory.Error, $"Failed to access the output file specified by the \"--output\" option.: \"{commandOptions.Output}\"");
                        TinyConsole.WriteLog(ex);
                        return 1;
                    }
                }

                if (commandOptions.IsHelpMode)
                {
                    CommandParameters.OutputHelpText(TinyConsole.Out);
                    return 0;
                }

                if (commandOptions.Verbose)
                    TinyConsole.WriteLog(LogCategory.Information, "Start processing.");
                try
                {
                    // 出力の準備をする (出力先が標準出力である場合は一時ファイルを作成する)
                    var (outputFormat, outputFile, temporaryOutputFile) = GetOutputMovieFile(commandOptions);

                    var success = false;
                    try
                    {
                        var inputFileInfos = new InputFileInfo[commandOptions.InputFiles.Length];
                        for (var index = 0; index < inputFileInfos.Length; ++index)
                        {
                            var inputFileInfo = commandOptions.InputFiles.Span[index];
                            var movieInfo = GetMovieInformation(inputFileInfo, commandOptions.Verbose);
                            var duration =
                                inputFileInfo.InputFileDuration
                                ?? movieInfo.Format.Duration
                                ?? movieInfo.VideoStreams.Select(stream => stream.Tags.Duration).Concat(movieInfo.AudioStreams.Select(stream => stream.Tags.Duration)).Max()
                                ?? throw new ApplicationException($"Unable to get the duration time of the input file.: \"{inputFileInfo.InputFilePath}\"");
                            inputFileInfos[index] = new InputFileInfo(inputFileInfo.InputFilePath, inputFileInfo.InputFileFormat, duration, movieInfo);
                        }

                        {
                            var (videoMin, videoMax, audioMin, audioMax) =
                                inputFileInfos
                                .Select(info => info.MovieInfo)
                                .Aggregate(
                                    (videoMin: int.MaxValue, videoMax: int.MinValue, audioMin: int.MaxValue, audioMax: int.MinValue),
                                    (value, info) =>
                                    {
                                        var videoStreamsCount = info.VideoStreams.Count();
                                        var audioStreamsCount = info.AudioStreams.Count();
                                        return (int.Min(value.videoMin, videoStreamsCount), int.Max(value.videoMax, videoStreamsCount), int.Min(value.audioMin, audioStreamsCount), int.Max(value.audioMax, audioStreamsCount));
                                    });
                            if (videoMin != videoMax || audioMin != audioMax)
                                throw new ApplicationException("Number of video or audio streams in the input file does not match.");
                        }

                        if (commandOptions.Verbose)
                        {
                            TinyConsole.WriteLog(LogCategory.Information, "Start movie conversion.");
                            for (var index = 0; index < inputFileInfos.Length; ++index)
                            {
                                var inputFileInfo = inputFileInfos[index];
                                TinyConsole.WriteLog(LogCategory.Information, $"  Input file[{index}]: \"{inputFileInfo.FilePath}\" ({(inputFileInfo.FileFormat is not null ? $"{inputFileInfo.FileFormat}, " : "")}{inputFileInfo.Duration.TotalSeconds:F6})");
                            }

                            TinyConsole.WriteLog(LogCategory.Information, $"  Output file: \"{outputFile}\"{(outputFormat is not null ? $" ({outputFormat})" : "")}");
                        }

                        ConcatMovieFile(
                            inputFileInfos,
                            outputFormat,
                            outputFile,
                            commandOptions.FfmpegOptions,
                            commandOptions.Verbose,
                            commandOptions.IsForceMode || temporaryOutputFile is not null);

                        if (commandOptions.Verbose)
                            TinyConsole.WriteLog(LogCategory.Information, "Movie conversion finished.");

                        if (temporaryOutputFile is not null)
                        {
                            using var inStream = temporaryOutputFile.OpenRead();
                            using var outStream = TinyConsole.OpenStandardOutput();
                            if (commandOptions.Verbose)
                                TinyConsole.WriteLog(LogCategory.Information, $"Copying from temporary file to standard output.: \"{temporaryOutputFile.FullName}\"");

                            CopyStream(inStream, outStream, inStream.Length);

                            if (commandOptions.Verbose)
                                TinyConsole.WriteLog(LogCategory.Information, "Copy finished.");
                        }

                        success = true;
                        return 0;
                    }
                    finally
                    {
                        // 一時ファイルが作成されていた場合は削除する
                        if (temporaryOutputFile is not null)
                        {
                            temporaryOutputFile.SafetyDelete();
                            if (commandOptions.Verbose)
                                TinyConsole.WriteLog(LogCategory.Information, $"Temporary file is deleted.: \"{temporaryOutputFile.FullName}\"");
                        }

                        // 正常に出力できなかった場合は出力先ファイルを削除する。
                        if (!success && outputFile is not null && outputFile.Exists)
                        {
                            outputFile.SafetyDelete();
                            if (commandOptions.Verbose)
                                TinyConsole.WriteLog(LogCategory.Information, $"Output is not successful, so output file will be deleted.: \"{outputFile.FullName}\"");
                        }
                    }
                }
                catch (Exception ex)
                {
                    TinyConsole.WriteLog(ex);
                    return 1;
                }
            }
            finally
            {
                TinyConsole.ResetColor();
            }
        }

        private static (string? outputFormat, FilePath outputFile, FilePath? outputTemporaryFile) GetOutputMovieFile(CommandParameters commandParameters)
        {

            if (commandParameters.Output is not null)
            {
                if (commandParameters.Verbose)
                    TinyConsole.WriteLog(LogCategory.Information, $"Output file path: \"{commandParameters.Output}\"");
                return (commandParameters.OutputFormat, new FilePath(commandParameters.Output), null);
            }
            else
            {
                var temporaryOutputFile = FilePath.CreateTemporaryFile(suffix: ".mkv");
                if (commandParameters.Verbose)
                {
                    TinyConsole.WriteLog(LogCategory.Information, "Write to standard output.");
                    TinyConsole.WriteLog(LogCategory.Information, $"Temprary file is created.: \"{temporaryOutputFile.FullName}\"");
                }

                return (commandParameters.OutputFormat, temporaryOutputFile, temporaryOutputFile);
            }
        }

        private static MovieInformation GetMovieInformation(InputFileSpecification inputFileSpec, bool verbose)
        {
            if (verbose)
                TinyConsole.WriteLog(LogCategory.Information, $"Probe movie information.: \"{inputFileSpec.InputFilePath}\"");
            try
            {
                return
                    Command.GetMovieInformation(
                        inputFileSpec.InputFileFormat,
                        inputFileSpec.InputFilePath,
                        MovieInformationType.Chapters | MovieInformationType.Streams | MovieInformationType.Format,
                        (level, message) =>
                        {
                            if (level != LogCategory.Information || verbose)
                                TinyConsole.WriteLog(level, message);
                        });
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Failed to get movie information.", ex);
            }
        }

        private static void ConcatMovieFile(ReadOnlyMemory<InputFileInfo> inputFileInfos, string? outputFormat, FilePath outputFile, ReadOnlyMemory<string> ffmpegOptions, bool verbose, bool doOverWrite)
        {

            var ffmpegCommandParameters =
                new List<string>
                {
                    "-hide_banner"
                };
            if (doOverWrite)
                ffmpegCommandParameters.Add("-y");
            var metadataFilePath = (FilePath?)null;
            try
            {
                for (var index = 0; index < inputFileInfos.Length; ++index)
                    ffmpegCommandParameters.Add($"-i {inputFileInfos.Span[index].FilePath.FullName.EncodeCommandLineArgument()}");

                metadataFilePath = FilePath.CreateTemporaryFile(suffix: ".txt");
                if (verbose)
                    TinyConsole.WriteLog(LogCategory.Information, $"Temprary file is created.: \"{metadataFilePath.FullName}\"");
                metadataFilePath.WriteAllText(GetMetadataText(inputFileInfos), Encoding.UTF8);
                ffmpegCommandParameters.Add($"-f ffmetadata -i {metadataFilePath.FullName.EncodeCommandLineArgument()}");
                ffmpegCommandParameters.Add($"-filter_complex {GetComlexFilterSpec(inputFileInfos).EncodeCommandLineArgument()}");
                for (var index = 0; index < ffmpegOptions.Length; ++index)
                    ffmpegCommandParameters.Add(ffmpegOptions.Span[index].EncodeCommandLineArgument());
                ffmpegCommandParameters.Add("-fflags +genpts");
                foreach (var stream in inputFileInfos.Span[0].MovieInfo.VideoStreams)
                    ffmpegCommandParameters.Add($"-map {$"[outv{stream.IndexWithinVideoStream}]".EncodeCommandLineArgument()}");
                foreach (var stream in inputFileInfos.Span[0].MovieInfo.VideoStreams)
                    ffmpegCommandParameters.Add($"-map {$"[outa{stream.IndexWithinVideoStream}]".EncodeCommandLineArgument()}");
                ffmpegCommandParameters.Add($"-map_chapters {inputFileInfos.Length}");

                if (outputFormat is not null)
                    ffmpegCommandParameters.Add($"-f {outputFormat.EncodeCommandLineArgument()}");
                ffmpegCommandParameters.Add($"{outputFile.FullName.EncodeCommandLineArgument()}");
                var ffmpegCommandLineText = string.Join(" ", ffmpegCommandParameters);
                if (verbose)
                    TinyConsole.WriteLog(LogCategory.Information, $"Execute: ffmpeg {ffmpegCommandLineText}");

                var exitCode =
                    Command.ExecuteCommand(
                        _ffmpegCommandFile,
                        ffmpegCommandLineText,
                        null,
                        null,
                        null,
                        null,
                        null,
                        (level, message) =>
                        {
                            if (level != LogCategory.Information)
                                TinyConsole.WriteLog(level, message);
                        },
                        null);
                if (exitCode != 0)
                    throw new ApplicationException($"An error occurred in the \"ffmpeg\" command. : exit-code={exitCode}");
            }
            finally
            {
                if (metadataFilePath is not null)
                {
                    metadataFilePath.SafetyDelete();
                    if (verbose)
                        TinyConsole.WriteLog(LogCategory.Information, $"Temporary file is deleted.: \"{metadataFilePath}\"");
                }
            }

            static string GetMetadataText(ReadOnlyMemory<InputFileInfo> inputFileInfos)
            {
                var totalDuration = TimeSpan.Zero;
                var chapters = new List<SimpleChapterElement>();
                for (var index = 0; index < inputFileInfos.Length; ++index)
                {
                    var newTotalDuration = totalDuration + inputFileInfos.Span[index].Duration;
                    chapters.Add(new SimpleChapterElement(totalDuration, newTotalDuration, ""));
                    totalDuration = newTotalDuration;
                }

                return chapters.ToMetadataString();
            }

            static string GetComlexFilterSpec(ReadOnlyMemory<InputFileInfo> inputFileInfos)
            {
                var filterElements = new List<string>();
                for (var fileIndex = 0; fileIndex < inputFileInfos.Length; ++fileIndex)
                {
                    var inputFileInfo = inputFileInfos.Span[fileIndex];
                    foreach (var stream in inputFileInfo.MovieInfo.VideoStreams)
                        filterElements.Add($"[{fileIndex}:v:{stream.IndexWithinVideoStream}]tpad=stop=-1,trim=0:{inputFileInfo.Duration.TotalSeconds:F6},setpts=PTS-STARTPTS[tempv{fileIndex}_{stream.IndexWithinVideoStream}]");
                    foreach (var stream in inputFileInfo.MovieInfo.AudioStreams)
                        filterElements.Add($"[{fileIndex}:a:{stream.IndexWithinAudioStream}]apad=whole_dur={inputFileInfo.Duration.TotalSeconds:F6},atrim=0:{inputFileInfo.Duration.TotalSeconds:F6},asetpts=PTS-STARTPTS[tempa{fileIndex}_{stream.IndexWithinAudioStream}]");
                }

                var concatSources = new List<string>();
                for (var fileIndex = 0; fileIndex < inputFileInfos.Length; ++fileIndex)
                {
                    var inputFileInfo = inputFileInfos.Span[fileIndex];
                    foreach (var stream in inputFileInfo.MovieInfo.VideoStreams)
                        concatSources.Add($"[tempv{fileIndex}_{stream.IndexWithinVideoStream}]");
                    foreach (var stream in inputFileInfo.MovieInfo.AudioStreams)
                        concatSources.Add($"[tempa{fileIndex}_{stream.IndexWithinAudioStream}]");
                }

                var (output, videoStreamsCount, audioStreamsCount) = GetFilterOutput(inputFileInfos);
                filterElements.Add($"{string.Concat(concatSources)}concat={inputFileInfos.Length}:v={videoStreamsCount}:a={audioStreamsCount}{string.Concat(output)}");
                return string.Join(";", filterElements);

                static (string output, int videoStreamsCount, int audioStreamsCount) GetFilterOutput(ReadOnlyMemory<InputFileInfo> inputFileInfos)
                {
                    var concatTargets = new List<string>();
                    var videoStreamsCount = 0;
                    var audioStreamsCount = 0;
                    var inputFileInfo = inputFileInfos.Span[0];
                    foreach (var stream in inputFileInfo.MovieInfo.VideoStreams)
                    {
                        concatTargets.Add($"[outv{stream.IndexWithinVideoStream}]");
                        ++videoStreamsCount;
                    }

                    foreach (var stream in inputFileInfo.MovieInfo.AudioStreams)
                    {
                        concatTargets.Add($"[outa{stream.IndexWithinAudioStream}]");
                        ++audioStreamsCount;
                    }

                    return (string.Concat(concatTargets), videoStreamsCount, audioStreamsCount);
                }
            }
        }

        private static void CopyStream(ISequentialInputByteStream instream, ISequentialOutputByteStream outstream, ulong copyLength)
        {
            TinyConsole.CursorVisible = ConsoleCursorVisiblity.Invisible;
            instream.CopyTo(
                outstream,
                new Progress<ulong>(CopiedLength =>
                {
                    var progress = (double)CopiedLength / copyLength;
                    TinyConsole.Write($"Copying... {progress * 100:F2}%          \r");
                }));
        }
    }
}
