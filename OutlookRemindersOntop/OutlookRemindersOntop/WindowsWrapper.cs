using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Timers;
using System.Collections;
using System.Text;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;

namespace OutlookRemindersOntop
{

    public class WindowsWrapper
    {
        private ConcurrentDictionary<IntPtr, WindowInfo> windows = new ConcurrentDictionary<IntPtr, WindowInfo>();
        public WindowsWrapper()
        {
            IntPtr shellWindow = GetShellWindow();
            Console.WriteLine($"getting all Windows {shellWindow}");

            EnumWindows(new EnumWindowsProc(delegate (IntPtr hWnd, int lParam)
            {
                if (hWnd == shellWindow) return true;
                if (!IsWindowVisible(hWnd)) return true;
                int length = GetWindowTextLength(hWnd);
                if (length == 0) return true;
                var info = new WindowInfo(hWnd);
                //info.Title = builder.ToString();
                windows[hWnd] = info;
                //Logger.Debug(info.ToString());
                return true;
            }), 0);
        }

        /// <summary>Returns a dictionary that contains the handle and title of all the open windows.</summary>
        /// <returns>A dictionary that contains the handle and title of all the open windows.</returns>
        public IDictionary<IntPtr, WindowInfo> GetOpenedWindows()
        {
            return windows;
        }

        private delegate bool EnumWindowsProc(IntPtr hWnd, int lParam);
        [DllImport("USER32.DLL")]
        private static extern bool EnumWindows(EnumWindowsProc enumFunc, int lParam);

        [DllImport("USER32.DLL")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("USER32.DLL")]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("USER32.DLL")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("USER32.DLL")]
        private static extern IntPtr GetShellWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr GetTopWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        const int GWL_EXSTYLE = -20;
        const int WS_EX_TOPMOST = 0x0008;

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPos(
           IntPtr hWnd, int hWndInsertAfter, int x, int y, int cx, int cy, int uFlags);


        public static bool IsWindowTopMost(IntPtr hWnd)
        {
            int exStyle = GetWindowLong(hWnd, GWL_EXSTYLE);
            return (exStyle & WS_EX_TOPMOST) == WS_EX_TOPMOST;
        }


        static void showallProcessTitles()
        {
            var processlist = Process.GetProcesses();

            foreach (Process process in processlist)
            {
                if (!String.IsNullOrEmpty(process.MainWindowTitle))
                {
                    var handle = process.MainWindowHandle;
                    Logger.Debug($"Process: {process.ProcessName} Handle {handle} Window title:{ process.MainWindowTitle}");
                }
            }
        }
        string nameToIgnore = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
        IEnumerable<WindowInfo> getWindowPtrForTitle(string processFilter, string titleFilter)
        {

            foreach (var curr in this.GetOpenedWindows())
            {
                var currTitle = curr.Value.Title;
                if (!String.IsNullOrEmpty(currTitle))
                {
                    var currProcName = curr.Value.ProcessName;
                    var handle = curr.Key;
                    if (currProcName == nameToIgnore)
                        continue;
                    if (currTitle.Contains(titleFilter, StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (string.IsNullOrEmpty(currProcName) || currProcName.Contains(processFilter, StringComparison.CurrentCultureIgnoreCase))
                        {
                            Logger.Debug($"Matched title: {currTitle}\t Handle: {handle} Window process:{currProcName}");

                            yield return curr.Value;
                        }
                        else
                        {
                            Logger.Debug($"process title matched {currTitle} \t unmatched process:{ currProcName} ");
                        }
                    }

                }
            }
        }
        public void getForegroundWindow()
        {
            var foreGroundWindow = GetForegroundWindow();

            if (foreGroundWindow == IntPtr.Zero)
            {
                Thread.Sleep(100);
                //var topWindow = GetTopWindow(IntPtr.Zero);
                var topWindow = WindowsWrapper.GetForegroundWindow();
                var win = new WindowInfo(topWindow);

                Logger.Alert($"Can't find the foreground window retry {win.ProcessName}-{win.Title}");
            }

        }

        public void changeWindowOnTopSetting(string proc, string windowTitle, Action<WindowInfo> showNotification, bool setOnTop = true)
        {

#if DEBUG
            this.getForegroundWindow();
#endif
            foreach (var winptr in getWindowPtrForTitle(proc, windowTitle))
            {
                changeWindowOnTopSetting(winptr, showNotification);
            }

        }


        void changeWindowOnTopSetting(WindowInfo window, Action<WindowInfo> showNotification, bool setOnTop = true)
        {
            if (window.Handle == IntPtr.Zero)
            {
                Trace.WriteLine($"Window handle is zero");
                return;
            }

            window.WasVisibleOnScreen = window.IsVisibleOnScreen;
            if (!window.WasVisibleOnScreen)
                showNotification(window);
            else
                Logger.Alert("Alert Window is already visible");
            ShowWindow(window.Handle);
            return;
            const int HWND_TOPMOST = -1;
            const int HWND_NOTOPMOST = -2;
            const int SWP_NOMOVE = 0x0002;
            const int SWP_NOSIZE = 0x0001;
            var windowSetting = setOnTop ? HWND_TOPMOST : HWND_NOTOPMOST;
            SetWindowPos(
                window.Handle,
                windowSetting, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);

        }

        [DllImport("user32.dll")]
        private static extern int ShowWindow(IntPtr hWnd, uint Msg);
        [DllImport("User32.dll")]
        public static extern bool SetForegroundWindow(int hWnd);
        [System.Runtime.InteropServices.DllImport("User32.dll")]
        private static extern bool IsIconic(IntPtr handle);

        private void ShowWindow(IntPtr handle)
        {
            const uint SW_RESTORE = 0x09;
            if (IsIconic(handle))
                ShowWindow(handle, SW_RESTORE);
            SetForegroundWindow(handle.ToInt32());

        }


    }
}
