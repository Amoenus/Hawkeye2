using System;
using System.Text;

namespace FxDetector
{
    /// <summary>
    /// </summary>
    internal static class StringBuilderExtensions
    {
        /// <summary>
        ///     Appends the formatted line.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public static void AppendFormattedLine(
            this StringBuilder builder,
            string format,
            params object[] args)
        {
            AppendFormattedLine(builder, 0, format, args);
        }

        /// <summary>
        ///     Appends the formatted line.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="tabCount">The tab count.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public static void AppendFormattedLine(
            this StringBuilder builder,
            int tabCount,
            string format,
            params object[] args)
        {
            var tabs = new string('\t', tabCount);
            string text = tabs + format + Environment.NewLine;
            builder.AppendFormat(text, args);
        }
    }
}