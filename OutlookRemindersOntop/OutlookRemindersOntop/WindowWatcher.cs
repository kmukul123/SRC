using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace OutlookRemindersOntop
{
    // A delegate type for hooking up Idle  notifications.
    public delegate void WindowFoundEventHandler(object sender, WindowFoundEventArgs e);
    class WindowWatcher
    {
        private Timer timer;

        // A delegate type for hooking up Idle  notifications.
        public event WindowFoundEventHandler WindowFoundHandler;

        public WindowWatcher()
        {
            this.timer = new Timer();
#if DEBUG
            timer.Interval = TimeSpan.FromMinutes(2).TotalMilliseconds;
#else
            timer.Interval = TimeSpan.FromMinutes(5).TotalMilliseconds;
#endif
            this.timer.Elapsed += idlechecktimer_Elapsed;
            this.timer.Start();
        }
        void idlechecktimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            scanAllWindows();
        }

        public void scanAllWindows()
        {
            try
            {
                this.timer.Stop();
                WindowsWrapper windows = new WindowsWrapper();
                windows.changeWindowOnTopSetting("outlook", "Reminder", this.showNotification);
                windows.changeWindowOnTopSetting("notepad", "test1234567.txt", this.showNotification);

            }
            catch (Exception ex)
            {
                Logger.Error($"{ex}");
            }
            finally
            {
                this.timer.Start();
            }
        }

        void showNotification(WindowInfo win)
        {
            string process = win.ProcessName; string title = win.Title;
            if (this.WindowFoundHandler != null)
                WindowFoundHandler(this, new WindowFoundEventArgs()
                {
                    window = win,
                });
        }

    }
}
