# MenuProcessors.cs — Code Index

**File:** `MenuProcessors.cs`
**Namespace:** `BlindMode`
**Imports (static):** `BlindMode.BaseClass`, `BlindMode.UIHelpers`

---

## `public static class MenuProcessors` (line 19)

All per-menu button handler methods. Called from `Patches.cs` (PatchOnSelected) based on current context.
Each method receives the `SelectionButton` instance that was just selected and reads/writes `textToCopy` or `currenElement` on `BaseClass`.

---

### Methods (declaration order)

#### `internal static void ProcessProfile(SelectionButton __instance)` — line 21
If the button is named `"ButtonPlayer"`, appends player level to `textToCopy` by reading a deeply nested child text element (hardcoded child indices).

---

#### `internal static void ProcessFriendsMenu(SelectionButton __instance)` — line 26
Handles two named buttons in the friends screen:
- `"SearchButton"` → sets `textToCopy` to `"Add friend button"`.
- `"OpenToggle"` → sets `textToCopy` to the text of the button's parent object.

---

#### `internal static void ProcessDailyReward(SelectionButton __instance)` — line 39
If `textToCopy` is exactly `"Day"`, appends the day number (from a specific child index path) and whether the reward was already received (`"RecievedCover"` child active state).

---

#### `internal static void ProcessPacks(SelectionButton __instance)` — line 44
Reads shop pack button data into `currenElement` (Name with pickup message and new-card count, Description from the group header label, TimeLeft, Price), then calls `SpeakText(FormatInfo())` immediately.
Only runs when button's grandparent contains `"Shop"` in its name.

---

#### `internal static void ProcessDuelMenu(SelectionButton __instance)` — line 60
Handles buttons in the Solo/Duel menu.
- If the button is inside `"SettingMenuArea"` (6 levels up), delegates to `ProcessSettingsMenu`.
- If the button's first child is named `"Main"`, reads solo chapter summary text (last element + completion status) into `textToCopy`.
- Always attempts `ProcessSoloChapter` in a separate try/catch to append chapter type and level info.

---

#### `private static void ProcessSoloChapter(SelectionButton __instance)` — line 91
Walks up to 4 levels up from the button looking for a parent whose name starts with `"Chapter"`. If found, strips the `"Chapter"` prefix to get a readable type name (e.g., `"Duel"`, `"Practice"`), then looks for a sibling `"Level"` object to read its TMP text. Appends `", {TypeName}, Level {N}"` to `textToCopy`.

```
/// <summary>
/// Process solo chapter map buttons. Each chapter node has a parent named
/// "ChapterDuel", "ChapterScenario", "ChapterGoal", "ChapterPractice", etc.
/// The button contains TextName (chapter name) and optionally Level/TextLevel.
/// </summary>
```

---

#### `internal static void ProcessDuelGame(SelectionButton __instance)` — line 135
Handles card selection during an active duel. Only runs when `IsInDuel` is true and button name contains `"HandCard"` or `"Anchor_"`.
- `"Anchor_"` buttons = field cards: sets `IsInHand = false`. If the `CardRoot` is face-down and belongs to the opponent (team != 0), speaks `"Opponent's face down card!"` immediately.
- `"HandCard"` buttons = hand cards: sets `IsInHand = true` and returns early (no field-card logic).

---

#### `internal static void ProcessMissionsMenu(SelectionButton __instance)` — line 168
Only runs for buttons named `"Locator"`. Navigates 9 levels up to a root parent, then reads mission name, reward text, and time-left into `textToCopy`.
Note: reward text strips its first character and prepends `"x"` (format correction).

---

#### `public static void ProcessDecksMenu(SelectionButton __instance)` — line 186
Handles multiple button types in the deck editor/browser:
- `"ImageCard"`: if not using mouse, calls `Instance.CopyUI()`; if using mouse, appends owned count and rarity (from `"IconRarity"` sprite name) to `textToCopy`.
- Button inside `"Category"` grandparent: appends category label text.
- Button inside `"InputButton"` grandparent: overrides to `"Rename deck button"`.
- Button inside `"AutoBuildButton"` grandparent: overrides to `"Auto-build button"`.

---

#### `internal static void ProcessSettingsMenu(SelectionButton __instance)` — line 209
Reads the current value of a settings entry and appends it to `textToCopy`.
- Skips buttons inside `"Layout"`, `"EntryButtonsScrollView"`, or named `"CancelButton"`.
- If a `Slider` child exists: value is `"{value} of {maxValue}"`.
- Otherwise: reads the text of the child `ExtendedTextMeshProUGUI` named `"ModeText"`.

---

#### `internal static void ProcessCardPack(SelectionButton __instance)` — line 228
For buttons named `"CardPict"` (individual card reveal in a pack opening): reads rarity from `"IconRarity"` sprite, checks `"NewIcon"` active state, reads owned count, and sets `textToCopy`.

---

#### `internal static void ProcessNotifications(SelectionButton __instance)` — line 238
For buttons with a `"BaseCategory"` child: reads body text from `"TextBody"` child. If `"BaseCategory"` is active, appends its first child's text as status.

---

#### `internal static void ProcessNotificationsPopup(SelectionButton __instance)` — line 248
Same logic as `ProcessNotifications`, but only runs when 6 levels up the hierarchy is named `"NotificationWidget"` and `currentMenu == Menus.NONE` (i.e., the notification overlay, not the full notifications screen).

---

#### `internal static void ProcessEventBanner(SelectionButton __instance)` — line 258
If button is named `"DuelShortcut"`, sets `textToCopy` to `"Event banner"`.

---

#### `internal static void ProcessTopicsBanner(SelectionButton __instance)` — line 263
If button is named `"ButtonBanner"`, sets `textToCopy` to `"Topic banner, page {N}"` using the `ScrollRectPageSnap.hpage` on the button's parent.

---

#### `internal static void ProcessDuelPass(SelectionButton __instance)` — line 268
For buttons whose name contains `"passRewardButton"`: determines Normal vs Gold pass from button name, reads grade from grandparent text, formats owned quantity by replacing the first character with `"x"`, and sets `textToCopy`.

---

#### `internal static void ProcessNewDeck(SelectionButton __instance)` — line 273
If the button has an active `"IconAddDeck"` child, sets `textToCopy` to `"New deck button"`. Used to distinguish the "create new deck" button from existing deck buttons.
