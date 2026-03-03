# Yu-Gi-Oh Master Duel - Game Screens & UI Structure

## Menus Enum

Defined in `BaseClass.cs`:

```csharp
public enum Menus { NONE, DUEL, DECK, SOLO, SHOP, Missions, Notifications, Settings, DuelPass }
```

| Value | Meaning | Triggered by nav button text |
|-------|---------|------------------------------|
| NONE | Home screen or unknown context | (default, reset on back-to-Home) |
| DUEL | Online/ranked duel matchmaking area | "DUEL" |
| DECK | Deck building and browsing | "DECK" |
| SOLO | Solo mode (story/CPU duels) | "SOLO" |
| SHOP | Card pack shop | "SHOP" |
| Missions | Mission/quest list | "MISSION" |
| Notifications | Notification list | "Notifications" |
| Settings | Game settings | "Game Settings" |
| DuelPass | Duel Pass seasonal reward track | "Duel Pass" |

The `menuNames` dictionary in `BaseClass.cs` maps button label strings to these enum values. When a `SelectionButton` is clicked and its text matches one of these keys, `currentMenu` is updated.

`currentMenu` is also set automatically when screen changes are detected via `vcNameToMenu` in `ScreenDetection.cs` (see VC-to-menu mapping below).

---

## Known ViewControllers (Game Screens)

### Setup / Entry Flow

| ViewController Name | Description | Detection method |
|---------------------|-------------|-----------------|
| `GameEntryV1` | Old-style entry sequence (pre-Unity6 path) | Detected in `CheckScreenChange`, explicitly skipped (no announcement) |
| `GameEntrySequenceV2` | New entry sequence managing country, age, ToS, privacy, survey, account | Detected in `CheckScreenChange`, explicitly skipped (no announcement); manages child VCs |
| `Title` | Title/splash screen with version number | `CheckScreenChange` - reads version from `CodeVer` EOM element only |
| `Enquete` | Survey screen (country/preference questions) | `CheckScreenChange` sets `pendingEnqueteCheck = true`; polled each frame via `CheckEnqueteScreen()` until `m_PageText` populates |
| `DownloadViewController` | Data download/update progress screen | Patched via `OnCreatedView` (`PatchDownloadViewControllerCreated`); progress polled in `CheckDownloadProgress()` |

### Main Menu / Home

| ViewController Name | Description | Detection method |
|---------------------|-------------|-----------------|
| `Home` | Main hub screen | Detected in `ViewController.OnBack` patch; resets `currentMenu = Menus.NONE` |

### Menus (mapped via `vcNameToMenu`)

| ViewController Name | Maps to Menu | Notes |
|---------------------|--------------|-------|
| `SoloMode` | `Menus.SOLO` | Solo mode gate/lobby |
| `SoloGate` | `Menus.SOLO` | Solo gate sub-screen |
| `SoloSelectChapter` | `Menus.SOLO` | Chapter selection map |
| `DuelClient` | `Menus.DUEL` | Active PvP duel (game client) |
| `DuelLive` | `Menus.DUEL` | Live duel view |
| `DeckEdit` | `Menus.DECK` | Deck editor |
| `DeckBrowser` | `Menus.DECK` | Deck browser/list |
| `Shop` | `Menus.SHOP` | Card pack shop |
| `SettingMenuViewController` | `Menus.Settings` | Settings menu |

Any other VC name resets `currentMenu` to `Menus.NONE` to avoid stale context.

---

## Dialog Types (Patched via OnCreatedView)

These dialog VCs are patched with `[HarmonyPatch(..., "OnCreatedView")]` to call `AnnounceDialogVC()` immediately when the dialog is created:

| Dialog ViewController | Namespace | Description |
|-----------------------|-----------|-------------|
| `ActionSheetViewController` | `Il2CppYgomGame.Menu` | Action sheet / context menu popup |
| `CommonDialogViewController` | `Il2CppYgomGame.Menu` | Generic confirmation/info dialog |
| `TitleDataLinkDialogViewController` | `Il2CppYgomGame.Menu` | Data transfer dialog from title screen |
| `DownloadViewController` | `Il2CppYgomGame.Download` | Download/update progress dialog |

Unknown dialog types not covered by these patches are caught by the fallback `CheckDialogTitle()` scanner in `Update()`, which scans `UI/OverlayCanvas/DialogManager` children.

---

## Screen Detection Mechanisms

### 1. Screen Change Detection (`CheckScreenChange`)
- Polls `UI/ContentCanvas/ContentManager` each frame for a `ViewControllerManager` component.
- Calls `GetFocusViewController()` to get the currently active screen.
- Compares against `lastFocusViewName` to detect changes.
- On change: reads `HeaderViewController.instance` for the localized screen name, then calls `FindScreenTitle()` on the VC for additional title/message text.
- Special cases: `Title` (version only), `Enquete` (async poll), `GameEntryV1`/`GameEntrySequenceV2` (skip).

### 2. Dialog Detection (`CheckDialogTitle` - fallback)
- Scans `UI/OverlayCanvas/DialogManager` children each frame.
- Looks for active children containing `(Clone)` in the name.
- Extracts title/body text by scanning `TMP_Text` components and matching path keywords (`title`, `header`, `message`, `desc`, `info`, `body`).
- Skips placeholder text (long strings with no spaces).
- Only resets `lastDialogTitle` when no active dialog exists.

### 3. Enquete Polling (`CheckEnqueteScreen`)
- Active when `pendingEnqueteCheck == true`.
- Casts focused VC to `EnqueteViewController` and reads `m_PageText`.
- Waits until `m_PageText` is non-empty (async content load complete).
- Re-announces on page changes (e.g. "1/3" -> "2/3").
- Skips toggle/entity/checkbox parent text (those are option labels, spoken via button navigation instead).

### 4. Download Progress (`CheckDownloadProgress`)
- Active when `activeDownloadVC != null` (set by `PatchDownloadViewControllerCreated`).
- Reads `downloadController.TotalProgress` (float 0-1) each frame.
- Announces each whole percentage change.
- On 100%, reads `DownloadingStateText` or `DownloadingText` for the game's completion message.

---

## UI Hierarchy Paths Used by the Mod

These absolute `GameObject.Find()` paths are used in the mod code:

| Path | Usage |
|------|-------|
| `UI/ContentCanvas/ContentManager` | Screen change detection - get `ViewControllerManager` |
| `UI/OverlayCanvas/DialogManager` | Fallback dialog scanning |
| `UI/ContentCanvas` | Root for `QueueFocusedItem` after screen change |
| `UI/ContentCanvas/ContentManager/DuelClient/CardInfo/CardInfo(Clone)/Root/Window` | Card detail panel during active duel |
| `UI/ContentCanvas/ContentManager/DeckEdit/DeckEditUI(Clone)/CardDetail/Root/Window` | Card detail panel in deck editor |
| `UI/ContentCanvas/ContentManager/DeckEdit/` | Existence check for deck editor context |
| `UI/ContentCanvas/ContentManager/DeckBrowser/DeckBrowserUI(Clone)/Root/CardDetail/Root/Window` | Card detail panel in deck browser |
| `UI/ContentCanvas/ContentManager/DeckBrowser/` | Existence check for deck browser context |
| `UI/ContentCanvas/ContentManager/DuelPass/DuelPassUI(Clone)/DuelPassArea/RootInfo/GradeAreaWidget/TextDuelPassLevel0` | Duel Pass grade level text |
| `UI/ContentCanvas/ContentManager/DuelPass/DuelPassUI(Clone)/DuelPassArea/RootInfo/LimitArea/LimitDateBase/LimitDateTextTMP` | Duel Pass expiry date |
| `UI/ContentCanvas/ContentManager/Shop/ShopUI(Clone)/Root/Main/ProductsRoot/ShowcaseWidget/ListRoot/ProductList/Viewport/Mask/Content/ShopGroupHeaderWidget(Clone)/Label` | Shop category group header label |
| `UI/OverlayCanvas/DialogManager/CommonDialog/CommonDialogUI(Clone)/Window/Content/TitleGrp/Text` | Common dialog title text (deck context) |
| `UI/OverlayCanvas/DialogManager/ItemPreview/ItemPreviewUI(Clone)/Root/RootMainArea/DescArea/RootDesc/` | Item preview popup content |
| `UI/OverlayCanvas/DialogManager/CardBrowser/CardBrowserUI(Clone)/Scroll View/Viewport/Content/Template(Clone){0}/CardInfoDetail_Browser(Clone)/Root/Window/StatusArea` | Card browser detail panel (page-indexed, `{0}` = `currentPage % 3`) |
| `UI/ContentCanvas/ContentManager/DuelClient/CardInfo/CardInfo(Clone)/Root/Window/DescriptionArea/TextArea/Viewport/TextDescriptionValue/` | Card description text in duel |

---

## EOM Element Labels Used

`ElementObjectManager.serializedElements` labels accessed by the mod:

| Label | Screen | Usage |
|-------|--------|-------|
| `CodeVer` | Title screen | Game version number |
| `TXT_LABEL` (from `HeaderViewController.TXT_LABEL`) | Header bar | Localized screen name |

Container labels that are skipped during title scanning (to avoid duplicates):
- `Root`
- `RootContent`
- `RootBottom`

Labels/path keywords used to identify title vs body text:
- Title: `title`, `header`, `start`
- Body: `message`, `description`, `desc`, `info`, `body`
- Skipped: path starts with `button`, `cancel`

---

## Named Buttons (ColorContainer / SelectionButton Patches)

These game object names are matched in `PatchColorContainerGraphic` and `PatchColorContainerImage` to produce accessible labels:

| GameObject Name | Announced As |
|----------------|--------------|
| `DuelMenuButton` | "Menu button" |
| `ButtonMaintenance` | "Maintenance" |
| `ButtonBug` | "Issues" |
| `ButtonNotification` | "Notification" |
| `InputButton` | "Rename button/input" (home context) or "Search card input" |
| `AutoBuildButton` | "Auto-build button" |
| `ButtonBookmark` | "Add card to bookmark button" |
| `BookmarkButton` | "Bookmarked cards button" |
| `HowToGetButton` | "How to get button" |
| `RelatedCard` | "Related cards button" |
| `DismantleButton` | "Dismantle card for: [amount] [rarity] cp" |
| `CreateButton` | "Create card for: [amount] [rarity] cp" |
| `AddButton` | "Add +1" |
| `RemoveButton` | "Remove -1" |
| `CardListButton` | "Card list button" |
| `HistotyButton` | "Card history button" (note: game typo preserved) |
| `ButtonRegulation` | "Regulation button" |
| `ButtonSecretPack` | "Secret pack button" |
| `ButtonInfoSwitching` | "Switch display mode button" |
| `ButtonSave` | "Save button" |
| `ButtonMenu` | "Menu button" |
| `ButtonPickupCard` | "Show cards on decks preview" |
| `BulkDecksDeletionButton` | "Bulk deck deletion button" |
| `ButtonOpenNeuronDecks` | "Link with Yu Gi Oh Database" |
| `FilterButton` | "Filters button" |
| `SortButton` | "Sort button" |
| `ClearButton` | "Clear filters button" |
| `Button0` | "[parent text], lower to higher" |
| `Button1` | "[parent text], higher to lower" |
| `ButtonDismantleIncrement` | "Increment dismantle amount" |
| `ButtonDismantleDecrement` | "Decrement dismantle amount" |
| `ButtonEnter` | "Play" |
| `CopyButton` | "Copy deck button" |
| `OKButton` | "Ok" |
| `ShowOwnedNumToggle` | "Show owned button" |
| `TabMyDeck` (grandparent) | "My Deck" |
| `TabRental` (grandparent) | "Loaner" |
| `ChapterDuel(Clone)` (grandparent) | "Duel, [stars] stars" |

---

## Solo Chapter Map Nodes

Chapter map nodes are detected by finding a parent GameObject starting with `Chapter` within 4 levels up from the button:

| Parent Name | Type Label |
|------------|------------|
| `ChapterDuel` | "Duel" |
| `ChapterScenario` | "Scenario" |
| `ChapterGoal` | "Goal" |
| `ChapterPractice` | "Practice" |
| (any `Chapter*`) | Substring after "Chapter" |

Each node announcement: "[chapter name], [type], Level [level]"

---

## SelectionButton Preview Elements (trigger CopyUI on click)

These SelectionButton names trigger a deferred `CopyUI()` call (card/item detail read-out):

```
CardPict, CardClone, CreateButton, ImageCard, NextButton, PrevButton,
Related Cards, ThumbButton, SlotTemplate(Clone), Locator,
GoldpassRewardButton, NormalpassRewardButton, ButtonDuelPass
```

Delay is 0.5 seconds normally, 1.5 seconds in `DuelPass` context.

---

## Duel-Specific UI

| SelectionButton Name | Description |
|---------------------|-------------|
| `HandCard*` | Card in the player's hand - sets `cardInfo.IsInHand = true` |
| `Anchor_*` | Card on the field - sets `cardInfo.IsInHand = false`, checks face-down state |
| `DuelListCard*` | Card in the field card list (triggers click + CopyUI) |
| `ButtonDecidePositive(Clone)` | End-of-duel confirm button - clears duel state |

In-duel keyboard shortcuts (from `Update()`):
- **Space**: Read current LP for both players
- **Left Alt**: Force card info panel open and read active card

Duel LP change announcements use `DuelLP.name` containing `"Far"` to distinguish opponent vs. player.
