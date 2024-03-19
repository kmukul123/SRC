namespace SystemSleeep
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.checkBoxstartup = new System.Windows.Forms.CheckBox();
            this.SleepNow = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.sleepIfMonitorOff = new System.Windows.Forms.CheckBox();
            this.donateButton = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.simulateActivity = new System.Windows.Forms.CheckBox();
            this.cbSleepIfSound = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.BalloonTipText = "SystemSleep is running here";
            this.notifyIcon1.BalloonTipTitle = "SystemSleep";
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "SystemSleeep";
            this.notifyIcon1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon1_MouseClick);
            this.notifyIcon1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon1_MouseDoubleClick);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(46, 55);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(100, 20);
            this.textBox1.TabIndex = 0;
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            this.textBox1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox1_KeyPress);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(43, 27);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(202, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Minutes of idle time to put system to sleep";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // checkBoxstartup
            // 
            this.checkBoxstartup.AutoSize = true;
            this.checkBoxstartup.Location = new System.Drawing.Point(168, 274);
            this.checkBoxstartup.Name = "checkBoxstartup";
            this.checkBoxstartup.Size = new System.Drawing.Size(96, 17);
            this.checkBoxstartup.TabIndex = 2;
            this.checkBoxstartup.Text = "Run on startup";
            this.checkBoxstartup.UseVisualStyleBackColor = true;
            this.checkBoxstartup.CheckedChanged += new System.EventHandler(this.checkBoxstartup_CheckedChanged);
            // 
            // SleepNow
            // 
            this.SleepNow.Location = new System.Drawing.Point(312, 270);
            this.SleepNow.Margin = new System.Windows.Forms.Padding(2);
            this.SleepNow.Name = "SleepNow";
            this.SleepNow.Size = new System.Drawing.Size(64, 21);
            this.SleepNow.TabIndex = 3;
            this.SleepNow.Text = "SleepNow";
            this.SleepNow.UseVisualStyleBackColor = true;
            this.SleepNow.Click += new System.EventHandler(this.SleepNow_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.label2.Location = new System.Drawing.Point(0, 313);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(37, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Status";
            // 
            // sleepIfMonitorOff
            // 
            this.sleepIfMonitorOff.AutoSize = true;
            this.sleepIfMonitorOff.Location = new System.Drawing.Point(46, 93);
            this.sleepIfMonitorOff.Name = "sleepIfMonitorOff";
            this.sleepIfMonitorOff.Size = new System.Drawing.Size(193, 17);
            this.sleepIfMonitorOff.TabIndex = 5;
            this.sleepIfMonitorOff.Text = "sleep if Monitor is Off (Experimental)";
            this.sleepIfMonitorOff.UseVisualStyleBackColor = true;
            this.sleepIfMonitorOff.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // donateButton
            // 
            this.donateButton.Location = new System.Drawing.Point(256, 240);
            this.donateButton.Name = "donateButton";
            this.donateButton.Size = new System.Drawing.Size(120, 25);
            this.donateButton.TabIndex = 6;
            this.donateButton.Text = "Donate";
            this.donateButton.UseVisualStyleBackColor = true;
            this.donateButton.Click += new System.EventHandler(this.DonateButton_Click);
            // 
            // toolTip1
            // 
            this.toolTip1.ToolTipTitle = "Support us!";
            // 
            // simulateActivity
            // 
            this.simulateActivity.AutoSize = true;
            this.simulateActivity.Location = new System.Drawing.Point(46, 148);
            this.simulateActivity.Name = "simulateActivity";
            this.simulateActivity.Size = new System.Drawing.Size(103, 17);
            this.simulateActivity.TabIndex = 7;
            this.simulateActivity.Text = "Simulate Activity";
            this.simulateActivity.UseVisualStyleBackColor = true;
            this.simulateActivity.Visible = false;
            this.simulateActivity.CheckedChanged += new System.EventHandler(this.simulateActivity_CheckedChanged);
            // 
            // cbSleepIfSound
            // 
            this.cbSleepIfSound.AutoSize = true;
            this.cbSleepIfSound.Location = new System.Drawing.Point(46, 116);
            this.cbSleepIfSound.Name = "cbSleepIfSound";
            this.cbSleepIfSound.Size = new System.Drawing.Size(106, 17);
            this.cbSleepIfSound.TabIndex = 8;
            this.cbSleepIfSound.Text = "sleep if no sound";
            this.cbSleepIfSound.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(406, 326);
            this.Controls.Add(this.cbSleepIfSound);
            this.Controls.Add(this.simulateActivity);
            this.Controls.Add(this.donateButton);
            this.Controls.Add(this.sleepIfMonitorOff);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.SleepNow);
            this.Controls.Add(this.checkBoxstartup);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox1);
            this.Name = "Form1";
            this.Text = "SystemSleeep";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Resize += new System.EventHandler(this.frmMain_Resize);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkBoxstartup;
        private string DefaultIdleTimeConfigName = "DefaultIdleTime";
        private string sleepifMonitorOffConfigName = "SleepIfMonitorOff";
        private System.Windows.Forms.Button SleepNow;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox sleepIfMonitorOff;
        private System.Windows.Forms.Button donateButton;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.CheckBox simulateActivity;
        private System.Windows.Forms.CheckBox cbSleepIfSound;
    }
}

