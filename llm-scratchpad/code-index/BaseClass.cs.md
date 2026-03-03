# Code Index: BaseClass.cs

```
// File: BaseClass.cs
namespace BlindMode
  class BaseClass : MonoBehaviour (line 21)

    // --- Singleton ---
    static BaseClass Instance (line 23)

    // --- Duel state ---
    static List<string> textRecord (line 25)
    static List<CardRoot> cardsInDuel (line 26)
    static PreviewElement currenElement (line 28)  // note: typo "curren" in original

    // --- Menu name → enum mapping ---
    static Dictionary<string, Menus> menuNames (line 30)

    enum Menus { NONE, DUEL, DECK, SOLO, SHOP, Missions, Notifications, Settings, DuelPass } (line 42)
    static Menus currentMenu (line 43)

    // --- Screen/dialog tracking ---
    internal static string lastDialogTitle (line 45)
    internal static string lastScreenHeader (line 46)
    internal static string lastFocusViewName (line 47)

    class CardCustomInfo (line 49)
      GameObject cardObject { get; set; } (line 51)
      string Link { get; set; } (line 52)
      string Stars { get; set; } (line 53)
      string Atk { get; set; } (line 54)
      string Def { get; set; } (line 55)
      string PendulumScale { get; set; } (line 56)
      string Attributes { get; set; } (line 57)
      string SpellType { get; set; } (line 58)
      string Element { get; set; } (line 59)
      string Owned { get; set; } (line 60)
      bool IsInHand { get; set; } (line 61)

    class PreviewElement (line 64)
      CardCustomInfo cardInfo { get; set; } (line 66)
      string Name { get; set; } (line 67)
      string Description { get; set; } (line 68)
      string TimeLeft { get; set; } (line 69)
      string Price { get; set; } (line 70)
      void Clear() (line 72)
        // Resets all string properties to string.Empty via reflection; does NOT reset cardInfo
      void LogValues() (line 83)
        // Prints all property name/value pairs to Console via reflection
      void CopyValuesFrom(PreviewElement source) (line 91)
        // Shallow-copies all properties from source; deep-copies cardInfo via DeepCopy<T>()
      private static T DeepCopy<T>(T source) where T : class, new() (line 113)
        // Generic reflection-based shallow clone; called only for CardCustomInfo

    enum Attribute { Light=1, Dark=2, Water=3, Fire=4, Earth=5, Wind=6, Divine=7 } (line 130)
      // Named BaseClass.Attribute to avoid conflict with System.Attribute

    enum Rarity { Normal=0, Rare=1, SuperRare=2, UltraRare=3 } (line 141)

    private enum DuelPositions { Attack=0, Defense=1, FaceDownAttack=2, FaceDownDefense=3 } (line 149)

    // --- Speech / copy state ---
    static List<string> bannedText (line 157)
    static string textToCopy (line 158)
    static string old_copiedText (line 159)

    // --- Duel flag ---
    static bool IsInDuel (line 161)

    // --- Speech timing / queuing ---
    static DateTime lastExecutionTime (line 163)
    static readonly TimeSpan cooldown (line 164)  // 0.1 s
    internal static bool queueNextSpeech (line 165)
      // When true, next SpeakText call appends (queue) rather than interrupts; bypasses cooldown
    internal static string pendingButtonText (line 166)
      // Deferred button text; processed in Update() after dialog/screen detection

    // --- Mouse tracking ---
    static bool UsingMouse (line 168)

    // --- UI scroll reference ---
    static SnapContentManager SnapContentManager (line 170)
      // Field type same name as Il2CppYgomSystem.UI.SnapContentManager; use BaseClass.SnapContentManager in UIHelpers to disambiguate

    // Download progress tracking
    internal static DownloadViewController activeDownloadVC (line 173)
    internal static int lastDownloadPercent (line 174)

    // Enquete (survey) page tracking — polls until async content loads
    internal static bool pendingEnqueteCheck (line 177)
    internal static string lastEnquetePage (line 178)

    // --- MonoBehaviour lifecycle ---
    void Awake() (line 180)
      // Sets Instance, inits DebugLog, loads Tolk with SAPI fallback, detects screen reader
    void Start() (line 193)
    void OnApplicationQuit() (line 197)
      // Unloads Tolk

    void Update() (line 202)
      // Handles Space (read LP) and LeftAlt (CopyUI) in duel.
      // Calls CheckDialogTitle(), CheckScreenChange(), CheckDownloadProgress(), CheckEnqueteScreen().
      // Flushes pendingButtonText AFTER dialog/screen checks so dialog headers speak first.

    // --- Speech helpers ---
    internal static void SpeakText(string text = "") (line 236)
      // Speaks text immediately (interrupt) or queued depending on queueNextSpeech.
      // Strips HTML tags. Skips duplicates, empty strings, and bannedText entries.
      // Bypasses cooldown when queueNextSpeech is true.
    internal static void SpeakScreenHeader(string text) (line 267)
      // Speaks a screen/dialog header as an interrupting utterance, then sets
      // queueNextSpeech=true so the next SpeakText call queues behind it.
      // Resets old_copiedText to allow button re-announcement after navigation.

    // --- UI utilities ---
    void CopyUI() (line 283)
      // Calls GetUITextElements() then speaks FormatInfo() result
    internal static void DeselectButton() (line 289)
      // Clears old_copiedText so next focus event re-speaks the same text
    internal static CardRoot GetCardRootOfCurrentCard() (line 294)
      // Finds CardRoot in cardsInDuel whose position matches currenElement's card object transform
```
