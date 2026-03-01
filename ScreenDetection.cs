using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using UnityEngine;

using Il2CppTMPro;
using Il2CppYgomSystem.UI;
using Il2CppYgomSystem.YGomTMPro;
using Il2CppYgomSystem.ElementSystem;
using Il2CppYgomGame.Menu;

using static BlindMode.BaseClass;
using static BlindMode.UIHelpers;

namespace BlindMode
{
    public static class ScreenDetection
    {
        /// <summary>
        /// Get the ElementObjectManager (m_View) from a BaseMenuViewController.
        /// m_View is a protected field at offset 0x88 that holds the VC's UI element manager.
        /// The view may NOT be a child of the VC's transform (ViewCreater can parent it elsewhere).
        /// </summary>
        internal static ElementObjectManager GetViewFromVC(ViewController vc)
        {
            try
            {
                var baseVC = vc.TryCast<BaseMenuViewController>();
                if (baseVC == null)
                {
                    DebugLog.Log($"[GetView] TryCast<BaseMenuViewController> returned null for: {vc.name} (type: {vc.GetIl2CppType().Name})");
                    return null;
                }
                var view = baseVC.m_View;
                if (view == null)
                    DebugLog.Log($"[GetView] m_View is null for: {vc.name}");
                return view;
            }
            catch (Exception ex)
            {
                DebugLog.Log($"[GetView] Error for {vc.name}: {ex.Message}");
            }
            return null;
        }

        /// <summary>
        /// Find screen title text using the EOM's serializedElements array and TMP_Text scanning.
        /// serializedElements is an ElementObject[] which works in Il2Cpp (unlike SortedDictionary).
        /// Falls back to GetComponentsInChildren with includeInactive=true.
        /// </summary>
        internal static string FindScreenTitle(ViewController vc)
        {
            if (vc == null) return null;

            ElementObjectManager eom = GetViewFromVC(vc);
            if (eom == null)
            {
                DebugLog.Log($"[FindTitle] No m_View on: {vc.name}");
                return null;
            }

            DebugLog.Log($"[FindTitle] Got m_View: {eom.name} for VC: {vc.name}");

            string title = null;
            string message = null;
            var textElements = new List<(string label, string text)>();

            // Approach 1: Use EOM's serializedElements array (game's own element system)
            // Only use leaf elements (skip container labels like Root, RootContent, RootBottom)
            // to avoid duplicate text from parent/child overlaps
            try
            {
                var serialized = eom.serializedElements;
                if (serialized != null && serialized.Length > 0)
                {
                    DebugLog.Log($"[FindTitle] serializedElements count: {serialized.Length}");

                    // Collect all element labels that are containers (have children that are also elements)
                    var containerLabels = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                        { "Root", "RootContent", "RootBottom" };

                    foreach (var elem in serialized)
                    {
                        if (elem == null) continue;
                        string label = elem.label;
                        if (string.IsNullOrEmpty(label)) continue;

                        // Skip container elements to avoid duplicate text
                        if (containerLabels.Contains(label)) continue;

                        GameObject go = elem.gameObject;
                        if (go == null) continue;

                        // Search for TMP_Text on this element and its children (include inactive)
                        var tmpTexts = go.GetComponentsInChildren<TMP_Text>(true);
                        if (tmpTexts == null) continue;

                        foreach (var tmp in tmpTexts)
                        {
                            if (tmp == null) continue;
                            string rawText = tmp.text;
                            if (string.IsNullOrEmpty(rawText?.Trim())) continue;

                            string cleanText = Regex.Replace(rawText, @"<[^>]+>", "").Trim();
                            if (string.IsNullOrEmpty(cleanText)) continue;

                            textElements.Add((label, cleanText));
                            DebugLog.Log($"[EOM] {vc.name} | [{label}] {tmp.transform.parent?.name}/{tmp.name} = {cleanText}");
                        }
                    }
                }
                else
                {
                    DebugLog.Log($"[FindTitle] serializedElements is null or empty for: {eom.name}");
                }
            }
            catch (Exception ex)
            {
                DebugLog.Log($"[FindTitle] Error reading serializedElements: {ex.Message}");
            }

            // Approach 2: Fall back to scanning all TMP_Text in the EOM hierarchy (includeInactive=true)
            if (textElements.Count == 0)
            {
                try
                {
                    var allText = eom.gameObject.GetComponentsInChildren<TMP_Text>(true);
                    DebugLog.Log($"[FindTitle] Fallback TMP_Text scan count: {allText?.Length ?? 0}");

                    if (allText != null)
                    {
                        foreach (var tmp in allText)
                        {
                            if (tmp == null) continue;

                            string rawText = tmp.text;
                            if (string.IsNullOrEmpty(rawText?.Trim())) continue;

                            string cleanText = Regex.Replace(rawText, @"<[^>]+>", "").Trim();
                            if (string.IsNullOrEmpty(cleanText)) continue;

                            string path = $"{tmp.transform.parent?.name ?? "root"}/{tmp.name}";
                            textElements.Add((path, cleanText));
                            DebugLog.Log($"[EOM-fallback] {vc.name} | {path} = {cleanText}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    DebugLog.Log($"[FindTitle] Error in fallback scan: {ex.Message}");
                }
            }

            // Approach 3: Last resort - scan VC's own gameObject children
            if (textElements.Count == 0)
            {
                try
                {
                    var allText = vc.gameObject.GetComponentsInChildren<TMP_Text>(true);
                    DebugLog.Log($"[FindTitle] VC scan count: {allText?.Length ?? 0}");

                    if (allText != null)
                    {
                        foreach (var tmp in allText)
                        {
                            if (tmp == null) continue;

                            string rawText = tmp.text;
                            if (string.IsNullOrEmpty(rawText?.Trim())) continue;

                            string cleanText = Regex.Replace(rawText, @"<[^>]+>", "").Trim();
                            if (string.IsNullOrEmpty(cleanText)) continue;

                            string path = $"{tmp.transform.parent?.name ?? "root"}/{tmp.name}";
                            textElements.Add((path, cleanText));
                            DebugLog.Log($"[EOM-vc] {vc.name} | {path} = {cleanText}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    DebugLog.Log($"[FindTitle] Error in VC scan: {ex.Message}");
                }
            }

            if (textElements.Count == 0) return null;

            // Identify title vs message by label/path name
            // Skip labels starting with "Button" - those are interactive elements, not screen text
            foreach (var (label, text) in textElements)
            {
                string labelLower = label.ToLower();

                // Skip button elements - they're interactive, not screen titles/messages
                if (labelLower.StartsWith("button")) continue;

                if (title == null && (labelLower.Contains("title") || labelLower.Contains("header") || labelLower.Contains("start")))
                    title = text;
                else if (message == null && (labelLower.Contains("message") || labelLower.Contains("description") || labelLower.Contains("desc") || labelLower.Contains("info")))
                    message = text;
            }

            // Fallback: use first non-button text elements
            if (title == null && message == null)
            {
                foreach (var (label, text) in textElements)
                {
                    if (label.ToLower().StartsWith("button")) continue;
                    if (title == null)
                        title = text;
                    else if (message == null)
                    {
                        message = text;
                        break;
                    }
                }
            }

            if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(message))
                return $"{title}. {message}";
            if (!string.IsNullOrEmpty(title))
                return title;
            if (!string.IsNullOrEmpty(message))
                return message;

            return null;
        }

        /// <summary>
        /// Announce a dialog ViewController that was just created.
        /// Called from OnCreatedView patches on dialog VCs.
        /// </summary>
        internal static void AnnounceDialogVC(ViewController vc)
        {
            try
            {
                var baseVC = vc.TryCast<BaseMenuViewController>();
                if (baseVC == null) return;
                var eom = baseVC.m_View;
                if (eom == null) return;

                string title = null;
                string body = null;

                var allTmp = eom.gameObject.GetComponentsInChildren<TMP_Text>(true);
                if (allTmp == null) return;

                foreach (var tmp in allTmp)
                {
                    if (tmp == null || !tmp.gameObject.activeInHierarchy) continue;
                    string text = tmp.text?.Trim();
                    if (string.IsNullOrEmpty(text)) continue;
                    text = Regex.Replace(text, @"<[^>]+>", "").Trim();
                    if (string.IsNullOrEmpty(text) || bannedText.Contains(text)) continue;

                    string pathLower = $"{tmp.transform.parent?.name}/{tmp.name}".ToLower();

                    if (title == null && (pathLower.Contains("title") || pathLower.Contains("header")))
                        title = text;
                    else if (body == null && (pathLower.Contains("message") || pathLower.Contains("desc") || pathLower.Contains("info") || pathLower.Contains("body")))
                        body = text;
                }

                // If no body found via keywords, look for first long text after title
                if (!string.IsNullOrEmpty(title) && string.IsNullOrEmpty(body))
                {
                    bool pastTitle = false;
                    foreach (var tmp in allTmp)
                    {
                        if (tmp == null || !tmp.gameObject.activeInHierarchy) continue;
                        string text = tmp.text?.Trim();
                        if (string.IsNullOrEmpty(text)) continue;
                        text = Regex.Replace(text, @"<[^>]+>", "").Trim();
                        if (text == title) { pastTitle = true; continue; }
                        if (!pastTitle) continue;
                        if (text.Length < 30) continue;
                        string pathLower = $"{tmp.transform.parent?.name}/{tmp.name}".ToLower();
                        if (pathLower.Contains("button") || pathLower.Contains("cancel")) continue;
                        body = text;
                        break;
                    }
                }

                if (string.IsNullOrEmpty(title)) return;

                // Mark this dialog as announced so CheckDialogTitle won't re-announce
                lastDialogTitle = eom.name;

                string announcement = !string.IsNullOrEmpty(body)
                    ? $"Dialog. {title}. {body}"
                    : $"Dialog. {title}";

                DebugLog.Log($"[Dialog-VC] {vc.name}: title='{title}', body='{body}'");
                SpeakScreenHeader(announcement);
            }
            catch (Exception ex)
            {
                DebugLog.Log($"[Dialog-VC] Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Check if text looks like a placeholder (e.g. "TitleTextTitleText..." - unreplaced key name).
        /// </summary>
        static bool IsPlaceholderText(string text)
        {
            if (string.IsNullOrEmpty(text)) return true;
            // Placeholder text is typically a key name repeated with no spaces, very long
            if (text.Length > 30 && !text.Contains(' ') && !text.Contains('\n'))
                return true;
            return false;
        }

        /// <summary>
        /// Fallback dialog detection via Update() scanning.
        /// Handles dialogs not covered by OnCreatedView patches (e.g. TitleDataLinkDialog).
        /// </summary>
        internal static void CheckDialogTitle()
        {
            try
            {
                GameObject dialogManager = GameObject.Find("UI/OverlayCanvas/DialogManager");
                if (dialogManager == null) return;

                bool foundActiveDialog = false;

                for (int i = 0; i < dialogManager.transform.childCount; i++)
                {
                    Transform dialogRoot = dialogManager.transform.GetChild(i);
                    if (!dialogRoot.gameObject.activeInHierarchy) continue;

                    for (int j = 0; j < dialogRoot.childCount; j++)
                    {
                        Transform dialogUI = dialogRoot.GetChild(j);
                        if (!dialogUI.gameObject.activeInHierarchy) continue;
                        if (!dialogUI.name.Contains("(Clone)")) continue;

                        foundActiveDialog = true;
                        string dialogKey = dialogUI.name;
                        if (dialogKey == lastDialogTitle) return;

                        // Scan all TMP_Text in the dialog to find title and body
                        string title = "";
                        string bodyText = "";

                        var allTmp = dialogUI.gameObject.GetComponentsInChildren<TMP_Text>(true);
                        if (allTmp != null)
                        {
                            var dialogTexts = new List<(string path, string text)>();

                            foreach (var tmp in allTmp)
                            {
                                if (tmp == null || !tmp.gameObject.activeInHierarchy) continue;
                                string rawText = tmp.text;
                                if (string.IsNullOrEmpty(rawText?.Trim())) continue;

                                string cleanText = Regex.Replace(rawText, @"<[^>]+>", "").Trim();
                                if (string.IsNullOrEmpty(cleanText)) continue;

                                string parentName = tmp.transform.parent?.name ?? "";
                                string fullPath = $"{parentName}/{tmp.name}";

                                DebugLog.Log($"[Dialog-scan] {dialogUI.name} | {fullPath} = {cleanText}");
                                dialogTexts.Add((fullPath, cleanText));
                            }

                            // Match title by path keyword
                            foreach (var (path, text) in dialogTexts)
                            {
                                string pathLower = path.ToLower();
                                if (string.IsNullOrEmpty(title) && (pathLower.Contains("title") || pathLower.Contains("header")))
                                    title = text;
                                else if (string.IsNullOrEmpty(bodyText) && (pathLower.Contains("message") || pathLower.Contains("desc") || pathLower.Contains("info") || pathLower.Contains("body")))
                                    bodyText = text;
                            }

                            // Skip if title is placeholder text (not yet localized)
                            if (IsPlaceholderText(title))
                            {
                                DebugLog.Log($"[Dialog] Skipping placeholder text for {dialogUI.name}");
                                return; // Don't set lastDialogTitle - will retry next frame
                            }

                            // If title found but no body, look for next substantial text
                            if (!string.IsNullOrEmpty(title) && string.IsNullOrEmpty(bodyText))
                            {
                                bool foundTitle = false;
                                foreach (var (path, text) in dialogTexts)
                                {
                                    if (text == title) { foundTitle = true; continue; }
                                    if (!foundTitle) continue;
                                    if (text.Length < 30) continue;
                                    string pathLower = path.ToLower();
                                    if (pathLower.Contains("button") || pathLower.Contains("cancel")) continue;
                                    bodyText = text;
                                    break;
                                }
                            }

                            // Fallback: first non-button text
                            if (string.IsNullOrEmpty(title) && string.IsNullOrEmpty(bodyText))
                            {
                                foreach (var (path, text) in dialogTexts)
                                {
                                    if (path.ToLower().Contains("button") || path.ToLower().Contains("cancel")) continue;
                                    if (IsPlaceholderText(text)) continue;

                                    if (string.IsNullOrEmpty(title))
                                        title = text;
                                    else if (string.IsNullOrEmpty(bodyText))
                                    {
                                        bodyText = text;
                                        break;
                                    }
                                }
                            }
                        }

                        if (string.IsNullOrEmpty(title) && string.IsNullOrEmpty(bodyText)) return;

                        // Now safe to mark as announced
                        lastDialogTitle = dialogKey;

                        string announcement;
                        if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(bodyText))
                            announcement = $"Dialog. {title}. {bodyText}";
                        else if (!string.IsNullOrEmpty(title))
                            announcement = $"Dialog. {title}";
                        else
                            announcement = $"Dialog. {bodyText}";

                        DebugLog.Log($"[Dialog] Detected: {dialogRoot.name}/{dialogUI.name}, title='{title}', body='{bodyText}'");
                        SpeakScreenHeader(announcement);
                        return;
                    }
                }

                // Only reset when no active dialog exists at all (dialog truly closed)
                if (!foundActiveDialog)
                    lastDialogTitle = "";
            }
            catch { }
        }

        /// <summary>
        /// Get text from a GameObject using TMP_Text (catches all TMP text types).
        /// Includes inactive children to handle text that may not be visibly active yet.
        /// </summary>
        private static string GetTextFromElement(GameObject element)
        {
            // Try includeInactive=true since some text elements may be inactive
            var tmpText = element.GetComponentInChildren<TMP_Text>(true);
            if (tmpText != null)
            {
                string t = tmpText.text;
                if (!string.IsNullOrEmpty(t?.Trim()))
                    return Regex.Replace(t, @"<[^>]+>", "").Trim();
            }

            return null;
        }

        /// <summary>
        /// Read the localized screen name from the game's HeaderViewController.
        /// </summary>
        internal static string ReadGameHeaderText()
        {
            try
            {
                var headerVC = HeaderViewController.instance;
                if (headerVC == null) return null;
                if (!headerVC.gameObject.activeInHierarchy) return null;

                var eom = headerVC.ui;
                if (eom == null)
                {
                    DebugLog.Log("[HeaderVC] ui (EOM) is null");
                    return null;
                }

                string labelKey = headerVC.TXT_LABEL;
                if (string.IsNullOrEmpty(labelKey))
                {
                    DebugLog.Log("[HeaderVC] TXT_LABEL is null/empty");
                    return null;
                }

                // Try GetElement first (returns the labeled GameObject)
                GameObject titleElement = eom.GetElement(labelKey);
                if (titleElement != null)
                {
                    string text = GetTextFromElement(titleElement);
                    if (!string.IsNullOrEmpty(text?.Trim()))
                    {
                        DebugLog.Log($"[HeaderVC] {labelKey} = {text}");
                        return text;
                    }
                }

                // Fallback: scan all text in the header EOM
                var allText = eom.gameObject.GetComponentsInChildren<TMP_Text>(true);
                if (allText != null)
                {
                    foreach (var tmp in allText)
                    {
                        if (tmp == null || !tmp.gameObject.activeInHierarchy) continue;
                        string t = tmp.text;
                        if (string.IsNullOrEmpty(t?.Trim())) continue;
                        string clean = Regex.Replace(t, @"<[^>]+>", "").Trim();
                        if (!string.IsNullOrEmpty(clean))
                        {
                            DebugLog.Log($"[HeaderVC] fallback text = {clean}");
                            return clean;
                        }
                    }
                }

                DebugLog.Log($"[HeaderVC] No text found for '{labelKey}'");
            }
            catch (Exception ex)
            {
                DebugLog.Log($"[HeaderVC] Error: {ex.Message}");
            }
            return null;
        }

        internal static void CheckScreenChange()
        {
            try
            {
                GameObject contentManager = GameObject.Find("UI/ContentCanvas/ContentManager");
                if (contentManager == null) return;

                ViewControllerManager vcm = contentManager.GetComponent<ViewControllerManager>();
                if (vcm == null) return;

                ViewController focusVC = vcm.GetFocusViewController();
                if (focusVC == null) return;

                string vcName = focusVC.name;
                if (vcName == lastFocusViewName) return;

                lastFocusViewName = vcName;

                string cleanName = vcName.EndsWith("(Clone)") ? vcName[..^7] : vcName;

                if (cleanName == "GameEntryV1" || cleanName == "GameEntrySequenceV2") return;

                string headerText = ReadGameHeaderText();
                string titleText = FindScreenTitle(focusVC);

                string announcement;
                if (!string.IsNullOrEmpty(headerText) && !string.IsNullOrEmpty(titleText))
                    announcement = $"{headerText}. {titleText}";
                else if (!string.IsNullOrEmpty(headerText))
                    announcement = headerText;
                else if (!string.IsNullOrEmpty(titleText))
                    announcement = titleText;
                else
                {
                    DebugLog.Log($"[ScreenChange] Unknown ViewController, no text found: {cleanName}");
                    return;
                }

                DebugLog.Log($"[ScreenChange] {cleanName} | header='{headerText}', title='{titleText}'");
                SpeakScreenHeader(announcement);

                GameObject contentCanvas = GameObject.Find("UI/ContentCanvas");
                QueueFocusedItem(contentCanvas ?? focusVC.gameObject);
            }
            catch { }
        }

        /// <summary>
        /// After announcing a screen header, queue the currently focused
        /// SelectionButton text so the user knows what they're on.
        /// </summary>
        internal static void QueueFocusedItem(GameObject vcRoot)
        {
            try
            {
                var buttons = vcRoot.GetComponentsInChildren<SelectionButton>();
                if (buttons == null) return;

                foreach (var btn in buttons)
                {
                    if (btn == null || !btn.gameObject.activeInHierarchy) continue;

                    var colorContainer = btn.GetComponentInChildren<ColorContainerGraphic>();
                    if (colorContainer != null && colorContainer.currentStatusMode == ColorContainer.StatusMode.Enter)
                    {
                        string btnText = FindExtendedTextElement(btn.gameObject);
                        if (!string.IsNullOrEmpty(btnText?.Trim()))
                        {
                            string pos = GetSelectionPosition(btn);
                            string itemAnnouncement = btnText + (pos ?? "");
                            DebugLog.Log($"[QueueItem] {itemAnnouncement}");
                            Tolk.Output(itemAnnouncement, false);
                            old_copiedText = btnText;
                        }
                        return;
                    }
                }
            }
            catch { }
        }
    }
}
