// file name：Logger.cs
// file description: Logger class, have a method called Log which can used for write log entry into specific path.

using System;
using System.IO;
using System.Configuration;

namespace GuessWordServerService
{
    public static class Logger
    {
        // attribute
        private static string logFilePath; // our log file path
        

        // constructor
        static Logger()
        {
            // Read log file path from app.config
            logFilePath = ConfigurationManager.AppSettings["LogFilePath"];
        }

        // method

        // method name: Log
        // parameter:
        //         -- string message: the message what we want to record.
        //         -- Exception ex = null: the exception can be record.
        // return value: void
        // description: Write record in the log file.
        public static void Log(string message, Exception ex = null)
        {
            // record the date time and log message
            string logEntry = $"{DateTime.Now:G} - {message}";

            if (ex != null)
            {
                logEntry += $"\nException: {ex.Message}";
            }

            try
            {
                // append log entry in the file
                File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
            }
            catch
            {
                // If have exception when write log entry,
                // dont care the exception, just ensure service continues running without interrupt by exception.
            }
        }
    }
}
