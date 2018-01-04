using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Hawkeye.Logging
{
    internal partial class LogManager
    {
        /// <summary>
        ///     This Logging service implementation logs its entries to the Visual
        ///     Studio output Window.
        /// </summary>
        /// <remarks>
        ///     It is used by the <see cref="LogManager" /> so that an
        ///     implementation of <see cref="ILogServiceFactory" /> is available in
        ///     design mode; otherwise, Visual Studio crashes (because of
        ///     <see langword="static" /> readonly log initializations in forms and
        ///     controls).
        /// </remarks>
        private class DebugLogger : BaseLogService, ILogServiceFactory
        {
            private static readonly Type ThisServiceType = typeof(DebugLogger);
            private readonly Type _sourceType;

            /// <summary>
            ///     Initializes a new instance of the
            ///     <see cref="LogManager.DebugLogger" /> class.
            /// </summary>
            /// <param name="type">The type.</param>
            public DebugLogger(Type type)
            {
                if (type == null)
                {
                    type = GetType();
                }

                _sourceType = type;
            }

            /// <summary>
            ///     Gets the type this logger is attached to.
            /// </summary>
            /// <value>
            ///     The type this logger is attached to.
            /// </value>
            /// <inheritdoc />
            protected override Type SourceType => _sourceType ?? ThisServiceType;

            /// <summary>
            ///     Logs the specified log entry.
            /// </summary>
            /// <param name="entry">The entry to log.</param>
            /// <inheritdoc />
            public override void LogEntry(ILogEntry entry)
            {
                entry.Source = SourceType.Name;
                Debug.WriteLine(entry.ToString());
            }

            #region ILogServiceFactory Members

            /// <inheritdoc />
            public ILogService GetLogger(Type type, IDictionary<string, object> additionalData = null)
            {
                return new DebugLogger(type);
            }

            /// <summary>
            ///     Closes all the resources held by the various loggers.
            /// </summary>
            /// <inheritdoc />
            public void Shutdown()
            {
                // Nothing to do here!
            }

            #endregion
        }
    }
}