using System;
using Utility;
using System.Diagnostics.CodeAnalysis;

namespace Experiment
{

    public static class Program
    {
        [SuppressMessage("Style", "IDE0060:未使用のパラメーターを削除します", Justification = "<保留中>")]
        public static void Main(string[] args)
        {
            var x = Numerics.GreatestCommonDivisor(int.MinValue, int.MinValue);
            Console.WriteLine(x);
            Console.WriteLine();
            Console.WriteLine("OK");
            Console.Beep();
            _ = Console.ReadLine();
        }
    }
}
