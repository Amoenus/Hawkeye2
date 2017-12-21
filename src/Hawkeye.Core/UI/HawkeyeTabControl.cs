using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Hawkeye.UI.Controls;

namespace Hawkeye.UI
{
    /// <summary>
    ///     Specialized tab control with a border drawn on three sides (bottom, left
    ///     and right)
    /// </summary>
    internal class HawkeyeTabControl : TabControlEx
    {
        /// <summary>
        ///     Paints the background of the control.
        /// </summary>
        /// <param name="pevent">
        ///     A <see cref="PaintEventArgs" /> that contains information about the
        ///     control to paint.
        /// </param>
        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            base.OnPaintBackground(pevent);
            var topRectangle = new Rectangle(
                SelectedTab.Left, 0, SelectedTab.Width, SelectedTab.Top);
            pevent.Graphics.FillRectangle(Brushes.White, topRectangle);
        }

        /// <summary>
        ///     Paints the tab page border.
        /// </summary>
        /// <param name="e">
        ///     The <see cref="PaintEventArgs" /> instance containing the event
        ///     data.
        /// </param>
        protected override void PaintTabPageBorder(PaintEventArgs e)
        {
            if (TabCount <= 0)
            {
                return;
            }

            int x1 = SelectedTab.Left - 1;
            var y1 = 0;
            int x2 = SelectedTab.Width + SelectedTab.Left;
            int y2 = SelectedTab.Height + SelectedTab.Top;

            SmoothingMode savedSmoothing = e.Graphics.SmoothingMode;
            e.Graphics.SmoothingMode = SmoothingMode.None;
            using (var p = new Pen(TabBorderColor, -1f))
            {
                e.Graphics.DrawLine(p, x1, y1, x1, y2);
                e.Graphics.DrawLine(p, x1, y2, x2, y2);
                e.Graphics.DrawLine(p, x2, y1, x2, y2);
            }

            e.Graphics.SmoothingMode = savedSmoothing;
        }
    }
}