using System;
using System.Runtime.InteropServices;

namespace BlindMode
{
    public static class Tolk
    {
        [DllImport("Tolk", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Tolk_Load();

        [DllImport("Tolk", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Tolk_Unload();

        [DllImport("Tolk", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Tolk_TrySAPI(bool trySAPI);

        [DllImport("Tolk", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Tolk_PreferSAPI(bool preferSAPI);

        [DllImport("Tolk", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr Tolk_DetectScreenReader();

        [DllImport("Tolk", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool Tolk_HasSpeech();

        [DllImport("Tolk", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool Tolk_HasBraille();

        [DllImport("Tolk", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool Tolk_Output([MarshalAs(UnmanagedType.LPWStr)] string str, bool interrupt);

        [DllImport("Tolk", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool Tolk_Speak([MarshalAs(UnmanagedType.LPWStr)] string str, bool interrupt);

        [DllImport("Tolk", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool Tolk_Braille([MarshalAs(UnmanagedType.LPWStr)] string str);

        [DllImport("Tolk", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool Tolk_IsSpeaking();

        [DllImport("Tolk", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool Tolk_Silence();

        public static void Load() => Tolk_Load();
        public static void Unload() => Tolk_Unload();
        public static void TrySAPI(bool trySAPI) => Tolk_TrySAPI(trySAPI);
        public static void PreferSAPI(bool preferSAPI) => Tolk_PreferSAPI(preferSAPI);
        public static string DetectScreenReader()
        {
            IntPtr ptr = Tolk_DetectScreenReader();
            return ptr != IntPtr.Zero ? Marshal.PtrToStringUni(ptr) : null;
        }
        public static bool HasSpeech() => Tolk_HasSpeech();
        public static bool HasBraille() => Tolk_HasBraille();
        public static bool Output(string str, bool interrupt = false) => Tolk_Output(str, interrupt);
        public static bool Speak(string str, bool interrupt = false) => Tolk_Speak(str, interrupt);
        public static bool Braille(string str) => Tolk_Braille(str);
        public static bool IsSpeaking() => Tolk_IsSpeaking();
        public static bool Silence() => Tolk_Silence();
    }
}
