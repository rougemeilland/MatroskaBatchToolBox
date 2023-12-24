using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MatroskaBatchToolBox.Utility;
using MatroskaBatchToolBox.Utility.Interprocess;
using MatroskaBatchToolBox.Utility.Movie;
using Palmtree;
using Palmtree.IO;
using Palmtree.Numerics;

namespace MatroskaBatchToolBox
{
    internal static class MatroskaAction
    {
        private const string _convertModeSymbolPatternText = @"(?<convertMode>==)";
        private const string _resolutionPatternText = @"(?<resolutionWidth>\d+)x(?<resolutionHeight>\d+)";
        private const string _separaterBetweenResolutionSpecAndAspectRatioPatternText = @" +|\-|_";
        private const string _aspectRatioPatternText = @"((?<aspectRatioWidth>\d+)(?<aspectRatioSeparater>to|：|:)(?<aspectRatioHeight>\d+))|(?<aspectRatioValue>\d+\.\d+)";
        private static readonly Regex _duplicatedFileNamePattern;
        private static readonly Regex _normalizedFileNamePattern;
        private static readonly Regex _encodedFileNamePattern;
        private static readonly Regex _startsWithConvertModeSymbolPattern;
        private static readonly Regex _startsWithResolutionSpecPattern;
        private static readonly Regex _startsWithSeparaterBetweenResolutionSpecAndAspectRatioPattern;
        private static readonly Regex _startsWithAspectRatioSpecPattern;
        private static readonly Regex _startsWithCommentPattern;
        private static readonly Regex _resolutionAndAspectRatioSpecInFileNamePattern;

        static MatroskaAction()
        {
            _duplicatedFileNamePattern = new Regex(@" \(\d+\)$", RegexOptions.Compiled);
            _normalizedFileNamePattern = new Regex(@"(?<prefix>\[([^\]]*? )?)audio-normalized(?<suffix>( [^\]]*)?\])", RegexOptions.Compiled);
            _encodedFileNamePattern = new Regex(@"(?<prefix>\[([^\]]*? )?)(H\.264|x264|MPEG\-4|AVC|H\.265|x265|HEVC|AV1) (CRF|crf)(?<suffix>( [^\]]*)?\])", RegexOptions.Compiled);
            _startsWithConvertModeSymbolPattern = new Regex($"^({_convertModeSymbolPatternText})", RegexOptions.Compiled);
            _startsWithResolutionSpecPattern = new Regex($"^({_resolutionPatternText})", RegexOptions.Compiled);
            _startsWithSeparaterBetweenResolutionSpecAndAspectRatioPattern = new Regex($"^({_separaterBetweenResolutionSpecAndAspectRatioPatternText})", RegexOptions.Compiled);
            _startsWithAspectRatioSpecPattern = new Regex($"^({_aspectRatioPatternText})", RegexOptions.Compiled);
            _startsWithCommentPattern = new Regex($"^ *#", RegexOptions.Compiled);
            var groupNamePattern = new Regex(@"\?<[^>]+>");
            var resolutionPatternTextWithoutGroup = groupNamePattern.Replace(_resolutionPatternText, "");
            var aspectRatioPatternTextWithoutGroup = groupNamePattern.Replace(_aspectRatioPatternText, "");
            _resolutionAndAspectRatioSpecInFileNamePattern =
                new Regex(
                    $"(?<prefix>\\[([^\\]]*? )?)(?<resolutionAndAspectRatioSpec>({resolutionPatternTextWithoutGroup})( ({aspectRatioPatternTextWithoutGroup}))?)(?<suffix>( [^\\]]*)?\\])",
                    RegexOptions.Compiled);
        }

        public static ActionResult NormalizeMovieFile(Settings localSettings, FilePath sourceFile, IProgress<double> progressReporter)
        {
            var logFile = new FilePath(sourceFile.FullName + ".log");
            CleanUpLogFile(logFile);
            var destinationFileEncodedByOpus = MakeDestinationFilePath(sourceFile, AudioEncoderType.Libopus);
            var destinationFileEncodedByVorbis = MakeDestinationFilePath(sourceFile, AudioEncoderType.Libvorbis);
            if (destinationFileEncodedByOpus.Exists || destinationFileEncodedByVorbis.Exists)
                return ActionResult.Skipped;
            var (actionResult, audioCodecIsNotSupported)
                = NormalizeMovieFile(
                    sourceFile,
                    logFile,
                    AudioEncoderType.Libopus,
                    destinationFileEncodedByOpus,
                    new Progress<double>(progress => progressReporter.Report((progress + 0) / 2)));
            if (audioCodecIsNotSupported)
            {
                CleanUpLogFile(logFile);
                ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}:INFORMATION: Failed to normalize the audio of the video with libopus, so normalize with libvorbis instead.: \"{sourceFile.FullName}\"", });
                (actionResult, audioCodecIsNotSupported) =
                    NormalizeMovieFile(
                        sourceFile,
                        logFile,
                        AudioEncoderType.Libvorbis,
                        destinationFileEncodedByVorbis,
                    new Progress<double>(progress => progressReporter.Report((progress + 1) / 2)));
            }

            return !audioCodecIsNotSupported ? actionResult : ActionResult.Failed;

            static FilePath MakeDestinationFilePath(FilePath sourceFile, AudioEncoderType audioEncoder)
                => sourceFile.Directory.GetFile($"{Path.GetFileNameWithoutExtension(sourceFile.Name)} [{audioEncoder.ToFormatName()} audio-normalized].mkv");
        }

        private static (ActionResult actionResult, bool audioCodecIsNotSupported) NormalizeMovieFile(FilePath sourceFile, FilePath logFile, AudioEncoderType audioEncoder, FilePath destinationFile, IProgress<double> progressReporter)
        {
            var workingFile = sourceFile.Directory.GetFile($".work.audio-normalize.{sourceFile.Name}.mkv");
            DeleteFileSafety(workingFile);
            var actionResult = ActionResult.Failed;
            FilePath? actualDestinationFilePath = null;
            try
            {
                if (_normalizedFileNamePattern.IsMatch(Path.GetFileNameWithoutExtension(sourceFile.Name)))
                {
                    // ファイル名に正規化されたマーク文字列があるので、何もせずに復帰する。
                    return (actionResult = ActionResult.Skipped, false);
                }

                if (destinationFile.Exists)
                {
                    // 出力先ファイルが既に存在しているので、何もせず復帰する。
                    return (actionResult = ActionResult.Skipped, false);
                }

                if (_duplicatedFileNamePattern.IsMatch(Path.GetFileNameWithoutExtension(sourceFile.Name)))
                {
                    // 入力ファイル名の末尾が " (<数字列>)" で終わっているので、ログを残した後にエラーで復帰する。
                    ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}:INFORMATION: Movie files with file names ending with \" (<digits>)\" will not be converted.: \"{sourceFile.FullName}\"", });
                    return (actionResult = ActionResult.Failed, false);
                }

                switch (ExternalCommand.NormalizeAudioFile(logFile, sourceFile, audioEncoder, workingFile, progressReporter))
                {
                    case CommandResultCode.NotSupported:
                        return (actionResult = ActionResult.Failed, true);
                    case CommandResultCode.Cancelled:
                        return (actionResult = ActionResult.Cancelled, false);
                    case CommandResultCode.Completed:
                    default:
                        break;
                }

                actualDestinationFilePath = MoveToDestinationFile(workingFile, destinationFile);
                ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}:INFORMATION: File moved from \"{workingFile.FullName}\" to \"{actualDestinationFilePath.FullName}\".", });
                return (actionResult = ActionResult.Success, false);
            }
            catch (AggregateException ex)
            {
                if (destinationFile.Exists)
                {
                    DeleteFileSafety(destinationFile);
                    ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}:INFORMATION: File was deleted: \"{destinationFile.FullName}\"", });
                }

                ExternalCommand.ReportAggregateException(logFile, ex);
                return (actionResult = ActionResult.Failed, false);
            }
            catch (Exception ex)
            {
                if (destinationFile.Exists)
                {
                    DeleteFileSafety(destinationFile);
                    ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}:INFORMATION: File was deleted: \"{destinationFile.FullName}\"", });
                }

                ExternalCommand.ReportException(logFile, ex);
                return (actionResult = ActionResult.Failed, false);
            }
            finally
            {
                if (workingFile.Exists)
                {
                    DeleteFileSafety(workingFile);
                    ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}:INFORMATION: File was deleted: \"{workingFile.FullName}\"", });
                }

                switch (actionResult)
                {
                    case ActionResult.Success:
                        ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}:INFORMATION: The audio in movie file was successfully normalized.: from \"{sourceFile.FullName}\" to \"{actualDestinationFilePath?.FullName ?? ""}\"", });
                        FinalizeLogFile(logFile, "OK");
                        break;
                    case ActionResult.Skipped:
                        CleanUpLogFile(logFile);
                        break;
                    case ActionResult.Failed:
                        ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: ERROR: Failed to normalize the audio of the movie file.: \"{sourceFile.FullName}\"", });
                        FinalizeLogFile(logFile, "NG");
                        break;
                    case ActionResult.Cancelled:
                        ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}:INFORMATION: Movie audio normalization was interrupted by the user.: \"{sourceFile.FullName}\"", });
                        break;
                    default:
                        break;
                }
            }
        }

        public static ActionResult ResizeMovieFile(Settings localSettings, FilePath sourceFile, IProgress<double> progressReporter)
        {
            var sourceFileDirectory = sourceFile.Directory;
            if (sourceFileDirectory is null)
            {
                // 親ディレクトが存在しない場合 (ディスクファイルの場合はありえない)

                // 何もせず復帰する。
                return ActionResult.Skipped;
            }

            var conversionSpec = sourceFileDirectory.Name;
            var match = _startsWithConvertModeSymbolPattern.Match(conversionSpec);
            if (match.Success)
            {
                // 親ディレクトリ名に単純変換の指示があった場合、単純変換を実行する。
                return ContertMovieFileToMatroska(localSettings, sourceFile, conversionSpec[match.Length..], progressReporter);
            }
            else
            {
                // 親ディレクトリ名に単純変換の指示がなかった場合、解像度変更を実行を試みる。
                return ChangeResolutionOfMovieFile(localSettings, sourceFile, conversionSpec, progressReporter);
            }
        }

        private static ActionResult ContertMovieFileToMatroska(Settings localSettings, FilePath sourceFile, string conversionSpec, IProgress<double> progressReporter)
        {
            var logFile = new FilePath(sourceFile.FullName + ".log");
            CleanUpLogFile(logFile);
            var (isParsedSuccessfully, resolutionSpec, aspectRatioSpec, aspectRatioSpecOnFileSystem) = ParseConversionSpecText(conversionSpec);
            if (!isParsedSuccessfully)
                return ActionResult.Skipped;
            var destinationFile =
                sourceFile.Directory.GetFile(
                    resolutionSpec is null
                        ? $"{Path.GetFileNameWithoutExtension(sourceFile.Name)}.mkv"
                        : ReplaceResolutionSpecInFileName(sourceFile.Name, resolutionSpec, aspectRatioSpecOnFileSystem, null));
            var workingFile1 = sourceFile.Directory.GetFile($".work.resize-resolution-1.{sourceFile.Name}.mkv");
            var workingFile2 = sourceFile.Directory.GetFile($".work.resize-resolution-2.{sourceFile.Name}.mkv");
            DeleteFileSafety(workingFile1);
            DeleteFileSafety(workingFile2);
            var actionResult = ActionResult.Failed;
            FilePath? actualDestinationFilePath = null;
            try
            {
                if (sourceFile.Name == destinationFile.Name)
                {
                    // 出力ファイル名が入力ファイル名と同じなので、既に変換済みとみなして、何もせず復帰する。
                    return actionResult = ActionResult.Skipped;
                }

                if (destinationFile.Exists)
                {
                    // 出力先ファイルが既に存在しているので、何もせず復帰する。
                    return actionResult = ActionResult.Skipped;
                }

                if (_duplicatedFileNamePattern.IsMatch(Path.GetFileNameWithoutExtension(sourceFile.Name)))
                {
                    // 入力ファイル名の末尾が " (<数字列>)" で終わっているので、ログを残した後にエラーで復帰する。
                    ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}:INFORMATION: Movie files with file names ending with \" (<digits>)\" will not be converted.: \"{sourceFile.FullName}\"", });
                    return actionResult = ActionResult.Failed;
                }

                var movieInfo =
                    Command.GetMovieInformation(
                        null,
                        sourceFile,
                        MovieInformationType.Streams | MovieInformationType.Chapters,
                        (level, message) =>
                            ExternalCommand.Log(
                                logFile,
                                new[] { $"{nameof(MatroskaBatchToolBox)}: {level}: {message}" }));

                if (!string.IsNullOrEmpty(resolutionSpec))
                {
                    var invalidStreams =
                        movieInfo.VideoStreams
                        .Where(stream => !stream.IsImageVideoStream && stream.Resolution != resolutionSpec)
                        .ToList();
                    if (invalidStreams.Count > 0)
                    {
                        ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: ERROR: Simple conversion is not possible because the movie file resolution ({string.Join(", ", invalidStreams.Select(stream => stream.Resolution))}) is different from the destination resolution ({resolutionSpec}).: \"{sourceFile.FullName}\"", });
                        return actionResult = ActionResult.Failed;
                    }
                }

                if (CheckingForNeedToDoMultiStepConversion(localSettings, movieInfo))
                {
                    var stepWeight1 = 1.0;
                    var stepWeight2 = 1.0;
                    var totalWeight = stepWeight1 + stepWeight2;

                    // 第1段階: メタデータの削除 (progress の重みづけ: 1)
                    var conversionResult1 =
                        ExternalCommand.ConvertMovieFile(
                            localSettings,
                            logFile,
                            sourceFile,
                            movieInfo,
                            null,
                            aspectRatioSpec,
                            VideoEncoderType.Copy,
                            true,
                            false,
                            false,
                            false,
                            false,
                            workingFile2,
                            new Progress<double>(progress => progressReporter.Report((0.0 + progress * stepWeight1) / totalWeight)));
                    if (conversionResult1 == CommandResultCode.Cancelled)
                        return actionResult = ActionResult.Cancelled;

                    // 第2段階: 最低限のメタデータ(チャプタータイトルを含む)の付加 (progress の重みづけ: 1)
                    var commandResult2 =
                        ExternalCommand.ConvertMovieFile(
                            localSettings,
                            logFile,
                            workingFile2,
                            movieInfo,
                            null,
                            null,
                            VideoEncoderType.Copy,
                            false,
                            true,
                            true,
                            true,
                            true,
                            workingFile1,
                            new Progress<double>(progress => progressReporter.Report((stepWeight1 + progress * stepWeight2) / totalWeight)));
                    if (commandResult2 == CommandResultCode.Cancelled)
                        return actionResult = ActionResult.Cancelled;
                }
                else
                {
                    var conversionResult =
                        ExternalCommand.ConvertMovieFile(
                            localSettings,
                            logFile,
                            sourceFile,
                            movieInfo,
                            null,
                            aspectRatioSpec,
                            VideoEncoderType.Copy,
                            localSettings.DeleteMetadata,
                            true,
                            true,
                            false,
                            false,
                            workingFile1,
                            progressReporter);
                    if (conversionResult == CommandResultCode.Cancelled)
                        return actionResult = ActionResult.Cancelled;
                }

                actualDestinationFilePath = MoveToDestinationFile(workingFile1, destinationFile);
                ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}:INFORMATION: File moved from \"{workingFile1.FullName}\" to \"{actualDestinationFilePath.FullName}\"." });
                return actionResult = ActionResult.Success;
            }
            catch (AggregateException ex)
            {
                if (destinationFile.Exists)
                {
                    DeleteFileSafety(destinationFile);
                    ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}:INFORMATION: File was deleted: \"{destinationFile.FullName}\"" });
                }

                ExternalCommand.ReportAggregateException(logFile, ex);
                return actionResult = ActionResult.Failed;
            }
            catch (Exception ex)
            {
                if (destinationFile.Exists)
                {
                    DeleteFileSafety(destinationFile);
                    ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}:INFORMATION: File was deleted: \"{destinationFile.FullName}\"" });
                }

                ExternalCommand.ReportException(logFile, ex);
                return actionResult = ActionResult.Failed;
            }
            finally
            {
                if (workingFile1.Exists)
                {
                    DeleteFileSafety(workingFile1);
                    ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}:INFORMATION: File was deleted: \"{workingFile1.FullName}\"", });
                }

                if (workingFile2.Exists)
                {
                    DeleteFileSafety(workingFile2);
                    ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}:INFORMATION: File was deleted: \"{workingFile2.FullName}\"", });
                }

                switch (actionResult)
                {
                    case ActionResult.Success:
                        ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}:INFORMATION: The movie file was successfully converted.: from \"{sourceFile.FullName}\" to \"{actualDestinationFilePath?.FullName ?? ""}\"", });
                        FinalizeLogFile(logFile, "OK");
                        break;
                    case ActionResult.Skipped:
                        CleanUpLogFile(logFile);
                        break;
                    case ActionResult.Failed:
                        ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: ERROR: Failed to convert the movie file.: \"{sourceFile.FullName}\"", });
                        FinalizeLogFile(logFile, "NG");
                        break;
                    case ActionResult.Cancelled:
                        ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}:INFORMATION: Movie file conversion was interrupted by the user.: \"{sourceFile.FullName}\"", });
                        break;
                    default:
                        break;
                }
            }
        }

        private static ActionResult ChangeResolutionOfMovieFile(Settings localSettings, FilePath sourceFile, string conversionSpec, IProgress<double> progressReporter)
        {
            var videoEncoder = localSettings.FfmpegVideoEncoder;
            var logFile = new FilePath(sourceFile.FullName + ".log");
            CleanUpLogFile(logFile);
            var calculateVmafScore = localSettings.CalculateVmafScore;
            if (calculateVmafScore && (localSettings.Cropping is not null || localSettings.Trimming is not null))
            {
                ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: WARNING: The \"calculate_vmaf_score\" property is set to true, but the \"cropping\" or \"trimming\" option is specified, so the VMAF score is not calculated." });
                calculateVmafScore = false;
            }

            var (isParsedSuccessfully, resolutionSpec, aspectRatioSpec, aspectRatioSpecOnFileSystem) = ParseConversionSpecText(conversionSpec);
            if (!isParsedSuccessfully || resolutionSpec is null || aspectRatioSpec is null)
            {
                // 親ディレクトの名前が解像度(およびアスペクト比の指定)ではないので、何もせず復帰する。
                return ActionResult.Skipped;
            }

            var destinationFileName = ReplaceResolutionSpecInFileName(sourceFile.Name, resolutionSpec, aspectRatioSpecOnFileSystem, $"{videoEncoder.ToFormatName()} CRF");
            var destinationFile = sourceFile.Directory.GetFile(destinationFileName);
            var workingFile1 = sourceFile.Directory.GetFile($".work.resize-resolution-1.{sourceFile.Name}.mkv");
            var workingFile2 = sourceFile.Directory.GetFile($".work.resize-resolution-2.{sourceFile.Name}.mkv");
            DeleteFileSafety(workingFile1);
            DeleteFileSafety(workingFile2);
            var actionResult = ActionResult.Failed;
            FilePath? actualDestinationFilePath = null;
            try
            {
                if (_encodedFileNamePattern.IsMatch(Path.GetFileNameWithoutExtension(sourceFile.Name)))
                {
                    // ファイル名に既にエンコードされているマーク文字列があるので、何もせずに復帰する。
                    return actionResult = ActionResult.Skipped;
                }

                if (string.Equals(destinationFileName, sourceFile.Name, StringComparison.InvariantCultureIgnoreCase))
                {
                    // 入力ファイルと出力ファイルのファイル名が一致しているので、何もせず復帰する。
                    return actionResult = ActionResult.Skipped;
                }

                if (destinationFile.Exists)
                {
                    // 出力先ファイルが既に存在しているので、何もせず復帰する。
                    return actionResult = ActionResult.Skipped;
                }

                if (_duplicatedFileNamePattern.IsMatch(Path.GetFileNameWithoutExtension(sourceFile.Name)))
                {
                    // 入力ファイル名の末尾が " (<数字列>)" で終わっているので、ログを残した後にエラーで復帰する。
                    ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: ERROR: Movie files with file names ending with \" (<digits>)\" will not be converted.: \"{sourceFile.FullName}\"", });
                    return actionResult = ActionResult.Failed;
                }

                var movieInfo =
                    Command.GetMovieInformation(
                        null,
                        sourceFile,
                        MovieInformationType.Streams | MovieInformationType.Chapters,
                        (level, message) =>
                            ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: {level}: {message}" }));

                if (CheckingForNeedToDoMultiStepConversion(localSettings, movieInfo))
                {
                    if (calculateVmafScore)
                    {
                        var stepWeight1 = 1.0;
                        var stepWeight2 = 0.1;
                        var stepWeight3 = 1.0;
                        var totalWeight = stepWeight1 + stepWeight2 + stepWeight3;

                        // 第1段階: エンコードとメタデータの削除 (progress の重みづけ: 1)
                        var commandResult1 =
                            ExternalCommand.ConvertMovieFile(
                                localSettings,
                                logFile,
                                sourceFile,
                                movieInfo,
                                resolutionSpec,
                                aspectRatioSpec,
                                videoEncoder,
                                true,
                                false,
                                false,
                                false,
                                false,
                                workingFile2,
                                new Progress<double>(progress => progressReporter.Report((0.0 + progress * stepWeight1) / totalWeight)));
                        if (commandResult1 == CommandResultCode.Cancelled)
                            return actionResult = ActionResult.Cancelled;

                        // 第2段階: ストリームの単純コピーと最低限のメタデータ(チャプタータイトル含む)の付加 (progress の重みづけ: 0.1)
                        var commandResult2 =
                            ExternalCommand.ConvertMovieFile(
                                localSettings,
                                logFile,
                                workingFile2,
                                movieInfo,
                                null,
                                null,
                                VideoEncoderType.Copy,
                                false,
                                true,
                                true,
                                true,
                                true,
                                workingFile1,
                                new Progress<double>(progress => progressReporter.Report((stepWeight1 + progress * stepWeight2) / totalWeight)));
                        if (commandResult2 == CommandResultCode.Cancelled)
                            return actionResult = ActionResult.Cancelled;

                        // 第3段階: VMAFスコアの計算 (progress の重みづけ: 1)
                        var commandResult3 =
                            ExternalCommand.CalculateVmafScoreFromMovieFile(
                                logFile,
                                sourceFile,
                                workingFile1,
                                resolutionSpec,
                                out double vmafScore,
                                new Progress<double>(progress => progressReporter.Report((stepWeight1 + stepWeight2 + progress * stepWeight3) / totalWeight)));
                        if (commandResult3 == CommandResultCode.Cancelled)
                            return actionResult = ActionResult.Cancelled;

                        ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}:INFORMATION: VMAF Score: {vmafScore:F6}" });
                    }
                    else
                    {
                        var stepWeight1 = 1.0;
                        var stepWeight2 = 0.1;
                        var totalWeight = stepWeight1 + stepWeight2;

                        // 第1段階: エンコードとメタデータの削除 (progress の重みづけ: 1)
                        var commandResult1 =
                            ExternalCommand.ConvertMovieFile(
                                localSettings,
                                logFile,
                                sourceFile,
                                movieInfo,
                                resolutionSpec,
                                aspectRatioSpec,
                                videoEncoder,
                                true,
                                false,
                                false,
                                false,
                                false,
                                workingFile2,
                                new Progress<double>(progress => progressReporter.Report((0.0 + progress * stepWeight1) / totalWeight)));
                        if (commandResult1 == CommandResultCode.Cancelled)
                            return actionResult = ActionResult.Cancelled;

                        // 第2段階: ストリームの単純コピーと最低限のメタデータ(チャプタータイトル含む)の付加 (progress の重みづけ: 0.1)
                        var commandResult2 =
                            ExternalCommand.ConvertMovieFile(
                                localSettings,
                                logFile,
                                workingFile2,
                                movieInfo,
                                null,
                                null,
                                VideoEncoderType.Copy,
                                false,
                                true,
                                true,
                                true,
                                true,
                                workingFile1,
                                new Progress<double>(progress => progressReporter.Report((stepWeight1 + progress * stepWeight2) / totalWeight)));
                        if (commandResult2 == CommandResultCode.Cancelled)
                            return actionResult = ActionResult.Cancelled;
                    }
                }
                else
                {
                    // このルートでは以下の理由によりチャプタータイトルの再設定はしない。
                    //   a) チャプターそのものを削除する指定がされている、または
                    //   b) メタデータを削除する指定がされていない、または
                    //   c) チャプタータイトルを保持しない指定がされている、または
                    //   d) チャプタータイトルが元々全く存在しない。
                    if (calculateVmafScore)
                    {
                        var stepWeight1 = 1.0;
                        var stepWeight2 = 1.0;
                        var totalWeight = stepWeight1 + stepWeight2;

                        // 第1段階: エンコード (progress の重みづけ: 1.0)
                        var commandResult1 =
                            ExternalCommand.ConvertMovieFile(
                                localSettings,
                                logFile,
                                sourceFile,
                                movieInfo,
                                resolutionSpec,
                                aspectRatioSpec,
                                videoEncoder,
                                localSettings.DeleteMetadata,
                                true,
                                true,
                                false,
                                false,
                                workingFile1,
                                new Progress<double>(progress => progressReporter.Report((0.0 + progress) / totalWeight)));
                        if (commandResult1 == CommandResultCode.Cancelled)
                            return actionResult = ActionResult.Cancelled;

                        // 第2段階: VMAFスコアの計算 (progress の重みづけ: 1)
                        var commandResult2 =
                            ExternalCommand.CalculateVmafScoreFromMovieFile(
                                logFile,
                                sourceFile,
                                workingFile1,
                                resolutionSpec,
                                out double vmafScore,
                                new Progress<double>(progress => progressReporter.Report((stepWeight1 + progress) / totalWeight)));
                        if (commandResult2 == CommandResultCode.Cancelled)
                            return actionResult = ActionResult.Cancelled;
                        ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}:INFORMATION: VMAF Score: {vmafScore:F6}" });
                    }
                    else
                    {
                        var commandResult1 =
                            ExternalCommand.ConvertMovieFile(
                                localSettings,
                                logFile,
                                sourceFile,
                                movieInfo,
                                resolutionSpec,
                                aspectRatioSpec,
                                videoEncoder,
                                localSettings.DeleteMetadata,
                                true,
                                true,
                                false,
                                false,
                                workingFile1,
                                progressReporter);
                        if (commandResult1 == CommandResultCode.Cancelled)
                            return actionResult = ActionResult.Cancelled;
                    }
                }

                actualDestinationFilePath = MoveToDestinationFile(workingFile1, destinationFile);
                ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}:INFORMATION: File moved from \"{workingFile1.FullName}\" to \"{actualDestinationFilePath.FullName}\"." });
                return actionResult = ActionResult.Success;
            }
            catch (AggregateException ex)
            {
                if (destinationFile.Exists)
                {
                    DeleteFileSafety(destinationFile);
                    ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}:INFORMATION: File was deleted: \"{destinationFile.FullName}\"" });
                }

                ExternalCommand.ReportAggregateException(logFile, ex);
                return actionResult = ActionResult.Failed;
            }
            catch (Exception ex)
            {
                if (destinationFile.Exists)
                {
                    DeleteFileSafety(destinationFile);
                    ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}:INFORMATION: File was deleted: \"{destinationFile.FullName}\"" });
                }

                ExternalCommand.ReportException(logFile, ex);
                return actionResult = ActionResult.Failed;
            }
            finally
            {
                if (workingFile1.Exists)
                {
                    DeleteFileSafety(workingFile1);
                    ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}:INFORMATION: File was deleted: \"{workingFile1.FullName}\"", });
                }

                if (workingFile2.Exists)
                {
                    DeleteFileSafety(workingFile2);
                    ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}:INFORMATION: File was deleted: \"{workingFile2.FullName}\"", });
                }

                switch (actionResult)
                {
                    case ActionResult.Success:
                        ExternalCommand.Log(
                            logFile,
                            new[]
                            {
                                $"{nameof(MatroskaBatchToolBox)}:INFORMATION: The video resolution of the movie fie was successfully changed.: from \"{sourceFile.FullName}\" to \"{actualDestinationFilePath?.FullName ?? "<???>"}\"",
                                $"{nameof(MatroskaBatchToolBox)}:INFORMATION: Source file: \"{sourceFile.FullName}\"",
                                $"{nameof(MatroskaBatchToolBox)}:INFORMATION: Source file size: {sourceFile.Length:N0}[bytes]",
                                $"{nameof(MatroskaBatchToolBox)}:INFORMATION: Destination file: \"{actualDestinationFilePath?.FullName ?? "<???>"}\"",
                                $"{nameof(MatroskaBatchToolBox)}:INFORMATION: Destination file size: {actualDestinationFilePath?.Length ?? 0:N0}[bytes]",
                                $"{nameof(MatroskaBatchToolBox)}:INFORMATION: Compression ratio (<Destination file size> / <Source file size>): {(actualDestinationFilePath is not null ? (100.0 * actualDestinationFilePath.Length / sourceFile.Length).ToString("F2") : "<???>" )}%",
                            });
                        FinalizeLogFile(logFile, "OK");
                        break;
                    case ActionResult.Skipped:
                        CleanUpLogFile(logFile);
                        break;
                    case ActionResult.Failed:
                        ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: ERROR: Failed to change the video resolution of the movie file.: \"{sourceFile.FullName}\"", });
                        FinalizeLogFile(logFile, "NG");
                        break;
                    case ActionResult.Cancelled:
                        ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}:INFORMATION: Changing the video resolution of the movie file was interrupted by the user.: \"{sourceFile.FullName}\"", });
                        break;
                    default:
                        break;
                }
            }
        }

        private static (bool success, string? resolutionSpec, string? aspectRatioSpec, string? aspectRatioSpecOnFileSystem) ParseConversionSpecText(string conversionSpec)
        {
            var resolutionSpecMatch = _startsWithResolutionSpecPattern.Match(conversionSpec);
            if (conversionSpec.Length <= 0)
                return (true, null, null, null);
            if (!resolutionSpecMatch.Success)
            {
                // 解像度指定がない場合(構文エラー)
                return (false, null, null, null);
            }

            // 解像度指定がある場合

            var resolutionWidth = resolutionSpecMatch.Groups["resolutionWidth"].Value.ParseAsInt32();
            var resolutionHeight = resolutionSpecMatch.Groups["resolutionHeight"].Value.ParseAsInt32();
            conversionSpec = conversionSpec[resolutionSpecMatch.Length..];
            if (conversionSpec.Length <= 0 || _startsWithCommentPattern.IsMatch(conversionSpec))
            {
                // 解像度指定で終わっている、または次がコメントである場合

                // 解像度から整数比を求めてそれをアスペクト比とする。
                var gcd = resolutionWidth.GreatestCommonDivisor(resolutionHeight);
                var aspectRatioWidth = resolutionWidth / gcd;
                var aspectRatioHeight = resolutionHeight / gcd;
                return (true, $"{resolutionWidth}x{resolutionHeight}", $"{aspectRatioWidth}:{aspectRatioHeight}", null);
            }

            // 解像度指定の後に続きがある場合
            var separaterMatch = _startsWithSeparaterBetweenResolutionSpecAndAspectRatioPattern.Match(conversionSpec);
            if (!separaterMatch.Success)
            {
                // 解像度指定の後がセパレータではない場合(構文ミス)

                return (false, null, null, null);
            }

            // 解像度指定の後にセパレータがある場合
            conversionSpec = conversionSpec[separaterMatch.Length..];
            var matchAspectRatioSpecMatch = _startsWithAspectRatioSpecPattern.Match(conversionSpec);
            if (!matchAspectRatioSpecMatch.Success)
            {
                // セパレータの後がアスペクト比ではない場合(構文ミス)

                return (false, null, null, null);
            }
            // セパレータの後がアスペクト比である場合
            conversionSpec = conversionSpec[matchAspectRatioSpecMatch.Length..];
            if (conversionSpec.Length > 0 && !_startsWithCommentPattern.IsMatch(conversionSpec))
            {
                // アスペクト比の後にコメント以外の続きがある場合(構文ミス)

                return (false, null, null, null);
            }
            // アスペクト比で終わっている場合
            if (matchAspectRatioSpecMatch.Groups["aspectRatioWidth"].Success &&
                matchAspectRatioSpecMatch.Groups["aspectRatioSeparater"].Success &&
                matchAspectRatioSpecMatch.Groups["aspectRatioHeight"].Success)
            {
                // アスペクト比が整数比で表現されている場合

                var aspectRatioWidth = matchAspectRatioSpecMatch.Groups["aspectRatioWidth"].Value.ParseAsInt32();
                var aspectRatioHeight = matchAspectRatioSpecMatch.Groups["aspectRatioHeight"].Value.ParseAsInt32();

                return (true, $"{resolutionWidth}x{resolutionHeight}", $"{aspectRatioWidth}:{aspectRatioHeight}", $"{aspectRatioWidth}{matchAspectRatioSpecMatch.Groups["aspectRatioSeparater"].Value}{aspectRatioHeight}");
            }
            else if (matchAspectRatioSpecMatch.Groups["aspectRatioValue"].Success)
            {
                // アスペクト比が実数値で表現されている場合

                if (!matchAspectRatioSpecMatch.Groups["aspectRatioValue"].Value.TryParse(out double aspectRatioValue) || aspectRatioValue <= 0)
                {
                    // 数値の書式が正しくないかまたは正ではない場合(構文ミス)
                    // アスペクト比として0を指定した場合のみここに到達する

                    return (false, null, null, null);
                }

                return (true, $"{resolutionWidth}x{resolutionHeight}", (string?)matchAspectRatioSpecMatch.Groups["aspectRatioValue"].Value, (string?)matchAspectRatioSpecMatch.Groups["aspectRatioValue"].Value);
            }
            else
            {
                throw Validation.GetFailErrorException("_startsWithAspectRatioSpecPattern.Match(conversionSpec).Success == true なので、このルートには到達しないはず");
            }
        }

        private static string ReplaceResolutionSpecInFileName(string sourceFileName, string resolutionSpec, string? aspectRatioSpecOnFileSystem, string? encoderDescription)
        {
            var resolutionAndAspectRatioSpec = aspectRatioSpecOnFileSystem is null ? resolutionSpec : $"{resolutionSpec} {aspectRatioSpecOnFileSystem}";
            if (string.IsNullOrEmpty(resolutionAndAspectRatioSpec))
            {
                // 解像度とアスペクト比が共に指定されていない場合

                return
                    encoderDescription is null
                    ? $"{Path.GetFileNameWithoutExtension(sourceFileName)}.mkv"
                    : $"{Path.GetFileNameWithoutExtension(sourceFileName)} [{encoderDescription}].mkv";
            }

            var matches = _resolutionAndAspectRatioSpecInFileNamePattern.Matches(sourceFileName);
            if (matches.Count == 1)
            {
                // 入力元ファイル名に解像度・アスペクト比指定がただ一つだけある場合

                // 入力ファイル名のの解像度・アスペクト比指定を変換先の解像度・アスペクト比指定に置き換える
                var replacedFileName =
                    _resolutionAndAspectRatioSpecInFileNamePattern.Replace(
                        Path.GetFileNameWithoutExtension(sourceFileName),
                        match =>
                        {
                            var prefix = match.Groups["prefix"].Value;
                            var suffix = match.Groups["suffix"].Value;
                            return prefix + resolutionAndAspectRatioSpec + suffix;
                        });
                return
                    encoderDescription is null
                    ? $"{replacedFileName}.mkv"
                    : $"{replacedFileName} [{encoderDescription}].mkv";

            }
            else if (matches.Count > 1)
            {
                // 入力元ファイルの名前に解像度指定が複数ある場合
                if (matches.Any(match => match.Groups["resolutionAndAspectRatioSpec"].Value == resolutionAndAspectRatioSpec))
                {
                    // 入力ファイル名の解像度指定の中に、変換先の解像度指定に一致するものが一つでもある場合

                    // 入力ファイル名に解像度指定を新たに付加せずに返す。
                    return
                        encoderDescription is null
                        ? $"{Path.GetFileNameWithoutExtension(sourceFileName)}.mkv"
                        : $"{Path.GetFileNameWithoutExtension(sourceFileName)} [{encoderDescription}].mkv";

                }
                else
                {
                    // 入力ファイル名の解像度指定の中に、変換先の解像度指定に一致するものが一つもない場合

                    // ファイル名の解像度指定の置換を行うと利用者が意図しない問題が発生する可能性があるため、新たな解像度指定をファイル名の末尾に付加するに留める。
                    return
                        encoderDescription is null
                        ? $"{Path.GetFileNameWithoutExtension(sourceFileName)} [{resolutionAndAspectRatioSpec}].mkv"
                        : $"{Path.GetFileNameWithoutExtension(sourceFileName)} [{encoderDescription} {resolutionAndAspectRatioSpec}].mkv";
                }
            }
            else
            {
                // 入力元ファイルの名前に解像度指定が一つもない場合

                // 解像度指定をファイル名の末尾に付加する。
                return
                    encoderDescription is null
                    ? $"{Path.GetFileNameWithoutExtension(sourceFileName)} [{resolutionAndAspectRatioSpec}].mkv"
                    : $"{Path.GetFileNameWithoutExtension(sourceFileName)} [{encoderDescription} {resolutionAndAspectRatioSpec}].mkv";
            }
        }

        private static bool CheckingForNeedToDoMultiStepConversion(Settings localSettings, MovieInformation movieInfo)
            // ffmpeg の仕様?により、メタデータファイルと "-map_chapters 1" が指定されていているにもかかわらず、"-map_metadata -1" を指定するとチャプターのタイトルが設定されない。
            // そのため、上記の条件に該当する場合は、メタデータの削除とチャプターの設定を2段階に分けて別々に行う。
            // 具体的な条件は以下の通り。
            //   a) チャプターの削除指定がされておらず、かつ
            //   b) メタデータの削除指定がされており、かつ
            //   c) チャプタータイトルを保持する指定がされており、かつ
            //   d) 削除するには惜しいタイトルを持つチャプターが1つ以上存在する場合。
            => !localSettings.DeleteChapters &&
                localSettings.DeleteMetadata &&
                localSettings.KeepChapterTitles &&
                movieInfo.Chapters.Any(chapter => chapter.HasUniqueChapterTitle);

        private static FilePath MoveToDestinationFile(FilePath sourceFile, FilePath destinationFile)
        {
            var destinationFileNameWithoutExtension = Path.GetFileNameWithoutExtension(destinationFile.Name);
            var destinationFileExtension = Path.GetExtension(destinationFile.Name);
            var count = 1;
            while (true)
            {
                try
                {
                    var actualDestinationFile
                        = destinationFile.Directory.GetFile($"{destinationFileNameWithoutExtension}{(count <= 1 ? "" : $" ({count})")}{destinationFileExtension}");
                    if (actualDestinationFile.Exists)
                    {
                        sourceFile.MoveTo(actualDestinationFile);
                        return new FilePath(actualDestinationFile.FullName);
                    }
                }
                catch (FileNotFoundException ex)
                {
                    throw new Exception("The source file was lost for some reason.", ex);
                }
                catch (IOException)
                {
                }

                ++count;
            }
        }

        private static void CleanUpLogFile(FilePath logFile)
        {
            try
            {
                DeleteFileSafety(logFile);
                DeleteFileSafety(ConstructLogFilePath(logFile, "OK"));
                DeleteFileSafety(ConstructLogFilePath(logFile, "NG"));
            }
            catch (Exception)
            {
            }
        }

        private static void DeleteFileSafety(FilePath targetFilePath)
        {
            try
            {
                if (targetFilePath.Exists)
                    targetFilePath.Delete();
            }
            catch (IOException)
            {
            }
        }

        private static void FinalizeLogFile(FilePath logFile, string result)
            => logFile.MoveTo(ConstructLogFilePath(logFile, result), true);

        private static FilePath ConstructLogFilePath(FilePath logFile, string result)
            => logFile.Directory.GetFile($"{Path.GetFileNameWithoutExtension(logFile.Name)}.{result}{logFile.Extension}");
    }
}
