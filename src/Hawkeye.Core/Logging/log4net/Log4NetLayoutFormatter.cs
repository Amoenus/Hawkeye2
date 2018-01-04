using System;
using System.IO;
using System.Text;
using log4net.Core;
using log4net.Layout;

namespace Hawkeye.Logging.log4net
{
    internal class Log4NetLayoutFormatter : ILogEntryFormatter
    {
        private readonly ILayout _layout;

        /// <summary>
        ///     Initializes a new instance of the
        ///     <see cref="Log4NetLayoutFormatter" /> class.
        /// </summary>
        /// <param name="log4netLayout">The log4net layout.</param>
        public Log4NetLayoutFormatter(ILayout log4netLayout)
        {
            _layout = log4netLayout ?? throw new ArgumentNullException(nameof(log4netLayout));
        }

        #region ILogEntryFormatter Members

        /// <inheritdoc />
        /// <summary>
        ///     Formats the specified log entry.
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <returns>
        ///     The log <paramref name="entry" /> as a string
        /// </returns>
        public string FormatEntry(LogEntry entry)
        {
            LoggingEvent loggingEvent = Log4NetHelper.LogEntryToLoggingEvent(entry);
            if (loggingEvent == null)
            {
                return string.Empty;
            }

            var builder = new StringBuilder();
            using (var writer = new StringWriter(builder))
            {
                _layout.Format(writer, loggingEvent);
                writer.Close();
            }

            return builder.ToString();
        }

        #endregion
    }
}