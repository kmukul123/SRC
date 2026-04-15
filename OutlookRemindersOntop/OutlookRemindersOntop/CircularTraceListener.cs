using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace OutlookRemindersOntop
{
    /// <summary>
    /// A custom TraceListener that maintains a maximum log size by rotating between two files.
    /// </summary>
    public class CircularTraceListener : TraceListener
    {
        private readonly string _filePath;
        private readonly long _maxSizeBytes;
        private readonly object _lock = new object();

        public CircularTraceListener(string filePath)
        {
            _filePath = Path.IsPathRooted(filePath) 
                ? filePath 
                : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filePath);
            
            // Hardcoded to 5KB as requested (5 * 1024)
            _maxSizeBytes = 5120;
        }

        public override void Write(string message)
        {
            LogMessage(message);
        }

        public override void WriteLine(string message)
        {
            LogMessage(message + Environment.NewLine);
        }

        private void LogMessage(string message)
        {
            lock (_lock)
            {
                try
                {
                    CheckRotation();
                    File.AppendAllText(_filePath, message);
                }
                catch (Exception ex)
                {
                    // Fail silently to avoid crashing the app due to logging issues
                    System.Console.WriteLine("Logging error: " + ex.Message);
                }
            }
        }

        private void CheckRotation()
        {
            if (!File.Exists(_filePath)) return;

            FileInfo fileInfo = new FileInfo(_filePath);
            if (fileInfo.Length >= _maxSizeBytes)
            {
                string oldFile = _filePath + ".old";
                
                try
                {
                    if (File.Exists(oldFile))
                    {
                        File.Delete(oldFile);
                    }
                    File.Move(_filePath, oldFile);
                }
                catch
                {
                    // If move fails (locked?), we'll just try again next time
                }
            }
        }
    }
}
