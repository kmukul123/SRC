namespace OutlookRemindersOntop
{
    partial class OutlookRemindersOnTop
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
            this.components = new System.ComponentModel.Container();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.scanAllWindows = new System.Windows.Forms.Button();
            this.donateButton = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.checkBoxstartup = new System.Windows.Forms.CheckBox();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.Text = "RemindersOnTop";
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.NotifyIcon1_MouseClick);
            this.notifyIcon1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.NotifyIcon1_MouseDoubleClick);
            // 
            // scanAllWindows
            // 
            this.scanAllWindows.Location = new System.Drawing.Point(304, 42);
            this.scanAllWindows.Name = "scanAllWindows";
            this.scanAllWindows.Size = new System.Drawing.Size(120, 22);
            this.scanAllWindows.TabIndex = 1;
            this.scanAllWindows.Text = "ScanAllWindows";
            this.scanAllWindows.UseVisualStyleBackColor = true;
            this.scanAllWindows.Click += new System.EventHandler(this.ScanAllWindows_Click);
            // 
            // donateButton
            // 
            this.donateButton.Location = new System.Drawing.Point(304, 90);
            this.donateButton.Name = "donateButton";
            this.donateButton.Size = new System.Drawing.Size(120, 25);
            this.donateButton.TabIndex = 2;
            this.donateButton.Text = "Donate";
            this.donateButton.UseVisualStyleBackColor = true;
            this.donateButton.Click += new System.EventHandler(this.Button1_Click);
            // 
            // toolTip1
            // 
            this.toolTip1.ToolTipTitle = "Support us!";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 335);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(494, 22);
            this.statusStrip1.TabIndex = 3;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(118, 17);
            this.toolStripStatusLabel1.Text = "toolStripStatusLabel1";
            // 
            // checkBoxstartup
            // 
            this.checkBoxstartup.AutoSize = true;
            this.checkBoxstartup.Location = new System.Drawing.Point(23, 293);
            this.checkBoxstartup.Name = "checkBoxstartup";
            this.checkBoxstartup.Size = new System.Drawing.Size(96, 17);
            this.checkBoxstartup.TabIndex = 4;
            this.checkBoxstartup.Text = "Run on startup";
            this.checkBoxstartup.UseVisualStyleBackColor = true;
            this.checkBoxstartup.CheckedChanged += new System.EventHandler(this.CheckBoxstartup_CheckedChanged);
            // 
            // OutlookRemindersOnTop
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(494, 357);
            this.Controls.Add(this.checkBoxstartup);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.donateButton);
            this.Controls.Add(this.scanAllWindows);
            this.Name = "OutlookRemindersOnTop";
            this.Text = "BringOutlookRemindersOnTop";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Resize += new System.EventHandler(this.OutlookRemindersOnTop_Resize);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.Button scanAllWindows;
        private System.Windows.Forms.Button donateButton;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.CheckBox checkBoxstartup;
    }
}

