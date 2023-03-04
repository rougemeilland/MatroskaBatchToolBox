using System;
using System.IO;

namespace Utility.IO
{
    /// <summary>
    /// <see cref="TextReader"/> から文字単位の読み込みと先読みをするラッパークラスです。
    /// </summary>
    public class CachedTextReader
        : IDisposable
    {
        private readonly TextReader _rawReader;
        private readonly char[] _cacheBuffer;
        private bool _isDisposed;
        private bool _endOfStream;
        private int _cacheLength;

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="reader">
        /// 基本となる <see cref="TextReader"/> オブジェクトです。
        /// </param>
        /// <param name="cacheSize">
        /// 先読みが可能な最大文字数です。
        /// </param>
        /// <exception cref="ArgumentException">
        /// <paramref name="cacheSize"/> には1以上の値を与えなければなりません。
        /// </exception>
        public CachedTextReader(TextReader reader, int cacheSize)
        {
            if (cacheSize < 1)
                throw new ArgumentException($"Invalid {nameof(cacheSize)} value.", nameof(cacheSize));
            _rawReader = reader;
            _cacheBuffer = new char[cacheSize];
            _isDisposed = false;
            _endOfStream = false;
            _cacheLength = 0;

        }

        /// <summary>
        /// ストリームから1文字読み込みます。
        /// </summary>
        /// <returns>
        /// nullである場合は、ストリームの終端に達したことを意味します。
        /// nullではない場合、それはストリームから読み込んだ文字です。
        /// </returns>
        public char? Read()
        {
            if (_cacheLength > 0)
            {
                var c = _cacheBuffer[0];
                if (_cacheLength > 1)
                    Array.Copy(_cacheBuffer, 1, _cacheBuffer, 0, _cacheLength - 1);
                --_cacheLength;
                return c;
            }
            else if (_endOfStream)
            {
                return null;
            }
            else
            {
                var c = _rawReader.Read();
                if (c < 0)
                {
                    _endOfStream = true;
                    return null;
                }

                return (char)c;
            }
        }

        /// <summary>
        /// ストリームの終端に達している場合は true、そうではない場合は false です。
        /// </summary>
        public bool IsEndOfReader
        {
            get
            {
                FillCache();
                return _cacheLength <= 0 && _endOfStream;
            }
        }

        /// <summary>
        /// ストリームのまだ読み込んでいない部分の先頭が指定した文字と一致しているかどうかを調べます。
        /// </summary>
        /// <param name="c">
        /// 比較する文字です。
        /// </param>
        /// <returns>
        /// ストリームのまだ読み込んでいない部分の先頭が <paramref name="c"/> と一致していれば true、そうではない場合は false です。
        /// </returns>
        public bool StartsWith(char c)
        {
            FillCache();
            return _cacheLength > 0 && _cacheBuffer[0] == c;
        }

        /// <summary>
        /// ストリームのまだ読み込んでいない部分の先頭が指定した文字列から始まっているかどうかを調べます。
        /// </summary>
        /// <param name="s">
        /// 比較する文字列です。
        /// </param>
        /// <returns>
        /// ストリームのまだ読み込んでいない部分の先頭が <paramref name="s"/> から始まっていれば true、そうではない場合は false です。
        /// </returns>
        public bool StartsWith(string s)
        {
            if (s.Length > _cacheBuffer.Length)
                throw new ArgumentException($"The string length of parameter \"{nameof(s)}\" must be less than or equal to {_cacheBuffer.Length}.", nameof(s));
            FillCache();
            if (_cacheLength < s.Length)
                return false;
            for (var index = 0; index < s.Length; ++index)
            {
                if (_cacheBuffer[index] != s[index])
                    return false;
            }

            return true;
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                    _rawReader.Dispose();
                _isDisposed = true;
            }
        }

        private void FillCache()
        {
            while (_cacheLength < _cacheBuffer.Length)
            {
                if (_endOfStream)
                    break;
                var c = _rawReader.Read();
                if (c < 0)
                {
                    _endOfStream = true;
                    break;
                }

                _cacheBuffer[_cacheLength] = (char)c;
                ++_cacheLength;
            }
        }
    }
}
