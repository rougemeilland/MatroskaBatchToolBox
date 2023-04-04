using System;

namespace Palmtree.IO
{
    internal interface IFifoBufferWriter<DATA_T>
    {
        void Write(DATA_T value);
        int Write(ReadOnlySpan<DATA_T> buffer);
        void Close();
        void Reference();
        void Unreference();
    }
}
