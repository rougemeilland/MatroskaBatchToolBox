using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using Palmtree;
using Palmtree.Terminal;

namespace Experiment
{
    public static partial class Program
    {
        public static void Main()
        {
            foreach (var color in Color8.GetValues())
                ValidateColor(color);
            foreach (var color in Color16.GetValues())
                ValidateColor(color);
            foreach (var color in Color88.GetValues())
                ValidateColor(color);
            foreach (var color in Color256.GetValues())
                ValidateColor(color);
        }

        private static void ValidateColor(Color8 c)
        {

            var (r, g, b) = c.ToRgb();
            var c2 = Color8.FromRgb(r, g, b);
            if (c.ToRgb() != c2.ToRgb())
                throw new Exception();
            Console.WriteLine($"{typeof(Color8).Name}.{c}: OK");
        }

        private static void ValidateColor(Color16 c)
        {

            var (r, g, b) = c.ToRgb();
            var c2 = Color16.FromRgb(r, g, b);
            if (c.ToRgb() != c2.ToRgb())
                throw new Exception();
            Console.WriteLine($"{typeof(Color16).Name}.{c}: OK");
        }

        private static void ValidateColor(Color88 c)
        {

            var (r, g, b) = c.ToRgb();
            var c2 = Color88.FromRgb(r, g, b);
            if (c.ToRgb() != c2.ToRgb())
                throw new Exception();
            Console.WriteLine($"{typeof(Color88).Name}.{c}: OK");
        }

        private static void ValidateColor(Color256 c)
        {

            var (r, g, b) = c.ToRgb();
            var c2 = Color256.FromRgb(r, g, b);
            if (c.ToRgb() != c2.ToRgb())
                throw new Exception();
            Console.WriteLine($"{typeof(Color256).Name}.{c}: OK");
        }
    }
}
