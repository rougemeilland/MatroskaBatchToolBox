using System.IO;
using MatroskaBatchToolBox.Utility.Interprocess;
using MatroskaBatchToolBox.Utility.Movie;
using Palmtree;
using Palmtree.IO.Console;

namespace Experiment
{
    public static partial class Program
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<保留中>")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0079:不要な抑制を削除します", Justification = "<保留中>")]
        public static int Main(string[] args)
        {
            foreach (var dir in args)
            {
#if true
                foreach (var file in Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories))
                {
                    if (Path.GetExtension(file).ToUpperInvariant().IsAnyOf(".MKV", ".MP4"))
                    {
                        var shortFile = Path.GetFileName(file);
                        if (shortFile.Length > 80)
                            shortFile = shortFile[..80];
                        TinyConsole.Write($"{shortFile}...");
                        TinyConsole.Erase(ConsoleEraseMode.FromCursorToEndOfLine);
                        TinyConsole.Write("\r");
                        TinyConsole.Error.WriteLine(file);
                        _ = Command.GetMovieInformation(null, new FileInfo(file), MovieInformationType.Streams, (level, message) => { });
                    }
                }
#elif false
                foreach (var file in Directory.EnumerateFiles(dir, "*.mkv", SearchOption.AllDirectories))
                {
                    var shortFile = Path.GetFileName(file);
                    if (shortFile.Length > 40)
                        shortFile = shortFile[..40];
                    TinyConsole.Write($"{shortFile}...");
                    TinyConsole.Erase(ConsoleEraseMode.FromCursorToEndOfLine);
                    TinyConsole.Write("\r");
                    try
                    {
                        var sourceFile = new FileInfo(file);
                        //TinyConsole.Error.WriteLine(sourceFile.FullName);
                        var info = Command.GetMovieInformation(null, sourceFile, MovieInformationType.Streams, (level, message) => { });
                        if (info.AudioStreams.Any(stream => stream.Tags["encoder"]?.Contains("flac") ?? false) && info.AudioStreams.All(stream => stream.CodecName.IsAnyOf("opus", "vorbis")))
                        {
                            var backupFile = new FileInfo(Path.Combine(sourceFile.DirectoryName ?? ".", Path.GetFileNameWithoutExtension(sourceFile.Name) + ".bak"));
                            var temporaryFile = new FileInfo(Path.Combine(sourceFile.DirectoryName ?? ".", Path.GetFileNameWithoutExtension(sourceFile.Name) + ".tmp"));
                            var options = new List<string>();
                            foreach (var stream in info.VideoStreams)
                                options.Add($"-disposition:v:{stream.IndexWithinVideoStream} -default");
                            foreach (var stream in info.AudioStreams)
                            {
                                options.Add($"-disposition:a:{stream.IndexWithinAudioStream} -default");
                                var encoder = stream.Tags["encoder"];
                                if (encoder is null || encoder.Contains("flac"))
                                {
                                    var newEncoder =
                                        stream.CodecName switch
                                        {
                                            "opus" => "Lavc60.9.100 libopus",
                                            "vorbis" => "Lavc60.9.100 libvorbis",
                                            _ => throw new Exception(),
                                        };
                                    options.Add($"-metadata:s:a:{stream.IndexWithinAudioStream} ENCODER={newEncoder.CommandLineArgumentEncode()}");
                                }
                            }

                            foreach (var stream in info.SubtitleStreams)
                                options.Add($"-disposition:s:{stream.IndexWithinSubtitleStream} -default");
                            foreach (var stream in info.DataStreams)
                                options.Add($"-disposition:d:{stream.IndexWithinDataStream} -default");
                            foreach (var stream in info.AttachmentStreams)
                                options.Add($"-disposition:t:{stream.IndexWithinAttachmentStream} -default");
                            TinyConsole.Erase(ConsoleEraseMode.FromCursorToEndOfLine);
                            TinyConsole.Out.WriteLine($"pffmpeg -hide_banner -y -i {sourceFile.FullName.CommandLineArgumentEncode()} -c copy -map 0 {string.Join(" ", options)} -f matroska {temporaryFile.FullName.CommandLineArgumentEncode()}");
                            TinyConsole.Out.WriteLine("if %ERRORLEVEL% equ 0 (");
                            TinyConsole.Out.WriteLine($"  ren {sourceFile.FullName.CommandLineArgumentEncode()} {backupFile.Name.CommandLineArgumentEncode()}");
                            TinyConsole.Out.WriteLine($"  ren {temporaryFile.FullName.CommandLineArgumentEncode()} {sourceFile.Name.CommandLineArgumentEncode()}");
                            TinyConsole.Out.WriteLine(") else (");
                            TinyConsole.Out.WriteLine($" del {temporaryFile.FullName.CommandLineArgumentEncode()}");
                            TinyConsole.Out.WriteLine(")");
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
#else
                foreach (var file in Directory.EnumerateFiles(dir, "*.bak", SearchOption.AllDirectories))
                {
                    var shortFile = Path.GetFileName(file);
                    if (shortFile.Length > 40)
                        shortFile = shortFile[..40];
                    TinyConsole.Write($"{shortFile}...");
                    TinyConsole.Erase(ConsoleEraseMode.FromCursorToEndOfLine);
                    TinyConsole.Write("\r");
                    try
                    {
                        var backupFile = new FileInfo(file);
                        var sourceFile = new FileInfo(Path.Combine(backupFile.DirectoryName ?? ".", Path.GetFileNameWithoutExtension(backupFile.Name) + ".mkv"));
                        if (sourceFile.Exists)
                        {
                            var backupFileInfo = Command.GetMovieInformation("matroska", backupFile, MovieInformationType.Streams | MovieInformationType.Format, (level, message) => { });
                            var sourceFileInfo = Command.GetMovieInformation("matroska", sourceFile, MovieInformationType.Streams | MovieInformationType.Format, (level, message) => { });

                            if (sourceFileInfo.AudioStreams.Any(stream => stream.Tags["encoder"]?.Contains("flac") ?? true))
                                TinyConsole.WriteLine($"NG: {sourceFile.FullName} (contains flac)");

                            if (sourceFileInfo.VideoStreams.Count() != backupFileInfo.VideoStreams.Count())
                                TinyConsole.WriteLine($"NG: {sourceFile.FullName} (!= video stream count)");
                            if (sourceFileInfo.AudioStreams.Count() != backupFileInfo.AudioStreams.Count())
                                TinyConsole.WriteLine($"NG: {sourceFile.FullName} (!= audiio stream count)");
                            if (sourceFileInfo.SubtitleStreams.Count() != backupFileInfo.SubtitleStreams.Count())
                                TinyConsole.WriteLine($"NG: {sourceFile.FullName} (!= subtitle stream count)");
                            if (sourceFileInfo.DataStreams.Count() != backupFileInfo.DataStreams.Count())
                                TinyConsole.WriteLine($"NG: {sourceFile.FullName} (!= data stream count)");
                            if (sourceFileInfo.AttachmentStreams.Count() != backupFileInfo.AttachmentStreams.Count())
                                TinyConsole.WriteLine($"NG: {sourceFile.FullName} (!= attachment stream count)");

                            if (sourceFileInfo.VideoStreams.Any(stream => stream.Disposition.Default))
                                TinyConsole.WriteLine($"NG: {sourceFile.FullName} (contains default stream)");
                            if (sourceFileInfo.AudioStreams.Any(stream => stream.Disposition.Default))
                                TinyConsole.WriteLine($"NG: {sourceFile.FullName} (contains default stream)");
                            if (sourceFileInfo.SubtitleStreams.Any(stream => stream.Disposition.Default))
                                TinyConsole.WriteLine($"NG: {sourceFile.FullName} (contains default stream)");
                            if (sourceFileInfo.DataStreams.Any(stream => stream.Disposition.Default))
                                TinyConsole.WriteLine($"NG: {sourceFile.FullName} (contains default stream)");
                            if (sourceFileInfo.AttachmentStreams.Any(stream => stream.Disposition.Default))
                                TinyConsole.WriteLine($"NG: {sourceFile.FullName} (contains default stream)");

                            if (sourceFileInfo.VideoStreams.Zip(backupFileInfo.VideoStreams, (stream1, stream2) => new { stream1, stream2 }).Any(item => item.stream1.Tags["encoder"] != item.stream2.Tags["encoder"]))
                                TinyConsole.WriteLine($"NG: {sourceFile.FullName} (!= video stream encoder");
                            if (sourceFileInfo.SubtitleStreams.Zip(backupFileInfo.SubtitleStreams, (stream1, stream2) => new { stream1, stream2 }).Any(item => item.stream1.Tags["encoder"] != item.stream2.Tags["encoder"]))
                                TinyConsole.WriteLine($"NG: {sourceFile.FullName} (!= subtitle stream encoder");
                            if (sourceFileInfo.DataStreams.Zip(backupFileInfo.DataStreams, (stream1, stream2) => new { stream1, stream2 }).Any(item => item.stream1.Tags["encoder"] != item.stream2.Tags["encoder"]))
                                TinyConsole.WriteLine($"NG: {sourceFile.FullName} (!= data stream encoder");
                            if (sourceFileInfo.AttachmentStreams.Zip(backupFileInfo.AttachmentStreams, (stream1, stream2) => new { stream1, stream2 }).Any(item => item.stream1.Tags["encoder"] != item.stream2.Tags["encoder"]))
                                TinyConsole.WriteLine($"NG: {sourceFile.FullName} (!= attachment stream encoder");

                            if (sourceFileInfo.AudioStreams.Any(stream => stream.Tags["encoder"].IsNoneOf("Lavc60.9.100 libopus", "Lavc60.9.100 libvorbis")))
                                TinyConsole.WriteLine($"NG: {sourceFile.FullName} (!= audio stream encoder");

                            if (sourceFileInfo.VideoStreams.Zip(backupFileInfo.VideoStreams, (stream1, stream2) => new { stream1, stream2 }).Any(item => item.stream1.Tags["duration"] != item.stream2.Tags["duration"]))
                                TinyConsole.WriteLine($"NG: {sourceFile.FullName} (!= video stream duration");
                            if (sourceFileInfo.AudioStreams.Zip(backupFileInfo.AudioStreams, (stream1, stream2) => new { stream1, stream2 }).Any(item => item.stream1.Tags["duration"] != item.stream2.Tags["duration"]))
                                TinyConsole.WriteLine($"NG: {sourceFile.FullName} (!= audio stream duration");
                            if (sourceFileInfo.SubtitleStreams.Zip(backupFileInfo.SubtitleStreams, (stream1, stream2) => new { stream1, stream2 }).Any(item => item.stream1.Tags["duration"] != item.stream2.Tags["duration"]))
                                TinyConsole.WriteLine($"NG: {sourceFile.FullName} (!= subtitle stream duration");
                            if (sourceFileInfo.DataStreams.Zip(backupFileInfo.DataStreams, (stream1, stream2) => new { stream1, stream2 }).Any(item => item.stream1.Tags["duration"] != item.stream2.Tags["duration"]))
                                TinyConsole.WriteLine($"NG: {sourceFile.FullName} (!= data stream duration");
                            if (sourceFileInfo.AttachmentStreams.Zip(backupFileInfo.AttachmentStreams, (stream1, stream2) => new { stream1, stream2 }).Any(item => item.stream1.Tags["duration"] != item.stream2.Tags["duration"]))
                                TinyConsole.WriteLine($"NG: {sourceFile.FullName} (!= attachment stream duration");

                            if (sourceFileInfo.Format.Duration != backupFileInfo.Format.Duration)
                                TinyConsole.WriteLine($"NG: {sourceFile.FullName} (!= duration");
                        }
                    }
                    catch (Exception ex)
                    {
                        TinyConsole.WriteLine($"NG: {file} ({ex.Message})");
                    }
                }
#endif
            }

            TinyConsole.Write("\r");
            TinyConsole.Erase(ConsoleEraseMode.FromCursorToEndOfLine);

            return 0;
        }
    }
}
