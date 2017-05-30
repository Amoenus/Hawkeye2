using System;

namespace Hawkeye.Extensibility
{
    /// <summary>
    /// Base class helping in building plugins
    /// </summary>
    public abstract class BasePlugin : IPlugin
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BasePlugin"/> class.
        /// </summary>
        /// <param name="pluginDescriptor">The plugin descriptor.</param>
        /// <exception cref="System.ArgumentNullException">pluginDescriptor</exception>
        protected BasePlugin(IPluginDescriptor pluginDescriptor)
        {
            if (pluginDescriptor == null)
                throw new ArgumentNullException(nameof(pluginDescriptor));
            Descriptor = pluginDescriptor;
        }

        #region IPlugin Members

        /// <summary>
        /// Gets the descriptor instance that created this plugin.
        /// </summary>
        public IPluginDescriptor Descriptor { get; }

        /// <summary>
        /// Initializes this plugin passing it the specified host.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <exception cref="System.ArgumentNullException">host</exception>
        public void Initialize(IHawkeyeHost host)
        {
            if (host == null) throw new ArgumentNullException(nameof(host));
            Host = host;
            IsInitialized = true;
            OnInitialized();
        }

        #endregion

        /// <summary>
        /// Gets the host for this plugin.
        /// </summary>
        protected IHawkeyeHost Host { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the plugin is initialized.
        /// </summary>
        /// <value>
        ///   <c>true</c> if initialized; otherwise, <c>false</c>.
        /// </value>
        protected bool IsInitialized { get; private set; }

        /// <summary>
        /// Called when the plugin has just been initialized.
        /// </summary>
        protected virtual void OnInitialized() { }

        /// <summary>
        /// Ensures this plugin is initialized; throws if not.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">The plugin is not initialized.</exception>
        protected void EnsureInitialized()
        {
            if (!IsInitialized)
                throw new InvalidOperationException("The plugin is not initialized.");
        }
    }
}
