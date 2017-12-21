using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Hawkeye.Properties;
using Hawkeye.WinApi;

namespace Hawkeye.UI
{
    /// <summary>
    ///     The "Window finder" user control.
    /// </summary>
    [DefaultEvent("ActiveWindowChanged")]
    internal partial class WindowFinderControl : UserControl
    {
        private Point _lastLocationOnScreen = Point.Empty;
        private bool _searching;

        /// <summary>
        ///     Initializes a new instance of the <see cref="WindowFinderControl" />
        ///     class.
        /// </summary>
        public WindowFinderControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets the active window handle.
        /// </summary>
        /// <value>
        /// The active window handle.
        /// </value>
        public IntPtr ActiveWindowHandle { get; private set; } = IntPtr.Zero;

        /// <summary>
        /// Occurs when [active window changed].
        /// </summary>
        public event EventHandler ActiveWindowChanged;

        /// <summary>
        /// Occurs when [window selected].
        /// </summary>
        public event EventHandler WindowSelected;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (DesignMode)
            {
                return;
            }

            MouseDown += (s, _) =>
            {
                if (!_searching)
                {
                    StartSearch();
                }
            };
            MouseMove += (s, ev) =>
            {
                if (_searching)
                {
                    Search(ev.Location);
                }
            };
            MouseUp += (s, _) => StopSearch();
        }

        private void StartSearch()
        {
            _searching = true;
            Cursor.Current = CursorHelper.LoadFrom(Resources.TargetIcon);
            Capture = true;
        }

        private void Search(Point mouseLocation)
        {
            // Grab the window from the screen location of the mouse.
            Point locationOnScreen = PointToScreen(mouseLocation);
            IntPtr foundWindowHandle = WindowHelper.FindWindow(locationOnScreen);

            // We found a handle.
            if (foundWindowHandle != IntPtr.Zero)
            {
                // give it another try, it might be a child window (disabled, hidden .. something else)
                // offset the point to be a client point of the active window
                Point locationInWindow = WindowHelper.ScreenToClient(foundWindowHandle, locationOnScreen);
                if (locationInWindow != Point.Empty)
                {
                    // check if there is some hidden/disabled child window at this point
                    IntPtr childWindowHandle = WindowHelper.FindChildWindow(foundWindowHandle, locationInWindow);
                    if (childWindowHandle != IntPtr.Zero)
                    {
                        foundWindowHandle = childWindowHandle;
                    }
                }
            }

            // Is this the same window as the last detected one?
            if (_lastLocationOnScreen != locationOnScreen)
            {
                _lastLocationOnScreen = locationOnScreen;
                if (ActiveWindowHandle != foundWindowHandle)
                {
                    if (ActiveWindowHandle != IntPtr.Zero)
                    {
                        WindowHelper.RemoveAdorner(ActiveWindowHandle); // Remove highlight
                    }

                    ActiveWindowHandle = foundWindowHandle;
                    WindowHelper.DrawAdorner(ActiveWindowHandle); // highlight the window
                    OnActiveWindowChanged();
                }
            }
        }

        private void StopSearch()
        {
            _searching = false;
            Cursor = Cursors.Default;
            Capture = false;

            if (ActiveWindowHandle != IntPtr.Zero)
            {
                WindowHelper.RemoveAdorner(ActiveWindowHandle); // Remove highlight
            }

            OnWindowSelected();
        }

        private void OnWindowSelected()
        {
            WindowSelected?.Invoke(this, EventArgs.Empty);
        }

        private void OnActiveWindowChanged()
        {
            ActiveWindowChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}