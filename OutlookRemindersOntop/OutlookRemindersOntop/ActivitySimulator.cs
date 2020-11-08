using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace OutlookRemindersOntop
{
    public class ActivitySimulator
    {
        public ActivitySimulator(string monitorUntilText, int defaultidletime= 4)
        {
#if DEBUG
            timerInterval = TimeSpan.FromMinutes(1);

#else           
            timerInterval = TimeSpan.FromMinutes(4);

#endif
            this.handle = handle;
            createNewTimer();
            try
            {
                
                this.simulateActivityUntilHours = int.Parse(monitorUntilText);
            } catch (Exception ex)
            {
                Logger.notifyError(ex.ToString());
            }
        }
        private void createNewTimer()
        {
            this.timer = new System.Timers.Timer();
            timer.Interval = timerInterval.TotalMilliseconds;
            Trace.TraceInformation($"created timer {timer.Interval}");
            this.timer.Elapsed += idlechecktimer_Elapsed;
            this.timer.Start();
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        internal void idlechecktimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!timerEnabled)
            {
                Trace.TraceInformation("timer is disabled");
                return;
            }
            try
            {
                this.timer.Stop();
                var idletime = Win32Helper.GetIdleTimeInSecs();
                if (DateTime.Now.Hour <= simulateActivityUntilHours)
                {
                    var shouldSend = idletime >= timerInterval.TotalSeconds - 10
                        || e == null;
                    Trace.TraceInformation($"send until {simulateActivityUntilHours} " +
                        $"sending {shouldSend} ");

                    if (shouldSend)
                    {
                        //SetForegroundWindow(this.handle);
                        SendKeys.SendWait("^{ESC}");
                        //SendKeys.SendWait("^");
                    }
                    
                } else
                {
                    Trace.TraceInformation($"skipping until {simulateActivityUntilHours}");
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("exception in activity timer " + ex);
            }
            finally
            {
                this.timer.Start();
            }
        }

        private System.Timers.Timer timer { get; set; }

        private readonly TimeSpan timerInterval;
        private readonly IntPtr handle;

        public bool timerEnabled { get => this.timer.Enabled;
            set {
                if (value == false)
                {
                    this.timer.Stop();
                    Trace.TraceInformation("disabled timer");
                }
                else
                {
                    createNewTimer();
                }
            }
        }
        public int simulateActivityUntilHours { get; set; } 
    }
}
