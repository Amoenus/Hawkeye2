using System.Windows.Forms;

namespace FxDetector
{
    /// <summary>
    /// </summary>
    internal static class ErrorBox
    {
        /// <summary>
        ///     Shows the <see cref="ErrorBox" /> .
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>
        ///     <see cref="DialogResult" />
        /// </returns>
        public static DialogResult Show(string text)
        {
            return Show(null, text);
        }

        /// <summary>
        ///     Shows the <see cref="ErrorBox" /> .
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="text">The text.</param>
        /// <returns>
        ///     <see cref="DialogResult" />
        /// </returns>
        private static DialogResult Show(IWin32Window owner, string text)
        {
            return MessageBox.Show(
                owner, text, "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1);
        }
    }
}