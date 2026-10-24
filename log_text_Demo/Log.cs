using System;
using System.IO;

namespace log_text
{
    internal class Log
    {
        public enum loglevel
        {
            Info,
            Warning,
            Error
        }

        public static void WriteLog_Info( string message)
        {
            string logLine =loglevel.Info +":" + " | " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " | " + message;
            string logName = DateTime.Now.ToString("yyyy-MM-dd") + ".log";
            string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, logName);
            File.AppendAllText(logPath, logLine + Environment.NewLine);
        }

        public static void WriteLog_Warning(string message, Exception ex)
        {
            string logLine = loglevel.Warning + ":" + ex.Message+" | " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " | " + message;
            string logName = DateTime.Now.ToString("yyyy-MM-dd") + ".log";
            string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, logName);
            File.AppendAllText(logPath, logLine + Environment.NewLine);
        }
        public static void WriteLog_Error(string message, Exception ex)
        {
            string logLine = loglevel.Error + ":" +ex.Message+ " | " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " | " + message;
            string logName = DateTime.Now.ToString("yyyy-MM-dd") + ".log";
            string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, logName);
            File.AppendAllText(logPath, logLine + Environment.NewLine);
       
        }

    }
}
