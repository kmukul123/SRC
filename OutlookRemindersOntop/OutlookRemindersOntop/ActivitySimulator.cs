using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace OutlookRemindersOntop
{
    public class ActivitySimulator
    {
        public ActivitySimulator(string monitorUntilText)
        {
            this.timer = new System.Timers.Timer();
#if DEBUG
            this.timerInterval = TimeSpan.FromMinutes(1);

#else
            this.timerInterval = TimeSpan.FromMinutes(5);

#endif
            timer.Interval = timerInterval.TotalMilliseconds;
            this.simulateActivityUntilHours = int.Parse(monitorUntilText);
            this.timer.Elapsed += idlechecktimer_Elapsed;
            this.timer.Start();
        }

        private void idlechecktimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                this.timer.Stop();
                var idletime = Win32Helper.GetIdleTimeInSecs();
                if (timerEnabled && DateTime.Now.Hour <= simulateActivityUntilHours)
                {
                    if (idletime >= timerInterval.TotalSeconds - 1)
                        SendKeys.SendWait("^{ESC}");
                }
            } catch (Exception ex)
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

        public bool timerEnabled { get; set; }
        public int simulateActivityUntilHours { get; set; } 
    }
}
