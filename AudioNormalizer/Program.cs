﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using MatroskaBatchToolBox.Utility;
using MatroskaBatchToolBox.Utility.Interprocess;
using MatroskaBatchToolBox.Utility.Movie;
using Palmtree;
using Palmtree.IO;

namespace AudioNormalizer
{
    public static partial class Program
    {
        private class CommandParameter
        {
            private CommandParameter(IEnumerable<CommandOption<OptionType>> options)
            {
                InputFormat = options.SingleOrNone(option => option.OptionType == OptionType.InputFormat)?.OptionParameter[1] as string;
                Input = options.SingleOrNone(option => option.OptionType == OptionType.Input)?.OptionParameter[1] as string;
                OutputFormat = options.SingleOrNone(option => option.OptionType == OptionType.OutputFormat)?.OptionParameter[1] as string;
                Output = options.SingleOrNone(option => option.OptionType == OptionType.Output)?.OptionParameter[1] as string;
                IsForceMode = options.Any(option => option.OptionType == OptionType.Force);
                KeepMetadata = options.Any(option => option.OptionType == OptionType.KeepMetadata);
                Verbose = options.Any(option => option.OptionType == OptionType.Verbose);
                IsHelpMode = options.Any(option => option.OptionType == OptionType.Help);
            }

            public string? InputFormat { get; }
            public string? Input { get; }
            public string? OutputFormat { get; }
            public string? Output { get; }
            public bool IsForceMode { get; }
            public bool KeepMetadata { get; }
            public bool Verbose { get; }
            public bool IsHelpMode { get; }

            public static CommandParameter Parse(IEnumerable<CommandOptionDefinition<OptionType>> optionDefinitions, string[] args)
                => new(optionDefinitions.ParseCommandArguments(args.AsReadOnlyArray()));
        }

        private const string _ffmpegPathEnvironmentVariableName = "FFMPEG_PATH";
        private const int _bitratePerChannelForLibVorbis = 96000;
        private static readonly string _thisProgramName;
        private static readonly IEnumerable<CommandOptionDefinition<OptionType>> _optionDefinitions;

        static Program()
        {
            _thisProgramName = Path.GetFileNameWithoutExtension(typeof(Program).Assembly.Location);
            _optionDefinitions = GetOptionDefinitions();
        }

        public static int Main(string[] args)
        {
            // このプロセスでは Ctrl+C を無視する。
            TinyConsole.CancelKeyPress += (sender, e) => e.Cancel = true;

            // コマンドの入出力エンコーディングを UTF8 にする
            TinyConsole.InputEncoding = Encoding.UTF8;
            TinyConsole.OutputEncoding = Encoding.UTF8;

            try
            {
                // コマンド引数の解析
                var commandOptions = ParseCommandOptions(args);
                if (commandOptions is null)
                    return 1;

                // -i オプションが指定されている場合は、その値のファイルが存在しなければならない
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

                // -o オプションが指定されており、かつ -f オプションが指定されていない場合は、-o オプションのファイルが存在してはならない。
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

                // -help オプションが指定されている場合は、ヘルプテキストを表示して終了する。
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

                    var temporaryIntermediateFile1 = (FileInfo?)null;
                    var temporaryIntermediateFile2 = (FileInfo?)null;

                    // 出力の準備をする (出力先が標準出力である場合は一時ファイルを作成する)
                    var (outputFormat, outputFile, temporaryOutputFile) = GetOutputMovieFile(commandOptions);

                    try
                    {
                        // 入力動画ファイルの情報を取得する
                        var movieInformation = GetMovieInformation(inputFormat, inputFile, commandOptions.Verbose);

                        if (movieInformation.AudioStreams.None())
                        {
                            // 入力動画ファイルに音声ストリームが存在しない場合

                            // 出力先に単純コピーする

                            if (commandOptions.Verbose)
                            {
                                PrintInformationMessage("Since the video file does not contain an audio track, simply copy it.");
                                if (inputFormat is not null)
                                    PrintInformationMessage($"  Input file format: {inputFormat}");
                                PrintInformationMessage($"  Input file format: \"{inputFile.FullName}\"");
                                if (outputFormat is not null)
                                    PrintInformationMessage($"  Output file format: {outputFormat}");
                                PrintInformationMessage($"  Output file format: \"{outputFile.FullName}\"");
                            }

                            using var instream = inputFile.OpenRead();
                            using var outstream = outputFile.Create();
                            CopyStream(instream, outstream, inputFile.Length);

                            if (commandOptions.Verbose)
                                PrintInformationMessage("Copy finished.");
                        }
                        else if (commandOptions.KeepMetadata)
                        {
                            // 動画ファイルの音声の正規化後にメタデータを消去しない場合

                            if (commandOptions.Verbose)
                            {
                                PrintInformationMessage("Start audio normalization.");
                                if (inputFormat is not null)
                                    PrintInformationMessage($"  Input file format: {inputFormat}");
                                PrintInformationMessage($"  Input file format: \"{inputFile.FullName}\"");
                                if (outputFormat is not null)
                                    PrintInformationMessage($"  Output file format: {outputFormat}");
                                PrintInformationMessage($"  Output file format: \"{outputFile.FullName}\"");
                            }

                            // 動画ファイルの音声を正規化する
                            NormalizeAudio(
                                movieInformation,
                                inputFormat,
                                inputFile,
                                outputFormat,
                                outputFile,
                                commandOptions.Verbose,
                                temporaryOutputFile is not null || commandOptions.IsForceMode);

                            if (commandOptions.Verbose)
                                PrintInformationMessage("Audio normalization finished.");
                        }
                        else
                        {
                            // 動画ファイルの音声の正規化後にメタデータを消去する場合

                            // 中間一時ファイルを作成する
                            temporaryIntermediateFile1 = new FileInfo(Path.GetTempFileName());
                            temporaryIntermediateFile2 = new FileInfo(Path.GetTempFileName());
                            var temporaryIntermediateFileFormat = outputFormat ?? "matroska";
                            if (commandOptions.Verbose)
                            {
                                PrintInformationMessage($"Temprary file is created.: \"{temporaryIntermediateFile1.FullName}\"");
                                PrintInformationMessage($"Temprary file is created.: \"{temporaryIntermediateFile2.FullName}\"");
                            }

                            // 動画ファイルの音声を正規化して中間一時ファイルに保存する
                            if (commandOptions.Verbose)
                            {
                                PrintInformationMessage("Start audio normalization.");
                                if (inputFormat is not null)
                                    PrintInformationMessage($"  Input file format: {inputFormat}");
                                PrintInformationMessage($"  Input file format: \"{inputFile.FullName}\"");
                                if (temporaryIntermediateFileFormat is not null)
                                    PrintInformationMessage($"  Output file format: {temporaryIntermediateFileFormat}");
                                PrintInformationMessage($"  Output file format: \"{temporaryIntermediateFile1.FullName}\"");
                            }

                            NormalizeAudio(
                                movieInformation,
                                inputFormat,
                                inputFile,
                                temporaryIntermediateFileFormat,
                                temporaryIntermediateFile1,
                                commandOptions.Verbose,
                                true);

                            if (commandOptions.Verbose)
                                PrintInformationMessage("Audio normalization finished.");

                            // 正規化された動画ファイルの情報を取得する (オーディオストリームの情報が変化しているはずなのでそれの取得が目的)
                            var normalizedMovieInformation = GetMovieInformation(temporaryIntermediateFileFormat, temporaryIntermediateFile1, commandOptions.Verbose);

                            // 動画ファイルのメタデータを消去する

                            if (commandOptions.Verbose)
                            {
                                PrintInformationMessage("Start clearing metadata.");
                                if (temporaryIntermediateFileFormat is not null)
                                    PrintInformationMessage($"  Input file format: {temporaryIntermediateFileFormat}");
                                PrintInformationMessage($"  Input file format: \"{temporaryIntermediateFile1.FullName}\"");
                                if (temporaryIntermediateFileFormat is not null)
                                    PrintInformationMessage($"  Output file format: {temporaryIntermediateFileFormat}");
                                PrintInformationMessage($"  Output file format: \"{temporaryIntermediateFile2.FullName}\"");
                            }

                            ClearMetadata(
                                temporaryIntermediateFileFormat,
                                temporaryIntermediateFile1,
                                temporaryIntermediateFileFormat,
                                temporaryIntermediateFile2,
                                commandOptions.Verbose,
                                true);

                            if (commandOptions.Verbose)
                                PrintInformationMessage("Metadata clearing completed.");

                            // 動画ファイルのメタデータを復元する
                            if (commandOptions.Verbose)
                            {
                                PrintInformationMessage("Start restoring metadata.");
                                if (temporaryIntermediateFileFormat is not null)
                                    PrintInformationMessage($"  Input file format: {temporaryIntermediateFileFormat}");
                                PrintInformationMessage($"  Input file format: \"{temporaryIntermediateFile2.FullName}\"");
                                if (outputFormat is not null)
                                    PrintInformationMessage($"  Output file format: {outputFormat}");
                                PrintInformationMessage($"  Output file format: \"{outputFile.FullName}\"");
                            }

                            SetMetadata(
                                movieInformation,
                                normalizedMovieInformation,
                                temporaryIntermediateFileFormat,
                                temporaryIntermediateFile2,
                                outputFormat,
                                outputFile,
                                commandOptions.Verbose,
                                commandOptions.IsForceMode || temporaryOutputFile is not null);

                            if (commandOptions.Verbose)
                                PrintInformationMessage("Audio normalization finished.");
                        }

                        if (temporaryOutputFile is not null)
                        {
                            // 出力先が標準出力である場合

                            // 一時ファイルの内容を標準出力へ出力する
                            using var inStream = temporaryOutputFile.OpenRead();
                            using var outStream = TinyConsole.OpenStandardOutput();

                            if (commandOptions.Verbose)
                                PrintInformationMessage($"Copying from temporary file to standard output.: {temporaryOutputFile.FullName}");

                            CopyStream(inStream, outStream, inStream.Length);

                            if (commandOptions.Verbose)
                                PrintInformationMessage("Copy finished.");
                        }

                        // 終了する
                        return 0;
                    }
                    finally
                    {
                        // 一時ファイルが作成されていた場合は削除する

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

                        if (temporaryIntermediateFile1 is not null)
                        {
                            try
                            {
                                File.Delete(temporaryIntermediateFile1.FullName);
                                if (commandOptions.Verbose)
                                    PrintInformationMessage($"Temporary file is deleted.: \"{temporaryIntermediateFile1.FullName}\"");
                            }
                            catch (Exception)
                            {
                            }
                        }

                        if (temporaryIntermediateFile2 is not null)
                        {
                            try
                            {
                                File.Delete(temporaryIntermediateFile2.FullName);
                                if (commandOptions.Verbose)
                                    PrintInformationMessage($"Temporary file is deleted.: \"{temporaryIntermediateFile2.FullName}\"");
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
                    for (var e = ex; e is not null; e = e.InnerException)
                        PrintExceptionMessage(e);
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

        private static (string? inputFormat, FileInfo inputFilePath, FileInfo? inputTemporaryFilePath) GetInputMovieFile(CommandParameter commandParameters)
        {
            if (commandParameters.Input is not null)
            {
                // -i オプションが指定されている場合 (入力元がファイルである場合)

                if (commandParameters.Verbose)
                    PrintInformationMessage($"Input file path: \"{commandParameters.Input}\"");
                return (commandParameters.InputFormat, new FileInfo(commandParameters.Input), null);
            }
            else
            {
                // -i オプションが指定されていない場合 (入力元が標準入力である場合)

                var temporaryInputFile = new FileInfo(Path.GetTempFileName());
                if (commandParameters.Verbose)
                {
                    PrintInformationMessage("Read from standard input.");
                    PrintInformationMessage($"Temprary file is created.: \"{temporaryInputFile.FullName}\"");
                }

                try
                {
                    // 標準入力から一時ファイルへコピーする

                    using var inputStream = TinyConsole.OpenStandardInput();
                    using var outputStream = temporaryInputFile.Create();

                    if (commandParameters.Verbose)
                        PrintInformationMessage($"Copying from standard input to temporary file.: {temporaryInputFile.FullName}");

                    CopyStream(inputStream, outputStream);

                    if (commandParameters.Verbose)
                        PrintInformationMessage("Copy finished.");

                    return (commandParameters.InputFormat, temporaryInputFile, temporaryInputFile);
                }
                catch (Exception ex)
                {
                    try
                    {
                        File.Delete(temporaryInputFile.FullName);
                    }
                    catch (Exception)
                    {
                    }

                    throw new Exception("An error occurred while preparing the input file.", ex);
                }
            }
        }

        private static (string? outputFormat, FileInfo outputFilePath, FileInfo? outputTemporaryFilePath) GetOutputMovieFile(CommandParameter commandParameters)
        {
            if (commandParameters.Output is not null)
            {
                // -o オプションが指定されている場合 (出力先がファイルである場合)

                if (commandParameters.Verbose)
                    PrintInformationMessage($"Output file path: \"{commandParameters.Output}\"");
                return (commandParameters.OutputFormat, new FileInfo(commandParameters.Output), null);
            }
            else
            {
                // -o オプションが指定されていない場合 (出力先が標準出力である場合)

                var temporaryOutputFile = new FileInfo(Path.GetTempFileName());
                if (commandParameters.Verbose)
                {
                    PrintInformationMessage("Write to standard output.");
                    PrintInformationMessage($"Temprary file is created.: \"{temporaryOutputFile.FullName}\"");
                }

                return (commandParameters.OutputFormat, temporaryOutputFile, temporaryOutputFile);
            }
        }

        private static MovieInformation GetMovieInformation(string? inputFormat, FileInfo inputFile, bool verbose)
        {
            if (verbose)
                PrintInformationMessage($"Probe movie information.: \"{inputFile.FullName}\"");
            try
            {
                return
                    Command.GetMovieInformation(
                        inputFormat,
                        inputFile,
                        MovieInformationType.Chapters | MovieInformationType.Streams,
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

        private static void NormalizeAudio(MovieInformation movieInformation, string? inputFormat, FileInfo inputFile, string? outputFormat, FileInfo outputFile, bool verbose, bool doOverwrite)
        {
            var audioEncoder = "libopus";
            var audioFormat = "Opus";
            var audioEncoderOptions = Array.Empty<string>();

            // チャンネルレイアウトが libopus によってサポートされていないオーディオストリームを抽出する
            var audioStreamsNotSupportedByLibOpus =
                movieInformation.AudioStreams
                .Where(stream => stream.ChannelLayout.IsAnyOf("5.0(side)", "5.1(side)"))
                .ToList();

            if (audioStreamsNotSupportedByLibOpus.Any())
            {
                // チャンネルレイアウトが libopus によってサポートされていないオーディオストリームが存在する場合

                if (verbose)
                    PrintInformationMessage($"Normalize the audio stream and encode it with \"libovorbis\". Because some audio stream channel layouts are not supported by \"libopus\".: {string.Join(", ", audioStreamsNotSupportedByLibOpus.Select(stream => $"a:{stream.IndexWithinAudioStream}(\"{stream.ChannelLayout}\")"))}");

                // 代わりに libvorbis でエンコードする
                audioEncoder = "libvorbis";
                audioFormat = "Vorbis";

                // libvorbis のエンコーダオプションを作成する
                // (元のオーディオストリームの (チャネル数 * _bitratePerChannelForLibVorbis) を q 値に変換して指定する)
                audioEncoderOptions =
                    movieInformation.AudioStreams
                    .Select(stream => new
                    {
                        index = stream.IndexWithinAudioStream,
                        channels = stream.Channels,
                    })
                    .SelectMany(item => new[]
                    {
                        $"-q:a:{item.index}",
                        CalculateLibVorbisQualityByBitRate(item.channels * _bitratePerChannelForLibVorbis).ToString("F1"),
                    })
                    .ToArray();
            }
            else
            {
                // すべてのオーディオストリームのチャンネルレイアウトが libopus によってサポートされている場合

                if (verbose)
                    PrintInformationMessage("Normalize the audio stream and encode it by \"libopus\".");
            }

            var ffmpegCommandFilePath =
                ProcessUtility.WhereIs("ffmpeg")
                ?? throw new Exception("ffmpeg command not found.");
            Environment.SetEnvironmentVariable(_ffmpegPathEnvironmentVariableName, ffmpegCommandFilePath);
            var normalizeCommandFilePath =
                ProcessUtility.WhereIs("ffmpeg-normalize")
                ?? throw new Exception("ffmpeg-normalize command not found.");
            var normalizerCommandParameters = new List<string>();
            if (verbose)
                normalizerCommandParameters.Add("-v");
            if (doOverwrite)
                normalizerCommandParameters.Add("-f");
            normalizerCommandParameters.Add("-pr");
            normalizerCommandParameters.Add("--keep-loudness-range-target");
            if (inputFormat is not null)
                normalizerCommandParameters.Add($"-ei={string.Join(" ", new[] { "-f", $"\"{inputFormat}\"" }).CommandLineArgumentEncode()}");
            normalizerCommandParameters.Add(inputFile.FullName.CommandLineArgumentEncode());
            normalizerCommandParameters.Add($"-c:a {audioEncoder.CommandLineArgumentEncode()}");
            if (audioEncoderOptions.Any())
                normalizerCommandParameters.Add($"-e={string.Join(" ", audioEncoderOptions).CommandLineArgumentEncode()}");
            if (outputFormat is not null)
                normalizerCommandParameters.Add($"-ofmt {outputFormat.CommandLineArgumentEncode()}");
            normalizerCommandParameters.Add($"-o {outputFile.FullName.CommandLineArgumentEncode()}");
            var startInfo = new ProcessStartInfo
            {
                Arguments = string.Join(" ", normalizerCommandParameters),
                FileName = normalizeCommandFilePath,
                UseShellExecute = false,
            };

            if (verbose)
                PrintInformationMessage($"Execute: {startInfo.FileName} {startInfo.Arguments}");

            using var process =
                Process.Start(startInfo)
                ?? throw new Exception($"Could not start process. :\"{normalizeCommandFilePath}\"");
            process.WaitForExit();

            if (process.ExitCode != 0)
                throw new Exception($"An error occurred in the ffmpeg-normalize command.: exit-code={process.ExitCode}");

            if (verbose)
                PrintInformationMessage($"Audio streams normalization is complete.: audio-format={audioFormat}");

            static double CalculateLibVorbisQualityByBitRate(int bitRate)
            {
                // -1 <= q <= 4 : 16k * (q + 4) bps  (48k <= bps <= 128k)
                // 4 < q < 8    : 32k * q bps        (128k < bps < 256k)    
                // 8 <= q <= 10 : 64k * (q - 4) bps  (256k <= bps <= 384k)

                if (bitRate <= 48000)
                    return -1;
                else if (bitRate <= 128000)
                    return (double)bitRate / 16000 - 4;
                else if (bitRate <= 256000)
                    return (double)bitRate / 32000;
                else if (bitRate <= 384000)
                    return (double)bitRate / 64000 + 4;
                else
                    return 10.0;
            }
        }

        private static void ClearMetadata(
            string? inputFormat,
            FileInfo inputFile,
            string? outputFormat,
            FileInfo outputFile,
            bool verbose,
            bool doOverwrite)
        {
            var ffmpegCommandParameters =
                new List<string>
                {
                    "-hide_banner",
                };
            if (doOverwrite)
                ffmpegCommandParameters.Add("-y");
            if (inputFormat is not null)
                ffmpegCommandParameters.Add($"-f {inputFormat.CommandLineArgumentEncode()}");
            ffmpegCommandParameters.Add($"-i {inputFile.FullName.CommandLineArgumentEncode()}");
            ffmpegCommandParameters.Add("-c copy -map 0");
            ffmpegCommandParameters.Add("-map_chapters -1");
            ffmpegCommandParameters.Add("-map_metadata -1");
            if (outputFormat is not null)
                ffmpegCommandParameters.Add($"-f {outputFormat.CommandLineArgumentEncode()}");
            ffmpegCommandParameters.Add($"{outputFile.FullName.CommandLineArgumentEncode()}");

            var ffmpegCommandParameterText = string.Join(" ", ffmpegCommandParameters);
            if (verbose)
                PrintInformationMessage($"Execute: ffmpeg {ffmpegCommandParameterText}");
            var exitCode =
                Command.ExecuteFfmpeg(
                    ffmpegCommandParameterText,
                    null,
                    null,
                    TinyConsole.Error.WriteLine,
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
                    new Progress<double>(_ => { }));

            if (exitCode != 0)
                throw new Exception($"An error occurred in the \"ffmpeg\" command. : exit-code={exitCode}");
        }

        private static void SetMetadata(
            MovieInformation movieInformation,
            MovieInformation normalizedMovieInformation,
            string? inputFormat,
            FileInfo inputFile,
            string? outputFormat,
            FileInfo outputFile,
            bool verbose,
            bool doOverwrite)
        {
            var audioEncoders =
                normalizedMovieInformation.AudioStreams
                .ToDictionary(stream => stream.IndexWithinAudioStream, stream => stream.Tags.Encoder);

            var streams =
                movieInformation.VideoStreams
                .Select(stream => new
                {
                    streamTypeSymbol = "v",
                    index = stream.IndexWithinVideoStream,
                    isDefault = stream.Disposition.Default,
                    isForced = stream.Disposition.Forced,
                    title = stream.Tags.Title,
                    language = stream.Tags.Language,
                    encoder = stream.Tags.Encoder,
                })
                .Concat(
                    movieInformation.AudioStreams
                    .Select(stream => new
                    {
                        streamTypeSymbol = "a",
                        index = stream.IndexWithinAudioStream,
                        isDefault = stream.Disposition.Default,
                        isForced = stream.Disposition.Forced,
                        title = stream.Tags.Title,
                        language = stream.Tags.Language,
                        encoder = audioEncoders.TryGetValue(stream.IndexWithinAudioStream, out var encoderValue) ? encoderValue : null,
                    }))
                .Concat(
                    movieInformation.SubtitleStreams
                    .Select(stream => new
                    {
                        streamTypeSymbol = "s",
                        index = stream.IndexWithinSubtitleStream,
                        isDefault = stream.Disposition.Default,
                        isForced = stream.Disposition.Forced,
                        title = stream.Tags.Title,
                        language = stream.Tags.Language,
                        encoder = stream.Tags.Encoder,
                    }))
                .Concat(
                    movieInformation.DataStreams
                    .Select(stream => new
                    {
                        streamTypeSymbol = "d",
                        index = stream.IndexWithinDataStream,
                        isDefault = stream.Disposition.Default,
                        isForced = stream.Disposition.Forced,
                        title = stream.Tags.Title,
                        language = stream.Tags.Language,
                        encoder = stream.Tags.Encoder,
                    }))
                .Concat(
                    movieInformation.AttachmentStreams
                    .Select(stream => new
                    {
                        streamTypeSymbol = "t",
                        index = stream.IndexWithinAttachmentStream,
                        isDefault = stream.Disposition.Default,
                        isForced = stream.Disposition.Forced,
                        title = stream.Tags.Title,
                        language = stream.Tags.Language,
                        encoder = stream.Tags.Encoder,
                    }));

            var ffmpegCommandParameters =
                new List<string>
                {
                    "-hide_banner",
                };
            if (doOverwrite)
                ffmpegCommandParameters.Add("-y");
            if (inputFormat is not null)
                ffmpegCommandParameters.Add($"-f {inputFormat.CommandLineArgumentEncode()}");
            ffmpegCommandParameters.Add($"-i {inputFile.FullName.CommandLineArgumentEncode()}");
            ffmpegCommandParameters.Add("-f ffmetadata -i -");
            ffmpegCommandParameters.Add("-c copy -map 0");
            foreach (var stream in streams)
            {
                ffmpegCommandParameters.Add($"-metadata:s:{stream.streamTypeSymbol}:{stream.index} title={(stream.title ?? "").CommandLineArgumentEncode()}");
                ffmpegCommandParameters.Add($"-metadata:s:{stream.streamTypeSymbol}:{stream.index} language={(stream.language ?? "").CommandLineArgumentEncode()}");
                ffmpegCommandParameters.Add($"-metadata:s:{stream.streamTypeSymbol}:{stream.index} ENCODER={(stream.encoder ?? "").CommandLineArgumentEncode()}");
                ffmpegCommandParameters.Add($"-disposition:{stream.streamTypeSymbol}:{stream.index} {(stream.isDefault ? "+" : "-")}default{(stream.isForced ? "+" : "-")}forced");
            }

            ffmpegCommandParameters.Add("-map_chapters 1");
            if (outputFormat is not null)
                ffmpegCommandParameters.Add($"-f {outputFormat.CommandLineArgumentEncode()}");
            ffmpegCommandParameters.Add($"{outputFile.FullName.CommandLineArgumentEncode()}");

            var ffmpegCommandParameterText = string.Join(" ", ffmpegCommandParameters);
            if (verbose)
                PrintInformationMessage($"Execute: ffmpeg {ffmpegCommandParameterText}");
            using var inMetadataReader = new StringReader(movieInformation.Chapters.ToMetadataString());
            var exitCode =
                Command.ExecuteFfmpeg(
                    ffmpegCommandParameterText,
                    Command.GetTextInputRedirector(inMetadataReader.ReadLine),
                    null,
                    TinyConsole.Error.WriteLine,
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
                    new Progress<double>(_ => { }));

            if (exitCode != 0)
                throw new Exception($"An error occurred in the \"ffmpeg\" command. : exit-code={exitCode}");
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
