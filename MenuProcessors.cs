using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using Il2CppTMPro;
using Il2CppYgomSystem.UI;
using Il2CppYgomGame.Duel;
using Il2CppYgomSystem.YGomTMPro;

using MelonLoader;

using static BlindMode.BaseClass;
using static BlindMode.UIHelpers;

namespace BlindMode
{
    public static class MenuProcessors
    {
        internal static void ProcessProfile(SelectionButton __instance)
        {
            if (__instance.name.Equals("ButtonPlayer")) textToCopy += $", level {FindExtendedTextElement(__instance.transform.GetChild(0).GetChild(1).GetChild(1).GetChild(1).gameObject, null, false)}";
        }

        internal static void ProcessFriendsMenu(SelectionButton __instance)
        {
            switch (__instance.name)
            {
                case "SearchButton":
                    textToCopy = "Add friend button";
                    break;
                case "OpenToggle":
                    textToCopy = FindExtendedTextElement(__instance.transform.parent.gameObject);
                    break;
            }
        }

        internal static void ProcessDailyReward(SelectionButton __instance)
        {
            if (textToCopy.Equals("Day")) textToCopy += $" {FindExtendedTextElement(__instance.transform.GetChild(3).GetChild(1).gameObject)}, Recieved: {(__instance.transform.Find("RecievedCover").gameObject.activeInHierarchy ? "Yes" : "No")}";
        }

        internal static void ProcessPacks(SelectionButton __instance)
        {
            if (!__instance.transform.parent.name.Contains("Shop")) return;

            List<(string, string)> ParametersTexts = FindListExtendedTextElement(__instance.gameObject);

            currentElement.Name = $"{ParametersTexts.Find(e => e.Item1.Contains("PickupMessage")).Item2 ?? ""} - {ParametersTexts.Find(e => e.Item1.Contains("Name")).Item2} ({ParametersTexts.Find(e => e.Item1.Contains("New")).Item2 ?? ""})";
            currentElement.Description = $"{FindExtendedTextElement(null, "UI/ContentCanvas/ContentManager/Shop/ShopUI(Clone)/Root/Main/ProductsRoot/ShowcaseWidget/ListRoot/ProductList/Viewport/Mask/Content/ShopGroupHeaderWidget(Clone)/Label", false)}";
            currentElement.TimeLeft = $"{ParametersTexts.Find(e => e.Item1.Contains("Limit")).Item2 ?? "None"}";
            currentElement.Price = $"{ParametersTexts.Find(e => e.Item1.Contains("PriceGroup")).Item2 ?? ""}";

            SpeakText(FormatInfo());
        }

        internal static void ProcessDuelMenu(SelectionButton __instance)
        {
            try
            {
                if (__instance.transform.parent.parent.parent.parent.parent.parent.name.Equals("SettingMenuArea")) ProcessSettingsMenu(__instance);

                if (__instance.transform.childCount > 0 && __instance.transform.GetChild(0).name.Equals("Main"))
                {
                    List<(string, string)> soloElements = FindListExtendedTextElement(__instance.gameObject, useRegex: false);
                    textToCopy = $"{soloElements.Last().Item2}, {soloElements.Find(e => e.Item1.Contains("Complete")).Item2}";
                }
            }
            catch (Exception ex)
            {
                DebugLog.Log($"[ProcessDuelMenu] {ex.Message}");
            }

            ProcessSoloChapter(__instance);
        }

        /// <summary>
        /// Process solo chapter map buttons. Each chapter node has a parent named
        /// "ChapterDuel", "ChapterScenario", "ChapterGoal", "ChapterPractice", etc.
        /// The button contains TextName (chapter name) and optionally Level/TextLevel.
        /// </summary>
        private static void ProcessSoloChapter(SelectionButton __instance)
        {
            // Check if this button is inside a Chapter* container on the chapter map
            string chapterType = null;
            Transform current = __instance.transform.parent;
            for (int i = 0; i < 4 && current != null; i++)
            {
                if (current.name.StartsWith("Chapter"))
                {
                    chapterType = current.name;
                    break;
                }
                current = current.parent;
            }
            if (chapterType == null) return;

            // Extract a readable type name from "ChapterDuel" → "Duel", "ChapterPractice" → "Practice"
            string typeName = chapterType.StartsWith("Chapter") ? chapterType.Substring(7) : chapterType;

            // Try to find level text in siblings
            string level = null;
            try
            {
                var levelTransform = __instance.transform.parent.Find("Level");
                if (levelTransform != null)
                {
                    var levelTmp = levelTransform.GetComponentInChildren<TMP_Text>(true);
                    if (levelTmp != null && !string.IsNullOrEmpty(levelTmp.text?.Trim()))
                        level = levelTmp.text.Trim();
                }
            }
            catch (Exception ex) { DebugLog.Log($"[ProcessSoloChapter] {ex.Message}"); }

            // Build the announcement: "Chapter1, Duel, Level 5"
            string extra = typeName;
            if (!string.IsNullOrEmpty(level))
                extra += $", Level {level}";

            if (!string.IsNullOrEmpty(textToCopy))
                textToCopy += $", {extra}";
            else
                textToCopy = extra;
        }

        internal static void ProcessDuelGame(SelectionButton __instance)
        {
            if (!IsInDuel) return;

            if (!(__instance.name.Contains("HandCard") || __instance.name.Contains("Anchor_"))) return;

            currentElement.cardInfo.cardObject = __instance.gameObject;

            if (__instance.name.Contains("Anchor_"))
            {
                currentElement.cardInfo.IsInHand = false;
            }
            else
            {
                currentElement.cardInfo.IsInHand = true;
                return;
            }

            try
            {
                CardRoot cardRoot = GetCardRootOfCurrentCard();

                if (!cardRoot.isFace && cardRoot.team != 0)
                {
                    SpeakText("Opponent's face down card!");
                }
            }
            catch (Exception ex)
            {
                DebugLog.Log($"[ProcessDuelGame] {ex.Message}");
            }
        }

        internal static void ProcessMissionsMenu(SelectionButton __instance)
        {
            if (!__instance.name.Equals("Locator")) return;

            Transform rootParent = __instance.transform.parent.parent.parent.parent.parent.parent.parent.parent.parent;

            if (rootParent != null)
            {
                if (rootParent.childCount > 0)
                {
                    string rewardText = FindExtendedTextElement(__instance.transform.GetChild(0).GetChild(2).gameObject, null, false);
                    rewardText = "x" + rewardText[1..];

                    textToCopy = $"{FindExtendedTextElement(rootParent.gameObject, null, false)}\n Reward: {rewardText}\n Time left: {FindExtendedTextElement(rootParent.GetChild(1).GetChild(0).GetChild(3).GetChild(0).gameObject, null, false) ?? "None"}";
                }
            }
        }

        public static void ProcessDecksMenu(SelectionButton __instance)
        {
            if(__instance.name.Equals("ImageCard"))
            {
                if(!UsingMouse) Instance.CopyUI();
                else textToCopy = $"Owned: {textToCopy}, rarity: {GetRarity(__instance.transform.Find("IconRarity").GetComponent<Image>().sprite.name)}";
            }

            switch (__instance.transform.parent.parent.parent.name)
            {
                case "Category":
                    textToCopy = $"{textToCopy}, category: {FindExtendedTextElement(__instance.transform.parent.parent.gameObject)}";
                    break;
                case "InputButton":
                    textToCopy = "Rename deck button";
                    break;
                case "AutoBuildButton":
                    textToCopy = "Auto-build button";
                    break;
            }
        }

        internal static void ProcessSettingsMenu(SelectionButton __instance)
        {
            if (__instance.transform.parent.parent.name == "Layout" || __instance.transform.parent.parent.parent.name == "EntryButtonsScrollView" || __instance.name == "CancelButton") return;

            string additionalText = "";
            Slider sliderElement = __instance.GetComponentInChildren<Slider>();

            if (sliderElement != null)
            {
                additionalText = $"{sliderElement.value} of {sliderElement.maxValue}";
            }
            else
            {
                additionalText = __instance.GetComponentsInChildren<ExtendedTextMeshProUGUI>().Where(e => e.name == "ModeText").First().text;
            }

            textToCopy += $"\nValue is {additionalText}";
        }

        internal static void ProcessCardPack(SelectionButton __instance)
        {
            if (__instance.name.Equals("CardPict"))
            {
                string ownedText = FindExtendedTextElement(__instance.transform.parent.Find("NumTextArea").gameObject);
                ownedText = "x" + ownedText[1..];
                textToCopy = $"Rarity: {GetRarity(__instance.transform.parent.Find("IconRarity").GetComponent<Image>().sprite.name)}, New: {(__instance.transform.parent.Find("NewIcon").gameObject.activeInHierarchy ? "Yes" : "No")}, Owned: {ownedText}";
            }
        }

        private static void ReadNotificationText(SelectionButton __instance)
        {
            textToCopy = FindExtendedTextElement(__instance.transform.Find("TextBody").gameObject, null, false);
            if (!__instance.transform.Find("BaseCategory").gameObject.activeInHierarchy) return;
            textToCopy += $"\nStatus: {__instance.transform.Find("BaseCategory").GetChild(0).GetComponentInChildren<ExtendedTextMeshProUGUI>().text}";
        }

        internal static void ProcessNotifications(SelectionButton __instance)
        {
            if (__instance.transform.Find("BaseCategory"))
                ReadNotificationText(__instance);
        }

        internal static void ProcessNotificationsPopup(SelectionButton __instance)
        {
            if (__instance.transform.parent.parent.parent.parent.parent.parent.name.Equals("NotificationWidget") && currentMenu == Menus.NONE)
                ReadNotificationText(__instance);
        }

        internal static void ProcessEventBanner(SelectionButton __instance)
        {
            if (__instance.name.Equals("DuelShortcut")) textToCopy = "Event banner";
        }

        internal static void ProcessTopicsBanner(SelectionButton __instance)
        {
            if (__instance.name.Equals("ButtonBanner")) textToCopy = $"Topic banner, page {__instance.transform.parent.GetComponent<ScrollRectPageSnap>().hpage}";
        }

        internal static void ProcessDuelPass(SelectionButton __instance)
        {
            if(__instance.name.Contains("passRewardButton")) textToCopy = $"{(__instance.name.Contains("Normalpass") ? "Normal" : "Gold")} pass, grade {FindExtendedTextElement(__instance.transform.parent.parent.gameObject)}, quantity: {"x" + textToCopy[1..]}";
        }

        internal static void ProcessNewDeck(SelectionButton __instance)
        {
            Transform IconAddDeck = __instance.transform.Find("IconAddDeck");
            if (IconAddDeck != null && IconAddDeck.gameObject.activeInHierarchy) textToCopy = "New deck button";
        }
    }
}
