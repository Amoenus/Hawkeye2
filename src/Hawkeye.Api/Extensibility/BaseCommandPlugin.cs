using System;
using System.Drawing;

using Hawkeye.Logging;

namespace Hawkeye.Extensibility
{
    /// <summary>
    /// Base class helping in building command plugins
    /// </summary>
    public abstract class BaseCommandPlugin : BasePlugin, ICommandPlugin
    {
        private ILogService log = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseCommandPlugin"/> class.
        /// </summary>
        /// <param name="pluginDescriptor">The plugin descriptor.</param>
        /// <exception cref="System.ArgumentNullException">pluginDescriptor</exception>
        public BaseCommandPlugin(IPluginDescriptor pluginDescriptor) : 
            base(pluginDescriptor) { }

        #region ICommandPlugin Members

        /// <summary>
        /// Gets the label displayed on the menu (or toolbar button) for this command.
        /// </summary>
        public abstract string Label { get; }

        /// <summary>
        /// Gets the image displayed on the menu (or toolbar button) for this command.
        /// </summary>
        public virtual Bitmap Image
        {
            get { return null; }
        }

        /// <summary>
        /// Executes this command.
        /// </summary>
        public void Execute()
        {
            base.EnsureInitialized();
            if (CanExecute())
                ExecuteCore();
        }

        /// <summary>
        /// Determines whether the command can be executed.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if the command can be executed; otherwise, <c>false</c>.
        /// </returns>
        public bool CanExecute()
        {
            try
            {
                base.EnsureInitialized();
                return CanExecuteCore();
            }
            catch (Exception ex)
            {
                log.Error($"Could not determine whether command can be executed: {ex.Message}", ex);
                return false;
            }
        }

		/// <summary>
		/// Is raised to enable plugins to check if they can get executed
		/// </summary>
        public event EventHandler CanExecuteChanged;

        #endregion
        
        /// <summary>
        /// Executes this plugin command.
        /// </summary>
        protected virtual void ExecuteCore() { }

        /// <summary>
        /// Determines whether this plugin command can be executed.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if the command can be executed; otherwise, <c>false</c>.
        /// </returns>
        protected virtual bool CanExecuteCore()
        {
            return false;
        }

		/// <summary>
		/// Raises the event to enable plugins to check if they can get executed
		/// </summary>
		/// <param name="sender">The sender.</param>
        protected void RaiseCanExecuteChanged(object sender)
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged(sender, EventArgs.Empty);
        }
    }
}
