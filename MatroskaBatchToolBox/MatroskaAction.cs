using System;
using System.Globalization;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MatroskaBatchToolBox
{
    internal static class MatroskaAction
    {
        private static readonly Regex _duplicatedFileNamePattern;
        private static readonly Regex _normalizedFileNamePattern;
        private static readonly Regex _simpleCopyDirectoryNamePattern;
        private static readonly Regex _resolutionSpecInParentDirectoryNamePattern;
        private static readonly Regex _resolutionSpecInFileNamePattern;

        static MatroskaAction()
        {
            _duplicatedFileNamePattern = new Regex(@" \(\d+\)$", RegexOptions.Compiled);
            _normalizedFileNamePattern = new Regex(@"(?<prefix>\[([^\]]* )?)audio-normalized(?<suffix>( [^\]]*)?\])", RegexOptions.Compiled);
            _simpleCopyDirectoryNamePattern = new Regex(@"^(\d+x\d+)==$", RegexOptions.Compiled);
            _resolutionSpecInParentDirectoryNamePattern = new Regex(@"^(?<resolutionWidth>\d+)x(?<resolutionHeight>\d+)(\s+(((?<acpectRateWidth>\d+)(to|：|:)(?<aspectRateHeight>\d+))|(?<aspectRate>\d+\.\d+)))?$", RegexOptions.Compiled);
            _resolutionSpecInFileNamePattern = new Regex(@"(?<prefix>\[([^\]]* )?)(?<resolutionSpec>\d+x\d+)(?<suffix>( [^\]]*)?\])", RegexOptions.Compiled);
        }

        public static ActionResult NormalizeMovieFile(FileInfo sourceFile, IProgress<double> progressReporter)
        {
            var logFile = new FileInfo(sourceFile.FullName + ".log");
            CleanUpLogFile(logFile);
            string codecName =
                Settings.CurrentSettings.AudioCodec switch
                {
                    "libopus" => "Opus",
                    _ => throw new Exception($"audio codec '{Settings.CurrentSettings.AudioCodec}' is not supported."),
                };
            var destinationFile =
                new FileInfo(
                    Path.Combine(
                        sourceFile.DirectoryName ?? ".",
                        $"{Path.GetFileNameWithoutExtension(sourceFile.Name)} [{codecName} audio-normalized].mkv"));
            var workingFile =
                new FileInfo(
                    Path.Combine(destinationFile.DirectoryName ?? ".",
                    $".work.audio-normalize.{destinationFile.Name}"));
            workingFile.Delete();
            var actionResult = ActionResult.Failed;
            FileInfo? actualDestinationFilePath = null;
            try
            {
                if (_normalizedFileNamePattern.IsMatch(Path.GetFileNameWithoutExtension(sourceFile.Name)))
                {
                    // ファイル名に正規化されたマーク文字列があるので、何もせずに復帰する。
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
                ExternalCommand.NormalizeAudioFile(logFile, sourceFile, workingFile, progressReporter);
                actualDestinationFilePath = MoveToDestinationFile(workingFile, destinationFile);
                ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: File moved from \"{workingFile.FullName}\" to \"{actualDestinationFilePath.FullName}\".", });
                return actionResult = ActionResult.Success;
            }
            catch (AggregateException ex)
            {
                if (destinationFile.Exists)
                {
                    destinationFile.Delete();
                    ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: File was deleted: \"{destinationFile.FullName}\"", });
                }
                ExternalCommand.ReportAggregateException(logFile, ex);
                return actionResult = ActionResult.Failed;
            }
            catch (Exception ex)
            {
                if (destinationFile.Exists)
                {
                    destinationFile.Delete();
                    ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: File was deleted: \"{destinationFile.FullName}\"", });
                }
                ExternalCommand.ReportException(logFile, ex);
                return actionResult = ActionResult.Failed;
            }
            finally
            {
                if (workingFile.Exists)
                {
                    workingFile.Delete();
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
                        ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: Failed to normalize the audio of the movie file.: \"{sourceFile.FullName}\"", });
                        FinalizeLogFile(logFile, "NG");
                        break;
                    default:
                        break;
                }
            }
        }

        public static ActionResult ResizeMovieFile(FileInfo sourceFile, IProgress<double> progressReporter)
        {
            var parentDirectoryName = sourceFile.Directory?.Name ?? "";
            if (_simpleCopyDirectoryNamePattern.IsMatch(parentDirectoryName))
                return ContertMovieFileToMatroska(sourceFile, progressReporter);
            else if (_resolutionSpecInParentDirectoryNamePattern.IsMatch(parentDirectoryName))
                return ChangeResolutionOfMovieFile(sourceFile, parentDirectoryName, progressReporter);
            else
            {
                // 処理することがないので、何もせず復帰する。
                return ActionResult.Skipped;
            }
        }

        private static ActionResult ContertMovieFileToMatroska(FileInfo sourceFile, IProgress<double> progressReporter)
        {
            var logFile = new FileInfo(sourceFile.FullName + ".log");
            CleanUpLogFile(logFile);
            var destinationFile = new FileInfo(Path.Combine(sourceFile.DirectoryName ?? ".", Path.GetFileNameWithoutExtension(sourceFile.Name) + ".mkv"));
            var workingFile =
                new FileInfo(
                    Path.Combine(destinationFile.DirectoryName ?? ".",
                    $".work.resize-resolution.{destinationFile.Name}"));
            workingFile.Delete();
            var actionResult = ActionResult.Failed;
            FileInfo? actualDestinationFilePath = null;
            try
            {
                if (string.Equals(sourceFile.Extension, ".mkv", StringComparison.InvariantCultureIgnoreCase))
                {
                    // 入力ファイルが既に .mkv 形式であるので、何もせず復帰する。
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
                ExternalCommand.CopyMovieFile(logFile, sourceFile, workingFile, progressReporter);
                actualDestinationFilePath = MoveToDestinationFile(workingFile, destinationFile);
                ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: File moved from \"{workingFile.FullName}\" to \"{actualDestinationFilePath.FullName}\"." });
                return actionResult = ActionResult.Success;
            }
            catch (AggregateException ex)
            {
                if (destinationFile.Exists)
                {
                    destinationFile.Delete();
                    ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: File was deleted: \"{destinationFile.FullName}\"" });
                }
                ExternalCommand.ReportAggregateException(logFile, ex);
                return actionResult = ActionResult.Failed;
            }
            catch (Exception ex)
            {
                if (destinationFile.Exists)
                {
                    destinationFile.Delete();
                    ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: File was deleted: \"{destinationFile.FullName}\"" });
                }
                ExternalCommand.ReportException(logFile, ex);
                return actionResult = ActionResult.Failed;
            }
            finally
            {
                if (workingFile.Exists)
                {
                    workingFile.Delete();
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
                        ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: Failed to convert the movie file.: \"{sourceFile.FullName}\"", });
                        FinalizeLogFile(logFile, "NG");
                        break;
                    default:
                        break;
                }
            }
        }

        private static ActionResult ChangeResolutionOfMovieFile(FileInfo sourceFile, string resolutionSpecAndAspectRateSpecText, IProgress<double> progressReporter)
        {
            var logFile = new FileInfo(sourceFile.FullName + ".log");
            CleanUpLogFile(logFile);
            var (resolutionSpec, aspectRateSpec) = ParseResolutionSpecAndAspectRateSpec(resolutionSpecAndAspectRateSpecText);
            if (resolutionSpec is null || aspectRateSpec is null)
            {
                // 親ディレクトの名前が解像度(およびアスペクト比の指定)ではないので、何もせず復帰する。
                return ActionResult.Skipped;
            }
            var destinationFileName = ReplaceResolutionSpecInFileName(sourceFile, resolutionSpec);
            var destinationFile = new FileInfo(Path.Combine(sourceFile.DirectoryName ?? ".", destinationFileName));
            var workingFile =
                new FileInfo(
                    Path.Combine(destinationFile.DirectoryName ?? ".",
                    $".work.resize-resolution.{destinationFile.Name}"));
            workingFile.Delete();
            var actionResult = ActionResult.Failed;
            FileInfo? actualDestinationFilePath = null;
            try
            {
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
                ExternalCommand.ResizeMovieFile(logFile, sourceFile, resolutionSpec, aspectRateSpec, workingFile, progressReporter);
                actualDestinationFilePath = MoveToDestinationFile(workingFile, destinationFile);
                ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: File moved from \"{workingFile.FullName}\" to \"{actualDestinationFilePath.FullName}\"." });
                return actionResult = ActionResult.Success;
            }
            catch (AggregateException ex)
            {
                if (destinationFile.Exists)
                {
                    destinationFile.Delete();
                    ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: File was deleted: \"{destinationFile.FullName}\"" });
                }
                ExternalCommand.ReportAggregateException(logFile, ex);
                return actionResult = ActionResult.Failed;
            }
            catch (Exception ex)
            {
                if (destinationFile.Exists)
                {
                    destinationFile.Delete();
                    ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: File was deleted: \"{destinationFile.FullName}\"" });
                }
                ExternalCommand.ReportException(logFile, ex);
                return actionResult = ActionResult.Failed;
            }
            finally
            {
                if (workingFile.Exists)
                {
                    workingFile.Delete();
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
                        ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: Failed to change the video resolution of the movie file.: \"{sourceFile.FullName}\"", });
                        FinalizeLogFile(logFile, "NG");
                        break;
                    default:
                        break;
                }
            }
        }

        private static (string? resolutionSpec, string? aspectRateSpec) ParseResolutionSpecAndAspectRateSpec(string resolutionSpecAndAspectRateSpecText)
        {
            var resolutionSpecMatch = _resolutionSpecInParentDirectoryNamePattern.Match(resolutionSpecAndAspectRateSpecText);
            if (!resolutionSpecMatch.Success)
                return (null, null);
            var resolutionWidthText = resolutionSpecMatch.Groups["resolutionWidth"].Value;
            var resolutionWidth = int.Parse(resolutionWidthText, NumberStyles.None, CultureInfo.InvariantCulture.NumberFormat);
            if (resolutionWidth <= 0)
                return (null, null);
            var resolutionHeightText = resolutionSpecMatch.Groups["resolutionHeight"].Value;
            var resolutionHeight = int.Parse(resolutionHeightText, NumberStyles.None, CultureInfo.InvariantCulture.NumberFormat);
            if (resolutionHeight <= 0)
                return (null, null);
            if (resolutionSpecMatch.Groups["acpectRateWidth"].Success && resolutionSpecMatch.Groups["aspectRateHeight"].Success)
            {
                var aspectRateWidthText = resolutionSpecMatch.Groups["acpectRateWidth"].Value;
                var aspectRateWidth = int.Parse(aspectRateWidthText, NumberStyles.None, CultureInfo.InvariantCulture.NumberFormat);
                if (aspectRateWidth <= 0)
                    return (null, null);
                var aspectRateHeightText = resolutionSpecMatch.Groups["aspectRateHeight"].Value;
                var aspectRateHeight = int.Parse(aspectRateHeightText, NumberStyles.None, CultureInfo.InvariantCulture.NumberFormat);
                if (aspectRateHeight <= 0)
                    return (null, null);
                return ($"{resolutionWidth}x{resolutionHeight}", $"{aspectRateWidth}:{aspectRateHeight}");
            }
            else if (resolutionSpecMatch.Groups["aspectRate"].Success)
            {
                var aspectRateText = resolutionSpecMatch.Groups["aspectRate"].Value;
                var aspectRate = double.Parse(aspectRateText, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture.NumberFormat);
                if (aspectRate <= 0)
                    return (null, null);
                return ($"{resolutionWidth}x{resolutionHeight}", $"{aspectRateText}");
            }
            else
            {
                var gcd = ExtendedMath.GreatestCommonDivisor(resolutionWidth, resolutionHeight);
                var aspectRateWidth = resolutionWidth / gcd;
                var aspectRateHeight = resolutionHeight / gcd;
                return ($"{resolutionWidth}x{resolutionHeight}", $"{aspectRateWidth}:{aspectRateHeight}");
            }
        }



        private static string ReplaceResolutionSpecInFileName(FileInfo sourceFile, string resolutionSpec)
        {
            var matches = _resolutionSpecInFileNamePattern.Matches(sourceFile.Name);
            if (matches.Count == 1)
            {
                // 入力元ファイル名に解像度指定がただ一つだけある場合
                
                // 入力ファイル名のの解像度指定を変換先の解像度指定に置き換える
                return
                    _resolutionSpecInFileNamePattern.Replace(
                        Path.GetFileNameWithoutExtension(sourceFile.Name),
                        match =>
                        {
                            var prefix = match.Groups["prefix"].Value;
                            var suffix = match.Groups["suffix"].Value;
                            return prefix + resolutionSpec + suffix;
                        })
                    + ".mkv";

            }
            else if (matches.Count > 1)
            {
                // 入力元ファイルの名前に解像度指定が複数ある場合
                if (matches.Any(match => string.Equals(match.Groups["resolutionSpec"].Value, resolutionSpec)))
                {
                    // 入力ファイル名の解像度指定の中に、変換先の解像度指定に一致するものが一つでもある場合

                    // 入力ファイル名に解像度指定を新たに付加せずに返す。
                    return $"{Path.GetFileNameWithoutExtension(sourceFile.Name)}.mkv";

                }
                else
                {
                    // 入力ファイル名の解像度指定の中に、変換先の解像度指定に一致するものが一つもない場合

                    // ファイル名の解像度指定の置換を行うと利用者が意図しない問題が発生する可能性があるため、新たな解像度指定をファイル名の末尾に付加するに留める。
                    return $"{Path.GetFileNameWithoutExtension(sourceFile.Name)} [{resolutionSpec}].mkv";
                }
            }
            else
            {
                // 入力元ファイルの名前に解像度指定が一つもない場合

                // 解像度指定をファイル名の末尾に付加する。
                return $"{Path.GetFileNameWithoutExtension(sourceFile.Name)} [{resolutionSpec}].mkv";
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
                catch (IOException)
                {
                }
                ++count;
            }
        }

        private static void CleanUpLogFile(FileInfo logFile)
        {
            logFile.Delete();
            File.Delete(ConstructLogFilePath(logFile, "OK"));
            File.Delete(ConstructLogFilePath(logFile, "NG"));
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
