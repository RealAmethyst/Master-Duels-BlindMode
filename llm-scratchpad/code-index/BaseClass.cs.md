# BaseClass.cs Code Index

## BaseClass (line 19)
Core MonoBehaviour singleton. Owns all shared state as public static fields. Coordinates the Update() loop, speech output, and duel input handling.

### Fields
- `Instance`: BaseClass (line 21) — singleton reference
- `textRecord`: List<string> (line 23) — history of spoken text
- `cardsInDuel`: List<CardRoot> (line 24) — tracks cards on the duel field
- `currentElement`: PreviewElement (line 26) — currently focused/previewed element
- `menuNames`: Dictionary<string, Menus> (line 28) — maps ViewController name substrings to Menus enum values
- `currentMenu`: Menus (line 41) — active menu context used by MenuProcessors
- `lastDialogTitle`: string (line 43) — dedup guard for dialog announcements
- `lastScreenHeader`: string (line 44) — dedup guard for screen header announcements
- `lastFocusViewName`: string (line 45) — dedup guard for focused ViewController name
- `bannedText`: List<string> (line 100) — strings that should never be spoken (e.g. "00:00", placeholder text)
- `textToCopy`: string (line 101) — text staged for the next SpeakText() call
- `old_copiedText`: string (line 102) — last spoken text, used for dedup
- `IsInDuel`: bool (line 104) — true when a duel is active
- `lastExecutionTime`: DateTime (line 106) — timestamp of last speech output, used for cooldown
- `cooldown`: TimeSpan (line 107) — 0.1s minimum interval between speech calls
- `queueNextSpeech`: bool (line 108) — when true, next SpeakText() queues (non-interrupting) after a header
- `pendingButtonText`: string (line 109) — deferred button text set by patches; processed in Update() after screen/dialog detection
- `UsingMouse`: bool (line 111) — tracks whether the user is using mouse input
- `SnapContentManager`: SnapContentManager (line 113) — field name conflicts with Il2CppYgomSystem.UI.SnapContentManager type; use `BaseClass.SnapContentManager` in other files
- `activeDownloadVC`: DownloadViewController (line 116) — reference to the active download screen for progress polling
- `lastDownloadPercent`: int (line 117) — last reported download percent, used to avoid repeat announcements
- `pendingEnqueteCheck`: bool (line 120) — set true when EnqueteViewController is detected but content not yet loaded (async); polled in Update()
- `lastEnquetePage`: string (line 121) — last announced survey page text, used for dedup

### Nested Enum: Menus (line 40)
Values: NONE, DUEL, DECK, SOLO, SHOP, Missions, Notifications, Settings, DuelPass

### Nested Enum: Attribute (line 81)
Yu-Gi-Oh card attribute. Conflicts with System.Attribute — must be referenced as `BaseClass.Attribute` in other files.
Values: Light=1, Dark=2, Water=3, Fire=4, Earth=5, Wind=6, Divine=7

### Nested Enum: Rarity (line 92)
Values: Normal=0, Rare=1, SuperRare=2, UltraRare=3

### Nested Class: CardCustomInfo (line 47)
Holds stat data for a card currently being previewed or focused in a duel.

- `cardObject`: GameObject (line 49)
- `Link`: string (line 50)
- `Stars`: string (line 51)
- `Atk`: string (line 52)
- `Def`: string (line 53)
- `PendulumScale`: string (line 54)
- `Attributes`: string (line 55)
- `SpellType`: string (line 56)
- `Element`: string (line 57)
- `Owned`: string (line 58)
- `IsInHand`: bool (line 59)

### Nested Class: PreviewElement (line 62)
Wraps CardCustomInfo with additional display fields for whatever is currently focused (card preview, shop item, etc.).

- `cardInfo`: CardCustomInfo (line 64)
- `Name`: string (line 65)
- `Description`: string (line 66)
- `TimeLeft`: string (line 67)
- `Price`: string (line 68)

#### Methods
- `Clear()` -> void (line 70) — resets all fields to defaults

### Methods
- `Awake()` -> void (line 123) — initializes singleton, DebugLog, and Tolk (screen reader library)
- `OnApplicationQuit()` -> void (line 136) — unloads Tolk on exit
- `Update()` -> void (line 141)
  // Main game loop. In duels: Space reads LP aloud, LeftAlt triggers CopyUI for card info. Always runs CheckDialogTitle, CheckScreenChange, CheckDownloadProgress, CheckEnqueteScreen. Then processes pendingButtonText so dialog headers always speak before button text.
- `SpeakText(string text = "")` -> void (line 175)
  // Core speech output. If text is empty, uses textToCopy. Enforces cooldown and dedup via old_copiedText. When queueNextSpeech is true, passes interrupt=false to Tolk (queues after header); otherwise interrupt=true. Records spoken text in textRecord.
- `SpeakScreenHeader(string text)` -> void (line 206)
  // Speaks a screen/dialog header immediately (interrupt=true), then sets queueNextSpeech=true so the next SpeakText() call queues behind it. Clears old_copiedText to allow re-speaking the next button even if it matches previous text.
- `CopyUI()` -> void (line 222) — calls GetUITextElements() then speaks FormatInfo(); used for duel card inspection (LeftAlt)
- `DeselectButton()` -> void (line 228) — clears old_copiedText to allow re-announcing the current button after deselection
- `GetCardRootOfCurrentCard()` -> CardRoot (line 233) — finds the CardRoot in cardsInDuel whose locator position matches currentElement's cardObject transform position
