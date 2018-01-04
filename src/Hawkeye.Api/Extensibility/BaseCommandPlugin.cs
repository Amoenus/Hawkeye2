using System;
using System.Drawing;
using Hawkeye.Logging;

namespace Hawkeye.Extensibility
{
    /// <summary>
    ///     Base class helping in building command plugins
    /// </summary>
    public abstract class BaseCommandPlugin : BasePlugin, ICommandPlugin
    {
        private readonly ILogService _log = null;

        /// <inheritdoc />
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:Hawkeye.Extensibility.BaseCommandPlugin" /> class.
        /// </summary>
        /// <param name="pluginDescriptor">The plugin descriptor.</param>
        /// <exception cref="T:System.ArgumentNullException">pluginDescriptor</exception>
        public BaseCommandPlugin(IPluginDescriptor pluginDescriptor) :
            base(pluginDescriptor)
        {
        }

        /// <summary>
        ///     Executes this plugin command.
        /// </summary>
        protected virtual void ExecuteCore()
        {
        }

        /// <summary>
        ///     Determines whether this plugin command can be executed.
        /// </summary>
        /// <returns>
        ///     <c>true</c> if the command can be executed; otherwise, <c>false</c>.
        /// </returns>
        protected virtual bool CanExecuteCore()
        {
            return false;
        }

        /// <summary>
        ///     Raises the event to enable plugins to check if they can get executed
        /// </summary>
        /// <param name="sender">The sender.</param>
        protected void RaiseCanExecuteChanged(object sender)
        {
            CanExecuteChanged?.Invoke(sender, EventArgs.Empty);
        }

        #region ICommandPlugin Members

        /// <inheritdoc />
        /// <summary>
        ///     Gets the label displayed on the menu (or toolbar button) for this command.
        /// </summary>
        public abstract string Label { get; }

        /// <inheritdoc />
        /// <summary>
        ///     Gets the image displayed on the menu (or toolbar button) for this command.
        /// </summary>
        public virtual Bitmap Image => null;

        /// <inheritdoc />
        /// <summary>
        ///     Executes this command.
        /// </summary>
        public void Execute()
        {
            EnsureInitialized();
            if (CanExecute())
            {
                ExecuteCore();
            }
        }

        /// <inheritdoc />
        /// <summary>
        ///     Determines whether the command can be executed.
        /// </summary>
        /// <returns>
        ///     <c>true</c> if the command can be executed; otherwise, <c>false</c>.
        /// </returns>
        public bool CanExecute()
        {
            try
            {
                EnsureInitialized();
                return CanExecuteCore();
            }
            catch (Exception ex)
            {
                _log.Error($"Could not determine whether command can be executed: {ex.Message}", ex);
                return false;
            }
        }

        /// <inheritdoc />
        /// <summary>
        ///     Is raised to enable plugins to check if they can get executed
        /// </summary>
        public event EventHandler CanExecuteChanged;

        #endregion
    }
}