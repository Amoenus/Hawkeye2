using System;
using System.Collections.Generic;
using log4net.Core;
using log4net.Layout;

namespace Hawkeye.Logging.log4net
{
    /// <summary>
    ///     <para>
    ///         Wraps the logging Log4Net logging framework into an
    ///         <see cref="Toaster.Logging.ILogService" />
    ///     </para>
    ///     <para>
    ///         so that it can be used as any logging service of the Sopra framework.
    ///     </para>
    /// </summary>
    internal class Log4NetService : BaseLogService, ILogServiceAppendable
    {
        /// <summary>
        ///     The pattern parameter
        /// </summary>
        public const string PatternParameter = "PATTERN";

        private static readonly Type ThisServiceType = typeof(Log4NetService);

        private readonly ILogger _currentLogger;

        private readonly Type _sourceType;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Log4NetService" />
        ///     class.
        /// </summary>
        /// <remarks>
        ///     As we don't state anything, log4net will look for its configuration
        ///     inside the application's configuration file (
        ///     <b>
        ///         web.config
        ///     </b>
        ///     or
        ///     <b>
        ///         app.exe.config
        ///     </b>
        ///     ) which will have to contain a
        ///     <b>
        ///         <log4net>
        ///     </b>
        ///     section.
        /// </remarks>
        /// <param name="type">
        ///     The type that requests the creation of a log service.
        /// </param>
        public Log4NetService(Type type)
        {
            _sourceType = type;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Log4NetService" />
        ///     class.
        /// </summary>
        /// <param name="log">
        ///     An existing Log4Net <see cref="log4net.ILog" /> object.
        /// </param>
        internal Log4NetService(ILogger logger)
        {
            _currentLogger = logger;
        }

        /// <summary>
        ///     Gets the current log object.
        /// </summary>
        /// <value>
        ///     The current log object.
        /// </value>
        private ILogger CurrentLogger => _currentLogger ?? global::log4net.LogManager.GetLogger(SourceType).Logger;

        /// <summary>
        ///     Gets the type this logger is attached to.
        /// </summary>
        /// <value>
        ///     The type this logger is attached to.
        /// </value>
        protected override Type SourceType => _sourceType ?? ThisServiceType;

        #region ILogServiceAppendable Members

        public ILogLevelThresholdSelector AppendLogService(ILogService logService,
            IDictionary<string, object> additionalData = null)
        {
            if (CurrentLogger == null)
            {
                return null;
            }

            var appenderAttachable = CurrentLogger as IAppenderAttachable;
            if (appenderAttachable == null)
            {
                return null;
            }

            var appender = new LogServiceAppender(logService);

            // let's examine potential parameters
            if (additionalData != null && additionalData.ContainsKey(PatternParameter))
            {
                object parameter = additionalData[PatternParameter];
                if (parameter != null && parameter is string)
                {
                    var pattern = (string) parameter;
                    appender.Layout = new PatternLayout(pattern);
                }
            }

            appender.LogLevelThreshold = LogLevel.All;
            appenderAttachable.AddAppender(appender);
            return appender;
        }

        #endregion

        /// <summary>
        ///     Logs the specified log entry.
        /// </summary>
        /// <param name="entry">The entry to log.</param>
        public override void LogEntry(ILogEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }

            CurrentLogger.Log(SourceType,
                Log4NetHelper.LogLevelToLog4NetLevel(entry.Level),
                entry.Message, entry.Exception);
        }
    }
}