using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Palmtree
{
    partial class TinyConsole
    {
        private static class InterOpWindows
        {
            public static readonly uint STD_INPUT_HANDLE = unchecked((uint)-10);
            public static readonly uint STD_OUTPUT_HANDLE = unchecked((uint)-11);
            public static readonly uint STD_ERROR_HANDLE = unchecked((uint)-12);
            public static readonly IntPtr INVALID_HANDLE_VALUE = new(-1);
            public static readonly uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;
            public static readonly uint ENABLE_VIRTUAL_TERMINAL_INPUT = 0x0200;

            [StructLayout(LayoutKind.Sequential)]
            public struct CONSOLE_SCREEN_BUFFER_INFO
            {
                public COORD dwSize;
                public COORD dwCursorPosition;
                public ushort wAttributes;
                public SMALL_RECT srWindow;
                public COORD dwMaximumWindowSize;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct COORD
            {
                public short X;
                public short Y;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct SMALL_RECT
            {
                public short Left;
                public short Top;
                public short Right;
                public short Bottom;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct CONSOLE_CURSOR_INFO
            {
                public uint dwSize;
                public bool bVisible;
            }

            [DllImport("kernel32.dll")]
            public extern static IntPtr GetStdHandle(uint nStdHandle);

            [DllImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public extern static bool GetConsoleScreenBufferInfo(IntPtr hConsoleHandle, out CONSOLE_SCREEN_BUFFER_INFO lpConsoleScreenBufferInfo);

            [DllImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public extern static bool SetConsoleTextAttribute(IntPtr hConsoleHandle, ushort wAttributes);

            [DllImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public extern static bool SetConsoleCursorPosition(IntPtr hConsoleOutput, COORD cursorPosition);

            [DllImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public extern static bool GetConsoleCursorInfo(IntPtr hConsoleHandle, out CONSOLE_CURSOR_INFO lpConsoleCursorInfo);

            [DllImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public extern static bool SetConsoleCursorInfo(IntPtr hConsoleHandle, ref CONSOLE_CURSOR_INFO lpConsoleCursorInfo);

            [DllImport("kernel32.dll", EntryPoint = "FillConsoleOutputCharacterW", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public extern static bool FillConsoleOutputCharacter(IntPtr hConsoleHandle, short cCharacter, uint nLength, COORD dwWriteCoord, out uint lpNumberOfCharsWritten);

            [DllImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public extern static bool FillConsoleOutputAttribute(IntPtr hConsoleHandle, ushort wAttribute, uint nLength, COORD dwWriteCoord, out uint lpNumberOfAttrsWritten);

            [DllImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public extern static bool GetConsoleMode(IntPtr hConsoleHandle, out uint mode);

            [DllImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public extern static bool SetConsoleMode(IntPtr hConsoleHandle, uint mode);

            [DllImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public extern static bool SetConsoleWindowInfo(IntPtr hConsoleHandle, bool bAbsolute, ref SMALL_RECT lpConsoleWindow);

            [DllImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public extern static bool SetConsoleScreenBufferSize(IntPtr hConsoleHandle, COORD dwSize);

            [DllImport("kernel32.dll", SetLastError = true)]
            public extern static COORD GetLargestConsoleWindowSize(IntPtr hConsoleHandle);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ushort FromConsoleColorsToConsoleAttribute(ConsoleColor backgroundColor, ConsoleColor foregroundColor)
                => (ushort)((((int)backgroundColor & 0x0f) << 4) | ((int)foregroundColor & 0x0f));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static (ConsoleColor backgroundColor, ConsoleColor foregroundColor) FromConsoleAttributeToConsoleColors(ushort consoleAttribute)
                => ((ConsoleColor)((consoleAttribute >> 4) & 0x0f), (ConsoleColor)(consoleAttribute & 0x0f));
        }
    }
}
