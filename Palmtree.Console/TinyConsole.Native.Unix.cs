using System.Runtime.InteropServices;

namespace Palmtree
{
    partial class TinyConsole
    {
        private static class InterOpUnix
        {
            public const int ENOTSUP = 95;
            public const int STANDARD_FILE_IN = 0;
            public const int STANDARD_FILE_OUT = 1;
            public const int STANDARD_FILE_ERR = 2;

            [StructLayout(LayoutKind.Sequential)]
            internal struct WinSize
            {
                internal ushort Row;
                internal ushort Col;
                internal ushort XPixel;
                internal ushort YPixel;
            };

            [DllImport("libSystem.Native", EntryPoint = "SystemNative_SetKeypadXmit")]
            public extern static unsafe void SetKeypadXmit(byte* terminfoString);

            [DllImport("libSystem.Native", EntryPoint = "SystemNative_ReadStdin", SetLastError = true)]
            public extern static unsafe int ReadStdin(byte* buffer, int bufferSize);

            [DllImport("libSystem.Native", EntryPoint = "SystemNative_InitializeConsoleBeforeRead")]
            public extern static void InitializeConsoleBeforeRead(byte minChars = 1, byte decisecondsTimeout = 0);

            [DllImport("libSystem.Native", EntryPoint = "SystemNative_UninitializeConsoleAfterRead")]
            public extern static void UninitializeConsoleAfterRead();

            [DllImport("libPalmtree.Console.InterOp.Unix", EntryPoint = "PalmtreeNative_GetStandardFileNo")]
            public extern static int GetStandardFileNo(int standardFileType);

            [DllImport("libPalmtree.Console.InterOp.Unix", EntryPoint = "PalmtreeNative_GetWindowSize")]
            public extern static int GetWindowSize(int consoleFileNo, out WinSize windowSize, out int errno);

            [DllImport("libPalmtree.Console.InterOp.Unix", EntryPoint = "PalmtreeNative_SetWindowSize")]
            public extern static int SetWindowSize(int consoleFileNo, ref WinSize windowSize, out int errno);
        }
    }
}
