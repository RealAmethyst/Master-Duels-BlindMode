# Patches.cs — Code Index

**File:** `Patches.cs`
**Namespace:** `BlindMode`

---

## Card Browser

#### `PatchBrowserViewControllerStart`
Captures SnapContentManager from CardBrowserViewController.

---

## Duels

#### `PatchChangeLP`
Speaks LP changes. Clears duel state when LP < 1.

#### `PatchDuelClientSetupPvp`
Sets currentMenu=DUEL and IsInDuel=true.

#### `PatchCardRoot`
Registers cards into cardsInDuel.

#### `PatchCardInfoSetCard`
Invokes CopyUI when card description updates.

---

## Dialogs

#### `PatchActionSheetCreated` / `PatchCommonDialogCreated` / `PatchTitleDataLinkDialogCreated`
Call AnnounceDialogVC on dialog creation.

#### `PatchDownloadViewControllerCreated`
Stores DownloadViewController for progress polling.

---

## Buttons

#### `PatchColorContainerGraphic`
Fires on StatusMode.Enter. Has two static dictionaries:
- `parentLabels` — Maps parent.parent name to label (~30 entries)
- `grandparentLabels` — Maps parent.parent.parent name to label (TabMyDeck, TabRental, DuelMenuButton)

Dynamic cases kept as explicit code: DismantleButton, CreateButton, InputButton, Button0, Button1, ChapterDuel(Clone).

In duels, triggers CopyUI for DuelListCard items.

---

#### `PatchOnClick`
Updates currentMenu via menuNames lookup. Clears duel state on positive button. Invokes CopyUI for previewElements list (readonly).

---

#### `PatchOnSelected`
Primary focus handler. Finds text via FindExtendedTextElement with sibling fallback. Dispatches to per-menu Process* methods. SOLO and DUEL cases merged into fall-through. Appends position text. Defers speech to pendingButtonText.

---

#### `PatchOnDeselected`
Calls DeselectButton().

---

## Navigation

#### `PatchViewController`
Resets currentMenu to NONE on back to Home.

---

### Notes
- PatchColorContainerImage removed (DuelMenuButton covered by grandparent lookup)
- Bare catch blocks now log via DebugLog
