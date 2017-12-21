using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Hawkeye.UI.Controls
{
    /// <summary>
    ///     Owner-drawn tab control
    /// </summary>
    internal class CustomTabControl : TabControl
    {
        protected readonly Color DefaultBorderColor = Color.FromArgb(149, 169, 212);
        protected readonly Color DefaultTextColor = Color.FromArgb(77, 103, 162);
        private bool showTabSeparator = true;

        /// <summary>
        ///     <para>
        ///         Initializes a new instance of the <see cref="CustomTabControl" />
        ///     </para>
        ///     <para>class.</para>
        /// </summary>
        public CustomTabControl()
        {
            TabBorderColor = DefaultBorderColor;
            TextColor = DefaultTextColor;

            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
        }

        /// <summary>
        ///     Gets or sets the color of the tab border.
        /// </summary>
        /// <value>
        ///     The color of the tab border.
        /// </value>
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue(typeof(Color), "149, 169, 212")]
        public Color TabBorderColor { get; set; }

        /// <summary>
        ///     Gets or sets the color of the text.
        /// </summary>
        /// <value>
        ///     The color of the text.
        /// </value>
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue(typeof(Color), "77, 103, 162")]
        public Color TextColor { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to show tab separators.
        /// </summary>
        /// <value>
        ///     <c>true</c> if tab separators should be shown; otherwise,
        ///     <c>false</c> .
        /// </value>
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue("true")]
        public bool ShowTabSeparator
        {
            get => showTabSeparator;
            set
            {
                if (showTabSeparator != value)
                {
                    showTabSeparator = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        ///     Gets a value indicating whether this tab control draws its tabs
        ///     horizontally.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this tab control is horizontal; otherwise,
        ///     <c>false</c> .
        /// </value>
        private bool IsHorizontal => Alignment == TabAlignment.Top || Alignment == TabAlignment.Bottom;

        /// <summary>
        ///     Raises the <see cref="System.Windows.Forms.Control.Paint" /> event.
        /// </summary>
        /// <param name="e">
        ///     A <see cref="PaintEventArgs" /> that contains the event data.
        /// </param>
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            PaintTabPageBorder(e);
            PaintAllTabs(e);
            PaintSelectedTab(e);
        }

        /// <summary>
        ///     Paints the background of the control.
        /// </summary>
        /// <param name="pevent">
        ///     A <see cref="PaintEventArgs" /> that contains information about the
        ///     control to paint.
        /// </param>
        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            Point o = Parent.PointToClient(PointToScreen(new Point(0, 0)));
            pevent.Graphics.TranslateTransform(-o.X, -o.Y);
            InvokePaintBackground(Parent, pevent);
            InvokePaint(Parent, pevent);
            pevent.Graphics.TranslateTransform(o.X, o.Y);
        }

        /// <summary>
        ///     This member overrides <see cref="Control.OnResize" /> .
        /// </summary>
        /// <param name="e">
        ///     An <see cref="EventArgs" /> that contains the event data.
        /// </param>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (TabCount > 0 && SelectedTab != null)
            {
                SelectedTab.Invalidate();
            }
        }

        /// <summary>
        ///     Gets a graphics path representing how to draw the tab at the
        ///     specified index.
        /// </summary>
        /// <remarks>
        ///     In this implementation, tab are drawn as square.
        /// </remarks>
        /// <param name="index">The tab index.</param>
        /// <returns>
        ///     A <see cref="GraphicsPath" /> object.
        /// </returns>
        protected virtual GraphicsPath GetPath(int index)
        {
            Rectangle rect = GetTabRect(index);
            var gp = new GraphicsPath();

            switch (Alignment)
            {
                case TabAlignment.Top:
                    gp.AddLine(rect.Left + 1, rect.Bottom, rect.Left + 1, rect.Top);
                    gp.AddLine(rect.Right, rect.Top, rect.Right, rect.Bottom);
                    break;
                case TabAlignment.Bottom:
                    gp.AddLine(rect.Left + 1, rect.Top - 1, rect.Left + 1, rect.Bottom);
                    gp.AddLine(rect.Right, rect.Bottom, rect.Right, rect.Top - 1);
                    break;
                case TabAlignment.Left:
                    gp.AddLine(rect.Right + 1, rect.Top + 1, rect.Left + 1, rect.Top + 1);
                    gp.AddLine(rect.Left + 1, rect.Bottom, rect.Right, rect.Bottom);
                    break;
                case TabAlignment.Right:
                    gp.AddLine(rect.Left - 1, rect.Top + 1, rect.Right - 2, rect.Top + 1);
                    gp.AddLine(rect.Right - 2, rect.Bottom, rect.Left - 1, rect.Bottom);
                    break;
            }

            return gp;
        }

        /// <summary>
        ///     Paints the tab at the specified index.
        /// </summary>
        /// <param name="e">
        ///     The <see cref="PaintEventArgs" /> instance containing the event
        ///     data.
        /// </param>
        /// <param name="index">The tab index.</param>
        protected virtual void PaintTab(PaintEventArgs e, int index)
        {
            PaintTabText(e.Graphics, index);
        }

        /// <summary>
        ///     Paints the tab separator at the specified index.
        /// </summary>
        /// <param name="e">
        ///     The <see cref="PaintEventArgs" /> instance containing the event
        ///     data.
        /// </param>
        /// <param name="index">The index.</param>
        protected virtual void PaintTabSeparator(PaintEventArgs e, int index)
        {
            Rectangle bounds = GetTabRect(index);
            Rectangle bounds2 = GetTabRect(index - 1);

            if (Alignment == TabAlignment.Top || Alignment == TabAlignment.Bottom)
            {
                int gap = bounds.Left - bounds2.Right;
                float x = bounds.Left - gap / 2f;

                using (var pen = new Pen(TabBorderColor))
                {
                    e.Graphics.DrawLine(pen, x, bounds.Top + 3f, x, bounds.Bottom - 3f);
                }
            }
            else
            {
                int gap = bounds.Top - bounds2.Bottom;
                float y = bounds.Top - gap / 2f;

                using (var pen = new Pen(TabBorderColor))
                {
                    e.Graphics.DrawLine(pen, bounds.Left + 3f, y, bounds.Right - 3f, y);
                }
            }
        }

        /// <summary>
        ///     Paints the border of the tab at the specified object.
        /// </summary>
        /// <param name="g">The graphics object on which to paint.</param>
        /// <param name="index">The tab index.</param>
        /// <param name="path">
        ///     The graphics path representing the tab border.
        /// </param>
        protected virtual void PaintTabBorder(Graphics g, int index, GraphicsPath path)
        {
            using (var pen = new Pen(TabBorderColor))
            {
                g.DrawPath(pen, path);
            }
        }

        private Image GetTabImage(int index)
        {
            if (ImageList == null)
            {
                return null;
            }

            TabPage tab = TabPages[index];

            if (tab.ImageIndex < 0 && string.IsNullOrEmpty(tab.ImageKey))
            {
                return null;
            }

            if (tab.ImageIndex >= 0)
            {
                return ImageList.Images[tab.ImageIndex];
            }

            return ImageList.Images[tab.ImageKey];
        }

        /// <summary>
        ///     Paints the text of the tab at the specified index.
        /// </summary>
        /// <param name="g">The graphics object on which to paint.</param>
        /// <param name="index">The tab index.</param>
        protected virtual void PaintTabText(Graphics g, int index)
        {
            var gap = 0f;
            Image image = GetTabImage(index);
            Rectangle rect = GetTabRect(index);
            var bounds = new RectangleF(
                rect.X, rect.Y, rect.Width, rect.Height);

            if (image != null)
            {
                if (IsHorizontal)
                {
                    gap = (bounds.Height - image.Height) / 2f;
                }
                else
                {
                    gap = (bounds.Width - image.Width) / 2f;
                }
            }

            var sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;
            sf.LineAlignment = StringAlignment.Center;
            sf.Trimming = StringTrimming.EllipsisCharacter;
            sf.FormatFlags |= StringFormatFlags.LineLimit;
            if (!IsHorizontal)
            {
                sf.FormatFlags |= StringFormatFlags.DirectionVertical;
            }

            if (image == null) // simple: just center the text in the tab
            {
                using (var brush = new SolidBrush(TextColor))
                {
                    g.DrawString(
                        TabPages[index].Text, Font, brush, GetTabRect(index), sf);
                }

                return;
            }

            // Otherwise, we have an image, let's try to measure the text.
            var rectf = (RectangleF) GetTabRect(index);
            string text = TabPages[index].Text;
            SizeF maxAvailableArea = SizeF.Empty;
            if (IsHorizontal)
            {
                maxAvailableArea = new SizeF(rectf.Width - 3f * gap - image.Width, image.Height);
            }
            else
            {
                maxAvailableArea = new SizeF(image.Width, rectf.Height - 3f * gap - image.Height);
            }

            SizeF textSize = g.MeasureString(text, Font, maxAvailableArea, sf);

            if (textSize.Width > maxAvailableArea.Width)
            {
                textSize.Width = maxAvailableArea.Width;
            }

            if (textSize.Height > maxAvailableArea.Height)
            {
                textSize.Height = maxAvailableArea.Height;
            }

            SizeF imageAndTextSize = SizeF.Empty;
            if (IsHorizontal)
            {
                imageAndTextSize.Width = image.Width + gap + textSize.Width;
                imageAndTextSize.Height = (float) image.Height > textSize.Height ? image.Height : textSize.Height;
            }
            else
            {
                imageAndTextSize.Height = image.Height + gap + textSize.Height;
                imageAndTextSize.Width = (float) image.Width > textSize.Width ? image.Width : textSize.Width;
            }

            var imageAndTextBounds = new RectangleF(
                bounds.X + (bounds.Width - imageAndTextSize.Width) / 2f,
                bounds.Y + (bounds.Height - imageAndTextSize.Height) / 2f,
                imageAndTextSize.Width, imageAndTextSize.Height);

            // Draw the image
            var imageBounds = new Rectangle(
                (int) imageAndTextBounds.X, (int) imageAndTextBounds.Y,
                image.Width, image.Height);

            g.DrawImageUnscaledAndClipped(image, imageBounds);

            // Draw the text
            RectangleF textBounds = RectangleF.Empty;
            if (IsHorizontal)
            {
                textBounds = new RectangleF(
                    imageAndTextBounds.X + image.Width + gap,
                    imageAndTextBounds.Y,
                    imageAndTextBounds.Width - (image.Width + gap),
                    imageAndTextBounds.Height);
            }
            else
            {
                textBounds = new RectangleF(
                    imageAndTextBounds.X,
                    imageAndTextBounds.Y + image.Height + gap,
                    imageAndTextBounds.Width,
                    imageAndTextBounds.Height - (image.Height + gap));
            }

            using (var brush = new SolidBrush(TextColor))
            {
                g.DrawString(
                    TabPages[index].Text, Font, brush, textBounds, sf);
            }
        }

        /// <summary>
        ///     Paints the selected tab.
        /// </summary>
        /// <param name="e">
        ///     The <see cref="PaintEventArgs" /> instance containing the event
        ///     data.
        /// </param>
        protected virtual void PaintSelectedTab(PaintEventArgs e)
        {
            if (SelectedIndex < 0)
            {
                return;
            }

            Rectangle rect = GetTabRect(SelectedIndex);
            using (GraphicsPath path = GetPath(SelectedIndex))
            {
                switch (Alignment)
                {
                    case TabAlignment.Top:
                        path.AddLine(rect.Right, rect.Bottom + 2, rect.Left + 2, rect.Bottom + 2);
                        break;
                    case TabAlignment.Bottom:
                        path.AddLine(rect.Right, rect.Top - 3, rect.Left + 2, rect.Top - 3);
                        break;
                    case TabAlignment.Left:
                        path.AddLine(rect.Right + 2, rect.Bottom, rect.Right + 2, rect.Top + 2);
                        break;
                    case TabAlignment.Right:
                        path.AddLine(rect.Left - 3, rect.Bottom, rect.Left - 3, rect.Top + 2);
                        break;
                }

                e.Graphics.FillPath(Brushes.White, path);
                PaintTabBorder(e.Graphics, SelectedIndex, path);
                PaintTabText(e.Graphics, SelectedIndex);
            }
        }

        /// <summary>
        ///     Paints the tab page border.
        /// </summary>
        /// <param name="e">
        ///     The <see cref="PaintEventArgs" /> instance containing the event
        ///     data.
        /// </param>
        protected virtual void PaintTabPageBorder(PaintEventArgs e)
        {
            if (TabCount <= 0)
            {
                return;
            }

            var rect = new Rectangle(
                new Point(SelectedTab.Left, SelectedTab.Top), SelectedTab.Size);
            rect.Inflate(1, 1);
            ControlPaint.DrawBorder(e.Graphics, rect, TabBorderColor, ButtonBorderStyle.Solid);
        }

        /// <summary>
        ///     Paints all tabs.
        /// </summary>
        /// <param name="e">
        ///     The <see cref="PaintEventArgs" /> instance containing the event
        ///     data.
        /// </param>
        private void PaintAllTabs(PaintEventArgs e)
        {
            for (int i = TabCount - 1; i >= 0; i--)
            {
                PaintTab(e, i);
                if (ShouldPaintTabSeparator(i))
                {
                    PaintTabSeparator(e, i);
                }
            }
        }

        /// <summary>
        ///     <para>
        ///         Get a value indicating if we should paint a tab separator
        ///         <c>before</c>
        ///     </para>
        ///     <para>
        ///         the tab identified by the <paramref name="index" />
        ///         <paramref name="index" /> .
        ///     </para>
        /// </summary>
        /// <param name="index">The tab index.</param>
        /// <returns>
        /// </returns>
        private bool ShouldPaintTabSeparator(int index)
        {
            return ShowTabSeparator &&
                   TabCount > 2 &&
                   index != SelectedIndex &&
                   index - 1 != SelectedIndex &&
                   index != 0;
        }
    }
}