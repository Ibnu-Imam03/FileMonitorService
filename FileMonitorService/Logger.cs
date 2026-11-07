using System;
using System.IO;
namespace FileMonitorService
{
    public static class Logger
    {
        private static string _LogFolder;

        public static void Initialize(string logFolder)
        {
            _LogFolder = logFolder;

            Directory.CreateDirectory(_LogFolder);
        }


        public static void Log(string message, string type = "INFO")
        {
            try
            {
                // Generate today's log file name.
                string fileName = DateTime.Now.ToString("yyyy-MM-dd") + ".txt";

                // Build the complete log file path.
                string filePath = Path.Combine(_LogFolder, fileName);

                // Generate the log entry.
                string logEntry =$"[{DateTime.Now:HH:mm:ss}] {type}: {message}";

                // Append the log entry to the file.
                File.AppendAllText(filePath, logEntry + Environment.NewLine);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Logging failed: {ex.Message}");
            }
        }
    }
}
