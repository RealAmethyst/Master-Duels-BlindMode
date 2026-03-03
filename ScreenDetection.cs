using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

using Il2CppTMPro;
using Il2CppYgomSystem.UI;
using Il2CppYgomSystem.YGomTMPro;
using Il2CppYgomSystem.ElementSystem;
using Il2CppYgomGame.Menu;
using Il2CppYgomGame.Download;
using Il2CppYgomGame.Enquete;

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
                DebugLog.Log($"[FindTitle] No m_View on: {vc.name}, trying VC scan fallback");
            }

            if (eom != null)
                DebugLog.Log($"[FindTitle] Got m_View: {eom.name} for VC: {vc.name}");

            string title = null;
            string message = null;
            var textElements = new List<(string label, string text)>();

            // Approach 1: Use EOM's serializedElements array (game's own element system)
            // Only use leaf elements (skip container labels like Root, RootContent, RootBottom)
            // to avoid duplicate text from parent/child overlaps
            if (eom != null)
            {
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

                                string cleanText = StripTags(rawText).Trim();
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

                                string cleanText = StripTags(rawText).Trim();
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

                            string cleanText = StripTags(rawText).Trim();
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
                    text = StripTags(text).Trim();
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
                        text = StripTags(text).Trim();
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

                                string cleanText = StripTags(rawText).Trim();
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
                    return StripTags(t).Trim();
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
                        string clean = StripTags(t).Trim();
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

        /// <summary>
        /// Map VC names to menu contexts so button processors work correctly
        /// even when the player didn't click a menu button to get there.
        /// </summary>
        private static readonly Dictionary<string, Menus> vcNameToMenu = new(StringComparer.OrdinalIgnoreCase)
        {
            { "SoloMode", Menus.SOLO },
            { "SoloGate", Menus.SOLO },
            { "SoloSelectChapter", Menus.SOLO },
            { "DuelClient", Menus.DUEL },
            { "DuelLive", Menus.DUEL },
            { "DeckEdit", Menus.DECK },
            { "DeckBrowser", Menus.DECK },
            { "Shop", Menus.SHOP },
            { "SettingMenuViewController", Menus.Settings },
        };

        private static void UpdateMenuFromVC(string cleanName)
        {
            if (vcNameToMenu.TryGetValue(cleanName, out Menus menu))
            {
                currentMenu = menu;
                DebugLog.Log($"[MenuFromVC] Set currentMenu to {menu} from VC: {cleanName}");
            }
            else
            {
                // Reset to NONE for unknown VCs to prevent stale menu context
                // from causing crashes (e.g. ProcessSettingsMenu on non-settings buttons)
                if (currentMenu != Menus.NONE)
                {
                    DebugLog.Log($"[MenuFromVC] Reset currentMenu from {currentMenu} to NONE for VC: {cleanName}");
                    currentMenu = Menus.NONE;
                }
            }
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

                // Update currentMenu based on VC name so button processors use the right menu context.
                // This handles cases where the player navigates to a menu without clicking a menu button
                // (e.g. tutorial flow auto-directing to Solo mode, or backing out of settings).
                UpdateMenuFromVC(cleanName);

                if (cleanName == "GameEntryV1" || cleanName == "GameEntrySequenceV2") return;

                // Enquete (survey) loads content asynchronously via coroutine,
                // so m_View is null at this point. Poll in Update() instead.
                if (cleanName == "Enquete")
                {
                    pendingEnqueteCheck = true;
                    lastEnquetePage = "";
                    return;
                }

                // Title screen: just read the version number from CodeVer EOM element
                if (cleanName == "Title")
                {
                    ElementObjectManager eom = GetViewFromVC(focusVC);
                    if (eom != null)
                    {
                        try
                        {
                            var serialized = eom.serializedElements;
                            if (serialized != null)
                            {
                                foreach (var elem in serialized)
                                {
                                    if (elem == null) continue;
                                    if (elem.label == "CodeVer")
                                    {
                                        var tmp = elem.gameObject?.GetComponentInChildren<TMP_Text>(true);
                                        if (tmp != null && !string.IsNullOrEmpty(tmp.text?.Trim()))
                                        {
                                            SpeakScreenHeader(tmp.text.Trim());
                                            DebugLog.Log($"[ScreenChange] Title | version='{tmp.text.Trim()}'");
                                        }
                                        break;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            DebugLog.Log($"[ScreenChange] Title version error: {ex.Message}");
                        }
                    }
                    return;
                }

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
        /// Poll for Enquete (survey) page changes. Content loads asynchronously,
        /// so we check each frame until m_PageText is populated, then announce
        /// the question text and queue the focused item.
        /// Also re-announces when page changes (e.g. "1/3" → "2/3").
        /// </summary>
        internal static void CheckEnqueteScreen()
        {
            if (!pendingEnqueteCheck) return;

            try
            {
                GameObject contentManager = GameObject.Find("UI/ContentCanvas/ContentManager");
                if (contentManager == null) return;
                var vcm = contentManager.GetComponent<ViewControllerManager>();
                if (vcm == null) return;
                var focusVC = vcm.GetFocusViewController();
                if (focusVC == null) return;

                string vcName = focusVC.name;
                string cleanName = vcName.EndsWith("(Clone)") ? vcName[..^7] : vcName;
                if (cleanName != "Enquete")
                {
                    pendingEnqueteCheck = false;
                    lastEnquetePage = "";
                    return;
                }

                var enqueteVC = focusVC.TryCast<EnqueteViewController>();
                if (enqueteVC == null) return;

                // Wait for page text to be populated (async loading)
                var pageTextComp = enqueteVC.m_PageText;
                string pageText = pageTextComp != null ? pageTextComp.text?.Trim() : null;
                if (string.IsNullOrEmpty(pageText)) return; // Still loading

                // Only announce on page change
                if (pageText == lastEnquetePage) return;
                lastEnquetePage = pageText;

                // Scan all TMP_Text in the VC hierarchy for question/description text
                var allTmp = focusVC.gameObject.GetComponentsInChildren<TMP_Text>(true);
                if (allTmp == null) return;

                var texts = new List<string>();
                foreach (var tmp in allTmp)
                {
                    if (tmp == null || !tmp.gameObject.activeInHierarchy) continue;
                    string rawText = tmp.text?.Trim();
                    if (string.IsNullOrEmpty(rawText)) continue;
                    string clean = StripTags(rawText).Trim();
                    if (string.IsNullOrEmpty(clean)) continue;

                    // Skip page indicator, button labels, and very short text
                    if (clean == pageText) continue;
                    string nameLower = tmp.name.ToLower();
                    if (nameLower.Contains("button") || nameLower.Contains("shortcut")) continue;

                    // Skip option texts (they'll be spoken via button navigation)
                    string parentName = tmp.transform.parent?.name?.ToLower() ?? "";
                    if (parentName.Contains("toggle") || parentName.Contains("entity") || parentName.Contains("checkbox")) continue;

                    texts.Add(clean);
                }

                if (texts.Count == 0) return;

                // Build announcement: description/question text + page indicator
                string announcement = string.Join(". ", texts.Distinct()) + $". {pageText}";
                SpeakScreenHeader(announcement);
                QueueFocusedItem(focusVC.gameObject);

                DebugLog.Log($"[Enquete] Page {pageText}: {announcement}");
            }
            catch (Exception ex)
            {
                DebugLog.Log($"[Enquete] Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Poll the active DownloadViewController for progress changes.
        /// Announces each whole percentage change.
        /// </summary>
        internal static void CheckDownloadProgress()
        {
            try
            {
                if (activeDownloadVC == null) return;

                // Check if VC is still alive
                if (activeDownloadVC.WasCollected || !activeDownloadVC.gameObject.activeInHierarchy)
                {
                    activeDownloadVC = null;
                    lastDownloadPercent = -1;
                    return;
                }

                var controller = activeDownloadVC.downloadController;
                if (controller == null) return;

                int percent = (int)(controller.TotalProgress * 100f);
                if (percent == lastDownloadPercent) return;

                lastDownloadPercent = percent;
                old_copiedText = ""; // Reset so same-text check doesn't block

                if (percent >= 100)
                {
                    // Read the game's own completion text
                    var stateText = activeDownloadVC.DownloadingStateText;
                    string completeMsg = stateText != null ? stateText.text?.Trim() : null;
                    if (string.IsNullOrEmpty(completeMsg))
                    {
                        var dlText = activeDownloadVC.DownloadingText;
                        completeMsg = dlText != null ? dlText.text?.Trim() : null;
                    }
                    SpeakText(!string.IsNullOrEmpty(completeMsg) ? completeMsg : $"{percent} percent");
                    activeDownloadVC = null;
                    lastDownloadPercent = -1;
                }
                else
                {
                    SpeakText($"{percent} percent");
                }
            }
            catch (Exception ex)
            {
                DebugLog.Log($"[Download] Error checking progress: {ex.Message}");
            }
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
