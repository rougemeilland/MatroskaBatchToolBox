using System;
using System.Collections.Generic;
using System.Linq;

namespace MatroskaBatchToolBox.Utility.Interprocess
{
    /// <summary>
    /// オプションの定義の基本クラスです。
    /// </summary>
    /// <typeparam name="COMMAND_OPTION_TYPE_T">
    /// オプションを識別するための列挙体です。
    /// </typeparam>
    public abstract class CommandOptionDefinition<COMMAND_OPTION_TYPE_T>
        where COMMAND_OPTION_TYPE_T : Enum
    {
        private readonly Func<int, string, string, IEnumerable<object>> _argumentConverter;
        private readonly Func<CommandOption<COMMAND_OPTION_TYPE_T>, CommandOption<COMMAND_OPTION_TYPE_T>, bool> _exclusiveOptionChecker;
        private readonly Func<IEnumerable<CommandOption<COMMAND_OPTION_TYPE_T>>, string?> _requiredOptionChecker;
        private readonly string _syntaxTextForHelp;
        private readonly IEnumerable<string> _descriptionTextLinesForHelp;

        /// <summary>
        /// オプションの定義オブジェクトを初期化するコンストラクタです。
        /// </summary>
        /// <param name="optionType">
        /// オプションを識別する値です。
        /// </param>
        /// <param name="optionParameters">
        /// オプションの追加引数の数です。
        /// </param>
        /// <param name="argumentConverter">
        /// オプションの引数を実際に使用する型に変換するメソッドのデリゲートです。
        /// コマンドラインの構文の誤りにより変換に失敗した場合には <see cref="InvalidCommandOptionException"/> 例外をスローしてください。
        /// </param>
        /// <param name="exclusiveOptionChecker">
        /// オプションと他のオプションの排他条件を満足しているかを調べるメソッドのデリゲートです。
        /// 排他条件を満たしていれば true, 排他条件に違反していれば false を返してください。
        /// </param>
        /// <param name="requiredOptionChecker">
        /// オプションの必須条件を満足しているかどうかを調べるメソッドのデリゲートです。
        /// 必須条件を満たしていれば null、必須条件に違反していればその情報のメッセージテキストを返してください。
        /// </param>
        /// <param name="syntaxTextForHelp">
        /// ヘルプ情報に表示されるオプションの構文です。
        /// </param>
        /// <param name="descriptionTextLinesForHelp">
        /// ヘルプ情報に表示されるオプションの説明の行のコレクションです。
        /// </param>
        protected CommandOptionDefinition(
            COMMAND_OPTION_TYPE_T optionType,
            int optionParameters,
            Func<int, string, string, IEnumerable<object>> argumentConverter,
            Func<CommandOption<COMMAND_OPTION_TYPE_T>, CommandOption<COMMAND_OPTION_TYPE_T>, bool> exclusiveOptionChecker,
            Func<IEnumerable<CommandOption<COMMAND_OPTION_TYPE_T>>, string?> requiredOptionChecker,
            string syntaxTextForHelp,
            IEnumerable<string> descriptionTextLinesForHelp)
        {
            OptionType = optionType;
            OptionParameters = optionParameters;
            _argumentConverter = argumentConverter;
            _exclusiveOptionChecker = exclusiveOptionChecker;
            _requiredOptionChecker = requiredOptionChecker;
            _syntaxTextForHelp = syntaxTextForHelp;
            _descriptionTextLinesForHelp = descriptionTextLinesForHelp.ToList();
        }

        /// <summary>
        /// オプションを識別する値です。
        /// </summary>
        public COMMAND_OPTION_TYPE_T OptionType { get; }

        /// <summary>
        /// オプションの追加引数の数です。
        /// </summary>
        public int OptionParameters { get; }

        /// <summary>
        /// 与えられた引数の文字列がオプションの名前とマッチするかどうかを調べます。
        /// </summary>
        /// <param name="argument">
        /// 引数の文字列です。
        /// </param>
        /// <returns>
        /// <paramref name="argument"/>で与えられた文字列がオプションの名前とマッチしていれば true、そうではない場合は false です。
        /// </returns>
        public abstract bool IsMatch(string argument);

        /// <summary>
        /// 与えられたオプション名と追加引数を解析してオプションオブジェクトを返します。
        /// </summary>
        /// <param name="optionName">
        /// オプションの名前です。
        /// </param>
        /// <param name="additionalArguments">
        /// 追加引数の配列です。
        /// </param>
        /// <returns>
        /// 解析されたオプションを示す <see cref="CommandOption{COMMAND_OPTION_TYPE_T}"/> オブジェクトです。
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="additionalArguments"/>の値が誤っています。
        /// </exception>
        public CommandOption<COMMAND_OPTION_TYPE_T> ParseArguments(string optionName, ReadOnlyMemory<string> additionalArguments)
            => additionalArguments.Length == OptionParameters
                ? new CommandOption<COMMAND_OPTION_TYPE_T>(
                            OptionType,
                            optionName,
                            additionalArguments.ToArray()
                                .Prepend(optionName)
                                .SelectMany((arg, index) => _argumentConverter(index, arg, optionName)))
                : throw new ArgumentException("Number of additional parameters does not match.", nameof(additionalArguments));

        /// <summary>
        /// オプションの排他条件を満たしているか検査します。
        /// </summary>
        /// <param name="option">
        /// オプションオブジェクトです。
        /// </param>
        /// <param name="otherOptions">
        /// もう片方のオプションオブジェクトです。
        /// </param>
        /// <exception cref="Exception">
        /// オプションの排他条件に違反しています。
        /// </exception>
        public void ValidateExclusiveOptions(CommandOption<COMMAND_OPTION_TYPE_T> option, IEnumerable<CommandOption<COMMAND_OPTION_TYPE_T>> otherOptions)
        {
            foreach (var otherOption in otherOptions)
            {
                if (!_exclusiveOptionChecker(option, otherOption))
                {
                    if (option.OptionName == otherOption.OptionName)
                        throw new InvalidCommandOptionException($"Multiple \"{option.OptionName}\" options cannot be specified.");
                    else
                        throw new InvalidCommandOptionException($"The \"{option.OptionName}\" option and \"{otherOption.OptionName}\" option cannot be specified at the same time.");
                }
            }
        }

        /// <summary>
        /// オプションの必須条件を満たしているか検査します。
        /// </summary>
        /// <param name="options">
        /// オプションオブジェクトのコレクションです。
        /// </param>
        /// <exception cref="InvalidCommandOptionException">
        /// オプションの必須条件に違反しています。
        /// </exception>
        public void ValidateOptionsRequirement(IEnumerable<CommandOption<COMMAND_OPTION_TYPE_T>> options)
        {
            var message = _requiredOptionChecker(options);
            if (message is not null)
                throw new InvalidCommandOptionException(message);
        }

        /// <summary>
        /// ヘルプ情報のための、オプションの構文を示すテキストを取得します。
        /// </summary>
        /// <returns>
        /// オプションの構文を示すテキストです。
        /// </returns>
        public string GetHelpSyntaxText()
            => _syntaxTextForHelp;

        /// <summary>
        /// ヘルプ情報のための、オプションの説明を示すテキスト行のコレクションを取得します。
        /// </summary>
        /// <returns>
        /// オプションの説明を示すテキスト行のコレクションです。
        /// </returns>
        public IEnumerable<string> GetHelpDescriptionTextLines()
            => _descriptionTextLinesForHelp;
    }
}
