using System;
using System.Windows.Forms;
using Hawkeye.DecompilePlugin.Reflector;
using Hawkeye.Extensibility;
using Hawkeye.Extensions;
using Hawkeye.Logging;

namespace Hawkeye.DecompilePlugin
{
    internal abstract class BaseDecompilerPluginCore : BaseCommandPlugin
    {
        private IDecompilerController _controller;
        private ILogService _log;
        private IWindowInfo _windowInfo;

        /// <summary>
        ///     <para>
        ///         Initializes a new instance of the <see cref="ReflectorPluginCore" />
        ///     </para>
        ///     <para>class.</para>
        /// </summary>
        /// <param name="descriptor">The descriptor.</param>
        public BaseDecompilerPluginCore(IPluginDescriptor descriptor) :
            base(descriptor)
        {
        }

        /// <inheritdoc />
        public override string Label => "&Decompile";

        protected virtual string DecompilerNotAvailable => @"A running instance of the decompiler could not be found. 
Hawkeye can not show you the source code for the selected item.

Make sure it is running";

        protected abstract IDecompilerController CreateDecompilerController();

        /// <summary>
        ///     Called when the plugin has just been initialized.
        /// </summary>
        /// <inheritdoc />
        protected override void OnInitialized()
        {
            base.OnInitialized();

            _controller = CreateDecompilerController();

            _log = Host.GetLogger<ReflectorPluginCore>();
            Host.CurrentWindowInfoChanged += (s, e) =>
            {
                _windowInfo = Host.CurrentWindowInfo;
                RaiseCanExecuteChanged(this);
            };

            _windowInfo = Host.CurrentWindowInfo;
            RaiseCanExecuteChanged(this);

            _log.Info($"'{Descriptor.Name}' was initialized.");
        }

        /// <summary>
        ///     Determines whether this plugin command can be executed.
        /// </summary>
        /// <returns>
        ///     <para>
        ///         <c>true</c> if the command can be executed; otherwise, <c>false</c>
        ///     </para>
        ///     <para>.</para>
        /// </returns>
        /// <inheritdoc />
        protected override bool CanExecuteCore()
        {
            return _windowInfo?.ControlInfo?.Control != null;
        }

        /// <summary>
        ///     Executes this plugin command.
        /// </summary>
        /// <inheritdoc />
        protected override void ExecuteCore()
        {
            if (!CanExecuteCore())
            {
                return;
            }

            Cursor savedCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                OpenInDecompiler();
            }
            finally
            {
                Cursor.Current = savedCursor;
            }
        }

        private void OpenInDecompiler()
        {
            if (!_controller.IsRunning)
            {
                MessageBox.Show(DecompilerNotAvailable,
                    "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Control control = _windowInfo.ControlInfo.Control;
            Type type = control.GetType();

            // Remark: the logic here is really simplified compared with the Hawkeye 1 Reflector facility:
            // In Hawkeye 1, we tried to get Reflector to point to the exact property, member, event or method
            // that was currently selected.
            // Here, we only open the current type in Reflector.
            // Indeed this makes more sense, because often times, in the previous version, what was selected
            // was some member inherited from Control and Reflector was not loading the really inspected
            // control but some member of the System.Windows.Forms.Control class.

            _controller.GotoType(type);
        }
    }
}