using System;
using Hawkeye.Extensibility;

namespace Hawkeye.DecompilePlugin.Reflector
{
    internal class ReflectorPluginDescriptor : IPluginDescriptor
    {
        private static readonly Version version = new Version(AssemblyVersionInfo.Version);

        #region IPluginDescriptor Members

        /// <summary>
        /// Gets this plugin name.
        /// </summary>
        public string Name => "Reflector Plugin";

        /// <summary>
        /// Gets this plugin version.
        /// </summary>
        public Version Version => version;

        /// <summary>
        /// Creates an instance of the plugin passing it the specified host.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <returns>
        /// Created plugin instance.
        /// </returns>
        public IPlugin Create(IHawkeyeHost host)
        {
            var plugin = new ReflectorPluginCore(this);
            plugin.Initialize(host);
            return plugin;
        }

        #endregion
    }
}
