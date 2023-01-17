using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MatroskaBatchToolBox
{
    internal static class MatroskaAction
    {
        private const string _convertModeSymbolPatternText = @"(?<convertMode>==)";
        private const string _resolutionPatternText = @"(?<resolutionWidth>\d+)x(?<resolutionHeight>\d+)";
        private const string _separaterBetweenResolutionSpecAndAspectRatePatternText = @" +|\-|_";
        private const string _aspectRatePatternText = @"((?<aspectRateWidth>\d+)(?<aspectRateSeparater>to|：|:)(?<aspectRateHeight>\d+))|(?<aspectRateValue>\d+\.\d+)";
        private static readonly Regex _duplicatedFileNamePattern;
        private static readonly Regex _normalizedFileNamePattern;
        private static readonly Regex _encodedFileNamePattern;
        private static readonly Regex _startsWithConvertModeSymbolPattern;
        private static readonly Regex _startsWithResolutionSpecPattern;
        private static readonly Regex _startsWithSeparaterBetweenResolutionSpecAndAspectRatePattern;
        private static readonly Regex _startsWithAspectRateSpecPattern;
        private static readonly Regex _resolutionAndAspectRateSpecInFileNamePattern;

        static MatroskaAction()
        {
            _duplicatedFileNamePattern = new Regex(@" \(\d+\)$", RegexOptions.Compiled);
            _normalizedFileNamePattern = new Regex(@"(?<prefix>\[([^\]]*? )?)audio-normalized(?<suffix>( [^\]]*)?\])", RegexOptions.Compiled);
            _encodedFileNamePattern = new Regex(@"(?<prefix>\[([^\]]*? )?)(AV1|x265) CRF(?<suffix>( [^\]]*)?\])", RegexOptions.Compiled);
            _startsWithConvertModeSymbolPattern = new Regex($"^({_convertModeSymbolPatternText})", RegexOptions.Compiled);
            _startsWithResolutionSpecPattern = new Regex($"^({_resolutionPatternText})", RegexOptions.Compiled);
            _startsWithSeparaterBetweenResolutionSpecAndAspectRatePattern = new Regex($"^({_separaterBetweenResolutionSpecAndAspectRatePatternText})", RegexOptions.Compiled);
            _startsWithAspectRateSpecPattern = new Regex($"^({_aspectRatePatternText})", RegexOptions.Compiled);
            var groupNamePattern = new Regex(@"\?<[^>]+>");
            var resolutionPatternTextWithoutGroup = groupNamePattern.Replace(_resolutionPatternText, "");
            var aspectRatePatternTextWithoutGroup = groupNamePattern.Replace(_aspectRatePatternText, "");
            _resolutionAndAspectRateSpecInFileNamePattern =
                new Regex(
                    $"(?<prefix>\\[([^\\]]*? )?)(?<resolutionAndAspectRateSpec>({resolutionPatternTextWithoutGroup})( ({aspectRatePatternTextWithoutGroup}))?)(?<suffix>( [^\\]]*)?\\])",
                    RegexOptions.Compiled);
        }

        public static ActionResult NormalizeMovieFile(FileInfo sourceFile, IProgress<double> progressReporter)
        {
            var logFile = new FileInfo(sourceFile.FullName + ".log");
            CleanUpLogFile(logFile);
            var destinationFileEncodedByOpus = MakeDestinationFilePath(sourceFile,  AudioEncoderType.Libopus);
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
                ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: Failed to normalize the audio of the video with libopus, so normalize with libvorbis instead.: \"{sourceFile.FullName}\"", });
                (actionResult, audioCodecIsNotSupported) =
                    NormalizeMovieFile(
                        sourceFile,
                        logFile,
                        AudioEncoderType.Libvorbis,
                        destinationFileEncodedByVorbis,
                    new Progress<double>(progress => progressReporter.Report((progress + 1) / 2)));
            }
            if (audioCodecIsNotSupported)
                return ActionResult.Failed;
            return actionResult;

            static FileInfo MakeDestinationFilePath(FileInfo sourceFile, AudioEncoderType audioEncoder)
            {
                return
                    new FileInfo(
                        Path.Combine(
                            sourceFile.DirectoryName ?? ".",
                            $"{Path.GetFileNameWithoutExtension(sourceFile.Name)} [{audioEncoder.ToFormatName()} audio-normalized].mkv"));
            }
        }

        private static (ActionResult actionResult, bool audioCodecIsNotSupported) NormalizeMovieFile(FileInfo sourceFile, FileInfo logFile, AudioEncoderType audioEncoder, FileInfo destinationFile, IProgress<double> progressReporter)
        {
            var workingFile =
                new FileInfo(
                    Path.Combine(sourceFile.DirectoryName ?? ".",
                    $".work.audio-normalize.{sourceFile.Name}"));
            DeleteFileSafety(workingFile);
            var actionResult = ActionResult.Failed;
            FileInfo? actualDestinationFilePath = null;
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
                    ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: Movie files with file names ending with \" (<digits>)\" will not be converted.: \"{sourceFile.FullName}\"", });
                    return (actionResult = ActionResult.Failed, false);
                }
                switch (ExternalCommand.NormalizeAudioFile(logFile, sourceFile, audioEncoder, workingFile, progressReporter))
                {
                    case ExternalCommand.ExternalCommandResult.NotSupported:
                        return (actionResult = ActionResult.Failed, true);
                    case ExternalCommand.ExternalCommandResult.Cancelled:
                        return (actionResult = ActionResult.Cancelled, false);
                    case ExternalCommand.ExternalCommandResult.Completed:
                    default:
                        break;
                }
                actualDestinationFilePath = MoveToDestinationFile(workingFile, destinationFile);
                ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: File moved from \"{workingFile.FullName}\" to \"{actualDestinationFilePath.FullName}\".", });
                return (actionResult = ActionResult.Success, false);
            }
            catch (AggregateException ex)
            {
                if (destinationFile.Exists)
                {
                    DeleteFileSafety(destinationFile);
                    ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: File was deleted: \"{destinationFile.FullName}\"", });
                }
                ExternalCommand.ReportAggregateException(logFile, ex);
                return (actionResult = ActionResult.Failed, false);
            }
            catch (Exception ex)
            {
                if (destinationFile.Exists)
                {
                    DeleteFileSafety(destinationFile);
                    ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: File was deleted: \"{destinationFile.FullName}\"", });
                }
                ExternalCommand.ReportException(logFile, ex);
                return (actionResult = ActionResult.Failed, false);
            }
            finally
            {
                if (workingFile.Exists)
                {
                    DeleteFileSafety(workingFile);
                    ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: File was deleted: \"{workingFile.FullName}\"", });
                }
                switch (actionResult)
                {
                    case ActionResult.Success:
                        ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: The audio in movie file was successfully normalized.: from \"{sourceFile.FullName}\" to \"{actualDestinationFilePath?.FullName ?? ""}\"", });
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
                        ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: Movie audio normalization was interrupted by the user.: \"{sourceFile.FullName}\"", });
                        break;
                    default:
                        break;
                }
            }
        }

        public static ActionResult ResizeMovieFile(FileInfo sourceFile, IProgress<double> progressReporter)
        {
            var conversionSpec = sourceFile.Directory?.Name ?? "";
            var match = _startsWithConvertModeSymbolPattern.Match(conversionSpec);
            if (match.Success)
            {
                // 親ディレクトリ名に単純変換の指示があった場合、単純変換を実行する。
                return ContertMovieFileToMatroska(sourceFile, conversionSpec[match.Length..], progressReporter);
            }
            else
            {
                // 親ディレクトリ名に単純変換の指示がなかった場合、解像度変更を実行を試みる。
                return ChangeResolutionOfMovieFile(sourceFile, conversionSpec, progressReporter);
            }
        }

        private static ActionResult ContertMovieFileToMatroska(FileInfo sourceFile, string conversionSpec, IProgress<double> progressReporter)
        {
            var logFile = new FileInfo(sourceFile.FullName + ".log");
            CleanUpLogFile(logFile);
            var (isParsedSuccessfully, resolutionSpec, aspectRateSpec, aspectRateSpecOnFileSystem) = ParseConversionSpecText(conversionSpec);
            if (!isParsedSuccessfully)
                return ActionResult.Skipped;
            var destinationFile =
                new FileInfo(
                    Path.Combine(
                        sourceFile.DirectoryName ?? ".",
                        resolutionSpec is null
                            ? $"{Path.GetFileNameWithoutExtension(sourceFile.Name)}.mkv"
                            : ReplaceResolutionSpecInFileName(sourceFile.Name, resolutionSpec, aspectRateSpecOnFileSystem, null)));
            var workingFile =
                new FileInfo(
                    Path.Combine(sourceFile.DirectoryName ?? ".",
                    $".work.resize-resolution.{sourceFile.Name}"));
            DeleteFileSafety(workingFile);
            var actionResult = ActionResult.Failed;
            FileInfo? actualDestinationFilePath = null;
            try
            {
                if (string.Equals(sourceFile.Name, destinationFile.Name, StringComparison.InvariantCultureIgnoreCase))
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
                    ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: Movie files with file names ending with \" (<digits>)\" will not be converted.: \"{sourceFile.FullName}\"", });
                    return actionResult = ActionResult.Failed;
                }
                if (ExternalCommand.ConvertMovieFile(logFile, sourceFile, aspectRateSpec, workingFile, progressReporter) == ExternalCommand.ExternalCommandResult.Cancelled)
                    return actionResult = ActionResult.Cancelled;
                actualDestinationFilePath = MoveToDestinationFile(workingFile, destinationFile);
                ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: File moved from \"{workingFile.FullName}\" to \"{actualDestinationFilePath.FullName}\"." });
                return actionResult = ActionResult.Success;
            }
            catch (AggregateException ex)
            {
                if (destinationFile.Exists)
                {
                    DeleteFileSafety(destinationFile);
                    ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: File was deleted: \"{destinationFile.FullName}\"" });
                }
                ExternalCommand.ReportAggregateException(logFile, ex);
                return actionResult = ActionResult.Failed;
            }
            catch (Exception ex)
            {
                if (destinationFile.Exists)
                {
                    DeleteFileSafety(destinationFile);
                    ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: File was deleted: \"{destinationFile.FullName}\"" });
                }
                ExternalCommand.ReportException(logFile, ex);
                return actionResult = ActionResult.Failed;
            }
            finally
            {
                if (workingFile.Exists)
                {
                    DeleteFileSafety(workingFile);
                    ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: File was deleted: \"{workingFile.FullName}\"", });
                }
                switch (actionResult)
                {
                    case ActionResult.Success:
                        ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: The movie file was successfully converted.: from \"{sourceFile.FullName}\" to \"{actualDestinationFilePath?.FullName ?? ""}\"", });
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
                        ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: Movie file conversion was interrupted by the user.: \"{sourceFile.FullName}\"", });
                        break;
                    default:
                        break;
                }
            }
        }

        private static ActionResult ChangeResolutionOfMovieFile(FileInfo sourceFile, string conversionSpec, IProgress<double> progressReporter)
        {
            var videoEncoder = Settings.CurrentSettings.VideoEncoderOnComplexConversion;
            var calculateVMAFScore = Settings.CurrentSettings.CalculateVMAFScore;
            var logFile = new FileInfo(sourceFile.FullName + ".log");
            CleanUpLogFile(logFile);
            var (isParsedSuccessfully, resolutionSpec, aspectRateSpec, aspectRateSpecOnFileSystem) = ParseConversionSpecText(conversionSpec);
            if (!isParsedSuccessfully || resolutionSpec is null || aspectRateSpec is null)
            {
                // 親ディレクトの名前が解像度(およびアスペクト比の指定)ではないので、何もせず復帰する。
                return ActionResult.Skipped;
            }

            var destinationFileName = ReplaceResolutionSpecInFileName(sourceFile.Name, resolutionSpec, aspectRateSpecOnFileSystem, $"{videoEncoder.ToFormatName()} CRF");
            var destinationFile = new FileInfo(Path.Combine(sourceFile.DirectoryName ?? ".", destinationFileName));
            var workingFile =
                new FileInfo(
                    Path.Combine(sourceFile.DirectoryName ?? ".",
                    $".work.resize-resolution.{sourceFile.Name}"));
            DeleteFileSafety(workingFile);
            var actionResult = ActionResult.Failed;
            FileInfo? actualDestinationFilePath = null;
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
                    ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: Movie files with file names ending with \" (<digits>)\" will not be converted.: \"{sourceFile.FullName}\"", });
                    return actionResult = ActionResult.Failed;
                }
                if (calculateVMAFScore)
                {
                    if (ExternalCommand.ResizeMovieFile(logFile, sourceFile, resolutionSpec, aspectRateSpec, videoEncoder, workingFile, new Progress<double>(progress => progressReporter.Report(progress / 2))) == ExternalCommand.ExternalCommandResult.Cancelled)
                        return actionResult = ActionResult.Cancelled;
                    if (ExternalCommand.CalculateVMAFScoreFromMovieFile(logFile, sourceFile, workingFile, resolutionSpec, out double vmafScore, new Progress<double>(progress => progressReporter.Report((1 + progress) / 2))) == ExternalCommand.ExternalCommandResult.Cancelled)
                        return actionResult = ActionResult.Cancelled;
                    ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: VMAF Score: {vmafScore:F6}" });
                }
                else
                {
                    if (ExternalCommand.ResizeMovieFile(logFile, sourceFile, resolutionSpec, aspectRateSpec, videoEncoder, workingFile, progressReporter) == ExternalCommand.ExternalCommandResult.Cancelled)
                        return actionResult = ActionResult.Cancelled;
                }
                actualDestinationFilePath = MoveToDestinationFile(workingFile, destinationFile);
                ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: File moved from \"{workingFile.FullName}\" to \"{actualDestinationFilePath.FullName}\"." });
                return actionResult = ActionResult.Success;
            }
            catch (AggregateException ex)
            {
                if (destinationFile.Exists)
                {
                    DeleteFileSafety(destinationFile);
                    ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: File was deleted: \"{destinationFile.FullName}\"" });
                }
                ExternalCommand.ReportAggregateException(logFile, ex);
                return actionResult = ActionResult.Failed;
            }
            catch (Exception ex)
            {
                if (destinationFile.Exists)
                {
                    DeleteFileSafety(destinationFile);
                    ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: File was deleted: \"{destinationFile.FullName}\"" });
                }
                ExternalCommand.ReportException(logFile, ex);
                return actionResult = ActionResult.Failed;
            }
            finally
            {
                if (workingFile.Exists)
                {
                    DeleteFileSafety(workingFile);
                    ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: File was deleted: \"{workingFile.FullName}\"", });
                }
                switch (actionResult)
                {
                    case ActionResult.Success:
                        ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: The video resolution of the movie fie was successfully changed.: from \"{sourceFile.FullName}\" to \"{actualDestinationFilePath?.FullName ?? ""}\"", });
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
                        ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: Changing the video resolution of the movie file was interrupted by the user.: \"{sourceFile.FullName}\"", });
                        break;
                    default:
                        break;
                }
            }
        }

        private static (bool success, string? resolutionSpec, string? aspectRateSpec, string? aspectRateSpecOnFileSystem) ParseConversionSpecText(string conversionSpec)
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

            var resolutionWidth = int.Parse(resolutionSpecMatch.Groups["resolutionWidth"].Value, NumberStyles.None, CultureInfo.InvariantCulture.NumberFormat);
            var resolutionHeight = int.Parse(resolutionSpecMatch.Groups["resolutionHeight"].Value, NumberStyles.None, CultureInfo.InvariantCulture.NumberFormat);
            conversionSpec = conversionSpec[resolutionSpecMatch.Length..];
            if (conversionSpec.Length <= 0)
            {
                // 解像度指定で終わっている場合

                // 解像度から整数比を求めてそれをアスペクト比とする。
                var gcd = ExtendedMath.GreatestCommonDivisor(resolutionWidth, resolutionHeight);
                var aspectRateWidth = resolutionWidth / gcd;
                var aspectRateHeight = resolutionHeight / gcd;
                return (true, $"{resolutionWidth}x{resolutionHeight}", $"{aspectRateWidth}:{aspectRateHeight}", null);
            }

            // 解像度指定の後に続きがある場合
            var separaterMatch = _startsWithSeparaterBetweenResolutionSpecAndAspectRatePattern.Match(conversionSpec);
            if (!separaterMatch.Success)
            {
                // 解像度指定の後がセパレータではない場合(構文ミス)

                return (false, null, null, null);
            }

            // 解像度指定の後にセパレータがある場合
            conversionSpec = conversionSpec[separaterMatch.Length..];
            var matchAspectRateSpecMatch = _startsWithAspectRateSpecPattern.Match(conversionSpec);
            if (!matchAspectRateSpecMatch.Success)
            {
                // セパレータの後がアスペクト比ではない場合(構文ミス)

                return (false, null, null, null);
            }
            // セパレータの後がアスペクト比である場合
            conversionSpec = conversionSpec[matchAspectRateSpecMatch.Length..];
            if (conversionSpec.Length > 0)
            {
                // アスペクト比の後に続きがある場合(構文ミス)

                return (false, null, null, null);
            }
            // アスペクト比で終わっている場合
            if (matchAspectRateSpecMatch.Groups["aspectRateWidth"].Success &&
                matchAspectRateSpecMatch.Groups["aspectRateSeparater"].Success &&
                matchAspectRateSpecMatch.Groups["aspectRateHeight"].Success)
            {
                // アスペクト比が整数比で表現されている場合

                var aspectRateWidth = int.Parse(matchAspectRateSpecMatch.Groups["aspectRateWidth"].Value, NumberStyles.None, CultureInfo.InvariantCulture.NumberFormat);
                var aspectRateHeight = int.Parse(matchAspectRateSpecMatch.Groups["aspectRateHeight"].Value, NumberStyles.None, CultureInfo.InvariantCulture.NumberFormat);

                return (true, $"{resolutionWidth}x{resolutionHeight}", $"{aspectRateWidth}:{aspectRateHeight}", $"{aspectRateWidth}{matchAspectRateSpecMatch.Groups["aspectRateSeparater"].Value}{aspectRateHeight}");
            }
            else if (matchAspectRateSpecMatch.Groups["aspectRateValue"].Success)
            {
                // アスペクト比が実数値で表現されている場合


                var aspectRateValueText = matchAspectRateSpecMatch.Groups["aspectRateValue"].Value;
                if (!double.TryParse(aspectRateValueText, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture.NumberFormat, out double aspectRateValue) ||
                    aspectRateValue <= 0)
                {
                    // 数値の書式が正しくないかまたは正ではない場合(構文ミス)
                    // アスペクト比として0を指定した場合のみここに到達する

                    return (false, null, null, null);
                }

                return (true, $"{resolutionWidth}x{resolutionHeight}", aspectRateValueText, aspectRateValueText);
            }
            else
            {
                // _startsWithAspectRateSpecPattern.Match(conversionSpec) == true なので、このルートには到達しないはず

                throw new Exception("internal error");
            }
        }

        private static string ReplaceResolutionSpecInFileName(string sourceFileName, string resolutionSpec, string? aspectRateSpecOnFileSystem, string? encoderDescription)
        {
            var resolutionAndAspectRateSpec = aspectRateSpecOnFileSystem is null ? resolutionSpec : $"{resolutionSpec} {aspectRateSpecOnFileSystem}";
            if (string.IsNullOrEmpty(resolutionAndAspectRateSpec))
            {
                // 解像度とアスペクト比が共に指定されていない場合

                return
                    encoderDescription is null
                    ? $"{Path.GetFileNameWithoutExtension(sourceFileName)}.mkv"
                    : $"{Path.GetFileNameWithoutExtension(sourceFileName)} [{encoderDescription}].mkv";
            }
            var matches = _resolutionAndAspectRateSpecInFileNamePattern.Matches(sourceFileName);
            if (matches.Count == 1)
            {
                // 入力元ファイル名に解像度・アスペクト比指定がただ一つだけある場合

                // 入力ファイル名のの解像度・アスペクト比指定を変換先の解像度・アスペクト比指定に置き換える
                var replacedFileName =
                    _resolutionAndAspectRateSpecInFileNamePattern.Replace(
                        Path.GetFileNameWithoutExtension(sourceFileName),
                        match =>
                        {
                            var prefix = match.Groups["prefix"].Value;
                            var suffix = match.Groups["suffix"].Value;
                            return prefix + resolutionAndAspectRateSpec + suffix;
                        });
                return
                    encoderDescription is null
                    ? $"{replacedFileName}.mkv"
                    : $"{replacedFileName} [{encoderDescription}].mkv";

            }
            else if (matches.Count > 1)
            {
                // 入力元ファイルの名前に解像度指定が複数ある場合
                if (matches.Any(match => string.Equals(match.Groups["resolutionAndAspectRateSpec"].Value, resolutionAndAspectRateSpec)))
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
                        ? $"{Path.GetFileNameWithoutExtension(sourceFileName)} [{resolutionAndAspectRateSpec}].mkv"
                        : $"{Path.GetFileNameWithoutExtension(sourceFileName)} [{encoderDescription} {resolutionAndAspectRateSpec}].mkv";
                }
            }
            else
            {
                // 入力元ファイルの名前に解像度指定が一つもない場合

                // 解像度指定をファイル名の末尾に付加する。
                return
                    encoderDescription is null
                    ? $"{Path.GetFileNameWithoutExtension(sourceFileName)} [{resolutionAndAspectRateSpec}].mkv"
                    : $"{Path.GetFileNameWithoutExtension(sourceFileName)} [{encoderDescription} {resolutionAndAspectRateSpec}].mkv";
            }
        }

        private static FileInfo MoveToDestinationFile(FileInfo sourceFile, FileInfo destinationFile)
        {
            var destinationFileDirectoryPath = destinationFile.DirectoryName ?? ".";
            var destinationFileNameWithoutExtension = Path.GetFileNameWithoutExtension(destinationFile.Name);
            var destinationFileExtension = Path.GetExtension(destinationFile.Name);
            var count = 1;
            while (true)
            {
                try
                {
                    var actualDestinationFilePath
                        = Path.Combine(
                            destinationFileDirectoryPath,
                            $"{destinationFileNameWithoutExtension}{(count <= 1 ? "" : $" ({count})")}{destinationFileExtension}");
                    if (!File.Exists(actualDestinationFilePath))
                    {
                        File.Move(sourceFile.FullName, actualDestinationFilePath);
                        return new FileInfo(actualDestinationFilePath);
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

        private static void CleanUpLogFile(FileInfo logFile)
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

        private static void DeleteFileSafety(string targetFilePath)
        {
            try
            {
                if (File.Exists(targetFilePath))
                    File.Delete(targetFilePath);
            }
            catch (IOException)
            {
            }
        }

        private static void DeleteFileSafety(FileInfo targetFile)
        {
            try
            {
                if (targetFile.Exists)
                    targetFile.Delete();
            }
            catch (IOException)
            {
            }
        }

        private static void FinalizeLogFile(FileInfo logFile, string result)
        {
            File.Move(logFile.FullName, ConstructLogFilePath(logFile, result), true);
        }

        private static string ConstructLogFilePath(FileInfo logFile, string result)
        {
            return
                Path.Combine(
                    logFile.DirectoryName ?? ".",
                    $"{Path.GetFileNameWithoutExtension(logFile.Name)}.{result}{logFile.Extension}");
        }
    }
}
