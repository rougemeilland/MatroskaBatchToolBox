using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MatroskaBatchToolBox.Utility;
using MatroskaBatchToolBox.Utility.Interprocess;
using MatroskaBatchToolBox.Utility.Movie;
using Palmtree;

namespace MatroskaBatchToolBox
{
    internal static class ExternalCommand
    {
        private const string _ffmpegPathEnvironmentVariableName = "FFMPEG_PATH";
        private static readonly object _loggingLockObject;
        private static readonly Regex _ffmpegNormalizeProgressPattern;
        private static readonly Regex _logsToIgnoreInFfmpegNormalize;
        private static readonly Regex _libopusNotAvailableOnFfmpegNormalizeInLogs;
        private static readonly Regex _moveStreamInfoPattern;
        private static readonly Regex _moveStreamInfoAudioDetailPattern;
        private static readonly Regex _audioChannelLayoutPattern;
        private static readonly Regex _ffmpegConversionDurationPattern;
        private static readonly Regex _ffmpegConversionProgressPattern;
        private static readonly Regex _ffmpegVmafScoreCalculationResultPattern;

        static ExternalCommand()
        {
            _loggingLockObject = new object();
            _ffmpegNormalizeProgressPattern = new Regex(@"^(((?<phase>Stream)\s+(?<currentStream>\d+)/(?<totalStream>\d+))|(?<phase>Second Pass)|(?<phase>File)):\s+(?<Percentage>\d+)%\|", RegexOptions.Compiled);
            _logsToIgnoreInFfmpegNormalize = new Regex(@"^(frame|fps|stream_\d+_\d+_q|bitrate|total_size|out_time_us|out_time_ms|out_time|dup_frames|drop_frames|speed|progress)=", RegexOptions.Compiled);
            _libopusNotAvailableOnFfmpegNormalizeInLogs = new Regex(@"^\[libopus @ [0-9a-fA-F]+\] Invalid channel layout 5.1\(side\) for specified mapping family -1\.", RegexOptions.Compiled);
            _moveStreamInfoPattern = new Regex(@"^  Stream #0:(?<id>\d+)(?<language>\([^\)]+\))?: (?<streamType>Video|Audio|Subtitle): (?<detail>.*)$", RegexOptions.Compiled);
            _moveStreamInfoAudioDetailPattern = new Regex(@"^\s*(?<codec>[^ ,][^,]*[^ ,])\s*,\s*(?<samplingFrequency>[^ ,][^,]*[^ ,]) Hz\s*,\s*(?<channelLayout>[^ ,][^,]*[^ ,])\s*,", RegexOptions.Compiled);
            _audioChannelLayoutPattern = new Regex(@"(?<layoutType>[^\(]+)(\([^\)]+\))?", RegexOptions.Compiled);
            _ffmpegConversionDurationPattern = new Regex(@"\s*(Duration|DURATION)\s*:\s*(?<time>\d+:\d+:\d+(\.\d+)?)", RegexOptions.Compiled);
            _ffmpegConversionProgressPattern = new Regex(@" time=(?<time>\d+:\d+:\d+(\.\d+)?) ", RegexOptions.Compiled);
            _ffmpegVmafScoreCalculationResultPattern = new Regex(@"^\[Parsed_libvmaf_\d+\s*@\s*[a-fA-F0-9]+\]\s*VMAF\s+score:\s*(?<vmafScoreValue>\d+(\.\d+)?)$", RegexOptions.Compiled);
        }

        public static void AbortExternalCommands()
            => Command.AbortExternalCommands();

        public static CommandResultCode NormalizeAudioFile(FileInfo logFile, FileInfo inFile, AudioEncoderType audioEncoder, FileInfo outFile, IProgress<double> progressReporter)
        {
            var ffmpegCommandFile =
                new FileInfo(
                    ProcessUtility.WhereIs("ffmpeg")
                    ?? throw new Exception("ffmpeg command is not installed."));
            Environment.SetEnvironmentVariable(_ffmpegPathEnvironmentVariableName, ffmpegCommandFile.FullName);

            var commandParameter = $"\"{inFile.FullName}\" -o \"{outFile.FullName}\" -pr -v -d --keep-loudness-range-target --audio-codec {audioEncoder.ToCodecSpec()}";
            try
            {
                var totalStream = 1;
                var isNotAvailableCodec = false;
                var exitCode =
                    Command.ExecuteCommand(
                        Settings.GlobalSettings.FfmpegNormalizeCommandFile,
                        commandParameter,

                        // ffmpeg-normalize 自体のエンコーディングはおそらくデフォルトエンコーディング (ターミナルのローカルエンコーディング 日本のPCなら shift_jis) だが、
                        // そこから呼び出される ffmpgのログは UTF-8になっている。
                        // エンコーディングによって内容に差異が発生するのはパス名に ASCII 文字以外が含まれている場合であるが、
                        // デバッグ時の情報として重要なのは ffmpeg のログ方なので、エンコーディングは ffmpeg と同じ UTF-8 にしておく。
                        // その代り、ffmpeg-normalize が出力するログの内容のうちパス名の部分は表示が乱れる可能性がある。
                        Encoding.UTF8,

                        null,
                        null,
                        Command.GetTextOutputRedirector(lineText =>
                        {
                            if (string.IsNullOrEmpty(lineText))
                                return;

                            if (audioEncoder == AudioEncoderType.Libopus && _libopusNotAvailableOnFfmpegNormalizeInLogs.IsMatch(lineText))
                            {
                                // libopus が一部のチャネルレイアウトをサポートしていないために発生する
                                isNotAvailableCodec = true;
                                return;
                            }

                            var match = _ffmpegNormalizeProgressPattern.Match(lineText);
                            if (match.Success)
                            {
                                var phaseText = match.Groups["phase"].Value;
                                var percentageText = match.Groups["Percentage"].Value;
                                int phase;
                                switch (phaseText)
                                {
                                    case "Stream":
                                    {
                                        if (!match.Groups["currentStream"].Value.TryParse(out int currentStream))
                                            return;
                                        if (!match.Groups["totalStream"].Value.TryParse(out totalStream))
                                            return;
                                        phase = currentStream - 1;
                                    }

                                    break;
                                    case "Second Pass":
                                        phase = totalStream;
                                        break;
                                    case "File":
                                        return; // NOP
                                    default:
                                        return; // NOP
                                }

                                if (!percentageText.TryParse(out int percentage))
                                    return;

                                // この時点で、 phase は完了しているフェーズの数、 totalStream + 1 は全フェーズの数、現在のフェーズの完了状況(%)は percentage
                                var progress = (phase + percentage / 100.0) / (totalStream + 1);
                                progressReporter.Report(progress);
                            }
                            else
                            {
                                // 進行状況などで頻出して、かつトラブル分析用としては重要ではない項目はログに記録しない。
                                if (!_logsToIgnoreInFfmpegNormalize.IsMatch(lineText))
                                    Log(logFile, new[] { lineText });
                                return;
                            }
                        }),
                        (level, message) => Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: {level}: {message}" }),
                        Command.GetChildProcessCanceller(childProcess =>
                        {
                            // 標準入出力を閉じることで ffmpeg-normalize のプロセスは終了するが、ffmpeg-normalize から起動された ffmpeg は終わらないまま。
                            childProcess.StandardInput.Close();
                            childProcess.StandardOutput.Close();
                            childProcess.StandardError.Close();

                            // プロセス名が "ffmpeg" で、かつ メインモジュールファイルが "MatroskaBatchToolBox にバンドルされた ffmpeg" であるプロセスをすべて列挙する。
                            // ※ ffmpeg-normalize は、例え PATH 環境変数で他の場所にある ffmpeg のパスが与えられていたとしても、
                            //    環境変数 FFMPEG_PATH で与えられた ffmpeg を  優先的に実行するので、kill したとしても MatroskaBatchToolBox 以外の
                            //    実行中のプログラムに被害はないはず。
                            var targetProcesses =
                                Process.GetProcessesByName(Path.GetFileNameWithoutExtension(ffmpegCommandFile.Name))
                                .Where(proc => string.Equals(proc?.MainModule?.FileName ?? "", ffmpegCommandFile.FullName, StringComparison.OrdinalIgnoreCase))
                                .ToList();

                            // 列挙されたプロセスをすべて kill する。
                            // タイミング次第で既に終了しているプロセスに対して kill しようとして例外が例外が発生することがあるため、例外はすべて無視する。
                            foreach (var proc in targetProcesses)
                            {
                                try
                                {
                                    if (!proc.HasExited)
                                        proc.Kill();
                                }
                                catch (Exception)
                                {
                                }
                            }
                        }));
                Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}:INFORMATION: ffmpeg-normalize exited with exit code {exitCode}." });
                if (isNotAvailableCodec)
                {
                    Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: ERROR: The specified movie's audio was not supported by libopus." });
                    return CommandResultCode.NotSupported;
                }

                return
                    exitCode == 0
                    ? CommandResultCode.Completed
                    : throw new Exception($"Failed in ffmpeg-normalize. (exit code: {exitCode})");
            }
            catch (OperationCanceledException)
            {
                Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}:INFORMATION: ffmpeg-normalize aborted at user request." });
                return CommandResultCode.Cancelled;
            }
        }

        public static CommandResultCode ConvertMovieFile(Settings localSettings, FileInfo logFile, FileInfo inFile, MovieInformation movieInfo, string? resolutionSpec, string? aspectRatioSpec, VideoEncoderType videoEncoder, bool deleteMetadata, bool setDisposition, bool setMetadata, bool setChapterTitles, bool passthroughStreams, FileInfo outFile, IProgress<double> progressReporter)
        {
            // ffmpeg ではメタデータの削除とタイトル付きチャプターの設定を同時に指定するとチャプターのタイトルが設定されないため、以下の条件での呼び出しを禁止する
            Validation.Assert(!deleteMetadata || !setChapterTitles, "!deleteMetadata || !setChapterTitles");

            Validation.Assert(!passthroughStreams || resolutionSpec == null, "!passthroughStreams || resolutionSpec == null");
            Validation.Assert(!passthroughStreams || aspectRatioSpec == null, "!passthroughStreams || aspectRatioSpec == null");
            Validation.Assert(!passthroughStreams || videoEncoder == VideoEncoderType.Copy, "!passthroughStreams || videoEncoder == VideoEncoderType.Copy");

            var videoStreams = movieInfo.VideoStreams.ToList();
            var audioStreams = movieInfo.AudioStreams.ToList();
            var subtitleStreams = movieInfo.SubtitleStreams.ToList();
            var dataStreams = movieInfo.DataStreams.ToList();
            if (localSettings.BehaviorForDataStreams == StreamOperationType.Error && dataStreams.Any())
                throw new Exception("Abort the conversion as there is a data stream in the movie.");
            var attachmentStreams = movieInfo.AttachmentStreams.ToList();
            if (localSettings.BehaviorForAttachmentStreams == StreamOperationType.Error && attachmentStreams.Any())
                throw new Exception("Abort the conversion as there is a attachment stream in the movie.");

            var outputVideoStreams =
                videoStreams
                .Where(stream => !(localSettings.DeleteImageVideoStream && stream.IsImageVideoStream))
                .ToList();
            var outputVideoStreamSummaries =
                outputVideoStreams
                .Select((stream, index) => new
                {
                    inIndex = stream.IndexWithinVideoStream,
                    streamTypeSymbol = "v",
                    outIndex = index,
                    encodrOptions =
                        (stream.IsImageVideoStream ? VideoEncoderType.Copy : videoEncoder).GetEncoderOptions(localSettings, index),
                    language = (localSettings.DefaultVideoLanguage is not null && outputVideoStreams.Count <= 1) ? localSettings.DefaultVideoLanguage : stream.Tags.Language ?? "",
                    title = stream.Tags.Title ?? "",
                })
                .ToList();
            var outputAudioStreamSummaries =
                audioStreams
                .Select((stream, index) => new
                {
                    inIndex = stream.IndexWithinAudioStream,
                    streamTypeSymbol = "a",
                    outIndex = index,
                    encodrOptions = new[] { $"-c:a:{index} copy" }.AsEnumerable(),
                    language = (localSettings.DefaultAudioLanguage is not null && audioStreams.Count <= 1) ? localSettings.DefaultAudioLanguage : stream.Tags.Language ?? "",
                    title = stream.Tags.Title ?? "",
                })
                .ToList();
            var outputSubtitleStreamSummaries =
                subtitleStreams
                .Select((stream, index) => new
                {
                    inIndex = stream.IndexWithinSubtitleStream,
                    streamTypeSymbol = "s",
                    outIndex = index,
                    encodrOptions = new[] { $"-c:s:{index} copy" }.AsEnumerable(),
                    language = stream.Tags.Language ?? "",
                    title = stream.Tags.Title ?? "",
                })
                .ToList();

            var outputDataStreamSummaries =
                dataStreams
                .Where(stream => localSettings.BehaviorForDataStreams == StreamOperationType.Keep)
                .Select((stream, index) => new
                {
                    inIndex = stream.IndexWithinDataStream,
                    streamTypeSymbol = "d",
                    outIndex = index,
                    encodrOptions = new[] { $"-c:d:{index} copy" }.AsEnumerable(),
                    language = stream.Tags.Language ?? "",
                    title = stream.Tags.Title ?? "",
                })
                .ToList();

            var outputAttachmentStreamSummaries =
                attachmentStreams
                .Where(stream => localSettings.BehaviorForAttachmentStreams == StreamOperationType.Keep)
                .Select((stream, index) => new
                {
                    inIndex = stream.IndexWithinAttachmentStream,
                    streamTypeSymbol = "t",
                    outIndex = index,
                    encodrOptions = new[] { $"-c:t:{index} copy" }.AsEnumerable(),
                    language = stream.Tags.Language ?? "",
                    title = stream.Tags.Title ?? "",
                })
                .ToList();

            // 出力対象のビデオストリームが存在するかどうかの確認
            if (outputVideoStreamSummaries.Count <= 0)
            {
                if (localSettings.DeleteImageVideoStream)
                    throw new Exception("The input movie file has no video streams other than images.");
                else
                    throw new Exception("The input movie file has no video streams.");
            }

            // 画像ビデオストリームは常に非変換対象なので、ビデオストリームが複数あるかどうかの判定にはカウントしない。
            var outputVideoStreamsCountExceptImage = videoStreams.Where(stream => !stream.IsImageVideoStream).Count();
            if (outputVideoStreamsCountExceptImage > 1 && !localSettings.AllowMultipleVideoStreams)
            {
                if (localSettings.DeleteImageVideoStream)
                    throw new Exception("You tried to convert a movie file with multiple video streams other than images. If you don't mind applying the same encoder and encoder options to all video streams, try setting the \"allow_multiple_vodeo_streams\" property in the configuration file to \"true\".");
                else
                    throw new Exception("You are trying to convert a movie file with multiple video streams. If you don't mind applying the same encoder and encoder options to all video streams, try setting the \"allow_multiple_vodeo_streams\" property in the configuration file to \"true\".");
            }

            var commandParameters = new List<string>
            {
                "-hide_banner",
                "-y",
                $"-i \"{inFile.FullName}\""
            };

            var metadataFilePath = (string?)null;
            try
            {
                if (setChapterTitles)
                {
                    metadataFilePath = CreateTemporaryMetadataFile(movieInfo, localSettings, logFile);
                    commandParameters.Add($"-f ffmetadata -i \"{metadataFilePath}\"");
                }

                // 呼び出し元から解像度指定が与えられていて、かつ元動画のビデオストリームの中に与えられた解像度指定と異なるものがある場合に、ffmpeg に解像度指定を与える。
                if (!passthroughStreams && resolutionSpec is not null && !outputVideoStreams.All(stream => stream.Resolution == resolutionSpec))
                    commandParameters.Add($"-s {resolutionSpec}");

                // 呼び出し元からアスペクト比が与えられていて、かつ元動画のビデオストリームの中に与えられたアスペクト比と異なるものがある場合に、ffmpeg にアスペクト比を与える。
                if (!passthroughStreams && aspectRatioSpec is not null && !outputVideoStreams.All(stream => stream.DisplayAspectRatio == aspectRatioSpec))
                    commandParameters.Add($"-aspect {aspectRatioSpec}");

                // クロッピング指定があればオプションに追加
                if (!passthroughStreams && localSettings.Cropping.IsValid)
                {
                    if (videoEncoder == VideoEncoderType.Copy)
                        Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: WARNING: Simple movie conversion ignores the \"cropping\" property." });
                    else
                        commandParameters.Add($"-vf crop={localSettings.Cropping.Width}:{localSettings.Cropping.Height}:{localSettings.Cropping.Left}:{localSettings.Cropping.Top}");
                }

                // トリミング指定があればオプションに追加
                if (!passthroughStreams && localSettings.Trimming.IsValid)
                {
                    if (videoEncoder == VideoEncoderType.Copy)
                        Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: WARNING: The \"trimming\" property is specified. The movie will be trimmed, but keep in mind that trimming in a simple conversion can result in unexpected gaps in the video and audio." });
                    if (!string.IsNullOrEmpty(localSettings.Trimming.Start))
                        commandParameters.Add($"-ss {localSettings.Trimming.Start}");
                    if (!string.IsNullOrEmpty(localSettings.Trimming.End))
                        commandParameters.Add($"-to {localSettings.Trimming.End}");
                }

                // ストリームごとのオプションを追加
                foreach (var outputStream in outputVideoStreamSummaries.Concat(outputAudioStreamSummaries).Concat(outputSubtitleStreamSummaries).Concat(outputDataStreamSummaries).Concat(outputAttachmentStreamSummaries))
                {
                    // エンコーダオプションとストリームマッピングの設定
                    foreach (var encoderOption in outputStream.encodrOptions)
                        commandParameters.Add(encoderOption);
                    commandParameters.Add($"-map 0:{outputStream.streamTypeSymbol}:{outputStream.inIndex}");

                    // disposition の設定
                    if (setDisposition)
                    {
                        if (localSettings.ResetDefaultStream || localSettings.ResetForcedStream)
                            commandParameters.Add($"-disposition:{outputStream.streamTypeSymbol}:{outputStream.outIndex} {(localSettings.ResetDefaultStream ? "-default" : "")}{(localSettings.ResetForcedStream ? "-forced" : "")}");
                    }

                    // メタデータ (言語及びタイトル) の設定
                    if (setMetadata)
                    {
                        if (string.IsNullOrEmpty(outputStream.language))
                            commandParameters.Add($"-metadata:s:{outputStream.streamTypeSymbol}:{outputStream.outIndex} language=");
                        else
                            commandParameters.Add($"-metadata:s:{outputStream.streamTypeSymbol}:{outputStream.outIndex} language=\"{outputStream.language}\"");
                        if (string.IsNullOrEmpty(outputStream.title))
                            commandParameters.Add($"-metadata:s:{outputStream.streamTypeSymbol}:{outputStream.outIndex} title=");
                        else
                            commandParameters.Add($"-metadata:s:{outputStream.streamTypeSymbol}:{outputStream.outIndex} title=\"{outputStream.title}\"");
                    }
                }

                // その他のオプションを追加
                if (localSettings.DeleteChapters)
                    commandParameters.Add("-map_chapters -1");
                else if (setChapterTitles)
                    commandParameters.Add("-map_chapters 1");

                if (deleteMetadata)
                    commandParameters.Add("-map_metadata -1");
                if (!passthroughStreams && videoEncoder != VideoEncoderType.Copy)
                    commandParameters.Add("-g 240");
                if (!passthroughStreams && !string.IsNullOrEmpty(localSettings.FfmpegOption))
                    commandParameters.Add(localSettings.FfmpegOption);
                commandParameters.Add($"\"{outFile.FullName}\"");

                try
                {
                    var exitCode =
                        Command.ExecuteFfmpeg(
                            string.Join(" ", commandParameters),
                            null,
                            null,
                            lineText => Log(logFile, new[] { lineText }),
                            (level, message) => Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: {level}: {message}" }),
                            progressReporter);
                    Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}:INFORMATION: ffmpeg exited with exit code {exitCode}." });
                    return
                        exitCode == 0
                        ? CommandResultCode.Completed
                        : throw new Exception($"ffmpeg failed. (exit code {exitCode})");
                }
                catch (OperationCanceledException)
                {
                    Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}:INFORMATION: ffmpeg aborted at user request." });
                    return CommandResultCode.Cancelled;
                }
            }
            finally
            {
                try
                {
                    if (metadataFilePath is not null && File.Exists(metadataFilePath))
                        File.Delete(metadataFilePath);
                }
                catch (Exception)
                {
                }
            }
        }

        public static CommandResultCode CalculateVmafScoreFromMovieFile(FileInfo logFile, FileInfo originalFile, FileInfo modifiedFile, string resolutionSpec, out double vmafScore, IProgress<double> progressReporter)
        {
            var commandParameter = new StringBuilder();
            commandParameter.Append("-hide_banner");
            commandParameter.Append($" -i \"{originalFile.FullName}\"");
            commandParameter.Append($" -i \"{modifiedFile.FullName}\"");
            commandParameter.Append($" -filter_complex \"[0:v]settb=1/AVTB,setpts=PTS-STARTPTS[original];[1:v]settb=1/AVTB,setpts=PTS-STARTPTS[encoded];[original][encoded]scale2ref=flags=bicubic,libvmaf=model=version=vmaf_v0.6.1\\\\:name=vmaf\\\\:n_threads=4:shortest=1:repeatlast=0\"");
#if false // -sn をつけると、VMAFスコア自体は正常に計算されるものの、実行経過に表示される time と speed に異常な値が表示され、進行状況が正しく把握できない。
            commandParameter.Append(" -an -sn");
#endif
            if (OperatingSystem.IsWindows())
                commandParameter.Append(" -f NULL -");
            else
                commandParameter.Append(" -f null /dev/null");
            try
            {
                var vmafScoreValue = (double?)null;
                var exitCode =
                    Command.ExecuteFfmpeg(
                        commandParameter.ToString(),
                        null,
                        null,
                        lineText =>
                        {
                            Log(logFile, new[] { lineText });
                            var vmafSoreMatch = _ffmpegVmafScoreCalculationResultPattern.Match(lineText);
                            if (vmafSoreMatch.Success)
                            {
                                // ログの中に VMAF スコアが含まれていた場合は記録する
                                vmafScoreValue = vmafSoreMatch.Groups["vmafScoreValue"].Value.ParseAsDouble();
                            }
                        },
                        (level, message) => Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}:{level}:{message}" }),
                        progressReporter);
                Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}:INFORMATION: ffmpeg exited with exit code {exitCode}." });
                if (exitCode != 0)
                    throw new Exception($"ffmpeg failed. (exit code {exitCode})");
                if (vmafScoreValue is null)
                    throw new Exception("VMAF score was not reported by ffmpeg.");
                vmafScore = vmafScoreValue.Value;
                return CommandResultCode.Completed;
            }
            catch (OperationCanceledException)
            {
                Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}:INFORMATION: ffmpeg aborted at user request." });
                vmafScore = double.NaN;
                return CommandResultCode.Cancelled;
            }
        }

        public static void Log(FileInfo logFile, IEnumerable<string> testLines)
        {
            lock (_loggingLockObject)
            {
                File.AppendAllLines(logFile.FullName, testLines);
            }
        }

        public static void ReportAggregateException(AggregateException ex)
        {
            ReportException(ex);
            foreach (var ex2 in ex.InnerExceptions)
                ReportException(ex2);
        }

        public static void ReportAggregateException(FileInfo logFile, AggregateException ex)
        {
            ReportException(logFile, ex);
            foreach (var ex2 in ex.InnerExceptions)
                ReportException(logFile, ex2);
        }

        public static void ReportException(Exception ex)
        {
            TinyConsole.CursorVisible = ConsoleCursorVisiblity.NormalMode;
            TinyConsole.WriteLine("----------");
            TinyConsole.WriteLine(ex.Message);
            TinyConsole.WriteLine(ex.StackTrace ?? "");
            for (var innerEx = ex.InnerException; innerEx is not null; innerEx = innerEx.InnerException)
            {
                TinyConsole.WriteLine("----------");
                TinyConsole.WriteLine(innerEx.Message);
                TinyConsole.WriteLine(innerEx.StackTrace ?? "");
            }

            TinyConsole.WriteLine("----------");
        }

        public static void ReportException(FileInfo logFile, Exception ex)
        {
            Log(logFile, new[] { "----------", ex.Message, ex.StackTrace ?? "" });
            for (var innerEx = ex.InnerException; innerEx is not null; innerEx = innerEx.InnerException)
                Log(logFile, new[] { "----------", innerEx.Message, innerEx.StackTrace ?? "" });
            Log(logFile, new[] { "----------" });
        }

        private static void ProcessFfmpegOutput(FileInfo logFile, string lineText, ref bool detectedToQuit, ref double maximumDurationSeconds, ref double vmafCalculationResult, IProgress<double> progressReporter)
        {
            if (lineText == "[q] command received. Exiting.")
            {
                detectedToQuit = true;
                return;
            }

            if (!lineText.StartsWith("frame=", StringComparison.InvariantCulture))
            {
                Log(logFile, new[] { lineText });

                var durationMatch = _ffmpegConversionDurationPattern.Match(lineText);
                if (durationMatch.Success)
                {
                    var duration = durationMatch.Groups["time"].Value.ParseAsTimeSpan(false).TotalSeconds;
                    if (double.IsNaN(maximumDurationSeconds) || duration > maximumDurationSeconds)
                        maximumDurationSeconds = duration;

                    return;
                }
                else
                {
                    var vmafSoreMatch = _ffmpegVmafScoreCalculationResultPattern.Match(lineText);
                    if (vmafSoreMatch.Success)
                    {
                        if (vmafSoreMatch.Groups["vmafScoreValue"].Value.TryParse(out double vmafScoreValue) &&
                            vmafScoreValue.IsInClosedInterval(0.0, 100.0))
                        {
                            vmafCalculationResult = vmafScoreValue;
                        }
                        else
                        {
                            // このルートには到達しないはず
                            return;
                        }
                    }
                    else
                    {
                        // Ignore unknown format
                        return;
                    }
                }
            }
            else
            {
                if (double.IsNaN(maximumDurationSeconds))
                {
                    // If the duration is not set, the progress cannot be calculated, so nothing is done.
                    return;
                }

                var match = _ffmpegConversionProgressPattern.Match(lineText);
                if (!match.Success)
                {
                    // Ignore unknown format
                    return;
                }

                var progress = match.Groups["time"].Value.ParseAsTimeSpan(false).TotalSeconds / maximumDurationSeconds;
                if (progress < 0)
                    progress = 0;
                if (progress > 1)
                    progress = 1;

                progressReporter.Report(progress);
                return;
            }
        }

        private static string CreateTemporaryMetadataFile(MovieInformation movieInfo, Settings localSettings, FileInfo logFile)
        {
            var startTime = localSettings.Trimming.StartTime ?? TimeSpan.Zero;
            var endTime = localSettings.Trimming.EndTime ?? Utility.SimpleChapterElement.DefaultMaximumDuration;
            var chapterFilterParameter = new ChapterFilterParameter
            {
                From = startTime,
                To = endTime,
                KeepEmptyChapter = false,
                WarningMessageReporter = message => Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: WARNING: {message}" }),
            };

            var metadataFilePath = Path.GetTempFileName();
            using var writer = new StreamWriter(metadataFilePath, false, new UTF8Encoding(false));
            writer.Write(movieInfo.Chapters.ChapterFilter(chapterFilterParameter).ToMetadataString());
            return metadataFilePath;
        }
    }
}
