using System;
using System.Text;

namespace Hawkeye.Extensions
{
    /// <summary>
    /// <see cref="Exception"/> formatting extension method.
    /// </summary>
    public static class ExceptionExtensions
    {
        #region Exception

        /// <summary>
        /// Returns <see cref="Exception"/> information into a formatted string.
        /// </summary>
        /// <param name="exception">The exception to describe.</param>
        /// <returns>Formatted (and indented) string giving information about <paramref name="exception"/>.</returns>
        public static string ToFormattedString(this Exception exception)
        {
            if (exception == null) return string.Empty;

            const string tab = "   ";
            const string leafEx = " + ";
            const string leafTr = " | ";
            string indent = string.Empty;

            var builder = new StringBuilder();
            for (var currentException = exception; currentException != null; currentException = currentException.InnerException)
            {
                builder.Append(indent);
                builder.Append(leafEx);
                builder.Append("[");
                builder.Append(currentException.GetType());
                builder.Append("] ");
                builder.Append(currentException.Message);
                builder.Append(Environment.NewLine);

                indent += tab;

                AppendStackTrace(currentException, builder, indent, leafTr);
            }

            return builder.ToString();
        }

        private static void AppendStackTrace(Exception currentException, StringBuilder builder, string indent, string leafTr)
        {
            if (currentException.StackTrace == null)
                return;

            var stackTrace = currentException.StackTrace
                .Replace(Environment.NewLine, "\n").Split('\n');

            foreach (string trace in stackTrace)
            {
                builder.Append(indent);
                builder.Append(leafTr);
                builder.Append(trace.Trim());
                builder.Append(Environment.NewLine);
            }
        }

        #endregion
    }
}
