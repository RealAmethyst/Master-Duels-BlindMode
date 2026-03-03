// File: Tolk.cs
// P/Invoke wrapper around the native Tolk.dll screen reader abstraction library.
// Private extern methods map 1-to-1 with Tolk C API functions (Unicode, Cdecl).
// Public methods are thin wrappers that expose the same API to managed code.

namespace BlindMode
  static class Tolk

    // --- Private DllImport externs (Tolk C API) ---
    [DllImport] private static extern void Tolk_Load() (line 9)
    [DllImport] private static extern void Tolk_Unload() (line 12)
    [DllImport] private static extern void Tolk_TrySAPI(bool trySAPI) (line 15)
    [DllImport] private static extern void Tolk_PreferSAPI(bool preferSAPI) (line 18)
    [DllImport] private static extern IntPtr Tolk_DetectScreenReader() (line 21)
    [DllImport] private static extern bool Tolk_HasSpeech() (line 24)
    [DllImport] private static extern bool Tolk_HasBraille() (line 27)
    // interrupt=true stops current speech before outputting; false queues after it.
    // Output routes through screen reader + braille; Speak is speech-only.
    [DllImport] private static extern bool Tolk_Output(string str, bool interrupt) (line 30)
    [DllImport] private static extern bool Tolk_Speak(string str, bool interrupt) (line 33)
    [DllImport] private static extern bool Tolk_Braille(string str) (line 36)
    [DllImport] private static extern bool Tolk_IsSpeaking() (line 39)
    [DllImport] private static extern bool Tolk_Silence() (line 42)

    // --- Public managed wrappers ---
    static void Load() (line 44)
    static void Unload() (line 45)
    static void TrySAPI(bool trySAPI) (line 46)
    static void PreferSAPI(bool preferSAPI) (line 47)
    // Marshals IntPtr result from Tolk_DetectScreenReader to a managed string.
    // Returns null if no screen reader is detected (ptr is IntPtr.Zero).
    static string DetectScreenReader() (line 48)
    static bool HasSpeech() (line 53)
    static bool HasBraille() (line 54)
    // interrupt defaults to false (queue). Pass true to cut off current speech.
    static bool Output(string str, bool interrupt = false) (line 55)
    static bool Speak(string str, bool interrupt = false) (line 56)
    static bool Braille(string str) (line 57)
    static bool IsSpeaking() (line 58)
    static bool Silence() (line 59)
