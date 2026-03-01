using System;
using System.IO;

namespace BlindMode
{
    public static class DebugLog
    {
        private static string _logPath;
        private static readonly object _lock = new();

        public static void Init()
        {
            try
            {
                string modsDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                _logPath = Path.Combine(modsDir, "BlindMode_debug.log");

                lock (_lock)
                {
                    File.WriteAllText(_logPath, $"[{DateTime.Now:HH:mm:ss.fff}] === BlindMode Debug Log Started ===\n");
                }
            }
            catch { }
        }

        public static void Log(string message)
        {
            if (string.IsNullOrEmpty(_logPath)) return;

            try
            {
                lock (_lock)
                {
                    File.AppendAllText(_logPath, $"[{DateTime.Now:HH:mm:ss.fff}] {message}\n");
                }
            }
            catch { }
        }
    }
}
