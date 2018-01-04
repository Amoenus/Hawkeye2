using System;
using System.Collections.Generic;

namespace Hawkeye.Logging
{
    /// <summary>
    ///     Interface implemented by logging service factories
    /// </summary>
    internal interface ILogServiceFactory
    {
        /// <summary>
        ///     Obtains an instance of the logger service for the specified
        ///     <paramref name="type" /> and optional additional data.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="additionalData">The additional data.</param>
        /// <returns>
        ///     An implementation of <see cref="ILogService" /> .
        /// </returns>
        ILogService GetLogger(Type type, IDictionary<string, object> additionalData = null);

        /// <summary>
        ///     Closes all the resources held by the various loggers.
        /// </summary>
        void Shutdown();
    }
}