using System;
using System.IO;
using System.Windows.Forms;

namespace FxDetector
{
    /// <summary>
    ///     Application's main form
    /// </summary>
    public partial class MainForm : Form
    {
        private WindowInfo _currentInfo;

        /// <inheritdoc />
        public MainForm()
        {
            InitializeComponent();
        }

        private WindowInfo CurrentInfo
        {
            get => _currentInfo;
            set
            {
                if (_currentInfo == value)
                {
                    return;
                }

                _currentInfo = value;
                OnCurrentInfoChanged();
            }
        }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (DesignMode)
            {
                return;
            }

            hwndBox.Text = Handle.ToString();
            OnCurrentInfoChanged();
        }

        private void OnCurrentInfoChanged()
        {
            pgrid.SelectedObject = CurrentInfo;
            pgrid.ExpandAllGridItems();

            dumpButton.Enabled = CurrentInfo != null;
        }

        private void Detect()
        {
            CurrentInfo = null;
            IntPtr hwnd = IntPtr.Zero;
            if (This.IsX64)
            {
                if (!long.TryParse(hwndBox.Text, out long windowHandle))
                {
                    MessageBox.Show(
                        this, $"{hwndBox.Text} is not a valid window handle.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    hwnd = new IntPtr(windowHandle);
                }
            }
            else
            {
                if (!int.TryParse(hwndBox.Text, out int windowHandle))
                {
                    MessageBox.Show(
                        this, $"{hwndBox.Text} is not a valid window handle.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    hwnd = new IntPtr(windowHandle);
                }
            }

            CurrentInfo = new WindowInfo(hwnd);
        }

        private void Dump()
        {
            if (CurrentInfo == null)
            {
                throw new InvalidOperationException(
                    "Can't dump if no window selected.");
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
                ErrorBox.Show(ex.ToString());
            }
        }

        private string GetFileName()
        {
            if (CurrentInfo == null)
            {
                throw new InvalidOperationException(
                    "Can't dump if no window selected.");
            }

            using (var dialog = new SaveFileDialog
            {
                //FileName = string.Format("dump_{0}.log", CurrentInfo.Hwnd),
                FileName = "dump.log",
                Filter = "Log files|*.log|All files|*.*"
            })
            {
                return dialog.ShowDialog(this) == DialogResult.OK ? dialog.FileName : string.Empty;
            }
        }

        /// <summary>
        ///     Handles the Click event of the detectButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="EventArgs" /> instance containing the event data.
        /// </param>
        private void detectButton_Click(object sender, EventArgs e)
        {
            Detect();
        }

        /// <summary>
        ///     Handles the Click event of the dumpButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="EventArgs" /> instance containing the event data.
        /// </param>
        private void dumpButton_Click(object sender, EventArgs e)
        {
            Dump();
        }
    }
}