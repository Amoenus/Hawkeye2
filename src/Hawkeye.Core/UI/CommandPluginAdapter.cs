using System;
using System.Windows.Forms;
using Hawkeye.Extensibility;

namespace Hawkeye.UI
{
    internal class CommandPluginAdapter
    {
        private readonly ICommandPlugin _plugin;
        private ToolStripButton _button;
        private bool _enabled;

        public CommandPluginAdapter(ICommandPlugin commandPlugin)
        {
            _plugin = commandPlugin ?? throw new ArgumentNullException(nameof(commandPlugin));

            CreateControls();
        }

        public bool Enabled
        {
            get => _enabled;
            private set
            {
                if (_enabled == value)
                {
                    return;
                }

                _enabled = value;
                EnableControls(_enabled);
            }
        }

        internal void InsertToolStripButton(ToolStrip strip, int index)
        {
            strip.Items.Insert(index, _button);
        }

        private void CreateControls()
        {
            _button = CreateToolStripButton();
            _button.Click += (s, _) => _plugin.Execute();

            _plugin.CanExecuteChanged += (s, _) =>
                Enabled = _plugin.CanExecute();
        }

        private void EnableControls(bool enable)
        {
            _button.Enabled = enable;
        }

        private ToolStripButton CreateToolStripButton()
        {
            string label = _plugin.Label;
            if (string.IsNullOrEmpty(label))
            {
                label = _plugin.Descriptor.Name;
            }

            var button = new ToolStripButton(label);

            if (_plugin.Image != null)
            {
                button.Image = _plugin.Image;
                button.DisplayStyle = ToolStripItemDisplayStyle.Image;
            }
            else
            {
                button.DisplayStyle = ToolStripItemDisplayStyle.Text;
            }

            return button;
        }
    }
}