# MenuProcessors.cs — Code Index

**File:** `MenuProcessors.cs`
**Namespace:** `BlindMode`
**Imports (static):** `BlindMode.BaseClass`, `BlindMode.UIHelpers`

---

## `public static class MenuProcessors`

All per-menu button handler methods. Called from PatchOnSelected based on currentMenu.

---

### Methods

#### `ProcessProfile` — ButtonPlayer: appends player level
#### `ProcessFriendsMenu` — SearchButton/OpenToggle handling
#### `ProcessDailyReward` — Day reward with received status (currently unused)

---

#### `ProcessPacks`
Reads shop pack data into currentElement. Only for Shop parent buttons.

---

#### `ProcessDuelMenu`
Settings delegation, solo chapter summary. Calls ProcessSoloChapter directly (no redundant try/catch wrapper).

---

#### `ProcessSoloChapter` (private)
Walks up to find Chapter* parent, extracts type name and level.

---

#### `ProcessDuelGame`
Card selection during duels. HandCard vs Anchor_ handling.

---

#### `ProcessMissionsMenu`
Locator buttons: reads mission name, reward, time-left.

---

#### `ProcessDecksMenu`
ImageCard, Category, InputButton, AutoBuildButton. Uses switch on parent.parent.parent.name.

---

#### `ProcessSettingsMenu`
Reads slider value or ModeText for settings entries.

---

#### `ProcessCardPack`
CardPict: rarity, new status, owned count.

---

#### `ReadNotificationText` (private)
Shared helper reading TextBody and BaseCategory status.

#### `ProcessNotifications`
Delegates to ReadNotificationText when BaseCategory exists.

#### `ProcessNotificationsPopup`
Same as ProcessNotifications but only for NotificationWidget overlay.

---

#### `ProcessEventBanner` — DuelShortcut → "Event banner"
#### `ProcessTopicsBanner` — ButtonBanner → "Topic banner, page N"
#### `ProcessDuelPass` — passRewardButton: Normal/Gold, grade, quantity
#### `ProcessNewDeck` — IconAddDeck → "New deck button"
