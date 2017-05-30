using System;
using System.Drawing;
using System.Windows.Forms;

using Hawkeye.Logging;
using Hawkeye.Extensibility;
using System.Drawing.Imaging;
using System.IO;
using Hawkeye.Extensions;

namespace Hawkeye.CapturePlugin
{
    internal class CapturePluginCore : BaseCommandPlugin
    {
        private ILogService _log = null;
        private IWindowInfo _windowInfo = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="CapturePluginCore"/> class.
        /// </summary>
        /// <param name="descriptor">The descriptor.</param>
        public CapturePluginCore(IPluginDescriptor descriptor) :
            base(descriptor) { }

        /// <summary>
        /// Gets the label displayed on the menu (or toolbar button) for this command.
        /// </summary>
        public override string Label => "&Capture";

        /// <summary>
        /// Gets the image displayed on the menu (or toolbar button) for this command.
        /// </summary>
        public override Bitmap Image => Properties.Resources.Camera;

        /// <summary>
        /// Called when the plugin has just been initialized.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            _log = Host.GetLogger<CapturePluginCore>();
            SubscribeEvent();

            OnHostOnCurrentWindowInfoChanged(null, null);

            _log.Info($"'{Descriptor.Name}' was initialized.");
        }

        private void SubscribeEvent()
        {
            Host.CurrentWindowInfoChanged += OnHostOnCurrentWindowInfoChanged;
        }

        private void OnHostOnCurrentWindowInfoChanged(object s, EventArgs e)
        {
            _windowInfo = Host.CurrentWindowInfo;
            RaiseCanExecuteChanged(this);
        }

        /// <summary>
        /// Determines whether this plugin command can be executed.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if the command can be executed; otherwise, <c>false</c>.
        /// </returns>
        protected override bool CanExecuteCore()
        {
            return
                _windowInfo?.ControlInfo?.Control != null;
        }

        /// <summary>
        /// Executes this plugin command.
        /// </summary>
        protected override void ExecuteCore()
        {
            _log.Debug("Executing capture command");
            var control = _windowInfo.ControlInfo.Control;
            using (var image = new Bitmap(control.Width, control.Height, PixelFormat.Format32bppArgb))
            {
                using (var g = Graphics.FromImage(image))
                {
                    g.Clear(Color.Transparent);
                }

                control.DrawToBitmap(image, new Rectangle(0, 0, control.Width, control.Height));

                var dataObject = new DataObject();

                // First png to retain transparency (though Paint.Net does not seem to use paste this if a bitmap is present in the clipboard...
                var stream = new MemoryStream(); // Note: the stream is not closed, nor disposed; otherwise, the image can't be pasted
                image.Save(stream, ImageFormat.Png);
                dataObject.SetData("PNG", false, stream);

                // Then bmp format for applications that do not support png
                dataObject.SetData(DataFormats.Bitmap, false, image);

                // Send to clipboard
                Clipboard.SetDataObject(dataObject, true);
            }
        }
    }
}
