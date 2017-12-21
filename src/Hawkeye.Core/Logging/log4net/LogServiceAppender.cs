using System;
using log4net.Appender;
using log4net.Core;

namespace Hawkeye.Logging.log4net
{
    internal class LogServiceAppender : AppenderSkeleton, ILogLevelThresholdSelector
    {
        private bool _closed;
        private readonly ILogService _logService;

        public LogServiceAppender(ILogService logServiceToAppend)
        {
            _logService = logServiceToAppend ?? throw new ArgumentNullException(nameof(logServiceToAppend));
        }

        #region ILogLevelThresholdSelector Members

        /// <inheritdoc />
        /// <summary>
        ///     Gets or sets the log level threshold.
        /// </summary>
        /// <value>The log level threshold.</value>
        public LogLevel LogLevelThreshold
        {
            get => Log4NetHelper.Log4NetLevelToLogLevel(Threshold);
            set => Threshold = Log4NetHelper.LogLevelToLog4NetLevel(value);
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Close();
        }

        #endregion

        /// <inheritdoc />
        /// <summary>
        ///     Subclasses of <see cref="T:log4net.Appender.AppenderSkeleton" /> should implement this method
        ///     to perform actual logging.
        /// </summary>
        /// <param name="loggingEvent">The event to append.</param>
        /// <remarks>
        ///     <para>
        ///         A subclass must implement this method to perform
        ///         logging of the <paramref name="loggingEvent" />.
        ///     </para>
        ///     <para>
        ///         This method will be called by
        ///         <see cref="M:log4net.Appender.AppenderSkeleton.DoAppend(log4net.Core.LoggingEvent)" />
        ///         if all the conditions listed for that method are met.
        ///     </para>
        ///     <para>
        ///         To restrict the logging of events in the appender
        ///         override the <see cref="M:log4net.Appender.AppenderSkeleton.PreAppendCheck" /> method.
        ///     </para>
        /// </remarks>
        protected override void Append(LoggingEvent loggingEvent)
        {
            if (_closed)
            {
                return;
            }

            // Only log if event's level is >= threshold
            LogLevel level = Log4NetHelper.Log4NetLevelToLogLevel(loggingEvent.Level);
            if (level < LogLevelThreshold)
            {
                return;
            }

            LogEntry entry = Log4NetHelper.LoggingEventToLogEntry(loggingEvent);
            if (Layout != null)
            {
                entry.Formatter = new Log4NetLayoutFormatter(Layout);
            }

            _logService.LogEntry(entry);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Is called when the appender is closed. Derived classes should override
        ///     this method if resources need to be released.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Releases any resources allocated within the appender such as file handles,
        ///         network connections, etc.
        ///     </para>
        ///     <para>
        ///         It is a programming error to append to a closed appender.
        ///     </para>
        /// </remarks>
        protected override void OnClose()
        {
            base.OnClose();
            _closed = true;
        }
    }
}