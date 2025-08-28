using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MatroskaBatchToolBox.Utility.Interprocess;
using Palmtree;
using Palmtree.IO;
using Palmtree.IO.Console;
using Palmtree.Linq;

namespace AudioNormalizer
{
    internal static partial class Program
    {
        private sealed class CommandParameter
        {
            private CommandParameter(IEnumerable<CommandOption<OptionType>> options)
            {
                InputFormat = options.SingleOrNone(option => option.OptionType == OptionType.InputFormat)?.OptionParameter.Span[1] as string;
                Input = options.SingleOrNone(option => option.OptionType == OptionType.Input)?.OptionParameter.Span[1] as string;
                OutputFormat = options.SingleOrNone(option => option.OptionType == OptionType.OutputFormat)?.OptionParameter.Span[1] as string;
                Output = options.SingleOrNone(option => option.OptionType == OptionType.Output)?.OptionParameter.Span[1] as string;
                MusicFileEncoder = options.SingleOrNone(option => option.OptionType == OptionType.Encoder)?.OptionParameter.Span[1] as string;
                MusicFileEncoderOption = options.SingleOrNone(option => option.OptionType == OptionType.EncoderOption)?.OptionParameter.Span[1] as string;
                IsForceMode = options.Any(option => option.OptionType == OptionType.Force);
                Verbose = options.Any(option => option.OptionType == OptionType.Verbose);
                DisableVideoStream = options.Any(option => option.OptionType == OptionType.DisableVideoStream);
                IsHelpMode = options.Any(option => option.OptionType == OptionType.Help);
            }

            public string? InputFormat { get; }
            public string? Input { get; }
            public string? OutputFormat { get; }
            public string? Output { get; }
            public string? MusicFileEncoder { get; }
            public string? MusicFileEncoderOption { get; }
            public bool IsForceMode { get; }
            public bool Verbose { get; }
            public bool DisableVideoStream { get; }
            public bool IsHelpMode { get; }

            public static CommandParameter Parse(IEnumerable<CommandOptionDefinition<OptionType>> optionDefinitions, string[] args)
                => new(optionDefinitions.ParseCommandArguments(args));
        }

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
                            TinyConsole.WriteLog(LogCategory.Error, $"The input file specified by the \"--input\" option does not exist.: \"{commandOptions.Input}\"");
                            return 1;
                        }
                    }
                    catch (Exception ex)
                    {
                        TinyConsole.WriteLog(LogCategory.Error, $"Failed to access the input file specified by the \"--input\" option.: \"{commandOptions.Input}\"");
                        TinyConsole.WriteLog(ex);
                        return 1;
                    }
                }

                // -o オプションが指定されており、かつ -f オプションが指定されていない場合は、-o オプションのファイルが存在してはならない。
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

                // -help オプションが指定されている場合は、ヘルプテキストを表示して終了する。
                if (commandOptions.IsHelpMode)
                {
                    PrintHelpMessage();
                    return 0;
                }

                if (commandOptions.Verbose)
                    TinyConsole.WriteLog(LogCategory.Information, "Start processing.");

                var success = false;
                try
                {
                    // 入力のフォーマットを調べる
                    var inputMusicFileProvider =
                        new[]
                        {
                            new ID3MusicFileMetadataProvider(TransferDirection.Input, commandOptions.InputFormat, commandOptions.Input is not null ? Path.GetExtension(commandOptions.Input).ToLowerInvariant() : null) as IMusicFileMetadataProvider,
                            new FlacMusicFileMetadataProvider(TransferDirection.Input, commandOptions.InputFormat, commandOptions.Input is not null ? Path.GetExtension(commandOptions.Input).ToLowerInvariant() : null),
                            new OggMusicFileMetadataProvider(TransferDirection.Input, commandOptions.InputFormat, commandOptions.Input is not null ? Path.GetExtension(commandOptions.Input).ToLowerInvariant() : null),
                            new Mp4MusicFileMetadataProvider(TransferDirection.Input, commandOptions.InputFormat, commandOptions.Input is not null ? Path.GetExtension(commandOptions.Input).ToLowerInvariant() : null),
                        }
                        .FirstOrDefault(provider => provider.Supported);

                    // 出力のフォーマットを調べる
                    var outputMusicFileProvider =
                        new[]
                        {
                            new ID3MusicFileMetadataProvider(TransferDirection.Output, commandOptions.OutputFormat, commandOptions.Output is not null ? Path.GetExtension(commandOptions.Output).ToLowerInvariant() : null) as IMusicFileMetadataProvider,
                            new FlacMusicFileMetadataProvider(TransferDirection.Output, commandOptions.OutputFormat, commandOptions.Output is not null ? Path.GetExtension(commandOptions.Output).ToLowerInvariant() : null),
                            new OggMusicFileMetadataProvider(TransferDirection.Output, commandOptions.OutputFormat, commandOptions.Output is not null ? Path.GetExtension(commandOptions.Output).ToLowerInvariant() : null),
                            new Mp4MusicFileMetadataProvider(TransferDirection.Output, commandOptions.OutputFormat, commandOptions.Output is not null ? Path.GetExtension(commandOptions.Output).ToLowerInvariant() : null),
                        }
                        .FirstOrDefault(provider => provider.Supported);

                    // 入力の準備をする (入力元が標準入力である場合は一時ファイルにコピーする)
                    var (inputFormat, inputFile, temporaryInputFile) = GetInputMovieFile(commandOptions, inputMusicFileProvider);

                    // 出力の準備をする (出力先が標準出力である場合は一時ファイルを作成する)
                    var (outputFormat, outputFile, temporaryOutputFile) = GetOutputMovieFile(commandOptions, outputMusicFileProvider);

                    try
                    {
                        var state =
                            new NormalizationState(
                                inputMusicFileProvider,
                                outputMusicFileProvider,
                                commandOptions.MusicFileEncoder,
                                commandOptions.MusicFileEncoderOption,
                                temporaryOutputFile is not null || commandOptions.IsForceMode,
                                commandOptions.Verbose,
                                commandOptions.DisableVideoStream,
                                CopyStream,
                                TinyConsole.WriteLog,
                                TinyConsole.WriteLog);

                        state.NormalizeFile(inputFormat, inputFile, outputFormat, outputFile);

                        if (temporaryOutputFile is not null)
                        {
                            // 出力先が標準出力である場合

                            // 一時ファイルの内容を標準出力へ出力する
                            using var inStream = temporaryOutputFile.OpenRead();
                            using var outStream = TinyConsole.OpenStandardOutput();

                            if (commandOptions.Verbose)
                                TinyConsole.WriteLog(LogCategory.Information, $"Copying from temporary file to standard output.: {temporaryOutputFile.FullName}");

                            CopyStream(inStream, outStream, inStream.Length);

                            if (commandOptions.Verbose)
                                TinyConsole.WriteLog(LogCategory.Information, "Copy finished.");
                        }

                        // 終了する
                        success = true;
                        return 0;
                    }
                    finally
                    {
                        // 一時ファイルが作成されていた場合は削除する
                        if (temporaryInputFile is not null)
                        {
                            temporaryInputFile.SafetyDelete();
                            if (commandOptions.Verbose)
                                TinyConsole.WriteLog(LogCategory.Information, $"Temporary file is deleted.: \"{temporaryInputFile.FullName}\"");
                        }

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

        private static CommandParameter? ParseCommandOptions(string[] args)
        {
            try
            {
                return CommandParameter.Parse(_optionDefinitions, args);
            }
            catch (InvalidCommandOptionException ex)
            {
                for (var e = ex as Exception; e is not null; e = e.InnerException)
                    TinyConsole.WriteLog(LogCategory.Error, e.Message);
                return null;
            }
        }

        private static (string? inputFormat, FilePath inputFilePath, FilePath? inputTemporaryFilePath) GetInputMovieFile(CommandParameter commandParameters, IMusicFileMetadataProvider? musicFileMetadataProvider)
        {
            if (commandParameters.Input is not null)
            {
                // -i オプションが指定されている場合 (入力元がファイルである場合)

                if (commandParameters.Verbose)
                    TinyConsole.WriteLog(LogCategory.Information, $"Input file path: \"{commandParameters.Input}\"");
                return (commandParameters.InputFormat, new FilePath(commandParameters.Input), null);
            }
            else
            {
                // -i オプションが指定されていない場合 (入力元が標準入力である場合)

                var temporaryInputFile = FilePath.CreateTemporaryFile(suffix: musicFileMetadataProvider?.DefaultExtension ?? ".tmp");
                if (commandParameters.Verbose)
                {
                    TinyConsole.WriteLog(LogCategory.Information, "Read from standard input.");
                    TinyConsole.WriteLog(LogCategory.Information, $"Temprary file is created.: \"{temporaryInputFile.FullName}\"");
                }

                try
                {
                    // 標準入力から一時ファイルへコピーする

                    using var inputStream = TinyConsole.OpenStandardInput();
                    using var outputStream = temporaryInputFile.Create();

                    if (commandParameters.Verbose)
                        TinyConsole.WriteLog(LogCategory.Information, $"Copying from standard input to temporary file.: {temporaryInputFile.FullName}");

                    CopyStream(inputStream, outputStream);

                    if (commandParameters.Verbose)
                        TinyConsole.WriteLog(LogCategory.Information, "Copy finished.");

                    return (commandParameters.InputFormat, temporaryInputFile, temporaryInputFile);
                }
                catch (Exception ex)
                {
                    temporaryInputFile.SafetyDelete();
                    if (commandParameters.Verbose)
                        TinyConsole.WriteLog(LogCategory.Information, $"Temporary file is deleted.: \"{temporaryInputFile.FullName}\"");
                    throw new ApplicationException("An error occurred while preparing the input file.", ex);
                }
            }
        }

        private static (string? outputFormat, FilePath outputFilePath, FilePath? outputTemporaryFilePath) GetOutputMovieFile(CommandParameter commandParameters, IMusicFileMetadataProvider? musicFileMetadataProvider)
        {
            if (commandParameters.Output is not null)
            {
                // -o オプションが指定されている場合 (出力先がファイルである場合)

                if (commandParameters.Verbose)
                    TinyConsole.WriteLog(LogCategory.Information, $"Output file path: \"{commandParameters.Output}\"");
                return (commandParameters.OutputFormat, new FilePath(commandParameters.Output), null);
            }
            else
            {
                // -o オプションが指定されていない場合 (出力先が標準出力である場合)

                var temporaryOutputFile = FilePath.CreateTemporaryFile(suffix: musicFileMetadataProvider?.DefaultExtension ?? ".tmp");
                if (commandParameters.Verbose)
                {
                    TinyConsole.WriteLog(LogCategory.Information, "Write to standard output.");
                    TinyConsole.WriteLog(LogCategory.Information, $"Temprary file is created.: \"{temporaryOutputFile.FullName}\"");
                }

                return (commandParameters.OutputFormat, temporaryOutputFile, temporaryOutputFile);
            }
        }

        private static void CopyStream(ISequentialInputByteStream instream, ISequentialOutputByteStream outstream)
        {
            try
            {
                var state = 0;
                TinyConsole.CursorVisible = ConsoleCursorVisiblity.Invisible;
                instream.CopyTo(
                    outstream,
                    new Progress<ulong>(CopiedLength =>
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

        private static void CopyStream(ISequentialInputByteStream instream, ISequentialOutputByteStream outstream, ulong copyLength)
        {
            try
            {
                TinyConsole.CursorVisible = ConsoleCursorVisiblity.Invisible;
                instream.CopyTo(
                    outstream,
                    new Progress<ulong>(CopiedLength =>
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
                    $"{Validation.DefaultApplicationName} <option list>",
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
    }
}
