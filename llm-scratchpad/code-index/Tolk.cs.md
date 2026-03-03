# Tolk.cs

P/Invoke interop wrapper for `Tolk.dll` (screen reader abstraction library). All native imports are private; public methods are thin wrappers.

## Tolk (line 6)
Static class. All DllImports use `CharSet.Unicode` and `CallingConvention.Cdecl`.

### Private DllImport fields (native bindings)
- `Tolk_Load()` -> void (line 9)
- `Tolk_Unload()` -> void (line 12)
- `Tolk_TrySAPI(bool trySAPI)` -> void (line 15)
- `Tolk_PreferSAPI(bool preferSAPI)` -> void (line 18)
- `Tolk_DetectScreenReader()` -> IntPtr (line 21)
- `Tolk_HasSpeech()` -> bool (line 24)
- `Tolk_HasBraille()` -> bool (line 27)
- `Tolk_Output(string str, bool interrupt)` -> bool (line 30) — `str` marshalled as `LPWStr`
- `Tolk_Speak(string str, bool interrupt)` -> bool (line 33) — `str` marshalled as `LPWStr`
- `Tolk_Braille(string str)` -> bool (line 36) — `str` marshalled as `LPWStr`
- `Tolk_IsSpeaking()` -> bool (line 39)
- `Tolk_Silence()` -> bool (line 42)

### Methods (public wrappers)
- `Load()` -> void (line 44)
- `Unload()` -> void (line 45)
- `TrySAPI(bool trySAPI)` -> void (line 46)
- `PreferSAPI(bool preferSAPI)` -> void (line 47)
- `DetectScreenReader()` -> string (line 48) — converts IntPtr to string via `Marshal.PtrToStringUni`; returns null if zero
- `HasSpeech()` -> bool (line 53)
- `HasBraille()` -> bool (line 54)
- `Output(string str, bool interrupt = false)` -> bool (line 55) — primary speech call used throughout the mod
- `Speak(string str, bool interrupt = false)` -> bool (line 56)
- `Braille(string str)` -> bool (line 57)
- `IsSpeaking()` -> bool (line 58)
- `Silence()` -> bool (line 59)
