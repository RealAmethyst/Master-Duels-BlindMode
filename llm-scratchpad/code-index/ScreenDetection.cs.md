// File: ScreenDetection.cs
namespace BlindMode
  public static class ScreenDetection

    // Fields

    // Maps VC name strings to Menus enum values for menu context inference
    private static readonly Dictionary<string, Menus> vcNameToMenu (line 538)

    // Methods

    /// Get the ElementObjectManager (m_View) from a BaseMenuViewController.
    /// m_View is a protected field at offset 0x88 that holds the VC's UI element manager.
    /// The view may NOT be a child of the VC's transform (ViewCreater can parent it elsewhere).
    internal static ElementObjectManager GetViewFromVC(ViewController vc) (line 28)

    /// Find screen title text using the EOM's serializedElements array and TMP_Text scanning.
    /// serializedElements is an ElementObject[] which works in Il2Cpp (unlike SortedDictionary).
    /// Falls back to GetComponentsInChildren with includeInactive=true.
    // Three-tier fallback: (1) EOM serializedElements, (2) EOM hierarchy scan, (3) VC hierarchy scan.
    // Skips labels starting with "Button" and known container labels (Root, RootContent, RootBottom).
    // Combines title + message into a single string if both are found.
    internal static string FindScreenTitle(ViewController vc) (line 55)

    /// Announce a dialog ViewController that was just created.
    /// Called from OnCreatedView patches on dialog VCs.
    // Scans EOM's active TMP_Text children for title/body by path keyword.
    // Falls back to finding first long text (>=30 chars) after the title element.
    // Sets lastDialogTitle = eom.name to prevent CheckDialogTitle from re-announcing.
    internal static void AnnounceDialogVC(ViewController vc) (line 240)

    /// Check if text looks like a placeholder (e.g. "TitleTextTitleText..." - unreplaced key name).
    // Treats long strings with no spaces or newlines as placeholders.
    static bool IsPlaceholderText(string text) (line 312)

    /// Fallback dialog detection via Update() scanning.
    /// Handles dialogs not covered by OnCreatedView patches (e.g. TitleDataLinkDialog).
    // Walks DialogManager children each frame looking for active "(Clone)" objects.
    // Skips if dialogKey == lastDialogTitle (already announced).
    // Skips placeholder text and retries next frame without setting lastDialogTitle.
    // Only resets lastDialogTitle to "" when no active dialog exists at all.
    internal static void CheckDialogTitle() (line 325)

    /// Get text from a GameObject using TMP_Text (catches all TMP text types).
    /// Includes inactive children to handle text that may not be visibly active yet.
    private static string GetTextFromElement(GameObject element) (line 456)

    /// Read the localized screen name from the game's HeaderViewController.
    // Uses HeaderViewController.instance, reads TXT_LABEL key from EOM via GetElement().
    // Falls back to scanning all active TMP_Text in the header EOM.
    internal static string ReadGameHeaderText() (line 473)

    // Sets currentMenu based on vcNameToMenu lookup.
    // Resets currentMenu to NONE for unknown VCs to prevent stale menu context crashes.
    private static void UpdateMenuFromVC(string cleanName) (line 551)

    // Detects focus VC changes by comparing vcName to lastFocusViewName.
    // Calls UpdateMenuFromVC, then handles special cases:
    //   - GameEntryV1/GameEntrySequenceV2: silently skipped (setup flow entry points)
    //   - Enquete: sets pendingEnqueteCheck=true, defers to CheckEnqueteScreen()
    //   - Title: reads version from "CodeVer" EOM element only, skips QueueFocusedItem
    // For all other VCs: combines ReadGameHeaderText() + FindScreenTitle() then announces.
    internal static void CheckScreenChange() (line 570)

    /// Poll for Enquete (survey) page changes. Content loads asynchronously,
    /// so we check each frame until m_PageText is populated, then announce
    /// the question text and queue the focused item.
    /// Also re-announces when page changes (e.g. "1/3" -> "2/3").
    // Skips option texts inside toggle/entity/checkbox parents (spoken via button navigation).
    // Uses pageText as change key stored in lastEnquetePage.
    internal static void CheckEnqueteScreen() (line 672)

    /// Poll the active DownloadViewController for progress changes.
    /// Announces each whole percentage change.
    // Reads TotalProgress float from downloadController, converts to int percent.
    // On completion (>=100): reads game's own DownloadingStateText or DownloadingText,
    //   then clears activeDownloadVC and lastDownloadPercent.
    // Resets old_copiedText on each percent change to unblock same-text dedup check.
    internal static void CheckDownloadProgress() (line 750)

    /// After announcing a screen header, queue the currently focused
    /// SelectionButton text so the user knows what they're on.
    // Finds SelectionButton with ColorContainerGraphic in StatusMode.Enter (hover/focus state).
    // Speaks via Tolk.Output with queue=false (appended, not interrupt).
    internal static void QueueFocusedItem(GameObject vcRoot) (line 802)
