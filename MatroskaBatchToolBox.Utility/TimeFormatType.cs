namespace MatroskaBatchToolBox.Utility
{
    public enum TimeFormatType
    {
        /// <summary>
        /// Format as "&lt;hours&gt;:&lt;minutes&gt;:&lt;seconds&gt;[.&lt;fractional seconds&gt;]".
        /// </summary>
        LongFormat,

        /// <summary>
        /// Format as "&lt;minutes&gt;:&lt;seconds&gt;[.&lt;fractional seconds&gt;]".
        /// </summary>
        ShortFormat,

        /// <summary>
        /// Format as "&lt;seconds&gt;[.&lt;fractional seconds&gt;]".
        /// </summary>
        OnlySeconds,

        /// <summary>
        /// Shortest format of any of the following:
        /// <list type="bullet">
        /// <item><see cref="LongFormat"/></item>
        /// <item><see cref="ShortFormat"/></item>
        /// <item><see cref="OnlySeconds"/></item>
        /// </list>
        /// </summary>
        LazyFormat,
    }
}
