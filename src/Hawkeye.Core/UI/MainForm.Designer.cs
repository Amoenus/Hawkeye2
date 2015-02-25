namespace Hawkeye.UI
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.sstrip = new System.Windows.Forms.StatusStrip();
			this.tstripContainer = new System.Windows.Forms.ToolStripContainer();
			this.mainControl = new Hawkeye.UI.MainControl();
			this.tstripContainer.ContentPanel.SuspendLayout();
			this.tstripContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// sstrip
			// 
			this.sstrip.Location = new System.Drawing.Point(0, 415);
			this.sstrip.Name = "sstrip";
			this.sstrip.Size = new System.Drawing.Size(368, 22);
			this.sstrip.TabIndex = 2;
			this.sstrip.Text = "statusStrip1";
			// 
			// tstripContainer
			// 
			// 
			// tstripContainer.ContentPanel
			// 
			this.tstripContainer.ContentPanel.Controls.Add(this.mainControl);
			this.tstripContainer.ContentPanel.Size = new System.Drawing.Size(368, 390);
			this.tstripContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tstripContainer.Location = new System.Drawing.Point(0, 0);
			this.tstripContainer.Name = "tstripContainer";
			this.tstripContainer.Size = new System.Drawing.Size(368, 415);
			this.tstripContainer.TabIndex = 3;
			this.tstripContainer.Text = "toolStripContainer1";
			// 
			// mainControl
			// 
			this.mainControl.CurrentInfo = null;
			this.mainControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainControl.Location = new System.Drawing.Point(0, 0);
			this.mainControl.Name = "mainControl";
			this.mainControl.Padding = new System.Windows.Forms.Padding(4);
			this.mainControl.Size = new System.Drawing.Size(368, 390);
			this.mainControl.TabIndex = 0;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(368, 437);
			this.Controls.Add(this.tstripContainer);
			this.Controls.Add(this.sstrip);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "MainForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Hawkeye";
			this.tstripContainer.ContentPanel.ResumeLayout(false);
			this.tstripContainer.ResumeLayout(false);
			this.tstripContainer.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

		private System.Windows.Forms.StatusStrip sstrip;
        private System.Windows.Forms.ToolStripContainer tstripContainer;
		private MainControl mainControl;
    }
}