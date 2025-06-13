using System.Collections.Generic;
using System.Linq;
using MatroskaBatchToolBox.Utility.Interprocess;
using Palmtree.Linq;

namespace AudioNormalizer
{
    internal static partial class Program
    {
        private static readonly IEnumerable<CommandOptionDefinition<OptionType>> _optionDefinitions
            = new CommandOptionDefinition<OptionType>[]
            {
                new StraightStringCommandOptionDefinition<OptionType>(
                    OptionType.InputFormat,
                     // エイリアスも含めたオプション名の配列
                    new string[]{ "-if", "--input_format" },
                    // 追加引数の数
                    1,
                    // 引数の変換
                    (index, arg, optionName) => new[]{ arg },
                    // 同一オプションの重複のみ NG
                    (option, otherOpton) => !(otherOpton.OptionType == option.OptionType),
                    // --input オプションが指定されていない(==標準入力からの入力)、かつ --input_format オプションが指定されていない場合は NG
                    (options) =>
                    {
                        // -help オプションが含まれている場合は OK (そもそも他のオプションは指定できないため)
                        if (options.Any(option => option.OptionType == OptionType.Help))
                            return null;

                        // --input オプションが指定されている場合はOK (標準入力からの入力ではないため、--input_format の指定は必須ではない)
                        if (options.Any(option => option.OptionType == OptionType.Input))
                            return null;

                        // --input_format オプションが指定されている場合はOK
                        if (options.Any(option => option.OptionType == OptionType.InputFormat))
                            return null;

                        // -help オプションが指定されておらず、かつ --input オプションが指定されておらず、かつ --input_format オプションが指定されていない場合

                        // 標準入力から入力される動画ファイルの形式を判別できないため、エラーとする。
                        return "The \"--input_format\" option is not specified even though the \"--input\" option is not specified (that is, the input is from standard input).";
                    },
                    "--input_format <format name>  or  -if <format name>",
                    new[]
                    {
                        "Specifies the format of the input movie file.",
                        "This is the same value that can be specified with the \"-f\" option of the \"ffmpeg\" command.",
                        "If this option is omitted, the format of the movie file is guessed from the extension of the input movie file specified by the \"--input\" option.",
                        "Note that this option must be specified if the \"--input\" option is omitted and the movie file is input from standard input.",
                    }),

                new StraightStringCommandOptionDefinition<OptionType>(
                    OptionType.Input,
                     // エイリアスも含めたオプション名の配列
                    new string[]{ "-i", "--input" },
                    // 追加引数の数
                    1,
                    // 引数の変換
                    (index, arg, optionName) => new[]{ arg },
                    // 同一オプションの重複のみ NG
                    (option, otherOpton) => !(otherOpton.OptionType == option.OptionType),
                    // 常に OK (必須ではない)
                    (options) => null,
                    "--input <input file path>  or  -i <input file path>",
                    new[]
                    {
                        "Specifies the path name of the input movie file.",
                        "If this option is omitted, the movie file is read from standard input.",
                    }),

                new StraightStringCommandOptionDefinition<OptionType>(
                    OptionType.OutputFormat,
                     // エイリアスも含めたオプション名の配列
                    new string[]{ "-of", "--output_format" },
                    // 追加引数の数
                    1,
                    // 引数の変換
                    (index, arg, optionName) => new[]{ arg },
                    // 同一オプションの重複のみ NG
                    (option, otherOpton) => !(otherOpton.OptionType == option.OptionType),
                    // --output オプションが指定されていない(==標準出力へ出力)、かつ --output_format オプションが指定されていない場合は NG
                    (options) =>
                    {
                        // -help オプションが含まれている場合は OK (そもそも他のオプションは指定できないため)
                        if (options.Any(option => option.OptionType == OptionType.Help))
                            return null;

                        // --output オプションが指定されている場合はOK (標準出力への出力ではないため、--output_format の指定は必須ではない)
                        if (options.Any(option => option.OptionType == OptionType.Output))
                            return null;

                        // --output_format オプションが指定されている場合はOK
                        if (options.Any(option => option.OptionType == OptionType.OutputFormat))
                            return null;

                        // -help オプションが指定されておらず、かつ --output オプションが指定されておらず、かつ --output_format オプションが指定されていない場合

                        // 標準出力へ出力する動画ファイルの形式を判別できないため、エラーとする。
                        return "The \"--output_format\" option is not specified even though the \"--output\" option is not specified (i.e. output to standard output).";
                    },
                    "--output_format <format name>  or  -of <format name>",
                    new[]
                    {
                        "Specifies the format of the output movie file.",
                        "This is the same value that can be specified with the \"-f\" option of the \"ffmpeg\" command.",
                        "If this option is omitted, the format of the movie file is guessed from the extension of the output movie file specified by the \"--output\" option.",
                        "Note that this option must be specified if the \"--output\" option is omitted and the movie file is output to standard output.",
                    }),

                new StraightStringCommandOptionDefinition<OptionType>(
                    OptionType.Output,
                     // エイリアスも含めたオプション名の配列
                    new string[]{ "-o", "--output" },
                    // 追加引数の数
                    1,
                    // 引数の変換
                    (index, arg, optionName) => new[]{ arg },
                    // 同一オプションの重複のみ NG
                    (option, otherOpton) => !(otherOpton.OptionType == option.OptionType),
                    // 常に OK (必須ではない)
                    (options) => null,
                    "--output <output file path>  or  -o <output file path>",
                    new[]
                    {
                        "Specifies the path name of the output movie file.",
                        "If this option is omitted, the movie file will be written to standard output.",
                    }),

                new StraightStringCommandOptionDefinition<OptionType>(
                    OptionType.Encoder,
                     // エイリアスも含めたオプション名の配列
                    new string[]{ "-e", "--encoder" },
                    // 追加引数の数
                    1,
                    // 引数の変換
                    (index, arg, optionName) => new[]{ arg },
                    // 同一オプションの重複のみ NG
                    (option, otherOpton) => !(otherOpton.OptionType == option.OptionType),
                    // 常に OK (必須ではない)
                    (options) => null,
                    "--encoder <music file encoder>  or  -e <music file encoder>",
                    new[]
                    {
                        "Specifies the encoder of the music file. (e.g. libopus/pcm_s16le/…)",
                        "The default value depends on the output file format (\"--output_format\") and the extension of the output file.",
                    }),

                new StraightStringCommandOptionDefinition<OptionType>(
                    OptionType.EncoderOption,
                     // エイリアスも含めたオプション名の配列
                    new string[]{ "-eop", "--encoder_option" },
                    // 追加引数の数
                    1,
                    // 引数の変換
                    (index, arg, optionName) => new[]{ arg },
                    // 同一オプションの重複のみ NG
                    (option, otherOpton) => !(otherOpton.OptionType == option.OptionType),
                    // "--encoder" オプションとともに指定する必要がある
                    (options)
                        => options.None(option => option.OptionType == OptionType.Encoder) && options.Any(option => option.OptionType == OptionType.EncoderOption)
                            ? "The ”--encoder_option” option must be specified with the \"--encoder\" option."
                            : null,
                    "--encoder_option <music file encoder option>  or  -eop <music file encoder option>",
                    new[]
                    {
                        "Specifies encoding options for music files.",
                    }),

                new StraightStringCommandOptionDefinition<OptionType>(
                    OptionType.Force,
                     // エイリアスも含めたオプション名の配列
                    new string[]{ "-f", "--force" },
                    // 同一オプションの重複のみ NG
                    (option, otherOpton) => !(otherOpton.OptionType == option.OptionType),
                    // 常に OK (必須ではない)
                    (options) => null,
                    "--force  or  -f",
                    new[]
                    {
                        "Overwrite the output file if it already exists.",
                        "If this option is not specified, an error will occur if the output file already exists.",
                    }),

                new StraightStringCommandOptionDefinition<OptionType>(
                    OptionType.Verbose,
                     // エイリアスも含めたオプション名の配列
                    new string[]{ "-v", "--verbose" },
                    // 同一オプションの重複のみ NG
                    (option, otherOpton) => !(otherOpton.OptionType == option.OptionType),
                    // 常に OK (必須ではない)
                    (options) => null,
                    "--verbose  or  -v",
                    new[]
                    {
                        "More information will be displayed.",
                    }),

                new StraightStringCommandOptionDefinition<OptionType>(
                    OptionType.DisableVideoStream,
                     // エイリアスも含めたオプション名の配列
                    new string[]{ "-vn", "--video_disable" },
                    // 同一オプションの重複のみ NG
                    (option, otherOpton) => !(otherOpton.OptionType == option.OptionType),
                    // 常に OK (必須ではない)
                    (options) => null,
                    "--video_disable  or  -vn",
                    new[]
                    {
                        "Strip the video stream during conversion.",
                        "For example, specify this option if you want to remove cover art when converting music files.",
                        "   * Some versions of ffmpeg have an error when converting music files with cover art to .opus/.ogg format.",
                        "     You can use this option to work around that problem.",
                    }),

                new StraightStringCommandOptionDefinition<OptionType>(
                    OptionType.Help,
                     // オプション名
                    "-help",
                    // 常にNG (同一オプションの重複または他のすべてのオプションとの混在がNG)
                    (option, otherOpton) => false,
                    // 常に OK (必須ではない)
                    (options) => null,
                    "-help",
                    new[]
                    {
                        "Prints this document to standard output."
                    }),
            };
    }
}
