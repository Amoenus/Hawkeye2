using System.Collections.Generic;

namespace Hawkeye.Logging
{
    /// <summary>
    ///     Implemented by logging services to which a text box (or any other output
    ///     implementation) can be attached.
    /// </summary>
    internal interface ILogServiceAppendable
    {
        /// <summary>
        ///     Appends the specified log service to a root log service (the current
        ///     instance).
        /// </summary>
        /// <param name="logService">The log service.</param>
        /// <param name="additionalData">The optional additional data.</param>
        /// <returns>
        ///     <para>
        ///         An implementation of <see cref="ILogLevelThresholdSelector" />
        ///     </para>
        ///     <para>allowing to set a maximum log level to trace.</para>
        /// </returns>
        ILogLevelThresholdSelector AppendLogService(ILogService logService,
            IDictionary<string, object> additionalData = null);
    }
}