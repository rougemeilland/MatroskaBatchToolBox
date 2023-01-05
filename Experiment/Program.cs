using System;
using System.Threading;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;

namespace Experiment
{

    public static class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("x が伸び縮みします。");
            Console.WriteLine();

            var _previousTextLength = 0;
            while (true)
            {
                foreach (var textLength in new[] { -10, -7, -4, -1, 2, 5, 8, 11, 8, 5, 2, -1, -4, -7 })
                {
                    var (leftPos0, topPos0) = Console.GetCursorPosition();
                    Console.Write(new string('x', Console.WindowWidth + textLength));
                    var (leftPos1, topPos1) = Console.GetCursorPosition();
                    var currentProgressTextLength = (leftPos1 - leftPos0) + (topPos1 - topPos0) * Console.WindowWidth;
                    if (_previousTextLength > currentProgressTextLength)
                        Console.Write(new string(' ', _previousTextLength - currentProgressTextLength));
                    Console.SetCursorPosition(leftPos0, topPos0);
                    _previousTextLength = currentProgressTextLength;
                    Thread.Sleep(500);
                }
            }
        }
    }
}
