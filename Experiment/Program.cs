using System.Text;
using Palmtree;
using Palmtree.IO.Console;

namespace Experiment
{
    public static partial class Program
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:未使用のパラメーターを削除します", Justification = "<保留中>")]
        public static int Main(string[] args)
        {
            var s = "She said, \"Don't make others suffer for you personal hatred.\"";
            TinyConsole.WriteLine(s.EncodeCommandLineArgument());

            TinyConsole.WriteLine("OK");
            TinyConsole.Beep();
            _ = TinyConsole.ReadLine();
            return 0;
        }
    }
}
