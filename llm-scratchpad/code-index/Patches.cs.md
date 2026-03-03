// File: Patches.cs
namespace BlindMode

  #region card browser patch

  [HarmonyPatch(typeof(CardBrowserViewController), nameof(CardBrowserViewController.Start))]
  class PatchBrowserViewControllerStart
    static void Postfix(CardBrowserViewController __instance) (line 32)
    // Captures the SnapContentManager component into BaseClass.SnapContentManager for use by UIHelpers.

  #endregion

  #region duels patch

  [HarmonyPatch(typeof(DuelLP), nameof(DuelLP.ChangeLP), MethodType.Normal)]
  class PatchChangeLP
    private static void Postfix(DuelLP __instance) (line 45)
    // Speaks LP change. Also clears duel state when LP drops below 1.

  [HarmonyPatch(typeof(DuelClient), nameof(DuelClient.Awake))]
  class PatchDuelClientSetupPvp
    static void Postfix(DuelClient __instance) (line 60)
    // Sets currentMenu = DUEL and IsInDuel = true when a duel starts.

  [HarmonyPatch(typeof(CardRoot), nameof(CardRoot.Initialize), MethodType.Normal)]
  class PatchCardRoot
    private static void Postfix(CardRoot __instance) (line 71)
    // Registers each card into cardsInDuel list as they are initialized.

  [HarmonyPatch(typeof(CardInfo), nameof(CardInfo.SetDescriptionArea))]
  class PatchCardInfoSetCard
    static void Postfix(CardInfo __instance) (line 81)
    // Invokes CopyUI (immediately or with 0.2s delay) when a card description panel is updated.

  #endregion

  #region dialog patches

  [HarmonyPatch(typeof(ActionSheetViewController), "OnCreatedView")]
  class PatchActionSheetCreated
    static void Postfix(ActionSheetViewController __instance) (line 95)
    // Calls AnnounceDialogVC to read dialog title/body when an action sheet dialog is created.

  [HarmonyPatch(typeof(CommonDialogViewController), "OnCreatedView")]
  class PatchCommonDialogCreated
    static void Postfix(CommonDialogViewController __instance) (line 105)
    // Calls AnnounceDialogVC to read dialog title/body when a common dialog is created.

  [HarmonyPatch(typeof(TitleDataLinkDialogViewController), "OnCreatedView")]
  class PatchTitleDataLinkDialogCreated
    static void Postfix(TitleDataLinkDialogViewController __instance) (line 115)
    // Calls AnnounceDialogVC to read dialog title/body when the Data Transfer dialog is created.

  [HarmonyPatch(typeof(DownloadViewController), "OnCreatedView")]
  class PatchDownloadViewControllerCreated
    static void Postfix(DownloadViewController __instance) (line 125)
    // Stores the DownloadViewController in activeDownloadVC for progress polling in Update().
    // Resets lastDownloadPercent and speaks the screen title.

  #endregion

  #region buttons patches

  [HarmonyPatch(typeof(ColorContainerImage), nameof(ColorContainerImage.SetColor), MethodType.Normal)]
  class PatchColorContainerImage
    private static void Postfix(ColorContainerImage __instance) (line 144)
    // Fires on color state changes; only acts on StatusMode.Enter (hover/focus).
    // Handles the DuelMenuButton case (speaks "Menu button").
    // Note: ColorContainerImage is a different component from ColorContainerGraphic; covers a smaller set of buttons.

  [HarmonyPatch(typeof(ColorContainerGraphic), nameof(ColorContainerGraphic.SetColor))]
  class PatchColorContainerGraphic
    static void Postfix(ColorContainerGraphic __instance) (line 169)
    // Fires on color state changes; only acts on StatusMode.Enter (hover/focus).
    // In duels, triggers CopyUI for DuelListCard items instead of speaking button text.
    // Outside duels, matches a large switch on parent/grandparent GameObject names to produce
    // spoken button labels (dismantle, create, add, remove, sort, filter, tabs, etc.).
    // Note: "HistotyButton" is a typo in the game's own asset name.

  [HarmonyPatch(typeof(SelectionButton), nameof(SelectionButton.OnClick), MethodType.Normal)]
  class PatchOnClick
    static void Postfix(SelectionButton __instance) (line 327)
    // Updates currentMenu when a named navigation button is clicked (via menuNames lookup).
    // Clears IsInDuel/cardsInDuel when the positive duel-end button is clicked.
    // Invokes CopyUI for buttons in previewElements list (card art, next/prev, reward buttons, etc.).

  [HarmonyPatch(typeof(SelectionButton), nameof(SelectionButton.OnSelected), MethodType.Normal)]
  class PatchOnSelected
    static void Postfix(SelectionButton __instance) (line 357)
    // Primary focus/selection handler. Finds text for the focused button via FindExtendedTextElement.
    // Falls back to searching sibling TMP_Text elements (up to 3 ancestor levels) for toggle/radio
    // widgets where the label is a sibling of the button, not a child.
    // Dispatches to per-menu Process* methods (e.g. ProcessDecksMenu, ProcessPacks).
    // Appends position text (e.g. ", 3 of 5") from GetSelectionPosition.
    // Stores result in pendingButtonText (not spoken directly) so dialog/screen detection
    // in Update() can fire first, preventing button text from interrupting dialog headers.

  [HarmonyPatch(typeof(SelectionButton), nameof(SelectionButton.OnDeselected), MethodType.Normal)]
  class PatchOnDeselected
    static void Postfix(SelectionButton __instance) (line 449)
    // Calls DeselectButton() to clear any pending button state on focus loss.

  #endregion

  [HarmonyPatch(typeof(ViewController), nameof(ViewController.OnBack))]
  class PatchViewController
    public static void Postfix(ViewController __instance) (line 460)
    // Resets currentMenu to NONE when navigating back to the Home screen.
