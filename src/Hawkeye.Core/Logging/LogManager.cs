using System;
using System.Collections.Generic;

namespace Hawkeye.Logging
{
    /// <summary>
    ///     Use this class to obtain an instance of a logging service.
    /// </summary>
    internal static partial class LogManager
    {
        private static readonly ILogServiceFactory Factory;

        /// <summary>
        ///     Initializes the <see cref="LogManager" /> class.
        /// </summary>
        static LogManager()
        {
            // Try to get a factory from HawkeyeApplication
            Factory = HawkeyeApplication.Shell.GetLogServiceFactory()
                      ?? new DebugLogger(null); // fall back
        }

        /// <summary>
        ///     Gets a value indicating whether a text box appender can be attached
        ///     to this <c>LogManager</c> .
        /// </summary>
        /// <value>
        ///     <c>true</c> if this <c>LogManager</c> is text box attachable;
        ///     otherwise, <c>false</c> .
        /// </value>
        public static bool CanAppendLogService => Factory is ILogServiceAppendable;

        /// <summary>
        ///     Gets the default logger.
        /// </summary>
        /// <returns>
        ///     An instance of an object implementing <see cref="ILogService" /> .
        /// </returns>
        public static ILogService GetLogger()
        {
            return GetLogger(null);
        }

        /// <summary>
        ///     Gets a logger instance for the specified type.
        /// </summary>
        /// <typeparam name="T">The type for which to log.</typeparam>
        /// <param name="additionalData">The optional additional data.</param>
        /// <returns>
        ///     An instance of an object implementing <see cref="ILogService" /> .
        /// </returns>
        public static ILogService GetLogger<T>(IDictionary<string, object> additionalData = null)
        {
            return GetLogger(typeof(T), additionalData);
        }

        /// <summary>
        ///     Gets a logger instance for the specified type.
        /// </summary>
        /// <param name="type">The type for which to log.</param>
        /// <param name="additionalData">The optional additional data.</param>
        /// <returns>
        ///     An instance of an object implementing <see cref="ILogService" /> .
        /// </returns>
        public static ILogService GetLogger(Type type, IDictionary<string, object> additionalData = null)
        {
            if (Factory == null) // This may happen in VS designer and this makes VS crash!
            {
                return new DebugLogger(type);
            }

            return Factory.GetLogger(type, additionalData);
        }

        /// <summary>
        ///     Appends the specified log service to the log services produced by
        ///     the inner log services factory.
        /// </summary>
        /// <param name="toAppend">To append.</param>
        /// <param name="additionalData">The additional data.</param>
        /// <returns>
        ///     <para>
        ///         An implementation of <see cref="ILogLevelThresholdSelector" />
        ///     </para>
        ///     <para>allowing to set a maximum log level to trace.</para>
        /// </returns>
        public static ILogLevelThresholdSelector AppendLogService(ILogService toAppend,
            IDictionary<string, object> additionalData = null)
        {
            if (!CanAppendLogService)
            {
                throw new NotSupportedException(
                    "This function is only supported when using a logging framework providing an implementation of 'ILogServiceAppendable'.");
            }

            if (toAppend == null)
            {
                throw new ArgumentNullException(nameof(toAppend));
            }

            return ((ILogServiceAppendable) Factory).AppendLogService(toAppend, additionalData);
        }

        /// <summary>
        ///     Closes all the resources held by the various loggers.
        /// </summary>
        public static void Shutdown()
        {
            Factory?.Shutdown();
        }
    }
}