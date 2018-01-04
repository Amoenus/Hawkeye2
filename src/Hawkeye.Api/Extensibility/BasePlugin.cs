using System;

namespace Hawkeye.Extensibility
{
    /// <summary>
    ///     Base class helping in building plugins
    /// </summary>
    /// <inheritdoc />
    public abstract class BasePlugin : IPlugin
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="BasePlugin" /> class.
        /// </summary>
        /// <param name="pluginDescriptor">The plugin descriptor.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="pluginDescriptor" />
        /// </exception>
        protected BasePlugin(IPluginDescriptor pluginDescriptor)
        {
            Descriptor = pluginDescriptor ?? throw new ArgumentNullException(nameof(pluginDescriptor));
        }

        /// <summary>
        ///     Gets the host for this plugin.
        /// </summary>
        protected IHawkeyeHost Host { get; private set; }

        /// <summary>
        ///     Gets a value indicating whether the plugin is initialized.
        /// </summary>
        /// <value>
        ///     <c>true</c> if initialized; otherwise, <c>false</c> .
        /// </value>
        protected bool IsInitialized { get; private set; }

        /// <summary>
        ///     Called when the plugin has just been initialized.
        /// </summary>
        protected virtual void OnInitialized()
        {
        }

        /// <summary>
        ///     Ensures this plugin is initialized; throws if not.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///     The plugin is not initialized.
        /// </exception>
        protected void EnsureInitialized()
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException("The plugin is not initialized.");
            }
        }

        #region IPlugin Members

        /// <summary>
        ///     Gets the descriptor instance that created this plugin.
        /// </summary>
        /// <inheritdoc />
        public IPluginDescriptor Descriptor { get; }

        /// <summary>
        ///     Initializes this plugin passing it the specified host.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="host" />
        /// </exception>
        /// <inheritdoc />
        public void Initialize(IHawkeyeHost host)
        {
            Host = host ?? throw new ArgumentNullException(nameof(host));
            IsInitialized = true;
            OnInitialized();
        }

        #endregion
    }
}