using System.Runtime.InteropServices;

namespace Palmtree
{
    partial class TinyConsole
    {
        private static class UnixNativeInterOp
        {
            [DllImport("libSystem.Native", EntryPoint = "SystemNative_SetKeypadXmit")]
            public extern static unsafe void SetKeypadXmit(byte* terminfoString);

            [DllImport("libSystem.Native", EntryPoint = "SystemNative_ReadStdin", SetLastError = true)]
            public extern static unsafe int ReadStdin(byte* buffer, int bufferSize);

            [DllImport("libSystem.Native", EntryPoint = "SystemNative_InitializeConsoleBeforeRead")]
            public extern static void InitializeConsoleBeforeRead(byte minChars = 1, byte decisecondsTimeout = 0);

            [DllImport("libSystem.Native", EntryPoint = "SystemNative_UninitializeConsoleAfterRead")]
            public extern static void UninitializeConsoleAfterRead();
        }
    }
}
