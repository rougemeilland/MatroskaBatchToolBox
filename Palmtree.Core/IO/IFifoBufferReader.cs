using System;

namespace Palmtree.IO
{
    internal interface IFifoBufferReader<DATA_T>
    {
        bool Peek(out DATA_T value);

        bool Read(out DATA_T value);

        int Read(Span<DATA_T> buffer);

        void Reference();

        void Unreference();
    }
}
