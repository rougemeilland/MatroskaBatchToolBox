using System;
using Palmtree.Terminal;

namespace DumpTermInfo
{
    internal class Program
    {
        private static void Main() => TerminalInfo.WriteAllTerminalInfos(Console.Out);
    }
}
