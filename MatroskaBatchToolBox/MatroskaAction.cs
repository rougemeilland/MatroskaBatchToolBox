using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MatroskaBatchToolBox
{
    internal static class MatroskaAction
    {
        private static readonly Regex _simpleCopyDirectoryNamePattern;
        private static readonly Regex _simpleResolutionSpecPattern;
        private static readonly Regex _resolutionSpecInFileNamePattern;

        static MatroskaAction()
        {
            _simpleCopyDirectoryNamePattern = new Regex(@"^(\d+x\d+)==$", RegexOptions.Compiled);
            _simpleResolutionSpecPattern = new Regex(@"^(\d+x\d+)$", RegexOptions.Compiled);
            _resolutionSpecInFileNamePattern = new Regex(@"(?<prefix>\[([^\]]+ )?)(?<resolutionSpec>\d+x\d+)(?<suffix>( [^\]]+)?\])", RegexOptions.Compiled);
        }

        public static ActionResult NormalizeMovieFile(FileInfo sourceFile, IProgress<double> progressReporter)
        {
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
            var logFile = new FileInfo(sourceFile.FullName + ".log");
            if (destinationFile.Exists)
                return ActionResult.Skipped;
            var workingFile =
                new FileInfo(
                    Path.Combine(destinationFile.DirectoryName ?? ".",
                    $".work.audio-normalize.{destinationFile.Name}"));
            CleanUpLogFile(logFile);
            workingFile.Delete();
            var success = false;
            FileInfo? actualDestinationFilePath = null;
            try
            {
                ExternalCommand.NormalizeAudioFile(logFile, sourceFile, workingFile, progressReporter);
                actualDestinationFilePath = MoveToDestinationFile(workingFile, destinationFile);
                ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: File moved from \"{workingFile.FullName}\" to \"{actualDestinationFilePath.FullName}\".", });
                success = true;
                return ActionResult.Success;
            }
            catch (AggregateException ex)
            {
                if (destinationFile.Exists)
                {
                    destinationFile.Delete();
                    ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: File was deleted: \"{destinationFile.FullName}\"", });
                }
                ReportAggregateException(logFile, ex);
                return ActionResult.Failed;
            }
            catch (Exception ex)
            {
                if (destinationFile.Exists)
                {
                    destinationFile.Delete();
                    ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: File was deleted: \"{destinationFile.FullName}\"", });
                }
                ReportException(logFile, ex);
                return ActionResult.Failed;
            }
            finally
            {
                if (workingFile.Exists)
                {
                    workingFile.Delete();
                    ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: File was deleted: \"{workingFile.FullName}\"", });
                }
                if (success)
                {
                    ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: The audio in movie file was successfully normalized.: from \"{sourceFile.FullName}\" to \"{actualDestinationFilePath?.FullName ?? ""}\"", });
                    FinalizeLogFile(logFile, "OK");
                }
                else
                {
                    ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: Failed to normalize the audio of the movie file.: \"{sourceFile.FullName}\"", });
                    FinalizeLogFile(logFile, "NG");
                }
            }
        }

        public static ActionResult ResizeMovieFile(FileInfo sourceFile, IProgress<double> progressReporter)
        {
            var parentDirectoryName = sourceFile.Directory?.Name ?? "";
            if (_simpleCopyDirectoryNamePattern.IsMatch(parentDirectoryName))
                return ContertMovieFileToMatroska(sourceFile, progressReporter);
            else if (_simpleResolutionSpecPattern.IsMatch(parentDirectoryName))
                return ChangeResolutionOfMovieFile(sourceFile, progressReporter, parentDirectoryName);
            else
            {
                // 処理することがないので、何もせず復帰する。
                return ActionResult.Skipped;
            }
        }

        private static ActionResult ContertMovieFileToMatroska(FileInfo sourceFile, IProgress<double> progressReporter)
        {
            if (string.Equals(sourceFile.Extension, ".mkv", StringComparison.InvariantCultureIgnoreCase))
            {
                // 入力ファイルが既に .mkv 形式であるので、何もせず復帰する。
                return ActionResult.Skipped;
            }
            var destinationFile = new FileInfo(Path.Combine(sourceFile.DirectoryName ?? ".", Path.GetFileNameWithoutExtension(sourceFile.Name) + ".mkv"));
            if (destinationFile.Exists)
            {
                // 出力先ファイルが既に存在しているので、何もせず復帰する。
                return ActionResult.Skipped;
            }
            var logFile = new FileInfo(sourceFile.FullName + ".log");
            var workingFile =
                new FileInfo(
                    Path.Combine(destinationFile.DirectoryName ?? ".",
                    $".work.resize-resolution.{destinationFile.Name}"));
            CleanUpLogFile(logFile);
            workingFile.Delete();
            var success = false;
            FileInfo? actualDestinationFilePath = null;
            try
            {
                ExternalCommand.CopyMovieFile(logFile, sourceFile, workingFile, progressReporter);
                actualDestinationFilePath = MoveToDestinationFile(workingFile, destinationFile);
                ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: File moved from \"{workingFile.FullName}\" to \"{actualDestinationFilePath.FullName}\"." });
                success = true;
                return ActionResult.Success;
            }
            catch (AggregateException ex)
            {
                if (destinationFile.Exists)
                {
                    destinationFile.Delete();
                    ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: File was deleted: \"{destinationFile.FullName}\"" });
                }
                ReportAggregateException(logFile, ex);
                return ActionResult.Failed;
            }
            catch (Exception ex)
            {
                if (destinationFile.Exists)
                {
                    destinationFile.Delete();
                    ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: File was deleted: \"{destinationFile.FullName}\"" });
                }
                ReportException(logFile, ex);
                return ActionResult.Failed;
            }
            finally
            {
                if (workingFile.Exists)
                {
                    workingFile.Delete();
                    ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: File was deleted: \"{workingFile.FullName}\"", });
                }
                if (success)
                {
                    ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: The movie file was successfully converted.: from \"{sourceFile.FullName}\" to \"{actualDestinationFilePath?.FullName ?? ""}\"", });
                    FinalizeLogFile(logFile, "OK");
                }
                else
                {
                    ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: Failed to convert the movie file.: \"{sourceFile.FullName}\"", });
                    FinalizeLogFile(logFile, "NG");
                }
            }
        }

        private static ActionResult ChangeResolutionOfMovieFile(FileInfo sourceFile, IProgress<double> progressReporter, string parentDirectoryName)
        {
            var resolutionSpec = parentDirectoryName;
            var destinationFileName = ReplaceResolutionSpecInFileName(sourceFile, resolutionSpec);
            if (string.Equals(destinationFileName, sourceFile.Name, StringComparison.InvariantCultureIgnoreCase))
            {
                // 入力ファイルと出力ファイルのファイル名が一致しているので、何もせず復帰する。
                return ActionResult.Skipped;
            }
            var destinationFile = new FileInfo(Path.Combine(sourceFile.DirectoryName ?? ".", destinationFileName));
            if (destinationFile.Exists)
            {
                // 出力先ファイルが既に存在しているので、何もせず復帰する。
                return ActionResult.Skipped;
            }
            var logFile = new FileInfo(sourceFile.FullName + ".log");
            var workingFile =
                new FileInfo(
                    Path.Combine(destinationFile.DirectoryName ?? ".",
                    $".work.resize-resolution.{destinationFile.Name}"));
            CleanUpLogFile(logFile);
            logFile.Delete();
            workingFile.Delete();
            var success = false;
            FileInfo? actualDestinationFilePath = null;
            try
            {
                ExternalCommand.ResizeMovieFile(logFile, sourceFile, resolutionSpec, workingFile, progressReporter);
                actualDestinationFilePath = MoveToDestinationFile(workingFile, destinationFile);
                ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: File moved from \"{workingFile.FullName}\" to \"{actualDestinationFilePath.FullName}\"." });
                success = true;
                return ActionResult.Success;
            }
            catch (AggregateException ex)
            {
                if (destinationFile.Exists)
                {
                    destinationFile.Delete();
                    ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: File was deleted: \"{destinationFile.FullName}\"" });
                }
                ReportAggregateException(logFile, ex);
                return ActionResult.Failed;
            }
            catch (Exception ex)
            {
                if (destinationFile.Exists)
                {
                    destinationFile.Delete();
                    ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: File was deleted: \"{destinationFile.FullName}\"" });
                }
                ReportException(logFile, ex);
                return ActionResult.Failed;
            }
            finally
            {
                if (workingFile.Exists)
                {
                    workingFile.Delete();
                    ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: File was deleted: \"{workingFile.FullName}\"", });
                }
                if (success)
                {
                    ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: The video resolution of the movie fie was successfully changed.: from \"{sourceFile.FullName}\" to \"{actualDestinationFilePath?.FullName ?? ""}\"", });
                    FinalizeLogFile(logFile, "OK");
                }
                else
                {
                    ExternalCommand.Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: Failed to change the video resolution of the movie file.: \"{sourceFile.FullName}\"", });
                    FinalizeLogFile(logFile, "NG");
                }
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

        private static void ReportAggregateException(FileInfo logFile, AggregateException ex)
        {
            ReportException(logFile, ex);
            foreach (var ex2 in ex.InnerExceptions)
                ReportException(logFile, ex2);
        }

        private static void ReportException(FileInfo logFile, Exception ex)
        {
            ExternalCommand.Log(logFile, new[] { "----------", ex.Message, ex.StackTrace ?? "" });
            for (var innerEx = ex.InnerException; innerEx is not null; innerEx = innerEx.InnerException)
                ExternalCommand.Log(logFile, new[] { "----------", innerEx.Message, innerEx.StackTrace ?? "" });
            ExternalCommand.Log(logFile, new[] { "----------" });
        }
    }
}
