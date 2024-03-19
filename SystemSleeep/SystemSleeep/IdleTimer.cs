using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace SystemSleeep
{
    // A delegate type for hooking up Idle  notifications.
    public delegate void IdleTimeReachedEventHandler(object sender, EventArgs e);

    class IdleTimer
    {
        private int maxidleTimeInMins;
        private Timer idlechecktimer;

        public event IdleTimeReachedEventHandler IdleTimeReached;
        public event IdleTimeReachedEventHandler BeforeIdleTimeReached;
        public event IdleTimeReachedEventHandler IdleTimeChanged;
        public const int internaltimerInterval = 30;

        public IdleTimer(int idleTimeInMins)
        {
            this.maxidleTimeInMins = idleTimeInMins;
            #if DEBUG
            this.idlechecktimer = new Timer(5*1000);
#else
            this.idlechecktimer = new Timer(internaltimerInterval * 1000);
#endif
            this.idlechecktimer.Elapsed += idlechecktimer_Elapsed;
            this.idlechecktimer.Start();
        }

        public void Start()
        {
            this.idlechecktimer.Start();
        }
        
        public void Stop()
        {
            this.idlechecktimer.Stop();
        }

        public void SetInterval(int idletime)
        {
            this.maxidleTimeInMins = idletime;
        }

        public int GetInterval()
        {
            return this.maxidleTimeInMins;
        }

        public uint idletimeinsecs;
        void idlechecktimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                this.idlechecktimer.Stop();
                idletimeinsecs = Win32Helper.GetIdleTimeInSecs();
                if (this.IdleTimeChanged!= null)
                {
                    this.IdleTimeChanged(this, null);
                }

                if (idletimeinsecs + this.idlechecktimer.Interval/1000>= maxidleTimeInMins * 60)
                {
                    if (this.BeforeIdleTimeReached != null)
                    {
                        this.BeforeIdleTimeReached(this, null);
                    }
                }
                if (idletimeinsecs >= maxidleTimeInMins * 60)
                {
                    if (this.IdleTimeReached != null)
                    {
                        this.IdleTimeReached(this, null);
                    }
                }
            } finally
            {
                this.idlechecktimer.Start();
            }
        }

    }
}
