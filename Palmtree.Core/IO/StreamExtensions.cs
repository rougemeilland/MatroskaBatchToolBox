using System;
using System.IO;

namespace Palmtree.IO
{
    /// <summary>
    /// <see cref="Stream"/> クラスの拡張メソッドのクラスです。
    /// </summary>
    public static class StreamExtensions
    {
        /// <summary>ストリームから 1 バイト読み込みます。</summary>
        /// <param name="inStream">入力バイトストリームである <see cref="Stream"/>オブジェクトです。</param>
        /// <returns>読み込んだ <see cref="byte"/> 値です。</returns>
        /// <exception cref="InvalidOperationException">入力ストリームの終端に達しました。</exception>
        public static byte ReadByte(this Stream inStream)
        {
            unsafe
            {
                Span<byte> buffer = stackalloc byte[1];
                var length = inStream.Read(buffer);
                return
                    length > 0
                    ? buffer[0]
                    : throw new InvalidOperationException($"Failed to read data: {nameof(ReadByte)}");
            }
        }

        /// <summary>ストリームから <see cref="short"/> 値を読み込みます。バイトオーダーはリトルエンディアンです。</summary>
        /// <param name="inStream">入力バイトストリームである <see cref="Stream"/>オブジェクトです。</param>
        /// <returns>読み込んだ <see cref="short"/> 値です。</returns>
        /// <exception cref="InvalidOperationException">入力ストリームの終端に達しました。</exception>
        public static short ReadInt16Le(this Stream inStream)
        {
            unsafe
            {
                Span<byte> buffer = stackalloc byte[sizeof(short)];
                inStream.ReadBytes(buffer);
                Validation.Assert(sizeof(short) == 2, "sizeof(short) == 2");
                return (short)(buffer[0] << 0 | buffer[1] << 8);
            }
        }

        /// <summary>ストリームから <see cref="int"/> 値を読み込みます。バイトオーダーはリトルエンディアンです。</summary>
        /// <param name="inStream">入力バイトストリームである <see cref="Stream"/>オブジェクトです。</param>
        /// <returns>読み込んだ <see cref="int"/> 値です。</returns>
        /// <exception cref="InvalidOperationException">入力ストリームの終端に達しました。</exception>
        public static int ReadInt32Le(this Stream inStream)
        {
            unsafe
            {
                Span<byte> buffer = stackalloc byte[sizeof(int)];
                inStream.ReadBytes(buffer);
                Validation.Assert(sizeof(int) == 4, "sizeof(int) == 4");
                return buffer[0] << 0 | buffer[1] << 8 | buffer[2] << 16 | buffer[3] << 24;
            }
        }

        /// <summary>ストリームからバイト列を読み込みます。</summary>
        /// <param name="inStream">入力バイトストリームである <see cref="Stream"/>オブジェクトです。</param>
        /// <param name="buffer">読み込んだバイト列を格納するためのバッファです。</param>
        /// <exception cref="InvalidOperationException">入力ストリームの終端に達しました。</exception>
        public static void ReadBytes(this Stream inStream, Span<byte> buffer)
        {
            var unreadBufferSpan = buffer;
            while (!unreadBufferSpan.IsEmpty)
            {
                var length = inStream.Read(unreadBufferSpan);
                if (length <= 0)
                    throw new InvalidOperationException($"Failed to read data: {nameof(ReadBytes)}({buffer.Length})");
                unreadBufferSpan = unreadBufferSpan[length..];
            }
        }
    }
}
