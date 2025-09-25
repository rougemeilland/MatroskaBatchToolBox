using System;
using System.Collections.Generic;
using System.IO;
using MatroskaBatchToolBox.Utility;
using Palmtree;
using Palmtree.IO;

namespace ConcatMovie
{
    internal sealed class CommandParameters
    {
        private sealed class HelpInfo
        {
            public ReadOnlyMemory<string> OptionSpecs { get; }
            public ReadOnlyMemory<string> Description { get; }

            public HelpInfo(string optionSpec, ReadOnlyMemory<string> description)
                : this(new string[] { optionSpec }, description)
            { 
            }

            public HelpInfo(ReadOnlyMemory<string> optionSpecs, ReadOnlyMemory<string> description)
            {
                OptionSpecs = optionSpecs;
                Description = description;
            }
        }

        private static readonly ReadOnlyMemory<HelpInfo> _helpInfos =
            new HelpInfo[] 
            {
                new(
                    new[]{ "--input_format <format name>", "-if <format name>" }, 
                    new[]
                    {
                        "Specifies the format of the input movie file.",
                        "This is the same value as the \"-f\" option in the \"ffmpeg\" command.",
                        "If this option is not specified, the format of the input movie file will be inferred from the file extension of the input movie file specified with the \"--input\" option.",
                        "This option affects subsequent \"--input\" options; therefore, this option must be specified before the \"--input\" option.",
                    }),
                new(
                    new string[]{"--input <input file path>", "-i <input file path>" },
                    new[]
                    {
                        "Specifies the path name of the input movie file.",
                    }),
                new(
                    new[]{"--output_format <format name>", "-of <format name>" },
                    new[]
                    {
                        "Specifies the format of the output movie file.",
                        "This is the same value that can be specified with the \"-f\" option of the \"ffmpeg\" command.",
                        "If this option is omitted, the format of the movie file is guessed from the extension of the output movie file specified by the \"--output\" option.",
                        "Note that this option must be specified if the \"--output\" option is omitted and the movie file is output to standard output.",
                    }),
                new(
                    new[]{"--output <output file path>", "-o <output file path>" },
                    new[]
                    {
                        "Specifies the path name of the output movie file.",
                        "If this option is omitted, the movie file will be written to standard output.",
                    }),
                new(
                    new[]{"--force", "-f" },
                    new[]
                    {
                        "Overwrite the output file if it already exists.",
                        "If this option is not specified, an error will occur if the output file already exists.",
                    }),
                new(
                    new[]{"--duration <movie duration>", "-t  <movie duration>" },
                    new[]
                    {
                        "Specifies the length of the input movie file in time.",
                        "If this option is not specified, the duration of the input movieo file will be adjusted to match the longest stream in the input file.",
                        "This option affects subsequent \"--input\" options, so it must be specified before the \"--input\" option.",
                    }),
                new(
                    new[]{"--verbose", "-v" },
                    new[]
                    {
                        "More information will be displayed.",
                    }),
                new(
                    "--ffmpeg_options",
                    new[]
                    {
                        "You can specify \"ffmpeg\" options directly.",
                        "All arguments following \"--ffmpeg_options\" will be treated as \"ffmpeg\" options.",
                    }),
                new(
                    "-help",
                    new[]
                    {
                        "Prints this document to standard output."
                    }),
                };

        public CommandParameters(string[] args)
        {
            var inputFileSpecifications = new List<InputFileSpecification>();
            var currentInputFileFormat = (string?)null;
            var currentInputFileDuration = (TimeSpan?)null;
            FfmpegOptions = Array.Empty<string>();
            IsForceMode = false;
            Verbose = false;
            IsHelpMode = false;
            for (var index = 0; index < args.Length; ++index)
            {
                var arg = args[index];
                switch (arg)
                {
                    case "-i":
                    case "--input":
                    {
                        if (index + 1 >= args.Length)
                            throw new ApplicationException("Input file path is not specified using \"--input\" option.");
                        var inputFile = CreateFilePath(args[index + 1]);
                        if (!inputFile.Exists)
                            throw new ApplicationException($"Input file does not exist.: \"{inputFile}\"");
                        inputFileSpecifications.Add(new InputFileSpecification(inputFile, currentInputFileFormat, currentInputFileDuration));
                        currentInputFileFormat = null;
                        currentInputFileDuration = null;
                        ++index;
                        break;
                    }
                    case "-if":
                    case "--input_format":
                        if (index + 1 >= args.Length)
                            throw new ApplicationException("Input file format is not specified using \"--input_format\" option.");
                        if (currentInputFileFormat is not null)
                            throw new ApplicationException("\"--input_format\" option is specified consecutively.");
                        currentInputFileFormat = args[index + 1];
                        ++index;
                        break;
                    case "-t":
                    case "--duration":
                        if (index + 1 >= args.Length)
                            throw new ApplicationException("Input file duration time is not specified using \"--duration\" option.");
                        if (currentInputFileDuration is not null)
                            throw new ApplicationException("\"--duration\" option is specified consecutively.");
                        try
                        {
                            currentInputFileDuration = args[index + 1].ParseAsTimeSpan(TimeParsingMode.Expression);
                        }
                        catch (Exception ex)
                        {
                            throw new ApplicationException("Invalid duration time format.", ex);
                        }

                        ++index;
                        break;
                    case "-o":
                    case "--output":
                        if (index + 1 >= args.Length)
                            throw new ApplicationException("Output file path is not specified using \"--output\" option.");
                        if (Output is not null)
                            throw new ApplicationException("Multiple output file paths is specified.");
                        Output = args[index + 1];
                        ++index;
                        break;
                    case "-of":
                    case "--output_format":
                        if (index + 1 >= args.Length)
                            throw new ApplicationException("Output file path is not specified using \"--output_format\" option.");
                        if (OutputFormat is not null)
                            throw new ApplicationException("Multiple output file formats is specified.");
                        OutputFormat = args[index + 1];
                        ++index;
                        break;
                    case "-f":
                    case "--force":
                        IsForceMode = true;
                        break;
                    case "-v":
                    case "--verbose":
                        Verbose = true;
                        break;
                    case "-help":
                        if (args.Length != 1)
                            throw new ApplicationException("\"-help\" opushon ga shitei sa reta baai wa, hoka no opushon ga shite sa reru koto wa dekimasen.\r\nIf the \"-help\" option is specified, no other options can be used.");
                        IsHelpMode = true;
                        break;
                    case "--ffmpeg_options":
                        var ffmpegOptions = new string[args.Length - (index + 1)];
                        args.AsSpan(index + 1).CopyTo(ffmpegOptions);
                        FfmpegOptions = ffmpegOptions;
                        index = args.Length;
                        break;
                    default:
                        throw Validation.GetFailErrorException();
                }
            }

            if (inputFileSpecifications.Count <= 0)
                throw new ApplicationException("No \"--input\" option has been specified.");
            if (Output is null && OutputFormat is null)
                throw new ApplicationException("Neither the \"--output\" option nor the \"--output_format\" option is specified.");

            InputFiles = inputFileSpecifications.ToArray();

            static FilePath CreateFilePath(string s)
            {
                try
                {
                    return new FilePath(s);
                }
                catch (Exception ex)
                {
                    throw new ApplicationException("Invalid file path name.", ex);
                }
            }
        }

        public ReadOnlyMemory<InputFileSpecification> InputFiles { get; }
        public string? OutputFormat { get; }
        public string? Output { get; }
        public bool IsForceMode { get; }
        public bool Verbose { get; }
        public ReadOnlyMemory<string> FfmpegOptions { get; }
        public bool IsHelpMode { get; }

        public static void OutputHelpText(TextWriter writer)
        {
            writer.WriteLine("[Usage]");
            writer.WriteLine($"{Validation.DefaultApplicationName} <option list>");
            writer.WriteLine();
            writer.WriteLine("[Options]");
            var isFirst = true;
            foreach (var helpInfo in _helpInfos.Span)
            {
                if (!isFirst)
                    writer.WriteLine();
                isFirst = false;
                writer.WriteLine($"  * {string.Join("  or  ", helpInfo.OptionSpecs)}");
                foreach (var text in helpInfo.Description.Span)
                    writer.WriteLine($"    {text}");
            }
        }
    }
}
