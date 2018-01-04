using System;
using System.Text;
using Hawkeye.Extensions;

namespace Hawkeye.Logging
{
    /// <summary>
    ///     This class stores the properties a logging message is made of.
    /// </summary>
    /// <inheritdoc />
    internal class LogEntry : ILogEntry
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ILogEntry" /> class.
        /// </summary>
        public LogEntry()
        {
            TimeStamp = DateTime.Now;
            Formatter = DefaultFormatter.Instance;
        }

        /// <summary>
        ///     Gets or sets the creation time of this entry.
        /// </summary>
        public DateTime TimeStamp { get; set; }

        /// <summary>
        ///     Gets or sets additional data related to this log entry.
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        ///     Gets or sets the formatter for this entry.
        /// </summary>
        /// <value>
        ///     The formatter.
        /// </value>
        internal ILogEntryFormatter Formatter { get; set; }

        /// <summary>
        ///     Gets or sets the source name for this entry.
        /// </summary>
        /// <value>
        ///     The source name.
        /// </value>
        /// <inheritdoc />
        public string Source { get; set; }

        /// <summary>
        ///     Gets or sets the trace level for this entry.
        /// </summary>
        /// <inheritdoc />
        public LogLevel Level { get; set; }

        /// <summary>
        ///     Gets or sets the message object for this entry.
        /// </summary>
        /// <inheritdoc />
        public string Message { get; set; }

        /// <summary>
        ///     Gets or sets the exception for this entry.
        /// </summary>
        /// <inheritdoc />
        public Exception Exception { get; set; }

        /// <summary>
        ///     Returns a <see cref="String" /> that represents the default string
        ///     representation of this entry.
        /// </summary>
        /// <returns>
        ///     A <see cref="String" /> that represents the current
        ///     <see cref="Object" /> .
        /// </returns>
        public override string ToString()
        {
            ILogEntryFormatter formatter = Formatter ?? DefaultFormatter.Instance;
            return formatter.FormatEntry(this);
        }

        private class DefaultFormatter : ILogEntryFormatter
        {
            public static readonly DefaultFormatter Instance = new DefaultFormatter();

            #region ILogEntryFormatter Members

            public string FormatEntry(LogEntry entry)
            {
                if (entry == null)
                {
                    return string.Empty;
                }

                var sb = new StringBuilder();

                string source = entry.Source;
                if (string.IsNullOrEmpty(entry.Source))
                {
                    source = "Default source";
                }

                sb.AppendFormat("[{0}] {1}",
                    source, entry.Level.ToString().ToUpperInvariant());

                if (entry.TimeStamp != DateTime.MinValue && entry.TimeStamp != DateTime.MaxValue)
                {
                    sb.AppendFormat(" [{0}]", entry.TimeStamp.ToLongInvariantString());
                }

                sb.Append(": ");

                var sbText = new StringBuilder();

                if (!string.IsNullOrEmpty(entry.Message))
                {
                    sbText.Append(entry.Message);
                    if (entry.Exception != null)
                    {
                        sbText.Append(" - ");
                    }
                }

                if (entry.Exception != null)
                {
                    sbText.Append(entry.Exception.ToFormattedString());
                }

                string text = sbText.ToString();
                sb.Append(string.IsNullOrEmpty(text) ? "No message" : text);

                return sb.ToString();
            }

            #endregion
        }
    }
}