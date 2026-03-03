# MenuProcessors.cs

Per-menu button text enhancement methods. All methods receive the focused `SelectionButton` instance from the `PatchOnSelected` Harmony patch and mutate `textToCopy` (or `currentElement`) to produce richer speech output. Methods are called from `Patches.cs` based on the value of `currentMenu`.

## MenuProcessors (line 19)
`public static class`. All methods are called by `PatchOnSelected` in Patches.cs after the base button text has been placed in `textToCopy`.

### Methods

- `ProcessProfile(SelectionButton __instance) -> void` (line 21)
  // Appends player level to `textToCopy` when the focused button is "ButtonPlayer". Navigates a fixed child transform path to find the level text.

- `ProcessFriendsMenu(SelectionButton __instance) -> void` (line 26)
  // Overrides `textToCopy` for "SearchButton" (renamed to "Add friend button") and "OpenToggle" (reads label from parent).

- `ProcessDailyReward(SelectionButton __instance) -> void` (line 39)
  // When `textToCopy` is exactly "Day", appends day number and received status by traversing fixed child indices.

- `ProcessPacks(SelectionButton __instance) -> void` (line 44)
  // Populates `currentElement` fields (Name, Description, TimeLeft, Price) for shop pack buttons and immediately calls `SpeakText(FormatInfo())`. Only acts when the button's parent name contains "Shop". Note: calls `SpeakText` directly rather than leaving speech to `Update()`.

- `ProcessDuelMenu(SelectionButton __instance) -> void` (line 58)
  // Handles buttons in the Solo/Duel menu. Delegates to `ProcessSettingsMenu` if the button is inside "SettingMenuArea". For buttons whose first child is named "Main", builds `textToCopy` from the last text element plus a "Complete" status element. Always falls through to `ProcessSoloChapter`.

- `ProcessSoloChapter(SelectionButton __instance) -> void` (line 83) `private`
  // Walks up to 4 levels of the transform hierarchy to find a parent whose name starts with "Chapter" (e.g. "ChapterDuel", "ChapterPractice"). Strips the "Chapter" prefix to get a type name, optionally appends a sibling "Level" text, and appends the result to `textToCopy`. No-ops silently if no Chapter* ancestor is found.

- `ProcessDuelGame(SelectionButton __instance) -> void` (line 127)
  // Handles in-duel card focus. Only acts when `IsInDuel` is true and the button name contains "HandCard" or "Anchor_". Sets `currentElement.cardInfo.cardObject`. For hand cards, sets `IsInHand = true` and returns early. For field cards (Anchor_), attempts to read face-down state via `GetCardRootOfCurrentCard()` and speaks "Opponent's face down card!" directly.

- `ProcessMissionsMenu(SelectionButton __instance) -> void` (line 160)
  // Only acts on buttons named "Locator". Traverses 9 levels up the hierarchy to find the root, then builds `textToCopy` with mission name, reward count, and time remaining. The reward string strips the first character and prepends "x" (format conversion).

- `ProcessDecksMenu(SelectionButton __instance) -> void` (line 178) `public`
  // Handles deck editor card/category buttons. For "ImageCard" buttons: calls `CopyUI()` when not using mouse, otherwise appends owned count and rarity. Switch on grandparent name handles "Category" (appends category label), "InputButton" (rename button), and "AutoBuildButton".

- `ProcessSettingsMenu(SelectionButton __instance) -> void` (line 200)
  // Appends current value to `textToCopy`. If a `Slider` child exists, formats as "X of Max". Otherwise reads the child `ExtendedTextMeshProUGUI` named "ModeText". Early-returns for Layout containers, EntryButtonsScrollView entries, and CancelButton. Note: also called from `ProcessDuelMenu` when inside "SettingMenuArea".

- `ProcessCardPack(SelectionButton __instance) -> void` (line 219)
  // Only acts on buttons named "CardPict". Builds `textToCopy` with rarity, new-icon status, and owned count (strips leading char and prepends "x"). Sibling transforms are accessed by name via `Find`.

- `ReadNotificationText(SelectionButton __instance) -> void` (line 229) `private`
  // Shared helper called by both `ProcessNotifications` and `ProcessNotificationsPopup`. Reads "TextBody" child text into `textToCopy`, then conditionally appends the active status category label.

- `ProcessNotifications(SelectionButton __instance) -> void` (line 236)
  // Delegates to `ReadNotificationText` if the button has a "BaseCategory" child.

- `ProcessNotificationsPopup(SelectionButton __instance) -> void` (line 242)
  // Like `ProcessNotifications` but adds a guard: only acts when the 6th-level ancestor is "NotificationWidget" and `currentMenu == Menus.NONE` (popup visible outside a dedicated menu screen).

- `ProcessEventBanner(SelectionButton __instance) -> void` (line 248)
  // Sets `textToCopy` to "Event banner" for buttons named "DuelShortcut".

- `ProcessTopicsBanner(SelectionButton __instance) -> void` (line 253)
  // Sets `textToCopy` to "Topic banner, page N" using `ScrollRectPageSnap.hpage` from the button's parent for buttons named "ButtonBanner".

- `ProcessDuelPass(SelectionButton __instance) -> void` (line 258)
  // For buttons whose name contains "passRewardButton", builds a string indicating Normal/Gold pass type, grade (from grandparent's text), and quantity (strips leading char and prepends "x").

- `ProcessNewDeck(SelectionButton __instance) -> void` (line 263)
  // Overrides `textToCopy` to "New deck button" if the button has an active child named "IconAddDeck".
