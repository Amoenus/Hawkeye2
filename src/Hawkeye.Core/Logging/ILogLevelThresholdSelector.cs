using System;

namespace Hawkeye.Logging
{
    /// <summary>
    ///     Interface implemented by classes that can specify the maximum log level
    ///     to trace.
    /// </summary>
    /// <inheritdoc />
    internal interface ILogLevelThresholdSelector : IDisposable
    {
        /// <summary>
        ///     Gets or sets the log level threshold.
        /// </summary>
        LogLevel LogLevelThreshold { get; set; }
    }
}