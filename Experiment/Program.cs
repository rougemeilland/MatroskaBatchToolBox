using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;

namespace Experiment
{

    public static class Program
    {
        public static void Main(string[] args)
        {

            var str = string.Concat(Enumerable.Repeat("あ", 30));
            //var str = string.Concat(Enumerable.Repeat("𩸽", 30));

            // 方法1:
            Console.WriteLine($"方法1: 以下の「{str[0]}」の列は消えていますか？");
            Console.WriteLine($"{str}\r\x1b[K");
            Console.WriteLine(new string(' ', 80));
            Console.WriteLine();

            // 方法2:
            Console.WriteLine($"方法2: 以下の「{str[0]}」の列は消えていますか？");
            Console.WriteLine($"{str}\r{new string(' ', Console.WindowWidth)}");
            Console.WriteLine(new string(' ', 80));
            Console.WriteLine();

            // 方法3:
            Console.WriteLine($"方法3: 以下の「{str[0]}」の列は消えていますか？");
            Console.WriteLine($"{str}{new string('\b', str.Length)}");
            Console.WriteLine(new string(' ', 80));
            Console.WriteLine();

            // 方法4:
            var si = new StringInfo(str);
            Console.WriteLine($"方法3: 以下の「{str[0]}」の列は消えていますか？");
            Console.WriteLine($"{str}{new string('\b', si.LengthInTextElements)}");
            Console.WriteLine(new string(' ', 80));
            Console.WriteLine();


            Console.Beep();
            Console.WriteLine("ENTERを押すと終了します。");
            Console.ReadLine();
        }
    }
}
