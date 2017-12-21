using System.Reflection;
using System.Windows.Forms;

namespace FxDetector
{
    // Inspiration found in Hawkeye's search box extender
    internal class PropertyGridEx : PropertyGrid
    {
        private bool _alreadyInitialized;
        private ToolStrip _thisToolStrip;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PropertyGridEx" />
        ///     class.
        /// </summary>
        /// <inheritdoc />
        public PropertyGridEx()
        {
            ToolStripRenderer = new ToolStripProfessionalRenderer
            {
                RoundedEdges = false
            };

            if (IsHandleCreated)
            {
                InitializeToolStrip();
            }
            else
            {
                HandleCreated += (s, e) => InitializeToolStrip();
                VisibleChanged += (s, e) => InitializeToolStrip();
            }
        }

        private ToolStrip ToolStrip
        {
            get
            {
                if (_thisToolStrip != null)
                {
                    return _thisToolStrip;
                }

                FieldInfo field = typeof(PropertyGrid).GetField("toolStrip",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                _thisToolStrip = (ToolStrip) field.GetValue(this);

                return _thisToolStrip;
            }
        }

        public ToolStripButton DetectButton { get; private set; }

        public ToolStripButton DumpButton { get; private set; }

        private void InitializeToolStrip()
        {
            if (_alreadyInitialized)
            {
                return;
            }

            if (ToolStrip == null)
            {
                return;
            }

            DetectButton = new ToolStripButton("Detect")
            {
                DisplayStyle = ToolStripItemDisplayStyle.Text
            };

            DumpButton = new ToolStripButton("Dump")
            {
                DisplayStyle = ToolStripItemDisplayStyle.Text
            };

            ToolStrip.Items.Add(new ToolStripSeparator());
            ToolStrip.Items.Add(DetectButton);
            ToolStrip.Items.Add(DumpButton);
            _alreadyInitialized = true;
        }
    }
}