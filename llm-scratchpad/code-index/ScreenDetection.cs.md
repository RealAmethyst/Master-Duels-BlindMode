# ScreenDetection.cs

Static class responsible for detecting screen/dialog changes and announcing them via speech.
Handles the game's ViewController focus system, dialog scanning, and async content polling.

Imports: `Il2CppTMPro`, `Il2CppYgomSystem.UI/YGomTMPro/ElementSystem`, `Il2CppYgomGame.Menu/Download/Enquete`
Uses: `using static BlindMode.BaseClass`, `using static BlindMode.UIHelpers`

---

## ScreenDetection (line 19)

`public static class`

### Fields

- `containerLabels`: `HashSet<string>` (line 21) — read-only set of EOM labels to skip when scanning serializedElements (`"Root"`, `"RootContent"`, `"RootBottom"`); avoids duplicate text from parent/child overlaps
- `vcNameToMenu`: `Dictionary<string, Menus>` (line 467) — maps VC name substrings to `Menus` enum values for context tracking (e.g. `"DeckEdit"` → `Menus.DECK`)

### Methods

- `CollectTexts(GameObject root, bool activeOnly, string logPrefix = null) -> List<(string path, string text)>` (line 28) `private static`
  // Walks all `TMP_Text` components under `root`, strips tags, returns `(parentName/componentName, cleanText)` tuples. Used as a general-purpose text harvester before structured extraction.

- `ExtractTitleAndBody(List<(string path, string text)> texts) -> (string title, string body)` (line 56) `private static`
  // Three-pass heuristic: (1) match path keywords `title`/`header` and `message`/`desc`/`info`/`body`; (2) if title found but no body, take first long text (≥30 chars) after the title; (3) fallback to first two non-button, non-placeholder texts.

- `GetFocusVC() -> ViewController` (line 114) `private static`
  // Finds `UI/ContentCanvas/ContentManager`, gets its `ViewControllerManager`, returns `GetFocusViewController()`. Returns null if not found.

- `GetViewFromVC(ViewController vc) -> ElementObjectManager` (line 128) `internal static`
  // Casts `vc` to `BaseMenuViewController` and returns `m_View` (EOM at offset 0x88). Logs if cast fails or `m_View` is null. Note: the view may NOT be a child of the VC's transform — `ViewCreater` can parent it elsewhere.

- `FindScreenTitle(ViewController vc) -> string` (line 155) `internal static`
  // Three-approach fallback chain: (1) iterate EOM `serializedElements` array, collect TMP_Text per element; (2) if empty, `CollectTexts` on EOM's `gameObject`; (3) if still empty, `CollectTexts` on VC's own `gameObject`. Then extracts title/message by label keyword matching, returns `"title. message"` or whichever is found.

- `AnnounceDialogVC(ViewController vc) -> void` (line 273) `internal static`
  // Called from `OnCreatedView` patches on known dialog VCs (`ActionSheetViewController`, `CommonDialogViewController`, `TitleDataLinkDialogViewController`). Collects active texts from the EOM, strips banned text, extracts title/body, speaks `"Dialog. title. body"` via `SpeakScreenHeader`. Sets `lastDialogTitle = eom.name` to prevent `CheckDialogTitle` from re-announcing the same dialog.

- `IsPlaceholderText(string text) -> bool` (line 308) `static` (package-private)
  // Returns true if text is empty or is a long string (>30 chars) with no spaces or newlines — characteristic of unreplaced localization key names like `"TitleTextTitleText..."`.

- `CheckDialogTitle() -> void` (line 321) `internal static`
  // Fallback dialog detection polled from `Update()`. Scans `UI/OverlayCanvas/DialogManager` children for active `(Clone)` GameObjects. Skips already-announced dialogs (by name match against `lastDialogTitle`). Skips placeholder text and retries next frame. Resets `lastDialogTitle` to `""` only when no active dialog exists (i.e. dialog was closed).

- `GetTextFromElement(GameObject element) -> string` (line 385) `private static`
  // Gets first `TMP_Text` (including inactive children) from a `GameObject`, strips tags, returns trimmed string or null.

- `ReadGameHeaderText() -> string` (line 402) `internal static`
  // Reads the localized screen name from the singleton `HeaderViewController`. Uses `TXT_LABEL` key to call `eom.GetElement()`, then falls back to scanning all active TMP_Text in the header EOM. Returns null if header is inactive or has no text.

- `UpdateMenuFromVC(string cleanName) -> void` (line 480) `private static`
  // Looks up `cleanName` in `vcNameToMenu`; sets `currentMenu` if found. If not found and `currentMenu` is not already `NONE`, resets it to `Menus.NONE` to prevent stale menu context from causing crashes in menu processors.

- `CheckScreenChange() -> void` (line 499) `internal static`
  // Main screen-change detector, polled from `Update()`. Gets focused VC, compares name against `lastFocusViewName`. On change: calls `UpdateMenuFromVC`, handles special cases (`GameEntryV1`/`GameEntrySequenceV2` are silently skipped; `Enquete` sets `pendingEnqueteCheck`; `Title` reads only the `CodeVer` EOM element). For all other VCs: combines `ReadGameHeaderText()` + `FindScreenTitle()` and speaks via `SpeakScreenHeader`, then calls `QueueFocusedItem`.

- `CheckEnqueteScreen() -> void` (line 595) `internal static`
  // Polled from `Update()` when `pendingEnqueteCheck` is true. Waits until `EnqueteViewController.m_PageText` is non-empty (async loading guard). On page change (`pageText != lastEnquetePage`): collects all active text, filters out page indicator, buttons, toggles/entities/checkboxes, then speaks description + page indicator and queues focused item. Cancels polling if focus leaves the Enquete VC.

- `CheckDownloadProgress() -> void` (line 656) `internal static`
  // Polled from `Update()` when `activeDownloadVC` is set (assigned by `OnCreatedView` patch). Reads `downloadController.TotalProgress` as a 0–100 integer. Speaks each whole-percent change. At 100%, reads `DownloadingStateText` or `DownloadingText` for a completion message, then clears `activeDownloadVC`.

- `QueueFocusedItem(GameObject vcRoot) -> void` (line 708) `internal static`
  // After announcing a screen header, finds the currently hovered `SelectionButton` (identified by `ColorContainerGraphic.currentStatusMode == Enter`) within `vcRoot`, reads its text via `FindExtendedTextElement`, appends position from `GetSelectionPosition`, and outputs directly via `Tolk.Output(..., false)` (queue, not interrupt). Sets `old_copiedText` to suppress duplicate on next frame.
