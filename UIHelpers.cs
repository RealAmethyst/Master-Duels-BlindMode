using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using UnityEngine;
using UnityEngine.UI;

using Il2CppTMPro;
using Il2CppYgomSystem.UI;
using Il2CppYgomSystem.YGomTMPro;

using MelonLoader;

using static BlindMode.BaseClass;

namespace BlindMode
{
    public static class UIHelpers
    {
        public static string GetElement(string attrname)
        {
            if (int.TryParse(attrname.Last().ToString(), out int num))
            {
                foreach (object obj in Enum.GetValues(typeof(BaseClass.Attribute)))
                {
                    BaseClass.Attribute attribute = (BaseClass.Attribute)obj;
                    if (attribute == (BaseClass.Attribute)num)
                    {
                        return attribute.ToString();
                    }
                }
            }
            return "";
        }

        public static string GetRarity(string rarity)
        {
            if (int.TryParse(rarity.Last().ToString(), out int num))
            {
                foreach (object obj in Enum.GetValues(typeof(Rarity)))
                {
                    Rarity attribute = (Rarity)obj;
                    if (attribute == (Rarity)num)
                    {
                        return attribute.ToString();
                    }
                }
            }
            return "";
        }

        public static List<(string, string)> FindListExtendedTextElement(GameObject obj, string objPath = "", bool useRegex = true)
        {
            List<(string, string)> resultList = new();
            if (obj == null && !string.IsNullOrEmpty(objPath)) obj = GameObject.Find(objPath);

            if (obj.TryGetComponent(out ExtendedTextMeshProUGUI textElement) && !IsBannedText(textElement.gameObject, textElement.text, useRegex))
                resultList.Add(($"{textElement.transform.parent.name}/{textElement.name}", textElement.text));
            if (obj.TryGetComponent(out RubyTextGX rubyTextElement) && !IsBannedText(rubyTextElement.gameObject, rubyTextElement.text, useRegex))
                resultList.Add(($"{rubyTextElement.transform.parent.name}/{rubyTextElement.name}", rubyTextElement.text));
            if (obj.TryGetComponent(out TMP_SubMeshUI submeshTextElement) && !IsBannedText(submeshTextElement.gameObject, submeshTextElement.m_TextComponent.text, useRegex))
                resultList.Add(($"{submeshTextElement.transform.parent.name}/{submeshTextElement.name}", submeshTextElement.textComponent.text));

            resultList.AddRange(FindInChildrenList(obj, null, useRegex));

            return resultList.Distinct().ToList();
        }

        public static string FindExtendedTextElement(GameObject obj, string objPath = "", bool useRegex = true)
        {
            if(obj == null && !string.IsNullOrEmpty(objPath)) obj = GameObject.Find(objPath);

            if (obj.TryGetComponent(out ExtendedTextMeshProUGUI textElement) && !IsBannedText(textElement.gameObject, textElement.text, useRegex))
                return textElement.text;
            if (obj.TryGetComponent(out RubyTextGX rubyTextElement) && !IsBannedText(rubyTextElement.gameObject, rubyTextElement.text, useRegex))
                return rubyTextElement.text;
            if (obj.TryGetComponent(out TMP_SubMeshUI submeshTextElement) && !IsBannedText(submeshTextElement.gameObject, submeshTextElement.m_TextComponent.text, useRegex))
                return submeshTextElement.m_TextComponent.text;

            return FindInChildren(obj, "", useRegex);
        }

        public static List<(string, string)> FindInChildrenList(GameObject obj, string objPath = "", bool useRegex = true)
        {
            if (obj == null)
            {
                if (!string.IsNullOrEmpty(objPath))
                {
                    obj = GameObject.Find(objPath);
                }
            }

            List<(string, string)> resultList = new();

            for (int i = 0; i < obj.transform.childCount; i++)
            {
                Transform objTransform = obj.transform.GetChild(i);

                if (objTransform.TryGetComponent(out ExtendedTextMeshProUGUI textElement) &&
                    !IsBannedText(textElement.gameObject, textElement.text, useRegex))
                {
                    resultList.Add(($"{textElement.transform.parent.name}/{textElement.name}", textElement.text));
                }

                if (objTransform.TryGetComponent(out RubyTextGX rubyTextElement) &&
                    !IsBannedText(rubyTextElement.gameObject, rubyTextElement.text, useRegex))
                {
                    resultList.Add(($"{rubyTextElement.transform.parent.name}/{rubyTextElement.name}", rubyTextElement.text));
                }

                if (objTransform.TryGetComponent(out TMP_SubMeshUI submeshTextElement) &&
                    !IsBannedText(submeshTextElement.gameObject, submeshTextElement.textComponent.text, useRegex))
                {
                    resultList.Add(($"{submeshTextElement.transform.parent.name}/{submeshTextElement.name}", submeshTextElement.textComponent.text));
                }

                if(objTransform.childCount > 0)
                {
                    resultList.AddRange(FindInChildrenList(objTransform.gameObject, useRegex: useRegex));
                }
            }

            return resultList.Distinct().ToList();
        }

        public static string FindInChildren(GameObject obj, string objPath = "", bool useRegex = true)
        {
            if (obj == null)
            {
                if (!string.IsNullOrEmpty(objPath))
                {
                    obj = GameObject.Find(objPath);
                }
            }

            Transform objTransform = null;
            ExtendedTextMeshProUGUI UGUIChild = null;
            RubyTextGX rubyChild = null;
            TMP_SubMeshUI submeshChild = null;

            for (int i = 0; i < obj.transform.childCount; i++)
            {
                objTransform = obj.transform.GetChild(i);

                if (objTransform.TryGetComponent(out ExtendedTextMeshProUGUI textElement) && !IsBannedText(textElement.gameObject, textElement.text, useRegex))
                {
                    return textElement.text;
                }

                UGUIChild = objTransform.GetComponentInChildren<ExtendedTextMeshProUGUI>();

                if (UGUIChild != null && !IsBannedText(UGUIChild.gameObject, UGUIChild.text, useRegex))
                {
                    return UGUIChild.text;
                }

                if (objTransform.TryGetComponent(out RubyTextGX rubyTextElement) && !IsBannedText(rubyTextElement.gameObject, rubyTextElement.text, useRegex))
                {
                    return rubyTextElement.text;
                }

                rubyChild = objTransform.GetComponentInChildren<RubyTextGX>();

                if (rubyChild != null && !IsBannedText(rubyChild.gameObject, rubyChild.text, useRegex))
                {
                    return rubyChild.text;
                }

                if (objTransform.TryGetComponent(out TMP_SubMeshUI submeshTextElement) && !IsBannedText(submeshTextElement.gameObject, submeshTextElement.textComponent.text, useRegex))
                {
                    return submeshTextElement.textComponent.text;
                }

                submeshChild = objTransform.GetComponentInChildren<TMP_SubMeshUI>();

                if (submeshChild != null && !IsBannedText(submeshChild.gameObject, submeshChild.textComponent.text, useRegex))
                {
                    return submeshChild.textComponent.text;
                }
            }

            return null;
        }

        public static bool IsBannedText(GameObject textElement, string text, bool useRegex)
        {
            if (textElement == null || string.IsNullOrEmpty(text) || (textElement.gameObject.activeInHierarchy == false)) return true;

            return (useRegex && Regex.IsMatch(text, (currentMenu != Menus.NONE || textElement.name.Equals("Button")) ? @"^\s*$" : @"^\s*$|[.!]+$")) || bannedText.Contains(text);
        }

        public static string GetSelectionPosition(SelectionButton button)
        {
            try
            {
                Transform parent = button.transform.parent;
                if (parent == null) return null;

                int index = 0;
                int total = 0;
                bool found = false;

                for (int i = 0; i < parent.childCount; i++)
                {
                    Transform child = parent.GetChild(i);
                    if (!child.gameObject.activeInHierarchy) continue;
                    if (child.GetComponent<SelectionButton>() == null) continue;

                    total++;
                    if (child.gameObject == button.gameObject)
                    {
                        index = total;
                        found = true;
                    }
                }

                if (found && total > 1)
                    return $", {index} of {total}";
            }
            catch { }
            return null;
        }

        public static void GetUITextElements()
        {
            switch (currentMenu)
            {
                case Menus.DuelPass:
                    if (textRecord.Count == 0)
                    {
                        textToCopy = $"Pass grade: {FindExtendedTextElement(null, "UI/ContentCanvas/ContentManager/DuelPass/DuelPassUI(Clone)/DuelPassArea/RootInfo/GradeAreaWidget/TextDuelPassLevel0")}, Time left: {FindExtendedTextElement(null, "UI/ContentCanvas/ContentManager/DuelPass/DuelPassUI(Clone)/DuelPassArea/RootInfo/LimitArea/LimitDateBase/LimitDateTextTMP")}";
                        return;
                    }
                break;
                case Menus.DECK:
                    if (textRecord.Last().Contains("Create card"))
                    {
                        if (FindExtendedTextElement(null, "UI/OverlayCanvas/DialogManager/CommonDialog/CommonDialogUI(Clone)/Window/Content/TitleGrp/Text").Contains("Unable"))
                            textToCopy = "Unable to create card";
                        SpeakText();
                    }
                    break;
            }

            // check if its an item preview

            if (BaseClass.SnapContentManager == null && !(currentMenu == Menus.DECK || currentMenu == Menus.SOLO || currentMenu == Menus.DUEL))
            {
                List<(string, string)> textElements = FindListExtendedTextElement(null, "UI/OverlayCanvas/DialogManager/ItemPreview/ItemPreviewUI(Clone)/Root/RootMainArea/DescArea/RootDesc/", false);
                currenElement.Name = $"{(textElements.Count > 2 ? $"{textElements.First().Item2} - " : "")}{textElements[textElements.Count - 2].Item2}";
                currenElement.Description = textElements.Last().Item2;
                return;
            }

            var pathConditions = new List<(string PathPrefix, bool Condition)>
            {
                ("UI/OverlayCanvas/DialogManager/CardBrowser/CardBrowserUI(Clone)/Scroll View/Viewport/Content/Template(Clone){0}/CardInfoDetail_Browser(Clone)/Root/Window/StatusArea", BaseClass.SnapContentManager != null),
                ("UI/ContentCanvas/ContentManager/DeckEdit/DeckEditUI(Clone)/CardDetail/Root/Window", GameObject.Find("UI/ContentCanvas/ContentManager/DeckEdit/") != null),
                ("UI/ContentCanvas/ContentManager/DeckBrowser/DeckBrowserUI(Clone)/Root/CardDetail/Root/Window", GameObject.Find("UI/ContentCanvas/ContentManager/DeckBrowser/") != null),
                ("UI/ContentCanvas/ContentManager/DuelClient/CardInfo/CardInfo(Clone)/Root/Window", true)
            };

            foreach (var pathCondition in pathConditions)
            {
                if (!pathCondition.Condition) continue;

                string pathPrefix = pathCondition.PathPrefix;

                if (pathCondition.PathPrefix == pathConditions[0].PathPrefix)
                {
                    pathPrefix = string.Format(pathCondition.PathPrefix, BaseClass.SnapContentManager.currentPage % 3);
                }

                List<(string, string)> ParametersTexts = FindListExtendedTextElement(null, pathPrefix);

                currenElement.Name = ParametersTexts[0].Item2;
                currenElement.Description = ParametersTexts.Find(e => e.Item1.Contains("DescriptionValue")).Item2 ?? "";
                currenElement.cardInfo.Stars = ParametersTexts.Find(e => e.Item1.Contains("Rank") || e.Item1.Contains("Level")).Item2 ?? "";
                currenElement.cardInfo.Atk = ParametersTexts.Find(e => e.Item1.Contains("Atk")).Item2 ?? "";
                currenElement.cardInfo.Def = ParametersTexts.Find(e => e.Item1.Contains("Def")).Item2 ?? "";
                currenElement.cardInfo.PendulumScale = ParametersTexts.Find(e => e.Item1.Contains("Pendulum")).Item2 ?? "";
                currenElement.cardInfo.Link = ParametersTexts.Find(e => e.Item1.Contains("Link")).Item2 ?? "";
                currenElement.cardInfo.Element = GetElement(GameObject.Find($"{pathPrefix}/{(pathConditions[0].Condition == false ? (pathConditions[1].Condition ? "TitleArea/PlateTitle/IconAttribute" : "TitleArea/AttributeRoot/IconAttribute") : "TitleAreaGroup/TitleArea/IconAttribute")}").GetComponent<Image>().sprite.name) ?? "";
                currenElement.cardInfo.Attributes = ParametersTexts.Find(e => e.Item1.Contains("DescriptionItem")).Item2 ?? "";
                currenElement.cardInfo.SpellType = ParametersTexts.Find(e => e.Item1.Contains("SpellTrap")).Item2 ?? "";
                currenElement.cardInfo.Owned = ParametersTexts.Find(e => e.Item1.Contains("CardNum")).Item2 ?? "";

                break;
            }
        }

        public static string FormatInfo()
        {
            if (string.IsNullOrWhiteSpace(currenElement.Name)) return string.Empty;

            List<string> resultList = new List<string>
            {
                !string.IsNullOrEmpty(currenElement.Name) ? $"Name: {currenElement.Name}" : null,
                !string.IsNullOrEmpty(currenElement.Description) ? $"Description: {currenElement.Description}" : null
            };

            if (BaseClass.SnapContentManager != null || currentMenu == Menus.SOLO || currentMenu == Menus.DUEL || currentMenu == Menus.DECK)
            {
                resultList = new List<string>
                {
                    !string.IsNullOrEmpty(currenElement.Name) ? $"Name: {currenElement.Name}" : null,
                    (!currenElement.cardInfo.IsInHand && IsInDuel) ? $"Is faced down?: {!GetCardRootOfCurrentCard().isFace}" : null,
                    !string.IsNullOrEmpty(currenElement.cardInfo.Atk) ? $"Attack: {currenElement.cardInfo.Atk}" : null,
                    !string.IsNullOrEmpty(currenElement.cardInfo.Link) ? $"Link level: {currenElement.cardInfo.Link}" : null,
                    !string.IsNullOrEmpty(currenElement.cardInfo.Def) ? $"Defense: {currenElement.cardInfo.Def}" : null,
                    !string.IsNullOrEmpty(currenElement.cardInfo.Stars) ? $"Stars: {currenElement.cardInfo.Stars}" : null,
                    !string.IsNullOrEmpty(currenElement.cardInfo.Element) ? $"Element: {currenElement.cardInfo.Element}" : null,
                    !string.IsNullOrEmpty(currenElement.cardInfo.PendulumScale) ? $"Pendulum scale: {currenElement.cardInfo.PendulumScale}" : null,
                    !string.IsNullOrEmpty(currenElement.cardInfo.Attributes) ? $"Attributes: {(currentMenu == Menus.DECK ? currenElement.cardInfo.Attributes[1..^1] : currenElement.cardInfo.Attributes)}" : null,
                    !string.IsNullOrEmpty(currenElement.cardInfo.SpellType) ? $"Spell type: {currenElement.cardInfo.SpellType}" : null,
                    !string.IsNullOrEmpty(currenElement.cardInfo.Owned) ? $"Owned: {currenElement.cardInfo.Owned}" : null,
                    !string.IsNullOrEmpty(currenElement.Description) ? $"Description: {currenElement.Description}" : FindExtendedTextElement(null, "UI/ContentCanvas/ContentManager/DuelClient/CardInfo/CardInfo(Clone)/Root/Window/DescriptionArea/TextArea/Viewport/TextDescriptionValue/"),
                };
            }
            else if (currentMenu == Menus.SHOP)
            {
                resultList = new List<string>
                {
                    $"Name: {currenElement.Name}",
                    $"Category: {currenElement.Description}",
                    $"Time left: {currenElement.TimeLeft}",
                    $"Price: {currenElement.Price}",
                };
            }

            resultList = resultList.Where(item => item?.Trim() != null).ToList();

            return string.Join("\n", resultList);
        }
    }
}
