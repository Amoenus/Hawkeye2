using System;
using System.IO;

namespace Hawkeye
{
    internal class HawkeyeApplicationInfo : IHawkeyeApplicationInfo
    {
        private static readonly string HawkeyeDataDirectory;

        /// <summary>
        /// Initializes the <see cref="HawkeyeApplicationInfo" /> class.
        /// </summary>
        static HawkeyeApplicationInfo()
        {
            HawkeyeDataDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "Hawkeye");
        }

        #region IHawkeyeApplicationInfo Members

        /// <summary>
        /// Gets the application data directory.
        /// </summary>
        public string ApplicationDataDirectory => HawkeyeDataDirectory;

        #endregion
    }
}
