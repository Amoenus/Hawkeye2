using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Hawkeye.ComponentModel;
using Hawkeye.Logging;
using Hawkeye.WinApi;

namespace Hawkeye.UI
{
    /// <inheritdoc />
    /// <summary>
    ///     The main Hawkeye UI control.
    /// </summary>
    internal partial class MainControl : UserControl
    {
        private static readonly ILogService Log = LogManager.GetLogger<MainControl>();

        private readonly History<WindowInfo> _history = new History<WindowInfo>();

        /// <inheritdoc />
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:Hawkeye.UI.MainControl" /> class.
        /// </summary>
        public MainControl()
        {
            InitializeComponent();

            // Remove the .NET Tab (to hide it)
            tabs.SuspendLayout();
            tabs.TabPages.Remove(dotNetTabPage);
            tabs.TabPages.Remove(scriptTabPage);
            tabs.ResumeLayout(false);

            dotNetPropertyGrid.ActionClicked += (s, e) => HandleDotNetPropertyGridAction(e.Action);
            dotNetPropertyGrid.ControlCreated += (s, e) => RefreshDotNetPropertyGridActions();
        }

        /// <summary>
        ///     Gets or sets the target Window handle.
        /// </summary>
        /// <value>
        ///     The handle of the spied window.
        /// </value>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IntPtr Target
        {
            get => CurrentInfo?.Handle ?? IntPtr.Zero;
            set
            {
                if (value != IntPtr.Zero)
                {
                    BuildCurrentWindowInfo(value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the current information.
        /// </summary>
        /// <value>
        /// The current information.
        /// </value>
        public WindowInfo CurrentInfo
        {
            get => _history.Current;
            set
            {
                _history.Push(value);
                OnCurrentInfoChanged();
            }
        }

        /// <summary>
        ///     Occurs when the <see cref="CurrentInfo" /> member changes.
        /// </summary>
        public event EventHandler CurrentInfoChanged;

        /// <inheritdoc />
        /// <summary>
        ///     Raises the <see cref="E:System.Windows.Forms.UserControl.Load" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (DesignMode)
            {
                return;
            }


            windowFinderControl.ActiveWindowChanged += (s, _) =>
                hwndBox.Text = windowFinderControl.ActiveWindowHandle.ToString();

            windowFinderControl.WindowSelected += (s, _) =>
            {
                IntPtr hwnd = windowFinderControl.ActiveWindowHandle;
                if (hwnd == IntPtr.Zero)
                {
                    CurrentInfo = null;
                }
                else
                {
                    BuildCurrentWindowInfo(hwnd);
                }
            };
        }

        protected void OnCurrentInfoChanged()
        {
            nativePropertyGrid.SelectedObject = CurrentInfo;

            dumpButton.Enabled = CurrentInfo != null;
            if (CurrentInfo == null)
            {
                return; // nope
            }

            hwndBox.Text = CurrentInfo.ToShortString();

            // Inject our self if possible
            if (HawkeyeApplication.CanInject(CurrentInfo))
            {
                HawkeyeApplication.Inject(CurrentInfo);
                return;
            }

            // Injection was not needed.

            CurrentInfo.DetectDotNetProperties();
            IControlInfo controlInfo = CurrentInfo.ControlInfo;
            if (controlInfo != null)
            {
                if (!tabs.TabPages.Contains(dotNetTabPage))
                {
                    tabs.TabPages.Add(dotNetTabPage);
                }

                FillControlInfo(controlInfo);
                tabs.SelectedTab = dotNetTabPage;
                if (!tabs.TabPages.Contains(scriptTabPage))
                {
                    tabs.TabPages.Add(scriptTabPage);
                }

                scriptBox1.ControlInfo = controlInfo;
            }
            else
            {
                if (tabs.TabPages.Contains(dotNetTabPage))
                {
                    tabs.TabPages.Remove(dotNetTabPage);
                }

                tabs.SelectedTab = nativeTabPage;
                if (tabs.TabPages.Contains(scriptTabPage))
                {
                    tabs.TabPages.Remove(scriptTabPage);
                }

                scriptBox1.ControlInfo = null;
            }

            // Update the hwnd box in case we detected .NET properties.
            hwndBox.Text = CurrentInfo.ToShortString();

            CurrentInfoChanged?.Invoke(this, EventArgs.Empty);
        }

        private void FillControlInfo(IControlInfo controlInfo)
        {
            dotNetPropertyGrid.SelectedObject = controlInfo;
            RefreshDotNetPropertyGridActions();
        }

        private void BuildCurrentWindowInfo(IntPtr hwnd)
        {
            Cursor = Cursors.WaitCursor;
            Enabled = false;
            Refresh();
            try
            {
                WindowInfo info = null;
                try
                {
                    info = new WindowInfo(hwnd);
                }
                catch (Exception ex)
                {
                    Log.Error($"Error while building window info: {ex.Message}", ex);
                }

                CurrentInfo = info;
            }
            finally
            {
                Enabled = true;
                Cursor = Cursors.Default;
            }
        }

        /// <summary>
        ///     Dumps the currently selected window information.
        /// </summary>
        private void Dump()
        {
            if (CurrentInfo == null)
            {
                ErrorBox.Show(this, "Can't dump if no window selected.");
                return;
            }

            string filename = GetFileName();
            if (string.IsNullOrEmpty(filename))
            {
                return;
            }

            try
            {
                string text = CurrentInfo.Dump();
                using (StreamWriter writer = File.CreateText(filename))
                {
                    writer.Write(text);
                    writer.Flush();
                }
            }
            catch (Exception ex)
            {
                string message = $"Could not create dump file for handle {CurrentInfo.Handle}: {ex.Message}";
                Log.Error(message, ex);
                ErrorBox.Show(this, message);
            }
        }

        /// <summary>
        ///     Gets the name of the file to save log to.
        /// </summary>
        /// <returns>A file name.</returns>
        private string GetFileName()
        {
            using (var dialog = new SaveFileDialog
            {
                FileName = "dump.log",
                Filter = @"Log files|*.log|All files|*.*"
            })
            {
                return dialog.ShowDialog(this) == DialogResult.OK ? dialog.FileName : string.Empty;
            }
        }

        /// <summary>
        ///     Handles the Click event of the dumpButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void dumpButton_Click(object sender, EventArgs e)
        {
            Dump();
        }

        #region DotNetPropertyGridAction Handling

        private void HandleDotNetPropertyGridAction(DotNetPropertyGridAction action)
        {
            switch (action)
            {
                case DotNetPropertyGridAction.Previous:
                    if (_history.HasPrevious)
                    {
                        _history.MoveToPrevious();
                    }

                    OnCurrentInfoChanged();
                    break;
                case DotNetPropertyGridAction.Next:
                    if (_history.HasNext)
                    {
                        _history.MoveToNext();
                    }

                    OnCurrentInfoChanged();
                    break;
                case DotNetPropertyGridAction.Parent:
                    if (CanExecuteAction(DotNetPropertyGridAction.Parent))
                    {
                        Target = CurrentInfo.ControlInfo.Control.Parent.Handle;
                    }

                    break;
                case DotNetPropertyGridAction.Highlight:
                    if (CanExecuteAction(DotNetPropertyGridAction.Highlight))
                    {
                        WindowHelper.DrawAdorner(Target);
                    }

                    break;
            }
        }

        private bool CanExecuteAction(DotNetPropertyGridAction action)
        {
            switch (action)
            {
                case DotNetPropertyGridAction.Previous: return _history.HasPrevious;
                case DotNetPropertyGridAction.Next: return _history.HasNext;
                case DotNetPropertyGridAction.Parent:
                    return
                        CurrentInfo?.ControlInfo?.Control?.Parent != null;
                case DotNetPropertyGridAction.Highlight:
                    return Target != IntPtr.Zero;
                default:
                    return false;
            }
        }

        private void RefreshDotNetPropertyGridActions()
        {
            foreach (DotNetPropertyGridAction action in Enum.GetValues(typeof(DotNetPropertyGridAction))
                .Cast<DotNetPropertyGridAction>())
            {
                dotNetPropertyGrid.EnableAction(action, CanExecuteAction(action));
            }
        }

        #endregion
    }
}