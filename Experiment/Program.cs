using System;
using System.IO;
using System.Linq;
using Palmtree;
using Palmtree.Terminal;

namespace Experiment
{
    public static partial class Program
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<保留中>")]
        public static void Main()
        {
#if false
            var windowWidth = Console.WindowWidth;
            var windowHeight = Console.WindowHeight;
            try
            {
                Console.WriteLine($"{nameof(Console.WindowWidth)}={Console.WindowWidth}");
                Console.WriteLine($"{nameof(Console.WindowHeight)}={Console.WindowHeight}");
                _ = Console.ReadLine();
                Console.WindowWidth = 180;
                Console.WindowHeight = 50;
                Console.WriteLine($"{nameof(Console.WindowWidth)}={Console.WindowWidth}");
                Console.WriteLine($"{nameof(Console.WindowHeight)}={Console.WindowHeight}");
                _ = Console.ReadLine();
                Console.WindowWidth = windowWidth;
                Console.WindowHeight = windowHeight;
                _ = Console.ReadLine();
            }
            finally
            {
                Console.WindowWidth = windowWidth;
                Console.WindowHeight = windowHeight;
            }
#else
            TinyConsole.WriteLine($"{nameof(TinyConsole.WindowWidth)}={TinyConsole.WindowWidth}");
            TinyConsole.WriteLine($"{nameof(TinyConsole.WindowHeight)}={TinyConsole.WindowHeight}");
#endif

        }
    }
}
