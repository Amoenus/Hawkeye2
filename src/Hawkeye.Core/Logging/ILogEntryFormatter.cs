namespace Hawkeye.Logging
{
    /// <summary>
    ///     This <see langword="interface" /> is implemented by classes providing
    ///     string formatting to a log entry.
    /// </summary>
    internal interface ILogEntryFormatter
    {
        /// <summary>
        ///     Formats the specified log entry.
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <returns>
        ///     The log <paramref name="entry" /> as a string
        /// </returns>
        string FormatEntry(LogEntry entry);
    }
}