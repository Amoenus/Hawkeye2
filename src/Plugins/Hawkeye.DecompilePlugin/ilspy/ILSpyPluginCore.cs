using System.Drawing;

namespace Hawkeye.DecompilePlugin.IlSpy
{
    internal class ILSpyPluginCore : BaseDecompilerPluginCore
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ILSpyPluginCore"/> class.
        /// </summary>
        /// <param name="descriptor">The descriptor.</param>
        public ILSpyPluginCore(IlSpyPluginDescriptor descriptor) :
            base(descriptor) { }

        /// <summary>
        /// Gets the label displayed on the menu (or toolbar button) for this command.
        /// </summary>
        public override string Label => "Decompile with &ILSpy";

        /// <summary>
        /// Gets the image displayed on the menu (or toolbar button) for this command.
        /// </summary>
        public override Bitmap Image => Properties.Resources.ILSpy;

        protected override string DecompilerNotAvailable => @"ILSpy is not started. 
Hawkeye can not show you the source code for the selected item.

Please open ILSpy to use this feature.";

        protected override IDecompilerController CreateDecompilerController()
        {
            return new ILSpyController();
        }
    }
}
