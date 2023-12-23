using System;
using Palmtree.IO.Console;

namespace DumpTermInfo
{
    internal class Program
    {
        private static void Main() => TerminalInfo.WriteAllTerminalInfos(Console.Out);
    }
}
