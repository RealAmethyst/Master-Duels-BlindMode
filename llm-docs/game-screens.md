# Game Screens & Menu Reference

## Menu Contexts (Menus enum)

The `currentMenu` field in BaseClass tracks which menu the player is in:

| Value | Context |
|---|---|
| NONE | Home screen or neutral state |
| DUEL | Active PvP duel |
| SOLO | Solo mode (story/campaign) |
| DECK | Deck building/editing |
| SHOP | Shop screen |
| Missions | Missions/daily missions |
| Notifications | Notifications list |
| Settings | Game settings |
| DuelPass | Duel Pass (battle pass) |

## ViewController → Menu Mapping

From `ScreenDetection.vcNameToMenu`:

| ViewController Name | Menu | Notes |
|---|---|---|
| SoloMode | SOLO | Solo mode hub |
| SoloGate | SOLO | Gate selection |
| SoloSelectChapter | SOLO | Chapter selection |
| DuelClient | DUEL | PvP duel in progress |
| DuelLive | DUEL | Live duel variant |
| DeckEdit | DECK | Deck editor |
| DeckBrowser | DECK | Browse decks |
| Shop | SHOP | Shop screen |
| SettingMenuViewController | Settings | Settings menu |

## Special Screens (not in vcNameToMenu)

| ViewController | Handling |
|---|---|
| Title | Title screen — reads version from CodeVer EOM element, skips QueueFocusedItem |
| GameEntryV1 / GameEntrySequenceV2 | Setup sequence (country, age, TOS, privacy, survey, account) |
| Enquete | Survey screen — async loading, polled via `pendingEnqueteCheck` in Update() |
| Home | Home/main menu — sets currentMenu to NONE |

## Dialog Types

### OnCreatedView Patches (immediate detection)

| Dialog ViewController | Purpose |
|---|---|
| ActionSheetViewController | Generic action sheets (menus, confirmations) |
| CommonDialogViewController | Common dialogs (alerts, info popups) |
| TitleDataLinkDialogViewController | Data linking dialogs |
| DownloadViewController | Data download progress — tracked via `activeDownloadVC` |

### Fallback Detection (CheckDialogTitle polling)

Scans `UI/OverlayCanvas/DialogManager` children for dialogs not caught by OnCreatedView patches. Includes placeholder text filtering to avoid unresolved locale keys.

## Process* Menu Handlers (MenuProcessors.cs)

Each method enriches button text for a specific menu context:

| Method | Menu | Key Info Extracted |
|---|---|---|
| ProcessProfile | Profile | Player level |
| ProcessFriendsMenu | Friends | Search button, friend toggle |
| ProcessDailyReward | Daily reward | Day selection, reward status |
| ProcessPacks | Shop packs | Pack name, price, time limit, new indicator |
| ProcessDuelMenu | Solo/Duel menu | Chapter type (Duel/Practice/Goal/Scenario), level, completion |
| ProcessSoloChapter | Solo chapter map | Chapter type, level, extra info |
| ProcessDuelGame | Active duel | Card selection (hand vs field), opponent face-down, positions |
| ProcessMissionsMenu | Missions | Description, reward amounts, time remaining |
| ProcessDecksMenu | Deck management | Card ownership, rarity, category, deck controls |
| ProcessSettingsMenu | Settings | Slider values, toggle states, mode selections |
| ProcessCardPack | Card pack preview | Rarity, new indicator, owned quantity |
| ProcessNotifications | Notification list | Body text, status |
| ProcessNotificationsPopup | Notification popup | Text body, status |
| ProcessEventBanner | Event banners | Event identification |
| ProcessTopicsBanner | Topic/news banners | Banner ID, page tracking |
| ProcessDuelPass | Battle pass | Normal/Gold variants, grade, quantity, time remaining |
| ProcessNewDeck | New deck button | Identified by icon |

## Duel Features

### State Management
- `DuelClient.Awake` triggers duel start, sets `IsInDuel = true`
- `cardsInDuel` list tracks all CardRoot objects
- Space key in Update() announces both players' LP

### Life Points
- `DuelLP.ChangeLP` patch announces LP changes
- Announces "Your life points: X" / "Opponent's life points: X"
- Detects duel end when LP < 1

### Card Information (in duel)
- `CardRoot.isFace` — face-up status
- `CardRoot.team` — 0 = player, 1+ = opponent
- `CardRoot.cardLocator.pos` — field zone position
- Face-down opponent cards identified and announced

### Card Properties (from UIHelpers.GetUITextElements)
Name, ATK, DEF, Stars/Rank, Link Level, Element (Attribute), Pendulum Scale, Monster type tags, Spell/Trap type, Owned quantity, Rarity, Description text.
