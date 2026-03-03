using System;
using System.IO;
using MelonLoader;

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
            catch (Exception ex) { MelonLogger.Warning($"[DebugLog.Init] {ex.Message}"); }
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
            catch (Exception ex) { MelonLogger.Warning($"[DebugLog.Log] {ex.Message}"); }
        }
    }
}
