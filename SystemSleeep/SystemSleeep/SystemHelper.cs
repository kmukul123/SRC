using System;
using System.Management;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SystemSleeep
{
    class SystemHelper
    {
        public static bool Suspend()
        {
            bool suspended = false;
            while (!(suspended = Application.SetSuspendState(PowerState.Suspend, true, true)))
            {
                Trace.WriteLine("Trying to suspend the system" + suspended);
                Thread.Sleep(5000);
            }
            return true;
        }

        private static bool LastMonitorState = false;

        public static DateTime LastMonitorStateChangeDate;

        public static bool IsMonitorOn(){
            bool monitorState = true;// on by default

            try
            {
                IntPtr handle = Win32Helper.GetForegroundWindow();

                bool IsNonZeroHandle = IntPtr.Zero != handle;
                int nchars = 255;
                StringBuilder Buff = new StringBuilder(nchars);

                if (IsNonZeroHandle  && Win32Helper.GetWindowText(handle, Buff, nchars) > 0)
                {
                    string title= Buff.ToString();
                    Trace.WriteLine($"Win32Helper.GetForegroundWindow():{handle} title:{title}");
                }

                monitorState = IsNonZeroHandle;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                Debug.WriteLine(ex);
            }

            if (LastMonitorState!= monitorState)
            {
                LastMonitorState = monitorState;
                LastMonitorStateChangeDate = DateTime.Now;
            }
            return monitorState;
        }

        public static bool IsMonitorOn_old()
        {
            var query = "select * from WmiMonitorBasicDisplayParams";
            using (var wmiSearcher = new ManagementObjectSearcher("\\root\\wmi", query))
            {
                var results = wmiSearcher.Get();
                foreach (ManagementObject wmiObj in results)
                {
                    // get the "Active" property and cast to a boolean, which should 
                    // tell us if the display is active. I've interpreted this to mean "on"
                    var active = (Boolean)wmiObj["Active"];
                    return active;
                }
            }
            // default is false so we can go to sleep
            return false;
        }
    }
}
