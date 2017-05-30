using System;
using Hawkeye.DecompilePlugin.ilspy;
using Hawkeye.Extensibility;

namespace Hawkeye.DecompilePlugin.IlSpy
{
    internal class IlSpyPluginDescriptor : IPluginDescriptor
    {
        private static readonly Version version = new Version(AssemblyVersionInfo.Version);

        #region IPluginDescriptor Members

        public string Name => "ILSpy Plugin";

        public Version Version => version;

        public IPlugin Create(IHawkeyeHost host)
        {
            var plugin = new ILSpyPluginCore(this);
            plugin.Initialize(host);
            return plugin;
        }

        #endregion
    }
}
