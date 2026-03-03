using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using Il2CppYgomSystem.UI;
using Il2CppYgomGame.Duel;
using Il2CppYgomGame.Menu;

using MelonLoader;

using static BlindMode.UIHelpers;
using static BlindMode.ScreenDetection;

namespace BlindMode
{
    public class BaseClass : MonoBehaviour
    {
        public static BaseClass Instance;

        public static List<string> textRecord = new();
        public static List<CardRoot> cardsInDuel = new();

        public static PreviewElement currenElement = new();

        public static Dictionary<string, Menus> menuNames = new()
        {
            { "DUEL", Menus.DUEL },
            { "DECK", Menus.DECK },
            { "SOLO", Menus.SOLO },
            { "SHOP", Menus.SHOP },
            { "MISSION", Menus.Missions },
            { "Notifications", Menus.Notifications },
            { "Game Settings", Menus.Settings },
            { "Duel Pass", Menus.DuelPass }
        };

        public enum Menus { NONE, DUEL, DECK, SOLO, SHOP, Missions, Notifications, Settings, DuelPass }
        public static Menus currentMenu = Menus.NONE;

        internal static string lastDialogTitle = "";
        internal static string lastScreenHeader = "";
        internal static string lastFocusViewName = "";

        public class CardCustomInfo
        {
            public GameObject cardObject { get; set; } = null;
            public string Link { get; set; } = string.Empty;
            public string Stars { get; set; } = string.Empty;
            public string Atk { get; set; } = string.Empty;
            public string Def { get; set; } = string.Empty;
            public string PendulumScale { get; set; } = string.Empty;
            public string Attributes { get; set; } = string.Empty;
            public string SpellType { get; set; } = string.Empty;
            public string Element { get; set; } = string.Empty;
            public string Owned { get; set; } = string.Empty;
            public bool IsInHand { get; set; } = true;
        }

        public class PreviewElement
        {
            public CardCustomInfo cardInfo { get; set; } = new CardCustomInfo();
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string TimeLeft { get; set; } = string.Empty;
            public string Price { get; set; } = string.Empty;

            public void Clear()
            {
                cardInfo = new CardCustomInfo();
                Name = string.Empty;
                Description = string.Empty;
                TimeLeft = string.Empty;
                Price = string.Empty;
            }

        }

        public enum Attribute
        {
            Light = 1,
            Dark = 2,
            Water = 3,
            Fire = 4,
            Earth = 5,
            Wind = 6,
            Divine = 7
        }

        public enum Rarity
        {
            Normal = 0,
            Rare = 1,
            SuperRare = 2,
            UltraRare = 3
        }

        public static List<string> bannedText = new(){ "00:00", "You can add new Cards to your Deck.", "ボタン" };
        public static string textToCopy;
        public static string old_copiedText;

        public static bool IsInDuel = false;

        public static DateTime lastExecutionTime;
        public static readonly TimeSpan cooldown = TimeSpan.FromSeconds(0.1f);
        internal static bool queueNextSpeech = false;
        internal static string pendingButtonText = null;

        public static bool UsingMouse = false;

        public static SnapContentManager SnapContentManager;

        // Download progress tracking
        internal static DownloadViewController activeDownloadVC = null;
        internal static int lastDownloadPercent = -1;

        // Enquete (survey) page tracking — polls until async content loads
        internal static bool pendingEnqueteCheck = false;
        internal static string lastEnquetePage = "";

        public void Awake()
        {
            Instance = this;
            DebugLog.Init();
            Tolk.TrySAPI(true);
            Tolk.Load();
            string screenReader = Tolk.DetectScreenReader();
            MelonLogger.Msg(screenReader != null
                ? $"Screen reader detected: {screenReader}"
                : "No screen reader detected, using SAPI fallback");
            DebugLog.Log($"[Init] Screen reader: {screenReader ?? "SAPI fallback"}");
        }

        public void OnApplicationQuit()
        {
            Tolk.Unload();
        }

        public void Update()
        {
            if (IsInDuel)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    List<DuelLP> DuelLPs = FindObjectsOfType<DuelLP>().ToList();
                    SpeakText($"Your life points: {DuelLPs.Find(e => e.m_IsNear).currentLP}\nOpponent's life points: {DuelLPs.Find(e => !e.m_IsNear).currentLP}");
                }

                if (Input.GetKeyDown(KeyCode.LeftAlt))
                {
                    currenElement.Clear();
                    CardInfo cardInfo = FindObjectOfType<CardInfo>();
                    if(!cardInfo.gameObject.activeInHierarchy) cardInfo.gameObject.SetActive(true);
                    CopyUI();
                }
            }

            CheckDialogTitle();
            CheckScreenChange();
            CheckDownloadProgress();
            CheckEnqueteScreen();

            // Process deferred button speech after dialog/screen detection.
            // This ensures dialog headers always speak before button text.
            if (pendingButtonText != null)
            {
                textToCopy = pendingButtonText;
                pendingButtonText = null;
                SpeakText();
            }
        }

        internal static void SpeakText(string text = "")
        {
            if (text == "") text = textToCopy;

            if (queueNextSpeech || DateTime.Now - lastExecutionTime >= cooldown)
            {
                if (!string.IsNullOrEmpty(old_copiedText) && old_copiedText.Equals(text)) return;
                if (string.IsNullOrEmpty(text?.Trim()) || bannedText.Contains(text)) return;

                text = StripTags(text);

                MelonLogger.Msg($"text to speak: {text}");
                DebugLog.Log($"[Speech] {text}");

                if (queueNextSpeech)
                {
                    Tolk.Output(text, false);
                    queueNextSpeech = false;
                }
                else
                {
                    Tolk.Output(text, true);
                }

                textRecord.Add(text);
                old_copiedText = text;

                lastExecutionTime = DateTime.Now;
            }
        }

        internal static void SpeakScreenHeader(string text)
        {
            if (string.IsNullOrEmpty(text?.Trim())) return;
            if (text == lastScreenHeader) return;

            text = StripTags(text);
            lastScreenHeader = text;
            old_copiedText = "";

            MelonLogger.Msg($"screen header: {text}");
            DebugLog.Log($"[Header] {text}");
            Tolk.Output(text, true);
            queueNextSpeech = true;
            lastExecutionTime = DateTime.Now;
        }

        public void CopyUI()
        {
            GetUITextElements();
            SpeakText(FormatInfo());
        }

        internal static void DeselectButton()
        {
            old_copiedText = "";
        }

        internal static CardRoot GetCardRootOfCurrentCard()
        {
            CardRoot cardRoot = cardsInDuel.Find(e => e.cardLocator.pos == currenElement.cardInfo.cardObject.transform.position);
            return cardRoot;
        }
    }
}
