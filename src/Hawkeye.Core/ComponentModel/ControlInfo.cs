using System;
using System.ComponentModel;
using System.Windows.Forms;
using Hawkeye.Logging;

namespace Hawkeye.ComponentModel
{
    [TypeConverter(typeof(DotNetInfoConverter))]
    internal class ControlInfo : IControlInfo, IProxy
    {
        private static readonly ILogService Log = LogManager.GetLogger<ControlInfo>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="ControlInfo" /> class.
        /// </summary>
        /// <param name="hwnd">The Window handle of the control.</param>
        public ControlInfo(IntPtr hwnd)
        {
            Control = GetControlFromHandle(hwnd);
            InitializeTypeDescriptor(Control);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ControlInfo" /> class.
        /// </summary>
        /// <param name="control">The control.</param>
        public ControlInfo(Control control)
        {
            if (control == null)
            {
                Log.Warning("Specified control is null.");
            }

            Control = control;
            InitializeTypeDescriptor(Control);
        }

        #region IProxy Members

        /// <inheritdoc />
        public object Value => Control;

        #endregion

        private static void InitializeTypeDescriptor(object instance)
        {
            if (instance == null)
            {
                return;
            }

            Type type = instance.GetType();
            CustomTypeDescriptors.AddGenericProviderToType(type);
        }

        private static Control GetControlFromHandle(IntPtr hwnd)
        {
            Control control = Control.FromHandle(hwnd); // Usually this is enough
            if (control == null)
            {
                // But in some cases (when inspecting most Visual Studio controls for instance), it is not...
                // Is there a workaround? It seems that only the VS property grid & the windows forms designer
                // can be inspected
                Log.Warning(
                    $"Handle {hwnd} is not associated with a .NET control: 'Control.FromHandle(hwnd)' returns null.");
            }

            return control;
        }

        #region IControlInfo Members

        /// <inheritdoc />
        public Control Control
        {
            get;
#if DEBUG
            set; // Needed for tests purpose
#else
            private set;
#endif
        }

        /// <inheritdoc />
        public string Name
        {
            get
            {
                if (Control == null)
                {
                    return string.Empty;
                }

                string name = Control.Name;
                return !string.IsNullOrEmpty(name) ? name : Control.GetType().Name;
            }
        }

        #endregion
    }
}