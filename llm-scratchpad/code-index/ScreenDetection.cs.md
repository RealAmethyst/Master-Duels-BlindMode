# ScreenDetection.cs — Code Index

**File:** `ScreenDetection.cs`
**Namespace:** `BlindMode`
**Imports (static):** `BlindMode.BaseClass`, `BlindMode.UIHelpers`

---

## `public static class ScreenDetection`

### Fields

- `private static readonly HashSet<string> containerLabels` — Labels to skip in EOM scanning ("Root", "RootContent", "RootBottom")
- `private static readonly Dictionary<string, Menus> vcNameToMenu` — Maps VC names to menu contexts

### Helper Methods

#### `private static List<(string, string)> CollectTexts(GameObject root, bool activeOnly, string logPrefix = null)`
Scans all TMP_Text components under root. Returns `(path, cleanText)` tuples. Strips tags. Optional active-only filter and debug logging.

---

#### `private static (string, string) ExtractTitleAndBody(List<(string, string)> texts)`
Keyword-based title/body extraction from path names. Matches title/header keywords, then message/desc/body. Falls back to first long text after title, then first non-button text.

---

#### `private static ViewController GetFocusVC()`
Gets focused ViewController from ContentManager. Used by CheckScreenChange and CheckEnqueteScreen.

---

#### `internal static ElementObjectManager GetViewFromVC(ViewController vc)`
Gets m_View (EOM) from a BaseMenuViewController via TryCast.

---

### Screen Detection

#### `internal static string FindScreenTitle(ViewController vc)`
Three-tier fallback: (1) EOM serializedElements, (2) EOM hierarchy via CollectTexts, (3) VC hierarchy via CollectTexts. Skips container labels and button elements.

---

#### `internal static void AnnounceDialogVC(ViewController vc)`
Announces dialog title/body using GetViewFromVC + CollectTexts + ExtractTitleAndBody. Sets lastDialogTitle to prevent re-announcement.

---

#### `static bool IsPlaceholderText(string text)`
Identifies unreplaced key names (long strings with no spaces).

---

#### `internal static void CheckDialogTitle()`
Fallback dialog detection via Update() scanning. Uses CollectTexts + ExtractTitleAndBody.

---

#### `private static string GetTextFromElement(GameObject element)`
Gets first TMP_Text from element children (includeInactive=true).

---

#### `internal static string ReadGameHeaderText()`
Reads localized screen name from HeaderViewController.instance.

---

#### `private static void UpdateMenuFromVC(string cleanName)`
Sets currentMenu based on vcNameToMenu lookup.

---

#### `internal static void CheckScreenChange()`
Detects focus VC changes. Uses GetFocusVC(). Special cases for GameEntry, Enquete, Title. Combines header + title for announcement.

---

#### `internal static void CheckEnqueteScreen()`
Polls for survey page changes. Uses CollectTexts with custom filtering.

---

#### `internal static void CheckDownloadProgress()`
Polls DownloadViewController for progress percentage changes.

---

#### `internal static void QueueFocusedItem(GameObject vcRoot)`
Finds focused SelectionButton and queues its text for speech.
