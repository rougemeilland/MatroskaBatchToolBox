using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MatroskaBatchToolBox.Utility.Interprocess;
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
                MusicFileEncoder = options.SingleOrNone(option => option.OptionType == OptionType.Encoder)?.OptionParameter[1] as string;
                MusicFileEncoderOption = options.SingleOrNone(option => option.OptionType == OptionType.EncoderOption)?.OptionParameter[1] as string;
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
                => new(optionDefinitions.ParseCommandArguments(args.AsReadOnlyArray()));
        }

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
                    // 入力のフォーマットを調べる
                    var inputMusicFileProvider =
                        new[]
                        {
                            new ID3MusicFileMetadataProvider(TransferDirection.Input, commandOptions.InputFormat, commandOptions.Input is not null ? Path.GetExtension(commandOptions.Input).ToLowerInvariant() : null) as IMusicFileMetadataProvider,
                            new FlacMusicFileMetadataProvider(TransferDirection.Input, commandOptions.InputFormat, commandOptions.Input is not null ? Path.GetExtension(commandOptions.Input).ToLowerInvariant() : null),
                            new OggMusicFileMetadataProvider(TransferDirection.Input, commandOptions.InputFormat, commandOptions.Input is not null ? Path.GetExtension(commandOptions.Input).ToLowerInvariant() : null),
                        }
                        .FirstOrDefault(provider => provider.Supported);

                    // 出力のフォーマットを調べる
                    var outputMusicFileProvider =
                        new[]
                        {
                            new ID3MusicFileMetadataProvider(TransferDirection.Output, commandOptions.OutputFormat, commandOptions.Output is not null ? Path.GetExtension(commandOptions.Output).ToLowerInvariant() : null) as IMusicFileMetadataProvider,
                            new FlacMusicFileMetadataProvider(TransferDirection.Output, commandOptions.OutputFormat, commandOptions.Output is not null ? Path.GetExtension(commandOptions.Output).ToLowerInvariant() : null),
                            new OggMusicFileMetadataProvider(TransferDirection.Output, commandOptions.OutputFormat, commandOptions.Output is not null ? Path.GetExtension(commandOptions.Output).ToLowerInvariant() : null),
                        }
                        .FirstOrDefault(provider => provider.Supported);

                    // 入力の準備をする (入力元が標準入力である場合は一時ファイルにコピーする)
                    var (inputFormat, inputFile, temporaryInputFile) = GetInputMovieFile(commandOptions);

                    // 出力の準備をする (出力先が標準出力である場合は一時ファイルを作成する)
                    var (outputFormat, outputFile, temporaryOutputFile) = GetOutputMovieFile(commandOptions);

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
                                PrintInformationMessage,
                                PrintWarningMessage,
                                PrintWarningMessage,
                                PrintErrorMessage,
                                PrintErrorMessage);

                        state.NormalizeFile(inputFormat, inputFile, outputFormat, outputFile);

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
