using System;

namespace Palmtree.IO
{
    /// <summary>
    /// 先読みが可能な入力文字ストリームのインターフェースです。
    /// </summary>
    public interface IPrefetchableTextReader
    {
        /// <summary>
        /// 入力ストリームのまだ読み込んでいない部分の先頭が指定された文字と一致しているかどうかを調べます。
        /// </summary>
        /// <param name="c">一致しているかどうかを調べる文字です。</param>
        /// <returns>
        /// 入力ストリームのまだ読み込んでいない部分の先頭が <paramref name="c"/> と一致しているなら true、そうでない場合は false です。
        /// </returns>
        /// <remarks>
        /// <list type="bullet">
        /// <item>入力ストリームの終端に達している場合は常に false が返ります。</item>
        /// <item>
        /// 例えば、コンソールや通信回線経由など、インタラクティブまたは遅延のあるストリームの場合、「入力ストリームはまだ閉じられていないが次の文字はまだ読めない」という状況があり得ます。
        /// そのような場合もこのメソッドは false を返します。
        /// </item>
        /// </list>
        /// </remarks>
        bool StartsWith(char c);

        /// <summary>
        /// 入力ストリームのまだ読み込んでいない部分の先頭が指定された文字列と一致しているかどうかを調べます。
        /// </summary>
        /// <param name="s">一致しているかどうかを調べる文字列です。</param>
        /// <returns>
        /// 入力ストリームのまだ読み込んでいない部分の先頭が <paramref name="s"/> と一致しているなら true、そうでない場合は false です。
        /// </returns>
        /// <remarks>
        /// <list type="bullet">
        /// <item>
        /// <para>
        /// 例えば、コンソールや通信回線経由などの非同期文字ストリームの場合、「入力ストリームはまだ閉じられていないが次の文字はまだ読めない」という状況があり得ます。
        /// そのような理由で文字列が一致しない場合でもこのメソッドは false を返します。
        /// </para>
        /// <para>
        /// 例えば、"Good morning\r\nHello" という文字列を受信出来る予定で、実際に受信できているのが、"Good morning\r\"までだとします。そのような状況で <see cref="StartsWith(string)"/> メソッドに "Good morning\r\nHello" を渡して呼び出しても false が返ります。
        /// </para>
        /// </item>
        /// </list>
        /// </remarks>
        bool StartsWith(string s);

        /// <summary>
        /// 入力ストリームから文字を読み込みます。
        /// </summary>
        /// <returns>
        /// null ではないならそれは読み込まれた文字です。
        /// null であれば入力ストリームの終端に達したことを意味します。
        /// </returns>
        char? Read();
    }
}
