using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using Il2CppTMPro;
using Il2CppYgomSystem.UI;
using Il2CppYgomSystem.YGomTMPro;
using Il2CppYgomGame.Duel;
using Il2CppYgomGame.CardBrowser;
using Il2CppYgomGame.Menu;
using Il2CppYgomGame.Enquete;

using HarmonyLib;

using static BlindMode.BaseClass;
using static BlindMode.UIHelpers;
using static BlindMode.MenuProcessors;
using static BlindMode.ScreenDetection;

namespace BlindMode
{
    #region card browser patch
    [HarmonyPatch(typeof(CardBrowserViewController), nameof(CardBrowserViewController.Start))]
    class PatchBrowserViewControllerStart
    {
        [HarmonyPostfix]
        static void Postfix(CardBrowserViewController __instance)
        {
            BaseClass.SnapContentManager = __instance.GetComponentInChildren<Il2CppYgomSystem.UI.SnapContentManager>();
        }
    }
    #endregion

    #region duels patch

    [HarmonyPatch(typeof(DuelLP), nameof(DuelLP.ChangeLP), MethodType.Normal)]
    class PatchChangeLP
    {
        [HarmonyPostfix]
        private static void Postfix(DuelLP __instance)
        {
            SpeakText(string.Format("{0} current life points: {1}", __instance.name.Contains("Far") ? "Opponent's" : "Your", __instance.currentLP));
            if (__instance.currentLP < 1)
            {
                IsInDuel = false;
                cardsInDuel.Clear();
            }
        }
    }

    [HarmonyPatch(typeof(DuelClient), nameof(DuelClient.Awake))]
    class PatchDuelClientSetupPvp
    {
        [HarmonyPostfix]
        static void Postfix(DuelClient __instance)
        {
            currentMenu = Menus.DUEL;
            IsInDuel = true;
        }
    }

    [HarmonyPatch(typeof(CardRoot), nameof(CardRoot.Initialize), MethodType.Normal)]
    class PatchCardRoot
    {
        [HarmonyPostfix]
        private static void Postfix(CardRoot __instance)
        {
            cardsInDuel.Add(__instance);
        }
    }

    [HarmonyPatch(typeof(CardInfo), nameof(CardInfo.SetDescriptionArea))]
    class PatchCardInfoSetCard
    {
        [HarmonyPostfix]
        static void Postfix(CardInfo __instance)
        {
            Instance.Invoke("CopyUI", __instance.gameObject.activeInHierarchy ? 0f : 0.2f);
        }
    }

    #endregion

    #region dialog patches

    [HarmonyPatch(typeof(ActionSheetViewController), "OnCreatedView")]
    class PatchActionSheetCreated
    {
        [HarmonyPostfix]
        static void Postfix(ActionSheetViewController __instance)
        {
            AnnounceDialogVC(__instance);
        }
    }

    [HarmonyPatch(typeof(CommonDialogViewController), "OnCreatedView")]
    class PatchCommonDialogCreated
    {
        [HarmonyPostfix]
        static void Postfix(CommonDialogViewController __instance)
        {
            AnnounceDialogVC(__instance);
        }
    }

    [HarmonyPatch(typeof(TitleDataLinkDialogViewController), "OnCreatedView")]
    class PatchTitleDataLinkDialogCreated
    {
        [HarmonyPostfix]
        static void Postfix(TitleDataLinkDialogViewController __instance)
        {
            AnnounceDialogVC(__instance);
        }
    }

    [HarmonyPatch(typeof(DownloadViewController), "OnCreatedView")]
    class PatchDownloadViewControllerCreated
    {
        [HarmonyPostfix]
        static void Postfix(DownloadViewController __instance)
        {
            activeDownloadVC = __instance;
            lastDownloadPercent = -1;
            string title = FindScreenTitle(__instance);
            if (!string.IsNullOrEmpty(title))
                SpeakScreenHeader(title);
            DebugLog.Log("[Download] DownloadViewController created, tracking progress");
        }
    }

    #endregion

    #region buttons patches

    [HarmonyPatch(typeof(ColorContainerGraphic), nameof(ColorContainerGraphic.SetColor))]
    class PatchColorContainerGraphic
    {
        // Simple parent-level (parent.parent) button name → label mappings
        private static readonly Dictionary<string, string> parentLabels = new()
        {
            { "ButtonMaintenance", "Maintenance" },
            { "ButtonBug", "Issues" },
            { "ButtonNotification", "Notification" },
            { "AutoBuildButton", "Auto-build button" },
            { "ButtonBookmark", "Add card to bookmark button" },
            { "BookmarkButton", "Bookmarked cards button" },
            { "HowToGetButton", "How to get button" },
            { "RelatedCard", "Related cards button" },
            { "AddButton", "Add +1" },
            { "RemoveButton", "Remove -1" },
            { "CardListButton", "Card list button" },
            { "HistotyButton", "Card history button" },
            { "ButtonRegulation", "Regulation button" },
            { "ButtonSecretPack", "Secret pack button" },
            { "ButtonInfoSwitching", "Switch display mode button" },
            { "ButtonSave", "Save button" },
            { "ButtonMenu", "Menu button" },
            { "ButtonPickupCard", "Show cards on decks preview" },
            { "BulkDecksDeletionButton", "Bulk deck deletion button" },
            { "ButtonOpenNeuronDecks", "Link with Yu Gi Oh Database" },
            { "FilterButton", "Filters button" },
            { "SortButton", "Sort button" },
            { "ClearButton", "Clear filters button" },
            { "ButtonDismantleIncrement", "Increment dismantle amount" },
            { "ButtonDismantleDecrement", "Decrement dismantle amount" },
            { "ButtonEnter", "Play" },
            { "CopyButton", "Copy deck button" },
            { "OKButton", "Ok" },
            { "ShowOwnedNumToggle", "Show owned button" },
        };

        // Grandparent-level (parent.parent.parent) button name → label mappings
        private static readonly Dictionary<string, string> grandparentLabels = new()
        {
            { "TabMyDeck", "My Deck" },
            { "TabRental", "Loaner" },
            { "DuelMenuButton", "Menu button" },
        };

        [HarmonyPostfix]
        static void Postfix(ColorContainerGraphic __instance)
        {
            try
            {
                if (__instance.currentStatusMode != ColorContainer.StatusMode.Enter) return;

                if (IsInDuel && __instance.transform.parent.parent.name.Contains("DuelListCard"))
                {
                    __instance.transform.parent.parent.GetComponent<SelectionButton>().Click();
                    Instance.CopyUI();
                    return;
                }

                textToCopy = "";
                string parentName = __instance.transform.parent.parent.name;

                // Dynamic cases that need computed text
                if (parentName == "DismantleButton")
                {
                    string dismantle = FindExtendedTextElement(__instance.transform.parent.parent.GetChild(6).gameObject);
                    textToCopy = string.IsNullOrEmpty(dismantle)
                        ? "Cant be dismantled"
                        : $"Dismantle card for: {dismantle} {GetRarity(__instance.transform.parent.parent.GetChild(6).GetComponentInChildren<Image>().sprite.name)} cp";
                }
                else if (parentName == "CreateButton")
                {
                    textToCopy = $"Create card for: {FindExtendedTextElement(__instance.transform.parent.parent.GetChild(6).gameObject)} {GetRarity(__instance.transform.parent.parent.GetChild(6).GetComponentInChildren<Image>().sprite.name)} cp";
                }
                else if (parentName == "InputButton")
                {
                    textToCopy = currentMenu == Menus.NONE ? "Rename button/input" : "Search card input";
                }
                else if (parentName == "Button0")
                {
                    textToCopy = $"{FindExtendedTextElement(__instance.transform.parent.parent.parent.gameObject)}, lower to higher";
                }
                else if (parentName == "Button1")
                {
                    textToCopy = $"{FindExtendedTextElement(__instance.transform.parent.parent.parent.gameObject)}, higher to lower";
                }
                else if (parentLabels.TryGetValue(parentName, out string label))
                {
                    textToCopy = label;
                }

                // Grandparent-level lookups (override parent match if found)
                string grandparentName = __instance.transform.parent.parent.parent.name;
                if (grandparentName == "ChapterDuel(Clone)")
                {
                    textToCopy = $"Duel, {FindExtendedTextElement(__instance.transform.parent.parent.GetChild(4).gameObject)} stars";
                }
                else if (grandparentLabels.TryGetValue(grandparentName, out string gpLabel))
                {
                    textToCopy = gpLabel;
                }

                if (textToCopy != "") SpeakText();
            }
            catch (System.Exception ex) { DebugLog.Log($"[ColorContainerGraphic] Error: {ex.Message}"); }
        }
    }


    [HarmonyPatch(typeof(SelectionButton), nameof(SelectionButton.OnClick), MethodType.Normal)]
    class PatchOnClick
    {
        static readonly List<string> previewElements = new() { "CardPict", "CardClone", "CreateButton", "ImageCard", "NextButton", "PrevButton", "Related Cards", "ThumbButton", "SlotTemplate(Clone)", "Locator", "GoldpassRewardButton", "NormalpassRewardButton", "ButtonDuelPass" };

        [HarmonyPostfix]
        static void Postfix(SelectionButton __instance)
        {
            try
            {
                if (menuNames.TryGetValue(FindExtendedTextElement(__instance.gameObject), out Menus menu))
                {
                    currentMenu = menu;
                    textRecord.Clear();
                }
            }
            catch (System.Exception ex) { DebugLog.Log($"[OnClick] Menu detect error: {ex.Message}"); }

            if (__instance.name.Equals("ButtonDecidePositive(Clone)") && IsInDuel)
            {
                IsInDuel = false;
                cardsInDuel.Clear();
                currenElement.Clear();
            }

            if (previewElements.Contains(__instance.name))
            {
                Instance.Invoke("CopyUI", currentMenu == Menus.DuelPass ? 1.5f : 0.5f);
            }
        }
    }

    [HarmonyPatch(typeof(SelectionButton), nameof(SelectionButton.OnSelected), MethodType.Normal)]
    class PatchOnSelected
    {
        [HarmonyPostfix]
        static void Postfix(SelectionButton __instance)
        {
            textToCopy = FindExtendedTextElement(__instance.gameObject);

            // Fallback: search sibling elements for text (not recursive subtrees).
            // Handles toggle/radio widgets where the label is a sibling of the button.
            if (string.IsNullOrEmpty(textToCopy?.Trim()))
            {
                var current = __instance.transform;
                for (int level = 0; level < 3 && current.parent != null; level++)
                {
                    var parent = current.parent;
                    for (int i = 0; i < parent.childCount; i++)
                    {
                        var sibling = parent.GetChild(i);
                        if (sibling == current) continue;
                        var tmp = sibling.GetComponent<TMP_Text>();
                        if (tmp != null && tmp.gameObject.activeInHierarchy)
                        {
                            string clean = StripTags(tmp.text ?? "").Trim();
                            if (!string.IsNullOrEmpty(clean))
                            {
                                textToCopy = clean;
                                goto siblingFound;
                            }
                        }
                    }
                    current = current.parent;
                }
                siblingFound:;
            }

            switch (currentMenu)
            {
                case Menus.NONE:
                    ProcessNotificationsPopup(__instance);
                    ProcessFriendsMenu(__instance);
                    ProcessProfile(__instance);
                    ProcessEventBanner(__instance);
                    ProcessTopicsBanner(__instance);
                break;
                case Menus.Settings:
                    ProcessSettingsMenu(__instance);
                break;
                case Menus.Notifications:
                    ProcessNotifications(__instance);
                break;
                case Menus.Missions:
                    ProcessMissionsMenu(__instance);
                break;
                case Menus.SHOP:
                    ProcessPacks(__instance);
                    ProcessCardPack(__instance);
                break;
                case Menus.DuelPass:
                    ProcessDuelPass(__instance);
                break;
                case Menus.DECK:
                    ProcessDecksMenu(__instance);
                    ProcessNewDeck(__instance);
                break;
                case Menus.SOLO:
                case Menus.DUEL:
                    ProcessDuelGame(__instance);
                    ProcessDuelMenu(__instance);
                break;
            }

            // Only speak if there's actual text content
            if (string.IsNullOrEmpty(textToCopy?.Trim())) return;

            // Append position index (e.g. ", 3 of 5")
            string posText = GetSelectionPosition(__instance);
            if (!string.IsNullOrEmpty(posText))
                textToCopy += posText;

            // Defer speech to Update() so dialog/screen detection runs first.
            // This prevents button text from speaking before a dialog header
            // when clicking a button that opens a dialog (e.g. "Data Transfer").
            pendingButtonText = textToCopy;
        }
    }

    [HarmonyPatch(typeof(SelectionButton), nameof(SelectionButton.OnDeselected), MethodType.Normal)]
    class PatchOnDeselected
    {
        [HarmonyPostfix]
        static void Postfix(SelectionButton __instance)
        {
            DeselectButton();
        }
    }

    #endregion

    [HarmonyPatch(typeof(ViewController), nameof(ViewController.OnBack))]
    class PatchViewController
    {
        [HarmonyPostfix]
        public static void Postfix(ViewController __instance)
        {
            try
            {
                if (__instance.manager == null) return;
                var focusVC = __instance.manager.GetFocusViewController();
                if (focusVC == null) return;
                if (focusVC.name == "Home")
                {
                    currentMenu = Menus.NONE;
                }
            }
            catch (System.Exception ex) { DebugLog.Log($"[OnBack] Error: {ex.Message}"); }
        }
    }
}
