using System;
using System.IO;
using log4net.Util;

namespace Hawkeye.Logging.log4net
{
    /// <inheritdoc />
    public class SpecialFolderPatternConverter : PatternConverter
    {
        /// <summary>
        ///     Evaluate this pattern converter and write the output to a writer.
        /// </summary>
        /// <param name="writer">
        ///     <see cref="TextWriter" /> that will receive the formatted result.
        /// </param>
        /// <param name="state">
        ///     The state object on which the pattern converter should be executed.
        /// </param>
        /// <inheritdoc />
        protected override void Convert(TextWriter writer, object state)
        {
            var specialFolder = (Environment.SpecialFolder) Enum.Parse(
                typeof(Environment.SpecialFolder), Option, true);
            writer.Write(Environment.GetFolderPath(specialFolder));
        }
    }
}