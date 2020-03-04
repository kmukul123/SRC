using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace OutlookRemindersOntop
{
    public class WindowInfo
    {
        public WindowInfo(IntPtr Handle)
        {
            this.Handle = Handle;
            //this.WasVisibleOnScreen = this.IsVisibleOnScreen;
        }
        private CachedPropertiesClass cache = new CachedPropertiesClass();
        public IntPtr Handle = IntPtr.Zero;
        public string ProcessName
        {
            get
            {
                try
                {
                    if (WndProcess != null)
                        return WndProcess.Id == 0 ? string.Empty : WndProcess.ProcessName;
                    else
                        return string.Empty;
                }
                catch (Exception ex)
                {
                    var msg = (WndProcess != null) ? $" cant get FilePath for {WndProcess.Id} " : "cant get filepath";
                    Logger.Debug($"msg exception {ex}");
                    return string.Empty;
                }
            }
        }
        public bool IsVisibleOnScreen
        {
            get
            {
                var visible = new VisibilityTester().IsVisibleOnScreen(this.Handle);
                return visible;
            }
        }

        [DllImport("USER32.DLL")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        public Process WndProcess
        {
            get
            {
                var _process = cache.GetCachedValue<Process>();
                if (_process == null)
                {
                    uint pid = 0;
                    if (Handle != IntPtr.Zero)
                    {
                        GetWindowThreadProcessId(Handle, out pid);
                        if (pid != 0)
                            _process = Process.GetProcessById((int)pid);
                        else
                            _process = null;

                        if (_process != null)
                        {
                            cache.SetCachedValue(_process);
                        }
                    }
                }
                return _process;
            }
        }
        public bool isTopMost;
        public string Title
        {
            get
            {
                var _title = cache.GetCachedValue<string>();
                if (_title == null)
                {
                    if (Handle != IntPtr.Zero)
                    {
                        int length = GetWindowTextLength(Handle);
                        StringBuilder builder = new StringBuilder(length);
                        GetWindowText(Handle, builder, length + 1);
                        _title = builder.ToString();
                        cache.SetCachedValue(_title);
                        //Console.WriteLine($"found {Handle}\t topMost{isTopMost}\t title {_title} ");
                    }
                    else
                    {
                        _title = string.Empty;
                    }
                }
                return _title;
            }

        }

        public bool WasVisibleOnScreen { get; internal set; }

        [DllImport("USER32.DLL")]
        private static extern int GetWindowTextLength(IntPtr hWnd);
        [DllImport("USER32.DLL")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
        //WARN: Only for "Any CPU":
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr handle, out uint processId);


        public override string ToString()
        {
            return WndProcess.Id + "\t>\t" + ProcessName + "\t>\t" + Title;
        }
    }
}
