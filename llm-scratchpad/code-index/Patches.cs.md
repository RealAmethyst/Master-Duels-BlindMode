# Patches.cs Code Index

All classes are in `namespace BlindMode`. All patches are `[HarmonyPostfix]`.
Common imports: `using static BlindMode.BaseClass`, `UIHelpers`, `MenuProcessors`, `ScreenDetection`.

---

## PatchBrowserViewControllerStart (line 27)
`[HarmonyPatch(typeof(CardBrowserViewController), nameof(CardBrowserViewController.Start))]`
Captures the `SnapContentManager` component from the card browser on startup.

### Methods
- `Postfix(CardBrowserViewController __instance) -> void` (line 30)

---

## PatchChangeLP (line 40)
`[HarmonyPatch(typeof(DuelLP), nameof(DuelLP.ChangeLP), MethodType.Normal)]`
Announces LP changes. Clears duel state when LP drops below 1 (duel ends).

### Methods
- `Postfix(DuelLP __instance) -> void` (line 43)
  // Distinguishes near/far player by checking if `__instance.name` contains "Far"

---

## PatchDuelClientSetupPvp (line 55)
`[HarmonyPatch(typeof(DuelClient), nameof(DuelClient.Awake))]`
Sets `currentMenu = Menus.DUEL` and `IsInDuel = true` when a duel starts.

### Methods
- `Postfix(DuelClient __instance) -> void` (line 58)

---

## PatchCardRoot (line 66)
`[HarmonyPatch(typeof(CardRoot), nameof(CardRoot.Initialize), MethodType.Normal)]`
Tracks all cards placed on the field by adding each `CardRoot` to `cardsInDuel`.

### Methods
- `Postfix(CardRoot __instance) -> void` (line 69)

---

## PatchCardInfoSetCard (line 76)
`[HarmonyPatch(typeof(CardInfo), nameof(CardInfo.SetDescriptionArea))]`
Triggers `CopyUI` (card info speech) when a card description panel is set.
Uses a 0.2s delay if the panel is not yet active in the hierarchy.

### Methods
- `Postfix(CardInfo __instance) -> void` (line 79)

---

## PatchActionSheetCreated (line 90)
`[HarmonyPatch(typeof(ActionSheetViewController), "OnCreatedView")]`
Fires `AnnounceDialogVC` immediately when an action sheet dialog is created,
ensuring the dialog header speaks before any auto-focused button text.

### Methods
- `Postfix(ActionSheetViewController __instance) -> void` (line 93)

---

## PatchCommonDialogCreated (line 100)
`[HarmonyPatch(typeof(CommonDialogViewController), "OnCreatedView")]`
Same pattern as `PatchActionSheetCreated` for common (yes/no) dialogs.

### Methods
- `Postfix(CommonDialogViewController __instance) -> void` (line 103)

---

## PatchTitleDataLinkDialogCreated (line 110)
`[HarmonyPatch(typeof(TitleDataLinkDialogViewController), "OnCreatedView")]`
Same pattern as `PatchActionSheetCreated` for the title screen data-link dialog.

### Methods
- `Postfix(TitleDataLinkDialogViewController __instance) -> void` (line 113)

---

## PatchDownloadViewControllerCreated (line 120)
`[HarmonyPatch(typeof(DownloadViewController), "OnCreatedView")]`
Stores the active download VC in `activeDownloadVC` for progress polling in `Update()`.
Resets `lastDownloadPercent = -1` and speaks the screen title immediately.

### Methods
- `Postfix(DownloadViewController __instance) -> void` (line 123)

---

## PatchColorContainerGraphic (line 139)
`[HarmonyPatch(typeof(ColorContainerGraphic), nameof(ColorContainerGraphic.SetColor))]`
Handles focus events for buttons that use `ColorContainerGraphic` for highlight state
rather than `SelectionButton.OnSelected`. Triggers on `StatusMode.Enter` only.

Contains two static lookup tables:
- `parentLabels` (line 142) — maps `parent.parent.name` → label string (~23 entries)
- `grandparentLabels` (line 176) — maps `parent.parent.parent.name` → label string (3 entries)

Dynamic cases handled inline: `DismantleButton`, `CreateButton`, `InputButton`, `Button0`, `Button1`, `ChapterDuel(Clone)`.
// Note: grandparent lookup can override a prior parent match (evaluated after).
// DuelListCard branch clicks the button and calls `CopyUI` directly, bypassing label lookup.

### Methods
- `Postfix(ColorContainerGraphic __instance) -> void` (line 184)
  // Wrapped in try/catch; sets `textToCopy` and calls `SpeakText()` if non-empty

---

## PatchOnClick (line 248)
`[HarmonyPatch(typeof(SelectionButton), nameof(SelectionButton.OnClick), MethodType.Normal)]`
Handles click (confirm) events on `SelectionButton`. Responsibilities:
1. Detects menu transitions by matching button text against `menuNames` dictionary.
2. Clears duel state when the positive confirm button is clicked mid-duel (`ButtonDecidePositive(Clone)`).
3. Invokes `CopyUI` with a delay for preview element buttons (card images, pack thumbs, etc.).

Static field:
- `previewElements` (line 250) — list of button names that trigger a deferred `CopyUI` call

### Methods
- `Postfix(SelectionButton __instance) -> void` (line 253)

---

## PatchOnSelected (line 279)
`[HarmonyPatch(typeof(SelectionButton), nameof(SelectionButton.OnSelected), MethodType.Normal)]`
Main button focus handler. The most complex patch. Responsibilities:
1. Extracts button text via `FindExtendedTextElement`.
2. Sibling fallback (lines 289-312): if no text found, walks up to 3 parent levels searching
   sibling TMP_Text components. Handles toggle/radio widgets where the label is not a child
   of the button. Uses `goto siblingFound` to break out of nested loops.
3. Dispatches to the appropriate `MenuProcessors` method(s) based on `currentMenu`.
4. Appends position text (e.g. ", 3 of 5") via `GetSelectionPosition`.
5. Stores result in `pendingButtonText` rather than speaking immediately, so `Update()` can
   run dialog/screen detection first (prevents button text from interrupting dialog headers).

// Note: SOLO and DUEL share the same switch cases (line 344-347).

### Methods
- `Postfix(SelectionButton __instance) -> void` (line 283)

---

## PatchOnDeselected (line 366)
`[HarmonyPatch(typeof(SelectionButton), nameof(SelectionButton.OnDeselected), MethodType.Normal)]`
Calls `DeselectButton()` when a button loses focus.

### Methods
- `Postfix(SelectionButton __instance) -> void` (line 370)

---

## PatchViewController (line 379)
`[HarmonyPatch(typeof(ViewController), nameof(ViewController.OnBack))]`
Resets `currentMenu` to `Menus.NONE` when the back navigation returns focus to the Home VC.

### Methods
- `Postfix(ViewController __instance) -> void` (line 382)
  // Checks `__instance.manager` and `GetFocusViewController()` for null before acting
