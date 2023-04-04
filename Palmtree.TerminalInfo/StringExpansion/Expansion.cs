using System;
using System.Text;

namespace Palmtree.Terminal.StringExpansion
{
    internal class Expansion
    {
        public static string ExpandArguments(string value, object[] args)
        {
            try
            {
                var (statementState, output) = ExpandArguments(new ExpansionState(value, args));
                return
                    statementState == ExpansionStatementState.FoundEndOfStresm
                    ? output
                    : throw new Exception($"Syntax error. Check whether \"%t\", \"%e\", or \"%;\" is written before \"%?\".: \"{value}\"");
            }
            catch (ExpansionStringSyntaxErrorExceptionException ex)
            {
                throw new ArgumentException($"Bad syntax for terminfo parameterized strings.: \"{value}\"", ex);
            }
            catch (ExpansionBadArgumentExceptionException ex)
            {
                throw new ArgumentException("Invalid number or types of arguments.", nameof(args), ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to expand string.: \"{value}\"", ex);
            }
        }

        private static (ExpansionStatementState statementState, string output) ExpandArguments(ExpansionState state)
        {
            var outputStringBuffer = new StringBuilder();

            while (true)
            {
                // '%' が見つかるまでそのまま出力する
                while (true)
                {
                    var c = state.ReadCharOrNull();
                    if (c is null)
                        return (ExpansionStatementState.FoundEndOfStresm, outputStringBuffer.ToString());
                    if (c == '%')
                        break;
                    _ = outputStringBuffer.Append(c);
                }

                // この時点で _reader の先頭は % の次の文字

                var code = state.ReadChar();
                switch (code)
                {
                    // エスケープ文字を出力
                    case '%':
                        _ = outputStringBuffer.Append(code);
                        break;

                    // パラメタを push
                    case 'p':
                    {
                        var index = state.ReadChar();
                        if (index.IsOutOfClosedInterval('1', '9'))
                            throw new Exception($"The index is out of range.: {index}");

                        state.Push(state.Arguments[index]);
                        break;
                    }

                    // pop して変数に設定
                    case 'P':
                    {
                        var c = state.ReadChar();
                        if (c.IsInClosedInterval('a', 'z'))
                            state.DynamicValues[c] = state.Pop();
                        else if (c.IsInClosedInterval('A', 'Z'))
                            state.StaticValues[c] = state.Pop();
                        else
                            throw new Exception($"Invalid variable name: '{c}'");
                        break;
                    }

                    // 変数の値を push
                    case 'g':
                    {
                        var c = state.ReadChar();
                        if (c.IsInClosedInterval('a', 'z'))
                            state.Push(state.DynamicValues[c]);
                        else if (c.IsInClosedInterval('A', 'Z'))
                            state.Push(state.StaticValues[c]);
                        else
                            throw new Exception($"Invalid variable name: '{c}'");
                        break;
                    }

                    // イミディエイト値をスタックに push
                    case '\'':
                    {
                        var c = state.ReadChar();
                        if (state.ReadChar() != '\'')
                            throw new Exception("Missing single quote at end of immediate character.");
                        state.Push(new ExpansionNumberParameter(c));
                        break;
                    }
                    case '{':
                    {
                        var valueBuffer = new StringBuilder();
                        while (true)
                        {
                            var c = state.ReadChar();
                            if (c == '}')
                                break;
                            if (c.IsOutOfClosedInterval('0', '9'))
                                throw new Exception($"A non-numeric character was specified in an immediate numeric value.: '{c}'");
                            _ = valueBuffer.Append(c);
                        }

                        if (!valueBuffer.ToString().TryParse(out int value))
                            throw new Exception("No immediate number specified.");

                        state.Push(new ExpansionNumberParameter(value));
                        break;
                    }

                    // スタックから pop して指定された書式に従って出力
                    case 'c':
                    case 'd':
                    case 'o':
                    case 's':
                    case 'u':
                    case 'x':
                    case 'X':
                    case '.':
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                    {
                        var formatSpec = ReadFormatSpec(state, code);
                        _ = outputStringBuffer.Append(state.Pop().Format(formatSpec));
                        break;
                    }

                    // 二項算術演算
                    case '+':
                        BinaryOperation(state, (p1, p2) => p1.AsNumber() + p2.AsNumber());
                        break;
                    case '-':
                        BinaryOperation(state, (p1, p2) => p1.AsNumber() - p2.AsNumber());
                        break;
                    case '*':
                        BinaryOperation(state, (p1, p2) => p1.AsNumber() * p2.AsNumber());
                        break;
                    case '/':
                        BinaryOperation(state, (p1, p2) => p1.AsNumber() / p2.AsNumber());
                        break;
                    case 'm':
                        BinaryOperation(state, (p1, p2) => p1.AsNumber() % p2.AsNumber());
                        break;

                    // 二項ビット演算
                    case '&':
                        BinaryOperation(state, (p1, p2) => p1.AsNumber() & p2.AsNumber());
                        break;
                    case '|':
                        BinaryOperation(state, (p1, p2) => p1.AsNumber() | p2.AsNumber());
                        break;
                    case '^':
                        BinaryOperation(state, (p1, p2) => p1.AsNumber() - p2.AsNumber());
                        break;

                    // 単項ビット演算
                    case '~':
                        UnaryOperation(state, p => ~p.AsNumber());
                        break;

                    // 二項比較演算
                    case '=':
                        BinaryOperation(state, (p1, p2) => p1.AsNumber() == p2.AsNumber());
                        break;
                    case '<':
                        BinaryOperation(state, (p1, p2) => p1.AsNumber() < p2.AsNumber());
                        break;
                    case '>':
                        BinaryOperation(state, (p1, p2) => p1.AsNumber() > p2.AsNumber());
                        break;

                    // 二項論理演算
                    case 'A':
                        BinaryOperation(state, (p1, p2) => p1.AsBool() && p2.AsBool());
                        break;
                    case 'O':
                        BinaryOperation(state, (p1, p2) => p1.AsBool() || p2.AsBool());
                        break;

                    // 単項論理演算
                    case '!':
                        UnaryOperation(state, p => !p.AsBool());
                        break;

                    // 単項文字列演算
                    case 'l':
                        UnaryOperation(state, p => p.AsString().Length);
                        break;

                    // 条件判断開始 (if)
                    case '?':
                        _ = outputStringBuffer.Append(ProcessIfStatement(state));
                        break;

                    case 't':
                        // 親ブロックの if 文の %t (then) を発見した場合
                        // StatementState.FoundThen とともに直ちに終了する
                        return (ExpansionStatementState.FoundThen, outputStringBuffer.ToString());

                    case 'e':
                        // 親ブロックの if 文の %e (else) を発見した場合
                        // StatementState.FoundElse とともに直ちに終了する
                        return (ExpansionStatementState.FoundElse, outputStringBuffer.ToString());

                    case ';':
                        // 親ブロックの if 文の %; (endif) を発見した場合
                        // StatementState.FoundEndIf とともに直ちに終了する
                        return (ExpansionStatementState.FoundEndIf, outputStringBuffer.ToString());

                    // 特殊演算
                    case 'i':
                    {
                        // %i が使われるということは少なくとも1つのパラメタが必要なはずなので、state.Arguments['1'] は必ず成功するはず
                        state.Arguments['1'] = new ExpansionNumberParameter(state.Arguments['1'].AsNumber() + 1);

                        // しかし、キャパビリティによっては第 2 パラメタが存在しないことがあるので、TryGet を使用する
                        if (state.Arguments.TryGet('2', out var secondArgument))
                            state.Arguments['2'] = new ExpansionNumberParameter(secondArgument.AsNumber() + 1);
                        break;
                    }

                    default:
                        throw new ArgumentException($"Unexpected escape character.: '{code}'");
                }
            }
        }

        private static void UnaryOperation(ExpansionState state, Func<ExpansionParameter, int> @operator)
        {
            var p = state.Pop();
            state.Push(new ExpansionNumberParameter(@operator(p)));
        }

        private static void UnaryOperation(ExpansionState state, Func<ExpansionParameter, bool> @operator)
        {
            var p = state.Pop();
            state.Push(new ExpansionNumberParameter(@operator(p)));
        }

        private static void BinaryOperation(ExpansionState state, Func<ExpansionParameter, ExpansionParameter, int> @operator)
        {
            var p2 = state.Pop();
            var p1 = state.Pop();
            state.Push(new ExpansionNumberParameter(@operator(p1, p2)));
        }

        private static void BinaryOperation(ExpansionState state, Func<ExpansionParameter, ExpansionParameter, bool> @operator)
        {
            var p2 = state.Pop();
            var p1 = state.Pop();
            state.Push(new ExpansionNumberParameter(@operator(p1, p2)));
        }

        private static string ReadFormatSpec(ExpansionState state, char code)
        {
            var valueBuffer = new StringBuilder();
            var c = code;
            while (true)
            {
                if (c.IsOutOfClosedInterval('0', '9'))
                    break;
                _ = valueBuffer.Append(c);
                c = state.ReadChar();
            }

            if (c == '.')
            {
                c = state.ReadChar();
                while (true)
                {
                    if (c.IsOutOfClosedInterval('0', '9'))
                        break;
                    _ = valueBuffer.Append(c);
                    c = state.ReadChar();
                }
            }

            if (c.IsNoneOf('c', 'd', 'o', 's', 'u', 'x', 'X'))
                throw new Exception("Invalid output spec.");
            _ = valueBuffer.Append(c);
            var formatSpec = valueBuffer.ToString();
            return formatSpec;
        }

        private static string ProcessIfStatement(ExpansionState state)
        {
            var outputStringBuffer = new StringBuilder();

            // この時点で reader の先頭は %? の次

            string? outputOfThen = null; // 条件が が true であった then 節の出力
            var isFirst = true;
            while (true)
            {
                // この時点で、reader の先頭は %? (if) か %e (else) の次

                var (statementState, output) = ExpandArguments(state);

                if (isFirst)
                    // 初回は %? (if) の後であり、%e (else) の後ではないので、出力内容をそのまま出力する
                    _ = outputStringBuffer.Append(output);

                // この時点で、reader の先頭は %t (then) か #; (endif) の次 

                if (statementState == ExpansionStatementState.FoundEndIf)
                {
                    // 最後に見つけたのが %; (endif) だった場合

                    if (isFirst)
                        // ループの初回 (つまり %? (if) の後) で %; (endif) が見つかるのは構文の誤り
                        throw new Exception("Found \"%;\" after \"%?\".");

                    // output は 最後の else 節の出力なので、最終的な出力が確定していない場合は output を最終的な出力とする

                    outputOfThen ??= output;

                    // if 文を終了する
                    break;
                }

                if (statementState != ExpansionStatementState.FoundThen)
                    // 最後に見つけたのが %t (then) でも %; (endif) でもない場合は構文エラー
                    throw new Exception($"Unexpected state: '{statementState}'");

                // 最後に見つけたのが %t (then) だった場合

                // スタックの最上位に条件節の結果が格納されているはずなので取り出す
                var condition = state.Pop().AsBool();

                // then 節を展開する
                (statementState, output) = ExpandArguments(state);

                if (condition && outputOfThen is null)
                    // 条件が true で、最終的な出力が未確定の場合、今回の出力を最終的な出力とする
                    outputOfThen = output;

                // この時点で reader の先頭は %e (else) または #; (endif) のはず。

                if (statementState == ExpansionStatementState.FoundEndIf)
                    // if 文を終了する
                    break;

                if (statementState != ExpansionStatementState.FoundElse)
                    // 最後に見つけたのが %e (else) でも %; (endif) でもない場合は構文エラー
                    throw new Exception($"Unexpected state: '{statementState}'");

                isFirst = false;
            }

            if (outputOfThen is not null)
                // if 文の出力が存在していたら出力する
                _ = outputStringBuffer.Append(outputOfThen);

            return outputStringBuffer.ToString();
        }
    }
}
