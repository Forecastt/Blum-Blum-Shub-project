using System;
using System.IO;

namespace Blum_Blum_Shub_project.Data
{
    public static class Logger
    {
        // %APPDATA%\AlgBBS
        public static readonly string BaseDir =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AlgBBS");

        // %APPDATA%\AlgBBS\Logs
        public static readonly string LogsDir = Path.Combine(BaseDir, "Logs");

        // %APPDATA%\AlgBBS\Logs\error_log.txt
        private static readonly string ErrorLogPath = Path.Combine(LogsDir, "error_log.txt");

        public static void EnsureLogsFolder()
        {
            Directory.CreateDirectory(LogsDir);
        }

        public static void Error(string message, Exception? ex = null)
        {
            try
            {
                EnsureLogsFolder();

                var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
                if (ex != null)
                    line += $" | {ex.GetType().Name}: {ex.Message}";

                File.AppendAllText(ErrorLogPath, line + Environment.NewLine);
            }
            catch
            {
            }
        }
    }
}