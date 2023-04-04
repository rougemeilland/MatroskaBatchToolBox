using System;
using Palmtree.Terminal;

namespace Palmtree
{
    internal static class ConsoleColorExtensions
    {
        public static Color16 ToAnsiColor16(this ConsoleColor color)
            => color switch
            {
                ConsoleColor.Black => Color16.Black,
                ConsoleColor.DarkBlue => Color16.Blue,
                ConsoleColor.DarkGreen => Color16.Green,
                ConsoleColor.DarkCyan => Color16.Cyan,
                ConsoleColor.DarkRed => Color16.Red,
                ConsoleColor.DarkMagenta => Color16.Magenta,
                ConsoleColor.DarkYellow => Color16.Yellow,
                ConsoleColor.Gray => Color16.White,
                ConsoleColor.DarkGray => Color16.BrightBlack,
                ConsoleColor.Blue => Color16.BrightBlue,
                ConsoleColor.Green => Color16.BrightGreen,
                ConsoleColor.Cyan => Color16.BrightCyan,
                ConsoleColor.Red => Color16.BrightRed,
                ConsoleColor.Magenta => Color16.BrightMagenta,
                ConsoleColor.Yellow => Color16.BrightYellow,
                ConsoleColor.White => Color16.BrightWhite,
                _ => throw new ArgumentOutOfRangeException(nameof(color)),
            };

        public static Color8 ToColor8(this ConsoleColor color)
            => color switch
            {
                ConsoleColor.Black => Color8.Black,
                ConsoleColor.DarkBlue => Color8.Blue,
                ConsoleColor.DarkGreen => Color8.Green,
                ConsoleColor.DarkCyan => Color8.Cyan,
                ConsoleColor.DarkRed => Color8.Red,
                ConsoleColor.DarkMagenta => Color8.Magenta,
                ConsoleColor.DarkYellow => Color8.Yellow,
                ConsoleColor.Gray => Color8.White,
                ConsoleColor.DarkGray => Color8.Black,
                ConsoleColor.Blue => Color8.Blue,
                ConsoleColor.Green => Color8.Green,
                ConsoleColor.Cyan => Color8.Cyan,
                ConsoleColor.Red => Color8.Red,
                ConsoleColor.Magenta => Color8.Magenta,
                ConsoleColor.Yellow => Color8.Yellow,
                ConsoleColor.White => Color8.White,
                _ => throw new ArgumentOutOfRangeException(nameof(color)),
            };
    }
}
