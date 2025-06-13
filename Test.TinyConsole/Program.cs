using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Palmtree;

namespace Test.TinyConsole
{
    internal static partial class Program
    {
        private static readonly Func<int, TestItemDirection>[] _testActions =
            [
                TestActions.Test_前景色の変更,
                TestActions.Test_背景色の変更,
                TestActions.Test_カーソル可視の変更,
                TestActions.Test_BEEP音の再生,
                TestActions.Test_画面消去,
                TestActions.Test_カーソル位置の設定,
                TestActions.Test_カーソル位置の取得,
                TestActions.Test_カーソル相対移動,
                TestActions.Test_画面部分消去_EntireConsoleBuffer,
                TestActions.Test_画面部分消去_EntireLine,
                TestActions.Test_画面部分消去_EntireScreen,
                TestActions.Test_画面部分消去_FromBeggingOfLineToCursor,
                TestActions.Test_画面部分消去_FromBeggingOfScreenToCursor,
                TestActions.Test_画面部分消去_FromCursorToEndOfLine,
                TestActions.Test_画面部分消去_FromCursorToEndOfScreen,
            ];

        private static int Main(string[] args)
        {
            var match = GetTestNumberRegionPattern().Match(args.Length >= 1 ? args[0] : "1-");
            if (!match.Success)
                return 1;

            int start;
            int end;

            if (match.Groups["start0"].Success)
            {
                start = int.Parse(match.Groups["start0"].Value, NumberStyles.None, CultureInfo.InvariantCulture);
                end = start;
            }
            else if (match.Groups["start1"].Success)
            {
                start = int.Parse(match.Groups["start1"].Value, NumberStyles.None, CultureInfo.InvariantCulture);
                end = _testActions.Length;
            }
            else if (match.Groups["start2"].Success)
            {
                start = int.Parse(match.Groups["start2"].Value, NumberStyles.None, CultureInfo.InvariantCulture);
                end = int.Parse(match.Groups["end1"].Value, NumberStyles.None, CultureInfo.InvariantCulture);
            }
            else if (match.Groups["end2"].Success)
            {
                start = 1;
                end = int.Parse(match.Groups["end2"].Value, NumberStyles.None, CultureInfo.InvariantCulture);
            }
            else
            {
                start = 1;
                end = _testActions.Length;
            }

            if (start < 1)
                return 1;

            if (end > _testActions.Length)
                return 1;

            if (start > end)
                return 1;

            for (var index = start; index <= end;)
            {
                var direction = _testActions[index - 1](index + 1);
                index =
                    direction switch
                    {
                        TestItemDirection.Previous => (index - 1).Maximum(1),
                        TestItemDirection.Next => index + 1,
                        TestItemDirection.Exit => _testActions.Length,
                        _ => index,
                    };
            }

            TestActions.PrintCompletion();

            return 0;
        }

        [GeneratedRegex(@"^((?<start0>\d+)|(?<start1>\d+)-|(?<start2>\d+)-(?<end1>\d+)|-(?<end2>\d+))$", RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.CultureInvariant)]
        private static partial Regex GetTestNumberRegionPattern();
    }
}
