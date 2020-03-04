using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Forms;

namespace OutlookRemindersOntop
{
    static class Logger
    {
        public static Action<string> notifyError;
        public static void Info(string logline)
        {
            Trace.TraceInformation(logline);
            Console.WriteLine(logline);
        }

        public static void Debug(string logline)
        {
            //System.Diagnostics.Debug.WriteLine(logline);
            Info(logline);
        }

        public static void Error(string logline)
        {
            Trace.TraceError(logline);
            notifyError?.Invoke($"Error {logline}");

            Console.WriteLine(logline);
        }

        public static void Alert(string logline)
        {
            Trace.TraceInformation(logline);
            notifyError?.Invoke($"Alert {logline} {DateTime.Now.ToShortTimeString()}");

            Console.WriteLine(logline);
        }
    }
}
