# DebugLog.cs

Thread-safe file logger. Writes to `BlindMode_debug.log` in the same directory as the mod assembly. All file operations are guarded by a lock and silent try/catch.

## DebugLog (line 6)
Static class.

### Fields
- `_logPath`: string (line 8) — private; set during `Init()`
- `_lock`: object (line 9) — private readonly; used to synchronize all file writes

### Methods
- `Init()` -> void (line 11)
  - Resolves log path relative to the executing assembly's directory
  - Overwrites the log file with a timestamped header line
- `Log(string message)` -> void (line 26)
  - No-ops if `_logPath` is null/empty
  - Appends a timestamped line (`[HH:mm:ss.fff] message`) to the log file
