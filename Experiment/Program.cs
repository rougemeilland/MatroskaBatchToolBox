using System;
using System.Threading;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

namespace Experiment
{

    public static class Program
    {
        public static void Main(string[] args)
        {
            var _ = Task.Run(() =>
            {
                while (true)
                {
                    var key = Console.ReadKey();
                    if (key.Key == ConsoleKey.F9)
                        Console.Beep();
                }
            });

            // F9 キーがコンソールの表示に影響を与えないことの確認
            for (var count = 0; ;++count)
            {
                Console.Write($"--{count}--\r");
                Thread.Sleep(1000);
            }
        }
    }
}
