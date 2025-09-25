namespace MatroskaBatchToolBox.Utility
{
    public enum TimeParsingMode
    {
        /// <summary>
        /// Exact format: &lt;hours&gt;:&lt;minutes&gt;:&lt;seconds&gt;[.&lt;second fraction&gt;]
        /// </summary>
        StrictForLongTimeFormat,

        /// <summary>
        /// Exact format: &lt;minutes&gt;:&lt;seconds&gt;[.&lt;second fraction&gt;]
        /// </summary>
        StrictForShortTimeFormat,

        /// <summary>
        /// Exact format: &lt;seconds&gt;[.&lt;second fraction&gt;]
        /// </summary>
        StrictForVeryShortTimeFormat,

        /// <summary>
        /// Accept any of the following formats:
        /// <list type="bullet">
        /// <item><see cref="StrictForLongTimeFormat"/></item>
        /// <item><see cref="StrictForShortTimeFormat"/></item>
        /// <item><see cref="StrictForVeryShortTimeFormat"/></item>
        /// </list>
        /// </summary>
        LazyMode,

        /// <summary>
        /// This describes the <see cref="LazyMode"/> format and the format of expressions that add or subtract values ​​in <see cref="LazyMode"/> format.
        /// </summary>
        Expression,
    }
}
