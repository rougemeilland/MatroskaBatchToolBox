using System;
using System.Linq;
using Palmtree;

namespace Test.TinyConsole
{
    internal static class TestActions
    {
        public static TestItemDirection Test_前景色の変更(int index)
        {
            try
            {
                Palmtree.TinyConsole.Title = $"テスト{index}: 前景色の変更";
                Palmtree.TinyConsole.ResetColor();
                Palmtree.TinyConsole.Clear();
                PrintSystemMessage($"前景色の変更のテストを開始します。ENTER キーを押してください。");
                WaitForEnterKey();
                Palmtree.TinyConsole.Clear();

                foreach (var consoleColor in Enum.GetValues<ConsoleColor>())
                {
                    Palmtree.TinyConsole.Out.Write($"次の文字の前景色は{consoleColor}です。: ");
                    Palmtree.TinyConsole.ForegroundColor = consoleColor;
                    try
                    {
                        Palmtree.TinyConsole.Out.Write("この文字列は標準出力へ出力されています。");
                    }
                    finally
                    {
                        Palmtree.TinyConsole.ResetColor();
                        Palmtree.TinyConsole.Out.WriteLine();
                    }

                    Palmtree.TinyConsole.Error.Write($"次の文字の前景色は{consoleColor}です。: ");
                    Palmtree.TinyConsole.ForegroundColor = consoleColor;
                    try
                    {
                        Palmtree.TinyConsole.Error.Write("この文字列は標準エラー出力へ出力されています。");
                    }
                    finally
                    {
                        Palmtree.TinyConsole.ResetColor();
                        Palmtree.TinyConsole.Error.WriteLine();
                    }
                }

                PrintSystemMessage($"以下の点を確認して ENTER キーまたは矢印キーを押してください。");
                PrintSystemMessage($"・表示されている色が正しいこと");
                return WaitForEnterOrArrowKey();
            }
            finally
            {
                Palmtree.TinyConsole.ResetColor();
                Palmtree.TinyConsole.Clear();
            }
        }

        public static TestItemDirection Test_背景色の変更(int index)
        {
            try
            {
                Palmtree.TinyConsole.Title = $"テスト{index}: 背景色の変更";
                Palmtree.TinyConsole.ResetColor();
                Palmtree.TinyConsole.Clear();
                PrintSystemMessage($"背景色の変更のテストを開始します。ENTER キーを押してください。");
                WaitForEnterKey();
                Palmtree.TinyConsole.Clear();

                foreach (var consoleColor in Enum.GetValues<ConsoleColor>())
                {
                    Palmtree.TinyConsole.Out.Write($"次の文字の背景色は{consoleColor}です。: ");
                    Palmtree.TinyConsole.BackgroundColor = consoleColor;
                    try
                    {
                        Palmtree.TinyConsole.Out.Write("この文字列は標準出力へ出力されています。");
                    }
                    finally
                    {
                        Palmtree.TinyConsole.ResetColor();
                        Palmtree.TinyConsole.Out.WriteLine();
                    }

                    Palmtree.TinyConsole.Error.Write($"次の文字の背景色は{consoleColor}です。: ");
                    Palmtree.TinyConsole.BackgroundColor = consoleColor;
                    try
                    {
                        Palmtree.TinyConsole.Error.Write("この文字列は標準エラー出力へ出力されています。");
                    }
                    finally
                    {
                        Palmtree.TinyConsole.ResetColor();
                        Palmtree.TinyConsole.Error.WriteLine();
                    }
                }

                PrintSystemMessage($"以下の点を確認して ENTER キーまたは矢印キーを押してください。");
                PrintSystemMessage($"・表示されている色が正しいこと");
                return WaitForEnterOrArrowKey();
            }
            finally
            {
                Palmtree.TinyConsole.ResetColor();
                Palmtree.TinyConsole.Clear();
            }
        }

        public static TestItemDirection Test_カーソル可視の変更(int index)
        {
            try
            {
                Palmtree.TinyConsole.Title = $"テスト{index}: カーソル可視の変更";
                Palmtree.TinyConsole.ResetColor();
                Palmtree.TinyConsole.Clear();
                PrintSystemMessage($"カーソル可視の変更のテストを開始します。ENTER キーを押してください。");
                WaitForEnterKey();
                Palmtree.TinyConsole.Clear();

                Palmtree.TinyConsole.CursorVisible = ConsoleCursorVisiblity.Invisible;

                PrintSystemMessage($"カーソルを不可視に設定しました。");
                PrintSystemMessage($"以下の点を確認して ENTER キーを押してください。");
                PrintSystemMessage($"・カーソルが消えていること");
                WaitForEnterKey();

                Palmtree.TinyConsole.CursorVisible = ConsoleCursorVisiblity.HighVisibilityMode;

                PrintSystemMessage($"カーソルを強い可視に設定しました。");
                PrintSystemMessage($"以下の点を確認して ENTER キーを押してください。");
                PrintSystemMessage($"・カーソルがブロックに見えていること");
                WaitForEnterKey();

                Palmtree.TinyConsole.CursorVisible = ConsoleCursorVisiblity.NormalMode;

                PrintSystemMessage($"カーソルを可視に設定しました。");
                PrintSystemMessage($"以下の点を確認して ENTER キーまたは矢印キーを押してください。");
                PrintSystemMessage($"・カーソルが下線に見えていること");
                return WaitForEnterOrArrowKey();
            }
            finally
            {
                Palmtree.TinyConsole.ResetColor();
                Palmtree.TinyConsole.Clear();
                Palmtree.TinyConsole.CursorVisible = ConsoleCursorVisiblity.NormalMode;
            }
        }

        public static TestItemDirection Test_BEEP音の再生(int index)
        {
            try
            {
                Palmtree.TinyConsole.Title = $"テスト{index}: BEEP音の再生";
                Palmtree.TinyConsole.ResetColor();
                Palmtree.TinyConsole.Clear();
                PrintSystemMessage($"BEEP音の再生のテストを開始します。ENTER キーを押してください。");
                WaitForEnterKey();
                Palmtree.TinyConsole.Clear();

                Palmtree.TinyConsole.Beep();

                PrintSystemMessage($"以下の点を確認して ENTER キーまたは矢印キーを押してください。");
                PrintSystemMessage($"・BEEP音が鳴ったこと");
                return WaitForEnterOrArrowKey();
            }
            finally
            {
                Palmtree.TinyConsole.ResetColor();
                Palmtree.TinyConsole.Clear();
            }
        }

        public static TestItemDirection Test_画面消去(int index)
        {
            try
            {
                Palmtree.TinyConsole.Title = $"テスト{index}: 画面消去";
                Palmtree.TinyConsole.ResetColor();
                Palmtree.TinyConsole.Clear();
                PrintSystemMessage($"画面消去のテストを開始します。ENTER キーを押してください。");
                WaitForEnterKey();

                PrintSystemMessage(string.Concat(Enumerable.Repeat("0123456789", 2400)));
                PrintSystemMessage("ENTERキーを押すと画面がクリアされます。");
                WaitForEnterKey();

                Palmtree.TinyConsole.Clear();

                PrintSystemMessage("以下の点を確認して ENTER キーを押してください。");
                PrintSystemMessage("・画面の文字がスクロール範囲外も含めてすべて消えたこと");
                PrintSystemMessage("・カーソルがホームポジションに戻ったこと");
                WaitForEnterKey();

                PrintSystemMessage(string.Concat(Enumerable.Repeat("0123456789", 2400)));
                PrintSystemMessage("ENTERキーを押すと画面がクリアされます。");
                WaitForEnterKey();

                Palmtree.TinyConsole.BackgroundColor = ConsoleColor.DarkGray;
                Palmtree.TinyConsole.Clear();

                Palmtree.TinyConsole.ResetColor();
                PrintSystemMessage("以下の点を確認して ENTER キーまたは矢印キーを押してください。");
                PrintSystemMessage("・画面の文字がすべて消えたこと");
                PrintSystemMessage($"・画面全体が{ConsoleColor.DarkGray}色になったこと");
                PrintSystemMessage("・カーソルがホームポジションに戻ったこと");
                return WaitForEnterOrArrowKey();
            }
            finally
            {
                Palmtree.TinyConsole.ResetColor();
                Palmtree.TinyConsole.Clear();
            }
        }

        public static TestItemDirection Test_カーソル位置の設定(int index)
        {
            try
            {
                Palmtree.TinyConsole.Title = $"テスト{index}: カーソル位置の設定";
                Palmtree.TinyConsole.ResetColor();
                Palmtree.TinyConsole.Clear();
                PrintSystemMessage($"カーソル位置の設定のテストを開始します。ENTER キーを押してください。");
                WaitForEnterKey();

                Palmtree.TinyConsole.Clear();
                PrintSystemMessage(string.Concat(Enumerable.Repeat("0123456789", 800)));
                Palmtree.TinyConsole.SetCursorPosition(9, 9);
                PrintSystemMessage("*");

                Palmtree.TinyConsole.SetCursorPosition(0, 0);
                PrintSystemMessage("以下の点を確認して ENTER キーまたは矢印キーを押してください。");
                PrintSystemMessage("・10行目10桁目に '*' が表示されたこと。");
                return WaitForEnterOrArrowKey();
            }
            finally
            {
                Palmtree.TinyConsole.ResetColor();
                Palmtree.TinyConsole.Clear();
            }
        }

        public static TestItemDirection Test_カーソル位置の取得(int index)
        {
            try
            {
                Palmtree.TinyConsole.Title = $"テスト{index}: カーソル位置の取得";
                Palmtree.TinyConsole.ResetColor();
                Palmtree.TinyConsole.Clear();
                PrintSystemMessage($"カーソル位置の取得のテストを開始します。ENTER キーを押してください。");
                WaitForEnterKey();

                Palmtree.TinyConsole.Clear();
                var (homeLeft, homeTop) = Palmtree.TinyConsole.GetCursorPosition();
                Palmtree.TinyConsole.SetCursorPosition(10, 10);
                Palmtree.TinyConsole.Write("    ");
                var (left, top) = Palmtree.TinyConsole.GetCursorPosition();
                Palmtree.TinyConsole.SetCursorPosition(0, 0);

                if (homeLeft == 0 && homeTop == 0)
                    PrintSystemMessage("・ホームポジションのカーソル位置 => OK");
                else
                    PrintSystemMessage($"・ホームポジションのカーソル位置 => NG ({homeLeft}, {homeTop})");

                if (left == 14 && top == 10)
                    PrintSystemMessage("・移動先のカーソル位置 => OK");
                else
                    PrintSystemMessage($"・移動先のカーソル位置 => NG ({left}, {top})");

                PrintSystemMessage("ENTER キーまたは矢印キーを押してください。");
                return WaitForEnterOrArrowKey();
            }
            finally
            {
                Palmtree.TinyConsole.ResetColor();
                Palmtree.TinyConsole.Clear();
            }
        }

        public static TestItemDirection Test_画面部分消去_EntireConsoleBuffer(int index)
        {
            try
            {
                Palmtree.TinyConsole.Title = $"テスト{index}: 画面部分消去(EntireConsoleBuffer)";
                Palmtree.TinyConsole.ResetColor();
                Palmtree.TinyConsole.Clear();
                PrintSystemMessage($"画面部分消去(EntireConsoleBuffer)のテストを開始します。ENTER キーを押してください。");
                WaitForEnterKey();

                PrintSystemMessage(string.Concat(Enumerable.Repeat("0123456789", 2400)));
                PrintSystemMessage("ENTERキーを押すと画面がクリアされます。");
                WaitForEnterKey();

                Palmtree.TinyConsole.SetCursorPosition(9, 9);
                Palmtree.TinyConsole.Erase(ConsoleEraseMode.EntireConsoleBuffer);
                Palmtree.TinyConsole.Write('*');

                Palmtree.TinyConsole.SetCursorPosition(0, 0);
                PrintSystemMessage("以下の点を確認して ENTER キーを押してください。");
                PrintSystemMessage("・画面の文字がスクロール範囲外も含めてすべて消えたこと");
                PrintSystemMessage("・10行目の10桁目に * が表示されていないこと。");
                WaitForEnterKey();

                PrintSystemMessage(string.Concat(Enumerable.Repeat("0123456789", 2400)));
                PrintSystemMessage("ENTERキーを押すと画面がクリアされます。");
                WaitForEnterKey();

                Palmtree.TinyConsole.BackgroundColor = ConsoleColor.DarkGray;
                Palmtree.TinyConsole.SetCursorPosition(9, 9);
                Palmtree.TinyConsole.Erase(ConsoleEraseMode.EntireConsoleBuffer);
                Palmtree.TinyConsole.ResetColor();
                Palmtree.TinyConsole.Write('*');

                Palmtree.TinyConsole.SetCursorPosition(0, 0);
                PrintSystemMessage("以下の点を確認して ENTER キーまたは矢印キーを押してください。");
                PrintSystemMessage("・画面の文字がすべて消えたこと");
                PrintSystemMessage($"・画面全体が{ConsoleColor.DarkGray}色になったこと");
                PrintSystemMessage("・10行目の10桁目に * が表示されていないこと。");
                return WaitForEnterOrArrowKey();
            }
            finally
            {
                Palmtree.TinyConsole.ResetColor();
                Palmtree.TinyConsole.Clear();
            }
        }

        public static TestItemDirection Test_画面部分消去_EntireLine(int index)
        {
            try
            {
                Palmtree.TinyConsole.Title = $"テスト{index}: 画面部分消去(EntireLine)";
                Palmtree.TinyConsole.ResetColor();
                Palmtree.TinyConsole.Clear();
                PrintSystemMessage($"画面部分消去(EntireLine)のテストを開始します。ENTER キーを押してください。");
                WaitForEnterKey();

                PrintSystemMessage(string.Concat(Enumerable.Repeat("0123456789", 800)));
                Palmtree.TinyConsole.SetCursorPosition(9, 9);
                Palmtree.TinyConsole.Erase(ConsoleEraseMode.EntireLine);
                Palmtree.TinyConsole.Write('*');

                Palmtree.TinyConsole.SetCursorPosition(0, 0);
                PrintSystemMessage("以下の点を確認して ENTER キーを押してください。");
                PrintSystemMessage("・10行目がすべて消去されたこと");
                PrintSystemMessage("・10行目の10桁目に * が表示されていること。");
                WaitForEnterKey();

                PrintSystemMessage(string.Concat(Enumerable.Repeat("0123456789", 800)));
                Palmtree.TinyConsole.SetCursorPosition(0, 9);
                Palmtree.TinyConsole.Erase(ConsoleEraseMode.EntireLine);
                Palmtree.TinyConsole.Write('*');

                Palmtree.TinyConsole.SetCursorPosition(0, 0);
                PrintSystemMessage("以下の点を確認して ENTER キーを押してください。");
                PrintSystemMessage("・10行目がすべて消去されたこと");
                PrintSystemMessage("・10行目の1桁目に * が表示されていること。");
                WaitForEnterKey();

                Palmtree.TinyConsole.SetCursorPosition(0, 0);
                PrintSystemMessage(string.Concat(Enumerable.Repeat("0123456789", 800)));
                Palmtree.TinyConsole.SetCursorPosition(9, 9);
                Palmtree.TinyConsole.BackgroundColor = ConsoleColor.DarkGray;
                Palmtree.TinyConsole.Erase(ConsoleEraseMode.EntireLine);
                Palmtree.TinyConsole.ResetColor();
                Palmtree.TinyConsole.Write('*');

                Palmtree.TinyConsole.SetCursorPosition(0, 0);
                PrintSystemMessage("以下の点を確認して ENTER キーまたは矢印キーを押してください。");
                PrintSystemMessage("・10行目がすべて消去されたこと");
                PrintSystemMessage($"・消去された部分が{ConsoleColor.DarkGray}色になっていること");
                PrintSystemMessage("・10行目の1桁目に * が表示されていること。");
                return WaitForEnterOrArrowKey();
            }
            finally
            {
                Palmtree.TinyConsole.ResetColor();
                Palmtree.TinyConsole.Clear();
            }
        }

        public static TestItemDirection Test_画面部分消去_EntireScreen(int index)
        {
            try
            {
                Palmtree.TinyConsole.Title = $"テスト{index}: 画面部分消去(EntireScreen)";
                Palmtree.TinyConsole.ResetColor();
                Palmtree.TinyConsole.Clear();
                PrintSystemMessage($"画面部分消去(EntireScreen)のテストを開始します。ENTER キーを押してください。");
                WaitForEnterKey();

                PrintSystemMessage(string.Concat(Enumerable.Repeat("0123456789", 2400)));
                PrintSystemMessage("ENTERキーを押すとウィンドウ内がクリアされます。");
                WaitForEnterKey();

                Palmtree.TinyConsole.SetCursorPosition(9, 9);
                Palmtree.TinyConsole.Erase(ConsoleEraseMode.EntireScreen);
                Palmtree.TinyConsole.Write('*');

                Palmtree.TinyConsole.SetCursorPosition(0, 0);
                PrintSystemMessage("以下の点を確認して ENTER キーまたは矢印キーを押してください。");
                PrintSystemMessage("・ウィンドウ内の文字が消えていること");
                PrintSystemMessage("・10行目の10桁目に * が表示されていないこと。");
                PrintSystemMessage("・スクロール範囲外の文字は残っていること");
                WaitForEnterKey();

                Palmtree.TinyConsole.SetCursorPosition(0, 0);
                PrintSystemMessage(string.Concat(Enumerable.Repeat("0123456789", 2400)));
                PrintSystemMessage("ENTERキーを押すと画面がクリアされます。");

                Palmtree.TinyConsole.SetCursorPosition(9, 9);
                Palmtree.TinyConsole.BackgroundColor = ConsoleColor.DarkGray;
                Palmtree.TinyConsole.Erase(ConsoleEraseMode.EntireScreen);
                Palmtree.TinyConsole.ResetColor();
                Palmtree.TinyConsole.Write('*');

                Palmtree.TinyConsole.SetCursorPosition(0, 0);
                PrintSystemMessage("以下の点を確認して ENTER キーまたは矢印キーを押してください。");
                PrintSystemMessage("・ウィンドウ内の文字が消えていること");
                PrintSystemMessage("・スクロール範囲外の文字は残っていること");
                PrintSystemMessage("・10行目の10桁目に * が表示されていないこと。");
                PrintSystemMessage($"・消えた部分の色が{ConsoleColor.DarkGray}になっていること");
                PrintSystemMessage("確認が終わったら ENTER キーまたは矢印キーを押してください。");
                return WaitForEnterOrArrowKey();
            }
            finally
            {
                Palmtree.TinyConsole.ResetColor();
                Palmtree.TinyConsole.Clear();
            }
        }

        public static TestItemDirection Test_画面部分消去_FromBeggingOfLineToCursor(int index)
        {
            try
            {
                Palmtree.TinyConsole.Title = $"テスト{index}: 画面部分消去(FromBeggingOfLineToCursor)";
                Palmtree.TinyConsole.ResetColor();
                Palmtree.TinyConsole.Clear();
                PrintSystemMessage($"画面部分消去(FromBeggingOfLineToCursor)のテストを開始します。ENTER キーを押してください。");
                WaitForEnterKey();

                PrintSystemMessage(string.Concat(Enumerable.Repeat("0123456789", 2400)));
                Palmtree.TinyConsole.SetCursorPosition(9, 9);
                Palmtree.TinyConsole.Erase(ConsoleEraseMode.FromBeggingOfLineToCursor);

                Palmtree.TinyConsole.SetCursorPosition(0, 0);
                PrintSystemMessage("以下の点を確認して ENTER キーを押してください。");
                PrintSystemMessage("・10行目の10桁目まで消えていること");
                WaitForEnterKey();

                Palmtree.TinyConsole.SetCursorPosition(0, 0);
                PrintSystemMessage(string.Concat(Enumerable.Repeat("0123456789", 800)));
                Palmtree.TinyConsole.SetCursorPosition(0, 9);
                Palmtree.TinyConsole.Erase(ConsoleEraseMode.FromBeggingOfLineToCursor);

                Palmtree.TinyConsole.SetCursorPosition(0, 0);
                PrintSystemMessage("以下の点を確認して ENTER キーを押してください。");
                PrintSystemMessage("・10行目が最初の桁だけ消えていること");
                WaitForEnterKey();

                Palmtree.TinyConsole.SetCursorPosition(0, 0);
                PrintSystemMessage(string.Concat(Enumerable.Repeat("0123456789", 800)));

                Palmtree.TinyConsole.SetCursorPosition(9, 9);
                Palmtree.TinyConsole.BackgroundColor = ConsoleColor.DarkGray;
                Palmtree.TinyConsole.Erase(ConsoleEraseMode.FromBeggingOfLineToCursor);

                Palmtree.TinyConsole.SetCursorPosition(0, 0);
                Palmtree.TinyConsole.ResetColor();
                PrintSystemMessage("以下の点を確認して ENTER キーまたは矢印キーを押してください。");
                PrintSystemMessage("・10行目の10桁目まで消えていること");
                PrintSystemMessage($"・消えた部分が{ConsoleColor.DarkGray}色になっていること");
                return WaitForEnterOrArrowKey();
            }
            finally
            {
                Palmtree.TinyConsole.ResetColor();
                Palmtree.TinyConsole.Clear();
            }
        }

        public static TestItemDirection Test_画面部分消去_FromBeggingOfScreenToCursor(int index)
        {
            try
            {
                Palmtree.TinyConsole.Title = $"テスト{index}: 画面部分消去(FromBeggingOfScreenToCursor)";
                Palmtree.TinyConsole.ResetColor();
                Palmtree.TinyConsole.Clear();
                PrintSystemMessage($"画面部分消去(FromBeggingOfScreenToCursor)のテストを開始します。ENTER キーを押してください。");
                WaitForEnterKey();

                Palmtree.TinyConsole.SetCursorPosition(0, 0);
                PrintSystemMessage(string.Concat(Enumerable.Repeat("0123456789", 800)));
                Palmtree.TinyConsole.SetCursorPosition(9, 9);
                Palmtree.TinyConsole.Erase(ConsoleEraseMode.FromBeggingOfScreenToCursor);
                Palmtree.TinyConsole.SetCursorPosition(0, 0);
                PrintSystemMessage("以下の点を確認して ENTER キーを押してください。");
                PrintSystemMessage("・画面の文字が10行目の10桁目まで消えていること");
                WaitForEnterKey();

                Palmtree.TinyConsole.SetCursorPosition(0, 0);
                PrintSystemMessage(string.Concat(Enumerable.Repeat("0123456789", 800)));
                Palmtree.TinyConsole.SetCursorPosition(0, 0);
                Palmtree.TinyConsole.Erase(ConsoleEraseMode.FromBeggingOfScreenToCursor);
                Palmtree.TinyConsole.SetCursorPosition(0, 10);
                PrintSystemMessage("以下の点を確認して ENTER キーを押してください。");
                PrintSystemMessage("・画面の文字が左上端だけ消えていること");
                WaitForEnterKey();

                Palmtree.TinyConsole.SetCursorPosition(0, 0);
                PrintSystemMessage(string.Concat(Enumerable.Repeat("0123456789", 800)));
                Palmtree.TinyConsole.SetCursorPosition(9, 9);
                Palmtree.TinyConsole.BackgroundColor = ConsoleColor.DarkGray;
                Palmtree.TinyConsole.Erase(ConsoleEraseMode.FromBeggingOfScreenToCursor);
                Palmtree.TinyConsole.SetCursorPosition(0, 0);
                Palmtree.TinyConsole.ResetColor();
                PrintSystemMessage("以下の点を確認して ENTER キーまたは矢印キーを押してください。");
                PrintSystemMessage("・画面の文字が10行目の10桁目まで消えていること");
                PrintSystemMessage($"・消えた部分が{ConsoleColor.DarkGray}色になっていること");
                return WaitForEnterOrArrowKey();
            }
            finally
            {
                Palmtree.TinyConsole.ResetColor();
                Palmtree.TinyConsole.Clear();
            }
        }

        public static TestItemDirection Test_画面部分消去_FromCursorToEndOfLine(int index)
        {
            try
            {
                Palmtree.TinyConsole.Title = $"テスト{index}: 画面部分消去(FromCursorToEndOfLine)";
                Palmtree.TinyConsole.ResetColor();
                Palmtree.TinyConsole.Clear();
                PrintSystemMessage($"画面部分消去(FromCursorToEndOfLine)のテストを開始します。ENTER キーを押してください。");
                WaitForEnterKey();

                Palmtree.TinyConsole.SetCursorPosition(0, 0);
                PrintSystemMessage(string.Concat(Enumerable.Repeat("0123456789", 800)));
                Palmtree.TinyConsole.SetCursorPosition(10, 9);
                Palmtree.TinyConsole.Erase(ConsoleEraseMode.FromCursorToEndOfLine);
                Palmtree.TinyConsole.SetCursorPosition(0, 0);
                PrintSystemMessage("以下の点を確認して ENTER キーを押してください。");
                PrintSystemMessage("・画面の文字が10行目の10桁目から行末まで消えていること");
                WaitForEnterKey();

                Palmtree.TinyConsole.SetCursorPosition(0, 0);
                PrintSystemMessage(string.Concat(Enumerable.Repeat("0123456789", 800)));
                Palmtree.TinyConsole.SetCursorPosition(0, 9);
                Palmtree.TinyConsole.Erase(ConsoleEraseMode.FromCursorToEndOfLine);
                Palmtree.TinyConsole.SetCursorPosition(0, 0);
                PrintSystemMessage("以下の点を確認して ENTER キーを押してください。");
                PrintSystemMessage("・画面の文字が10行目の1桁目の行頭から行末まで消えていること");
                WaitForEnterKey();

                Palmtree.TinyConsole.SetCursorPosition(0, 0);
                PrintSystemMessage(string.Concat(Enumerable.Repeat("0123456789", 800)));
                Palmtree.TinyConsole.SetCursorPosition(10, 9);
                Palmtree.TinyConsole.BackgroundColor = ConsoleColor.DarkGray;
                Palmtree.TinyConsole.Erase(ConsoleEraseMode.FromCursorToEndOfLine);
                Palmtree.TinyConsole.SetCursorPosition(0, 0);
                Palmtree.TinyConsole.ResetColor();
                PrintSystemMessage("以下の点を確認して ENTER キーまたは矢印キーを押してください。");
                PrintSystemMessage("・画面の文字が10行目の10桁目から行末まで消えていること");
                PrintSystemMessage($"・消えた部分が{ConsoleColor.DarkGray}色になっていること");
                return WaitForEnterOrArrowKey();
            }
            finally
            {
                Palmtree.TinyConsole.ResetColor();
                Palmtree.TinyConsole.Clear();
            }
        }

        public static TestItemDirection Test_画面部分消去_FromCursorToEndOfScreen(int index)
        {
            try
            {
                Palmtree.TinyConsole.Title = $"テスト{index}: 画面部分消去(FromCursorToEndOfScreen)";
                Palmtree.TinyConsole.ResetColor();
                Palmtree.TinyConsole.Clear();
                PrintSystemMessage($"画面部分消去(FromCursorToEndOfScreen)のテストを開始します。ENTER キーを押してください。");
                WaitForEnterKey();

                Palmtree.TinyConsole.SetCursorPosition(0, 0);
                PrintSystemMessage(string.Concat(Enumerable.Repeat("0123456789", 800)));
                Palmtree.TinyConsole.SetCursorPosition(9, 10);
                Palmtree.TinyConsole.Erase(ConsoleEraseMode.FromCursorToEndOfScreen);
                Palmtree.TinyConsole.SetCursorPosition(0, 0);
                PrintSystemMessage("以下の点を確認して ENTER キーを押してください。");
                PrintSystemMessage("・画面の文字が10行目の10桁目から最後まで消えていること");
                WaitForEnterKey();

                Palmtree.TinyConsole.SetCursorPosition(0, 0);
                PrintSystemMessage(string.Concat(Enumerable.Repeat("0123456789", 800)));
                Palmtree.TinyConsole.SetCursorPosition(0, 0);
                Palmtree.TinyConsole.Erase(ConsoleEraseMode.FromCursorToEndOfScreen);
                Palmtree.TinyConsole.SetCursorPosition(0, 9);
                PrintSystemMessage("以下の点を確認して ENTER キーを押してください。");
                PrintSystemMessage("・画面の文字がすべて消えていること");
                WaitForEnterKey();

                Palmtree.TinyConsole.SetCursorPosition(0, 0);
                PrintSystemMessage(string.Concat(Enumerable.Repeat("0123456789", 800)));
                Palmtree.TinyConsole.SetCursorPosition(9, 10);
                Palmtree.TinyConsole.BackgroundColor = ConsoleColor.DarkGray;
                Palmtree.TinyConsole.Erase(ConsoleEraseMode.FromCursorToEndOfScreen);
                Palmtree.TinyConsole.SetCursorPosition(0, 0);
                Palmtree.TinyConsole.ResetColor();
                PrintSystemMessage("以下の点を確認して ENTER キーまたは矢印キーを押してください。");
                PrintSystemMessage("・画面の文字が10行目の10桁目から最後まで消えていること");
                PrintSystemMessage($"・消えた部分が{ConsoleColor.DarkGray}色になっていること");
                return WaitForEnterOrArrowKey();
            }
            finally
            {
                Palmtree.TinyConsole.ResetColor();
                Palmtree.TinyConsole.Clear();
            }
        }

        public static TestItemDirection Test_カーソル相対移動(int index)
        {
            try
            {
                Palmtree.TinyConsole.Title = $"テスト{index}: カーソル相対移動";
                Palmtree.TinyConsole.ResetColor();
                Palmtree.TinyConsole.Clear();
                PrintSystemMessage($"カーソル相対移動のテストを開始します。ENTER キーを押してください。");
                WaitForEnterKey();

                Palmtree.TinyConsole.Clear();
                Palmtree.TinyConsole.Write("*");
                Palmtree.TinyConsole.SetCursorPosition(9, 0);
                Palmtree.TinyConsole.CursorUp(1);
                var upOver = Palmtree.TinyConsole.CursorTop == 0;
                Palmtree.TinyConsole.CursorBack(1);
                Palmtree.TinyConsole.SetCursorPosition(0, 9);
                var leftOver = Palmtree.TinyConsole.CursorLeft == 0;

                Palmtree.TinyConsole.SetCursorPosition(0, 0);
                Palmtree.TinyConsole.CursorDown(10);
                var down = Palmtree.TinyConsole.CursorTop == 10;
                Palmtree.TinyConsole.CursorForward(10);
                var right = Palmtree.TinyConsole.CursorLeft == 10;

                Palmtree.TinyConsole.CursorUp(5);
                var up = Palmtree.TinyConsole.CursorTop == 5;
                Palmtree.TinyConsole.CursorBack(5);
                var left = Palmtree.TinyConsole.CursorLeft == 5;

                Palmtree.TinyConsole.SetCursorPosition(0, 2);
                PrintSystemMessage($"・上へ移動: {(up ? "OK" : "NG")}");
                PrintSystemMessage($"・下へ移動: {(down ? "OK" : "NG")}");
                PrintSystemMessage($"・右へ移動: {(right ? "OK" : "NG")}");
                PrintSystemMessage($"・左へ移動: {(left ? "OK" : "NG")}");
                PrintSystemMessage($"・上端から上への移動: {(upOver ? "OK" : "NG")}");
                PrintSystemMessage($"・左端から左への移動: {(leftOver ? "OK" : "NG")}");
                PrintSystemMessage("以下の点を確認して ENTER キーを押してください。");
                PrintSystemMessage("・左上に * が表示されていること");
                PrintSystemMessage("・カーソルが右端にあること");
                Palmtree.TinyConsole.SetCursorPosition(9, 9);
                Palmtree.TinyConsole.CursorForward(1000);
                WaitForEnterKey();

                Palmtree.TinyConsole.Clear();
                Palmtree.TinyConsole.Write("*");
                Palmtree.TinyConsole.SetCursorPosition(0, 2);
                PrintSystemMessage("以下の点を確認して ENTER キーまたは矢印キーを押してください。");
                PrintSystemMessage("・左上に * が表示されていること");
                PrintSystemMessage("・カーソルが下端にあること");
                Palmtree.TinyConsole.SetCursorPosition(9, 9);
                Palmtree.TinyConsole.CursorDown(1000);
                return WaitForEnterOrArrowKey();
            }
            finally
            {
                Palmtree.TinyConsole.ResetColor();
                Palmtree.TinyConsole.Clear();
            }
        }

        public static void PrintCompletion()
        {
            PrintSystemMessage($"テストが終了しました。ENTER キーを押してください。");
            WaitForEnterKey();
        }

        #region private methods

        private static void PrintSystemMessage(string message) => Palmtree.TinyConsole.WriteLine(message);

        private static void WaitForEnterKey()
        {
            while (Palmtree.TinyConsole.ReadKey(true).Key != ConsoleKey.Enter)
            {
            }
        }

        private static TestItemDirection WaitForEnterOrArrowKey()
        {
            while (true)
            {
                switch (Palmtree.TinyConsole.ReadKey(true).Key)
                {
                    case ConsoleKey.Enter:
                        return TestItemDirection.Next;
                    case ConsoleKey.RightArrow:
                        return TestItemDirection.Next;
                    case ConsoleKey.LeftArrow:
                        return TestItemDirection.Previous;
                    case ConsoleKey.UpArrow:
                        return TestItemDirection.Again;
                    case ConsoleKey.DownArrow:
                        return TestItemDirection.Exit;
                }
            }
        }

        #endregion
    }
}
