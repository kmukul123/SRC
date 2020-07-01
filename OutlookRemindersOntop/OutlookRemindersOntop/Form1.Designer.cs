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
            this.checkBoxMonitor = new System.Windows.Forms.CheckBox();
            this.textBoxHours = new System.Windows.Forms.TextBox();
            this.labelhours = new System.Windows.Forms.Label();
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
            this.scanAllWindows.Location = new System.Drawing.Point(405, 52);
            this.scanAllWindows.Margin = new System.Windows.Forms.Padding(4);
            this.scanAllWindows.Name = "scanAllWindows";
            this.scanAllWindows.Size = new System.Drawing.Size(160, 27);
            this.scanAllWindows.TabIndex = 1;
            this.scanAllWindows.Text = "Test";
            this.scanAllWindows.UseVisualStyleBackColor = true;
            this.scanAllWindows.Click += new System.EventHandler(this.ScanAllWindows_Click);
            // 
            // donateButton
            // 
            this.donateButton.Location = new System.Drawing.Point(405, 111);
            this.donateButton.Margin = new System.Windows.Forms.Padding(4);
            this.donateButton.Name = "donateButton";
            this.donateButton.Size = new System.Drawing.Size(160, 31);
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
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 413);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 19, 0);
            this.statusStrip1.Size = new System.Drawing.Size(659, 26);
            this.statusStrip1.TabIndex = 3;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(151, 20);
            this.toolStripStatusLabel1.Text = "toolStripStatusLabel1";
            // 
            // checkBoxstartup
            // 
            this.checkBoxstartup.AutoSize = true;
            this.checkBoxstartup.Location = new System.Drawing.Point(31, 361);
            this.checkBoxstartup.Margin = new System.Windows.Forms.Padding(4);
            this.checkBoxstartup.Name = "checkBoxstartup";
            this.checkBoxstartup.Size = new System.Drawing.Size(124, 21);
            this.checkBoxstartup.TabIndex = 4;
            this.checkBoxstartup.Text = "Run on startup";
            this.checkBoxstartup.UseVisualStyleBackColor = true;
            this.checkBoxstartup.CheckedChanged += new System.EventHandler(this.CheckBoxstartup_CheckedChanged);
            // 
            // checkBoxMonitor
            // 
            this.checkBoxMonitor.AutoSize = true;
            this.checkBoxMonitor.Location = new System.Drawing.Point(31, 167);
            this.checkBoxMonitor.Margin = new System.Windows.Forms.Padding(4);
            this.checkBoxMonitor.Name = "checkBoxMonitor";
            this.checkBoxMonitor.Size = new System.Drawing.Size(107, 21);
            this.checkBoxMonitor.TabIndex = 5;
            this.checkBoxMonitor.Text = "monitor until";
            this.checkBoxMonitor.UseVisualStyleBackColor = true;
            this.checkBoxMonitor.CheckedChanged += new System.EventHandler(this.checkBoxMonitor_CheckedChanged);
            // 
            // textBoxHours
            // 
            this.textBoxHours.Location = new System.Drawing.Point(149, 167);
            this.textBoxHours.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxHours.MaxLength = 2;
            this.textBoxHours.Name = "textBoxHours";
            this.textBoxHours.Size = new System.Drawing.Size(56, 22);
            this.textBoxHours.TabIndex = 6;
            this.textBoxHours.Text = "17";
            this.textBoxHours.TextChanged += new System.EventHandler(this.textBoxHours_TextChanged);
            // 
            // labelhours
            // 
            this.labelhours.AutoSize = true;
            this.labelhours.Location = new System.Drawing.Point(215, 172);
            this.labelhours.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelhours.Name = "labelhours";
            this.labelhours.Size = new System.Drawing.Size(44, 17);
            this.labelhours.TabIndex = 7;
            this.labelhours.Text = "hours";
            // 
            // OutlookRemindersOnTop
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(659, 439);
            this.Controls.Add(this.labelhours);
            this.Controls.Add(this.textBoxHours);
            this.Controls.Add(this.checkBoxMonitor);
            this.Controls.Add(this.checkBoxstartup);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.donateButton);
            this.Controls.Add(this.scanAllWindows);
            this.Margin = new System.Windows.Forms.Padding(4);
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
        private System.Windows.Forms.CheckBox checkBoxMonitor;
        private System.Windows.Forms.TextBox textBoxHours;
        private System.Windows.Forms.Label labelhours;
    }
}

