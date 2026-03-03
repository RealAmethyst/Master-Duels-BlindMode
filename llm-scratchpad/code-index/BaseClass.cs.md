# Code Index: BaseClass.cs

```
// File: BaseClass.cs
namespace BlindMode
  class BaseClass : MonoBehaviour

    // --- Singleton ---
    static BaseClass Instance

    // --- Duel state ---
    static List<string> textRecord
    static List<CardRoot> cardsInDuel
    static PreviewElement currentElement  // renamed from currenElement

    // --- Menu name → enum mapping ---
    static Dictionary<string, Menus> menuNames

    enum Menus { NONE, DUEL, DECK, SOLO, SHOP, Missions, Notifications, Settings, DuelPass }
    static Menus currentMenu

    // --- Screen/dialog tracking ---
    internal static string lastDialogTitle
    internal static string lastScreenHeader
    internal static string lastFocusViewName

    class CardCustomInfo
      GameObject cardObject { get; set; }
      string Link { get; set; }
      string Stars { get; set; }
      string Atk { get; set; }
      string Def { get; set; }
      string PendulumScale { get; set; }
      string Attributes { get; set; }
      string SpellType { get; set; }
      string Element { get; set; }
      string Owned { get; set; }
      bool IsInHand { get; set; }

    class PreviewElement
      CardCustomInfo cardInfo { get; set; }
      string Name { get; set; }
      string Description { get; set; }
      string TimeLeft { get; set; }
      string Price { get; set; }
      void Clear()
        // Resets all properties to defaults including cardInfo (direct assignments)

    enum Attribute { Light=1, Dark=2, Water=3, Fire=4, Earth=5, Wind=6, Divine=7 }
      // Named BaseClass.Attribute to avoid conflict with System.Attribute

    enum Rarity { Normal=0, Rare=1, SuperRare=2, UltraRare=3 }

    // --- Speech / copy state ---
    static List<string> bannedText
    static string textToCopy
    static string old_copiedText

    // --- Duel flag ---
    static bool IsInDuel

    // --- Speech timing / queuing ---
    static DateTime lastExecutionTime
    static readonly TimeSpan cooldown  // 0.1 s
    internal static bool queueNextSpeech
    internal static string pendingButtonText

    // --- Mouse tracking ---
    static bool UsingMouse

    // --- UI scroll reference ---
    static SnapContentManager SnapContentManager

    // Download progress tracking
    internal static DownloadViewController activeDownloadVC
    internal static int lastDownloadPercent

    // Enquete (survey) page tracking
    internal static bool pendingEnqueteCheck
    internal static string lastEnquetePage

    // --- MonoBehaviour lifecycle ---
    void Awake()
      // Sets Instance, inits DebugLog, loads Tolk with SAPI fallback
    void OnApplicationQuit()
      // Unloads Tolk

    void Update()
      // Handles Space (read LP) and LeftAlt (CopyUI) in duel.
      // Calls CheckDialogTitle(), CheckScreenChange(), CheckDownloadProgress(), CheckEnqueteScreen().
      // Flushes pendingButtonText AFTER dialog/screen checks.

    // --- Speech helpers ---
    internal static void SpeakText(string text = "")
      // Speaks text (interrupt or queued). Strips tags via StripTags().
    internal static void SpeakScreenHeader(string text)
      // Speaks header as interrupt, sets queueNextSpeech=true.

    // --- UI utilities ---
    void CopyUI()
    internal static void DeselectButton()
    internal static CardRoot GetCardRootOfCurrentCard()
```
