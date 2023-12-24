using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MatroskaBatchToolBox.Utility.Interprocess
{
    /// <summary>
    /// オプションの名前を正規表現で比較するオプションの定義のクラスです。
    /// </summary>
    /// <typeparam name="COMMAND_OPTION_TYPE_T">
    /// オプションを識別するための列挙体です。
    /// </typeparam>
    public class RegexCommandOptionDefinition<COMMAND_OPTION_TYPE_T>
        : CommandOptionDefinition<COMMAND_OPTION_TYPE_T>
        where COMMAND_OPTION_TYPE_T : Enum
    {
        private readonly Regex _optionNamePattern;

        /// <summary>
        /// 追加引数がないオプションの定義オブジェクトを初期化するコンストラクタです。
        /// </summary>
        /// <param name="optionType">
        /// オプションを識別する値です。
        /// </param>
        /// <param name="optionNamePattern">
        /// オプションの名前と一致する正規表現です。
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
        public RegexCommandOptionDefinition(
            COMMAND_OPTION_TYPE_T optionType,
            Regex optionNamePattern,
            Func<int, string, string, IEnumerable<object>> argumentConverter,
            Func<CommandOption<COMMAND_OPTION_TYPE_T>, CommandOption<COMMAND_OPTION_TYPE_T>, bool> exclusiveOptionChecker,
            Func<IEnumerable<CommandOption<COMMAND_OPTION_TYPE_T>>, string?> requiredOptionChecker,
            string syntaxTextForHelp,
            IEnumerable<string> descriptionTextLinesForHelp)
            : this(optionType, optionNamePattern, 0, argumentConverter, exclusiveOptionChecker, requiredOptionChecker, syntaxTextForHelp, descriptionTextLinesForHelp)
        {
        }

        /// <summary>
        /// オプションの定義オブジェクトを初期化するコンストラクタです。
        /// </summary>
        /// <param name="optionType">
        /// オプションを識別する値です。
        /// </param>
        /// <param name="optionNamePattern">
        /// オプションの名前と一致する正規表現です。
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
        public RegexCommandOptionDefinition(
            COMMAND_OPTION_TYPE_T optionType,
            Regex optionNamePattern,
            int optionParameters,
            Func<int, string, string, IEnumerable<object>> argumentConverter,
            Func<CommandOption<COMMAND_OPTION_TYPE_T>, CommandOption<COMMAND_OPTION_TYPE_T>, bool> exclusiveOptionChecker,
            Func<IEnumerable<CommandOption<COMMAND_OPTION_TYPE_T>>, string?> requiredOptionChecker,
            string syntaxTextForHelp,
            IEnumerable<string> descriptionTextLinesForHelp)
            : base(optionType, optionParameters, argumentConverter, exclusiveOptionChecker, requiredOptionChecker, syntaxTextForHelp, descriptionTextLinesForHelp)
        {
            _optionNamePattern = optionNamePattern;
        }

        /// <summary>
        /// 与えられた引数の文字列がオプションの名前とマッチするかどうかを調べます。
        /// </summary>
        /// <param name="argument">
        /// 引数の文字列です。
        /// </param>
        /// <returns>
        /// <paramref name="argument"/>で与えられた文字列がオプションの名前とマッチしていれば true、そうではない場合は false です。
        /// </returns>
        public override bool IsMatch(string argument)
            => _optionNamePattern.IsMatch(argument);
    }
}
