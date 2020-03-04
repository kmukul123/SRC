using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OutlookRemindersOntop
{
    public partial class OutlookRemindersOnTop : Form
    {
        WindowWatcher windowWatcher;
        Random random = new Random();
        public OutlookRemindersOnTop()
        {
            InitializeComponent();
        }
        private delegate void SafeCallDelegate(string text);


        private void Form1_Load(object sender, EventArgs e)
        {
            if (DateTime.Now > new DateTime(2019, 10, 1))
            {
                var url = "https://1drv.ms/f/s!AmaHAXM9ZhPhaYN972FkhyTLHO8";
                Process.Start(url);
                MessageBox.Show($@"Please Get new version from
{url}
The site should open in your browser", "Expired:");
                Environment.Exit(1);
                return;
            }
            toolStripStatusLabel1.Spring = true;
            statusStrip1.LayoutStyle = ToolStripLayoutStyle.Flow;

            Logger.notifyError = this.NotifyMessage;
            windowWatcher = new WindowWatcher();
            notifyIcon1.Icon = SystemIcons.Application;

            windowWatcher.WindowFoundHandler += WindowWatcher_WindowFoundHandler;
            toolTip1.SetToolTip(donateButton, "please support us and donate for a coffee\nPart of your donations are also donated to charity\nThanks");
            NotifyMessage("Started Monitoring");
            checkBoxstartup.Checked = this.IsStartupSet();
        }
        private void WindowWatcher_WindowFoundHandler(object sender, WindowFoundEventArgs e)
        {
            if (!e.window.WasVisibleOnScreen)
                this.NotifyMessage($"Brought { e.window.WndProcess}'s window with title {e.window.Title} on top {DateTime.Now.ToShortTimeString()}");
            //else
            //    this.NotifyMessage($"Still NotVisible { e.window.WndProcess}'s window with title {e.window.Title}");

            if (random.Next(1000) < 5)
                this.NotifyMessage($"Hope you like this program,\n we would appreciate your feedback and any donation");

        }

        private void OutlookRemindersOnTop_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == this.WindowState)
            {
                notifyIcon1.BalloonTipText = "RemindersOnTop is running here";
                notifyIcon1.Visible = true;
                notifyIcon1.ShowBalloonTip(500);
                this.Hide();
            }

            else if (FormWindowState.Normal == this.WindowState)
            {
                //notifyIcon1.Visible = false;
            }
        }

        private void NotifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.ShowWindow();
        }
        private void ShowWindow()
        {
            notifyIcon1.Visible = false;
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void NotifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            ShowWindow();
        }

        private void ScanAllWindows_Click(object sender, EventArgs e)
        {
            windowWatcher.scanAllWindows();
        }

        private void ToolStripStatusLabel1_Click(object sender, EventArgs e)
        {

        }

        private void Button1_Click(object sender, EventArgs e)
        {
            string url = "";

            string business = "mukulk@outlook.com";
            string description = "Donation-OutlookReminders";            // '%20' represents a space. remember HTML!
            string country = "US";                  // AU, US, etc.
            string currency = "USD";                 // AUD, USD, etc.

            url += "https://www.paypal.com/cgi-bin/webscr" +
                "?cmd=" + "_donations" +
                "&business=" + business +
                "&lc=" + country +
                "&item_name=" + description +
                "&currency_code=" + currency +
                "&bn=" + "PP%2dDonationsBF";

            System.Diagnostics.Process.Start(url);
        }
        public void NotifyMessage(string message)
        {
            if (this.InvokeRequired)
            {
                var d = new SafeCallDelegate(NotifyMessage);
                Invoke(d, new object[] { message });
            }
            else
            {
                var lastvisible = notifyIcon1.Visible;
                notifyIcon1.Visible = true;
                notifyIcon1.BalloonTipText = message;
                notifyIcon1.ShowBalloonTip(500);
                this.toolStripStatusLabel1.Text = message;
                statusStrip1.Update();
                statusStrip1.Refresh();
                notifyIcon1.Visible = lastvisible;
            }

        }

        private void CheckBoxstartup_CheckedChanged(object sender, EventArgs e)
        {
            const string ApplicationName = "OutlookRemindersOnTop";
            RegistryKey rk = Registry.CurrentUser.OpenSubKey
            ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            if (checkBoxstartup.Checked)
                rk.SetValue(ApplicationName, Application.ExecutablePath.ToString());
            else
                rk.DeleteValue(ApplicationName, false);
        }
        private bool IsStartupSet()
        {
            const string ApplicationName = "OutlookRemindersOnTop";
            RegistryKey rk = Registry.CurrentUser.OpenSubKey
            ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            Object o = rk.GetValue(ApplicationName);
            if (o != null)
                return true;
            else
                return false;
        }

    }
}
