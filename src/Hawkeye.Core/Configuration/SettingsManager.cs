using System;
using System.IO;
using System.Text;
using Hawkeye.Logging;

namespace Hawkeye.Configuration
{
    internal static partial class SettingsManager
    {
        private const string DefaultSettingsFileName = "hawkeye.settings";

        /// <summary>
        ///     The hawkeye store key
        /// </summary>
        public const string HawkeyeStoreKey = "hawkeye";

        private static readonly ILogService Log = LogManager.GetLogger(typeof(SettingsManager));
        private static SettingsManagerImplementation _implementation;

        private static SettingsManagerImplementation Implementation
        {
            get
            {
                if (_implementation == null)
                {
                    throw new ApplicationException("SettingsManager class was not initialized.");
                }

                return _implementation;
            }
        }

        /// <summary>
        ///     Gets the name of the settings file.
        /// </summary>
        /// <value>
        ///     The name of the settings file.
        /// </value>
        public static string SettingsFileName { get; private set; } = string.Empty;

        /// <summary>
        ///     Initializes the Settings manager with the specified filename.
        /// </summary>
        /// <param name="filename">The Settings file name (optional).</param>
        public static void Initialize(string filename = "")
        {
            string resolved = filename;
            try
            {
                if (string.IsNullOrEmpty(resolved))
                {
                    resolved = DefaultSettingsFileName;
                }

                if (!Path.IsPathRooted(resolved)) // combine with default directory
                {
                    resolved = Path.Combine(
                        HawkeyeApplication.Shell.ApplicationInfo.ApplicationDataDirectory, resolved);
                }

                SettingsFileName = resolved; // This is the settings file

                _implementation = new SettingsManagerImplementation();
                if (!File.Exists(SettingsFileName)) // Check file exists
                {
                    _implementation.CreateDefaultSettingsFile(SettingsFileName);
                }

                _implementation.Load(SettingsFileName);
            }
            catch (Exception ex)
            {
                var builder = new StringBuilder();
                builder.AppendLine("There was an error during SettingsManager initialization:");
                builder.AppendFormat("\t- Provided settings file name was: '{0}'.", filename ?? "[NULL]");
                builder.AppendFormat("\t- Resolved settings file name was: '{0}'.", resolved ?? "[NULL]");
                builder.AppendFormat("\t- Error is: {0}.", ex.Message);
                Log.Error(builder.ToString(), ex);
#if DEBUG
                throw;
#endif
            }
        }

        /// <summary>
        ///     Gets the store.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static ISettingsStore GetStore(string key)
        {
            return Implementation.GetStore(key);
        }

        /// <summary>
        ///     Gets the hawkeye store.
        /// </summary>
        /// <returns></returns>
        public static ISettingsStore GetHawkeyeStore()
        {
            return GetStore(HawkeyeStoreKey);
        }

        /// <summary>
        ///     Saves this instance.
        /// </summary>
        public static void Save()
        {
            Implementation.Save(SettingsFileName);
        }
    }
}