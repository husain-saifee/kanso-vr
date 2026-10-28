using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using DePhobia.Core;
using DePhobia.VR;

/// <summary>
/// Redesigned Main Menu Controller: Forest Mineral & Sage Sanctuary.
/// Single central panel with 3 top tabs (Enclosed Space, Rising Water, High Elevation),
/// inline intensity presets (Gentle, Standard, Deep), a prominent "Begin Session" CTA,
/// and in-place view swaps for Session History and Comfort/Audio settings.
/// Strictly non-blue, non-violet, WCAG AAA compliant.
/// </summary>
public class mainmenu : MonoBehaviour
{
    // Legacy inspector bindings preserved to prevent scene deserialization errors
    public GameObject a;
    public GameObject b;
    public GameObject c;
    public static int flag = 0;
    public GameObject tex;
    private TextMeshProUGUI t;

    // View State
    private enum MenuState { Portal, History, Settings }
    private MenuState _currentState = MenuState.Portal;
    private int _activeTabIndex = 0; // 0 = Claustro, 1 = Aqua, 2 = Acro
    private string _activePreset = "Standard";

    // UI References
    private GameObject _sanctuaryCanvas;
    private GameObject _mainCard;
    private GameObject _historyCard;
    private GameObject _settingsCard;

    // Portal Tab UI References
    private Button[] _tabButtons = new Button[3];
    private Image[] _tabBackgrounds = new Image[3];
    private TextMeshProUGUI[] _tabTexts = new TextMeshProUGUI[3];

    private TextMeshProUGUI _spaceTitleText;
    private TextMeshProUGUI _spaceDescText;
    private TextMeshProUGUI _paramSummaryText;

    private Button _gentleBtn;
    private Button _standardBtn;
    private Button _deepBtn;
    private Image _gentleImg;
    private Image _standardImg;
    private Image _deepImg;

    private Button _beginBtn;
    private TextMeshProUGUI _historyContentText;

    private void Start()
    {
        // Fog is disabled in Main Menu (only active in Aquaphobia and Acrophobia)
        RenderSettings.fog = false;

        // 1. Decommission legacy submenus and privacy violations
        DecommissionLegacyMenus();

        // 2. Build the unified Sanctuary Interface
        BuildSanctuaryInterface();

        // 3. Select default tab and standard preset
        SelectTab(0);
        SelectPreset("Standard");

        // 4. Soften legacy ukulele audio if present
        var audioSources = FindObjectsOfType<AudioSource>();
        foreach (var src in audioSources)
        {
            if (src.clip != null && src.clip.name.ToLowerInvariant().Contains("bensound"))
            {
                src.volume = 0.15f;
            }
        }

        // 5. Fade in gently
        if (VRFadeManager.Instance != null)
        {
            VRFadeManager.Instance.FadeIn(0.6f);
        }
    }

    private void DecommissionLegacyMenus()
    {
        // Deactivate all legacy confusing submenus
        string[] legacyNames = new string[]
        {
            "MainMenu", "GameSelect", "Settings", "GameSelectSettings",
            "ClausSettingsMenu", "AquaSettingsMenu", "AcroSettingsMenu",
            "STATMENU", "FOLLOWOPT", "Follow", "lOADING", "Wait"
        };

        foreach (var name in legacyNames)
        {
            var go = GameObject.Find(name);
            if (go != null)
            {
                go.SetActive(false);
            }
        }
    }

    private void BuildSanctuaryInterface()
    {
        // Ensure Canvas exists in World Space at eye level
        var canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            var cGo = new GameObject("SanctuaryCanvas");
            canvas = cGo.AddComponent<Canvas>();
            cGo.AddComponent<CanvasScaler>();
            cGo.AddComponent<GraphicRaycaster>();
        }

        var canvasHelper = canvas.GetComponent<VRWorldSpaceCanvasHelper>();
        if (canvasHelper == null)
        {
            canvasHelper = canvas.gameObject.AddComponent<VRWorldSpaceCanvasHelper>();
        }
        canvasHelper.spawnDistance = 1.8f;
        canvasHelper.worldScale = 0.0022f;
        canvasHelper.SetupWorldSpace();

        _sanctuaryCanvas = canvas.gameObject;

        // Create the Main Master Container Card (640 x 480)
        _mainCard = new GameObject("SanctuaryMainCard");
        _mainCard.transform.SetParent(_sanctuaryCanvas.transform, false);

        var cardImg = _mainCard.AddComponent<Image>();
        cardImg.color = ThemeTokens.SurfacePanel;
        var cardRect = _mainCard.GetComponent<RectTransform>();
        cardRect.sizeDelta = new Vector2(640, 480);
        cardRect.anchoredPosition = Vector2.zero;

        // Header Section (Title + Subtitle)
        var title = ThemeTokens.CreateLabel(_mainCard.transform, "AppTitle", "Kanso VR", 22, ThemeTokens.TextPrimary, TextAlignmentOptions.Center, true);
        var titleRect = title.GetComponent<RectTransform>();
        titleRect.anchoredPosition = new Vector2(0, 215);
        titleRect.sizeDelta = new Vector2(580, 30);

        var subTitle = ThemeTokens.CreateLabel(_mainCard.transform, "AppSubTitle", "Therapeutic Exposure Platform • Simplicity & Composure", 13, ThemeTokens.TextSecondary, TextAlignmentOptions.Center);
        var subRect = subTitle.GetComponent<RectTransform>();
        subRect.anchoredPosition = new Vector2(0, 192);
        subRect.sizeDelta = new Vector2(580, 22);

        // -------------------------------------------------------------
        // 3 Tabs Switcher (Width 580, Height 40)
        // -------------------------------------------------------------
        var tabsObj = new GameObject("TabsContainer");
        tabsObj.transform.SetParent(_mainCard.transform, false);
        var tabsRect = tabsObj.AddComponent<RectTransform>();
        tabsRect.anchoredPosition = new Vector2(0, 150);
        tabsRect.sizeDelta = new Vector2(580, 40);

        string[] tabNames = new string[] { "Enclosed Space", "Rising Water", "High Elevation" };
        float tabWidth = 190f;
        float startX = -193f;

        for (int i = 0; i < 3; i++)
        {
            int index = i;
            var tabBtnObj = new GameObject($"Tab_{index}");
            tabBtnObj.transform.SetParent(tabsObj.transform, false);

            var tImg = tabBtnObj.AddComponent<Image>();
            tImg.color = ThemeTokens.SurfaceInactive;

            var tBtn = tabBtnObj.AddComponent<Button>();
            tBtn.targetGraphic = tImg;
            tBtn.onClick.AddListener(() =>
            {
                TherapyAudioManager.Instance.PlayFeedbackClick();
                SelectTab(index);
            });

            var tRect = tabBtnObj.GetComponent<RectTransform>();
            tRect.anchoredPosition = new Vector2(startX + (index * 193f), 0);
            tRect.sizeDelta = new Vector2(tabWidth, 38);

            var tLabel = ThemeTokens.CreateLabel(tabBtnObj.transform, "Label", tabNames[i], 14, ThemeTokens.TextSecondary, TextAlignmentOptions.Center, true);
            var lRect = tLabel.GetComponent<RectTransform>();
            lRect.anchorMin = Vector2.zero; lRect.anchorMax = Vector2.one;
            lRect.offsetMin = Vector2.zero; lRect.offsetMax = Vector2.zero;

            _tabButtons[i] = tBtn;
            _tabBackgrounds[i] = tImg;
            _tabTexts[i] = tLabel;
        }

        // -------------------------------------------------------------
        // Center Exposure Details Section
        // -------------------------------------------------------------
        _spaceTitleText = ThemeTokens.CreateLabel(_mainCard.transform, "SpaceTitle", "Enclosed Space (Claustrophobia)", 18, ThemeTokens.TextPrimary, TextAlignmentOptions.Center, true);
        var stRect = _spaceTitleText.GetComponent<RectTransform>();
        stRect.anchoredPosition = new Vector2(0, 105);
        stRect.sizeDelta = new Vector2(580, 28);

        _spaceDescText = ThemeTokens.CreateLabel(_mainCard.transform, "SpaceDesc", "A quiet room where walls contract gently at calibrated speeds.", 14, ThemeTokens.TextSecondary, TextAlignmentOptions.Center);
        var sdRect = _spaceDescText.GetComponent<RectTransform>();
        sdRect.anchoredPosition = new Vector2(0, 68);
        sdRect.sizeDelta = new Vector2(560, 42);

        // Intensity Preset Label
        var presetLabel = ThemeTokens.CreateLabel(_mainCard.transform, "PresetLabel", "Exposure Intensity", 12, ThemeTokens.TextMuted, TextAlignmentOptions.Center);
        var plRect = presetLabel.GetComponent<RectTransform>();
        plRect.anchoredPosition = new Vector2(0, 28);
        plRect.sizeDelta = new Vector2(580, 20);

        // 3 Preset Buttons: Gentle, Standard, Deep
        var presetsRow = new GameObject("PresetsRow");
        presetsRow.transform.SetParent(_mainCard.transform, false);
        var prRect = presetsRow.AddComponent<RectTransform>();
        prRect.anchoredPosition = new Vector2(0, -2);
        prRect.sizeDelta = new Vector2(480, 34);

        CreatePresetButton(presetsRow.transform, "Gentle", -160, out _gentleBtn, out _gentleImg);
        CreatePresetButton(presetsRow.transform, "Standard", 0, out _standardBtn, out _standardImg);
        CreatePresetButton(presetsRow.transform, "Deep", 160, out _deepBtn, out _deepImg);

        // Parameter summary text
        _paramSummaryText = ThemeTokens.CreateLabel(_mainCard.transform, "ParamSummary", "Calibrated Pace: Standard Exposure", 13, ThemeTokens.TextSecondary, TextAlignmentOptions.Center);
        var psRect = _paramSummaryText.GetComponent<RectTransform>();
        psRect.anchoredPosition = new Vector2(0, -38);
        psRect.sizeDelta = new Vector2(560, 22);

        // -------------------------------------------------------------
        // Primary Call to Action: [ Begin Session ]
        // -------------------------------------------------------------
        var beginObj = new GameObject("BeginSessionButton");
        beginObj.transform.SetParent(_mainCard.transform, false);

        var bImg = beginObj.AddComponent<Image>();
        bImg.color = ThemeTokens.AccentSage;

        _beginBtn = beginObj.AddComponent<Button>();
        _beginBtn.targetGraphic = bImg;
        ThemeTokens.StyleButtonColors(_beginBtn, ThemeTokens.AccentSage, ThemeTokens.AccentSageHover, ThemeTokens.AccentSage);
        _beginBtn.onClick.AddListener(LaunchCurrentSession);

        var bRect = beginObj.GetComponent<RectTransform>();
        bRect.anchoredPosition = new Vector2(0, -100);
        bRect.sizeDelta = new Vector2(400, 50);

        var bText = ThemeTokens.CreateLabel(beginObj.transform, "Label", "Begin Session", 18, ThemeTokens.TextPrimary, TextAlignmentOptions.Center, true);
        var btRect = bText.GetComponent<RectTransform>();
        btRect.anchorMin = Vector2.zero; btRect.anchorMax = Vector2.one;
        btRect.offsetMin = Vector2.zero; btRect.offsetMax = Vector2.zero;

        // -------------------------------------------------------------
        // Quiet Footer Anchors (History • Comfort/Settings • Exit)
        // -------------------------------------------------------------
        var footerObj = new GameObject("FooterRow");
        footerObj.transform.SetParent(_mainCard.transform, false);
        var fRect = footerObj.AddComponent<RectTransform>();
        fRect.anchoredPosition = new Vector2(0, -180);
        fRect.sizeDelta = new Vector2(580, 36);

        CreateFooterLink(footerObj.transform, "HistoryLink", "Session History", new Vector2(-150, 0), ShowHistoryView);
        CreateFooterDivider(footerObj.transform, new Vector2(-60, 0));
        CreateFooterLink(footerObj.transform, "SettingsLink", "Comfort & Audio", new Vector2(25, 0), ShowSettingsView);
        CreateFooterDivider(footerObj.transform, new Vector2(110, 0));
        CreateFooterLink(footerObj.transform, "ExitLink", "Exit", new Vector2(165, 0), Quit, ThemeTokens.GentleExitClay);

        // -------------------------------------------------------------
        // Build History & Settings Cards (Hidden by default)
        // -------------------------------------------------------------
        BuildHistoryCard();
        BuildSettingsCard();
    }

    private void CreatePresetButton(Transform parent, string label, float x, out Button btn, out Image img)
    {
        var btnObj = new GameObject($"Preset_{label}");
        btnObj.transform.SetParent(parent, false);

        img = btnObj.AddComponent<Image>();
        img.color = ThemeTokens.SurfaceInactive;

        btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = img;

        string presetName = label;
        btn.onClick.AddListener(() =>
        {
            TherapyAudioManager.Instance.PlayFeedbackClick();
            SelectPreset(presetName);
        });

        var rect = btnObj.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(x, 0);
        rect.sizeDelta = new Vector2(140, 32);

        var text = ThemeTokens.CreateLabel(btnObj.transform, "Label", label, 13, ThemeTokens.TextSecondary, TextAlignmentOptions.Center, true);
        var tRect = text.GetComponent<RectTransform>();
        tRect.anchorMin = Vector2.zero; tRect.anchorMax = Vector2.one;
        tRect.offsetMin = Vector2.zero; tRect.offsetMax = Vector2.zero;
    }

    private void CreateFooterLink(Transform parent, string name, string label, Vector2 pos, UnityEngine.Events.UnityAction action, Color? textColor = null)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);

        var btn = go.AddComponent<Button>();
        btn.onClick.AddListener(() =>
        {
            TherapyAudioManager.Instance.PlayFeedbackClick();
            action?.Invoke();
        });

        var text = ThemeTokens.CreateLabel(go.transform, "Text", label, 13, textColor ?? ThemeTokens.TextSecondary, TextAlignmentOptions.Center);
        var tRect = text.GetComponent<RectTransform>();
        tRect.sizeDelta = new Vector2(140, 30);

        var rect = go.GetComponent<RectTransform>();
        rect.anchoredPosition = pos;
        rect.sizeDelta = new Vector2(140, 30);
    }

    private void CreateFooterDivider(Transform parent, Vector2 pos)
    {
        var div = ThemeTokens.CreateLabel(parent, "Dot", "•", 13, ThemeTokens.TextMuted, TextAlignmentOptions.Center);
        var rect = div.GetComponent<RectTransform>();
        rect.anchoredPosition = pos;
        rect.sizeDelta = new Vector2(15, 30);
    }

    public void SelectTab(int index)
    {
        _activeTabIndex = Mathf.Clamp(index, 0, 2);

        for (int i = 0; i < 3; i++)
        {
            bool isSelected = (i == _activeTabIndex);
            _tabBackgrounds[i].color = isSelected ? ThemeTokens.AccentSage : ThemeTokens.SurfaceInactive;
            _tabTexts[i].color = isSelected ? ThemeTokens.TextPrimary : ThemeTokens.TextSecondary;
        }

        switch (_activeTabIndex)
        {
            case 0: // Claustrophobia
                _spaceTitleText.text = "Enclosed Space (Claustrophobia)";
                _spaceDescText.text = "A quiet room where walls contract gently at calibrated speeds, allowing you to practice slow diaphragmatic breathing and regain composure.";
                break;
            case 1: // Aquaphobia
                _spaceTitleText.text = "Rising Water (Aquaphobia)";
                _spaceDescText.text = "A calm shoreline where ocean water rises steadily to a safe threshold, supporting gradual desensitization at your own pace.";
                break;
            case 2: // Acrophobia
                _spaceTitleText.text = "High Elevation (Acrophobia)";
                _spaceDescText.text = "An open platform suspended high above a quiet city avenue. Looking downward into the view smoothly elevates you, rewarding confrontation.";
                break;
        }

        UpdateParamSummary();
    }

    public void SelectPreset(string preset)
    {
        _activePreset = preset;

        string phobiaType = _activeTabIndex switch
        {
            0 => "Claustrophobia",
            1 => "Aquaphobia",
            _ => "Acrophobia"
        };

        string actualPreset = (preset == "Deep") ? "Intense" : preset;
        TherapySettingsManager.Instance.ApplyPreset(phobiaType, actualPreset);

        // Highlight preset buttons
        _gentleImg.color = (preset == "Gentle") ? ThemeTokens.AccentSage : ThemeTokens.SurfaceInactive;
        _standardImg.color = (preset == "Standard") ? ThemeTokens.AccentSage : ThemeTokens.SurfaceInactive;
        _deepImg.color = (preset == "Deep") ? ThemeTokens.AccentSage : ThemeTokens.SurfaceInactive;

        UpdateParamSummary();
    }

    private void UpdateParamSummary()
    {
        var sm = TherapySettingsManager.Instance;
        if (sm == null) return;

        switch (_activeTabIndex)
        {
            case 0:
                _paramSummaryText.text = $"Pace: {_activePreset} • Contraction: {sm.clausSpeed * 100f:F1} cm/s • Interval: {sm.clausMoveAfter:F1}s";
                break;
            case 1:
                _paramSummaryText.text = $"Pace: {_activePreset} • Water Speed: {sm.aquaWaterSpeed * 100f:F1} cm/s • Interval: {sm.aquaRiseAfter:F1}s";
                break;
            case 2:
                _paramSummaryText.text = $"Pace: {_activePreset} • Look-Away Tolerance: {sm.acroWaitTime:F1}s • Gaze Hold: {sm.acroStillFocusTime:F1}s";
                break;
        }
    }

    public void LaunchCurrentSession()
    {
        TherapyAudioManager.Instance.PlayFeedbackClick();

        int targetScene = _activeTabIndex switch
        {
            0 => 1, // the last revelation.unity (Claustrophobia)
            1 => 2, // Island Example 3.unity (Aquaphobia)
            _ => 3  // Pack01Demo01.unity (Acrophobia)
        };

        VRFadeManager.Instance.FadeAndLoadScene(targetScene, 0.5f);
    }

    // -------------------------------------------------------------
    // In-place View Swapping: History Card
    // -------------------------------------------------------------
    private void BuildHistoryCard()
    {
        _historyCard = new GameObject("SanctuaryHistoryCard");
        _historyCard.transform.SetParent(_sanctuaryCanvas.transform, false);

        var cardImg = _historyCard.AddComponent<Image>();
        cardImg.color = ThemeTokens.SurfacePanel;
        var cardRect = _historyCard.GetComponent<RectTransform>();
        cardRect.sizeDelta = new Vector2(640, 480);
        cardRect.anchoredPosition = Vector2.zero;

        // Top Back Button: [ ← Back to Sessions ]
        var backBtn = CreateBackButton(_historyCard.transform, ShowMainPortal);

        // Header Title
        var title = ThemeTokens.CreateLabel(_historyCard.transform, "Title", "Session History", 20, ThemeTokens.TextPrimary, TextAlignmentOptions.Center, true);
        var tRect = title.GetComponent<RectTransform>();
        tRect.anchoredPosition = new Vector2(0, 185);
        tRect.sizeDelta = new Vector2(580, 30);

        // History Content
        _historyContentText = ThemeTokens.CreateLabel(_historyCard.transform, "HistoryContent", "", 13, ThemeTokens.TextSecondary, TextAlignmentOptions.TopLeft);
        var hRect = _historyContentText.GetComponent<RectTransform>();
        hRect.anchoredPosition = new Vector2(0, 0);
        hRect.sizeDelta = new Vector2(560, 300);

        _historyCard.SetActive(false);
    }

    public void ShowHistoryView()
    {
        _currentState = MenuState.History;
        _mainCard.SetActive(false);
        _settingsCard.SetActive(false);
        _historyCard.SetActive(true);

        string history = PatientSessionManager.Instance.GetFormattedHistoryForUI();
        _historyContentText.text = history;
    }

    // -------------------------------------------------------------
    // In-place View Swapping: Settings Card
    // -------------------------------------------------------------
    private void BuildSettingsCard()
    {
        _settingsCard = new GameObject("SanctuarySettingsCard");
        _settingsCard.transform.SetParent(_sanctuaryCanvas.transform, false);

        var cardImg = _settingsCard.AddComponent<Image>();
        cardImg.color = ThemeTokens.SurfacePanel;
        var cardRect = _settingsCard.GetComponent<RectTransform>();
        cardRect.sizeDelta = new Vector2(640, 480);
        cardRect.anchoredPosition = Vector2.zero;

        // Top Back Button
        CreateBackButton(_settingsCard.transform, ShowMainPortal);

        // Header Title
        var title = ThemeTokens.CreateLabel(_settingsCard.transform, "Title", "Comfort & Audio Settings", 20, ThemeTokens.TextPrimary, TextAlignmentOptions.Center, true);
        var tRect = title.GetComponent<RectTransform>();
        tRect.anchoredPosition = new Vector2(0, 185);
        tRect.sizeDelta = new Vector2(580, 30);

        var sm = TherapySettingsManager.Instance;

        // Toggle 1: Snap Turning
        CreateSettingsToggleRow(_settingsCard.transform, "SnapTurnRow", 115, "Snap Turning (45°)", sm.snapTurn ? "Enabled" : "Disabled", () =>
        {
            sm.SetSnapTurn(!sm.snapTurn);
            BuildSettingsCardRowsRefresh();
        });

        // Toggle 2: Seated Mode
        CreateSettingsToggleRow(_settingsCard.transform, "SeatedRow", 55, "Seated Mode Height Offset (+0.35m)", sm.seatedMode ? "Enabled" : "Disabled", () =>
        {
            sm.SetSeatedMode(!sm.seatedMode);
            BuildSettingsCardRowsRefresh();
        });

        // Toggle 3: Comfort Vignette
        CreateSettingsToggleRow(_settingsCard.transform, "VignetteRow", -5, "Motion Tunneling Vignette", sm.comfortVignetteEnabled ? "Enabled" : "Disabled", () =>
        {
            sm.SetComfortVignette(!sm.comfortVignetteEnabled);
            BuildSettingsCardRowsRefresh();
        });

        // Toggle 4: Audio Volume
        CreateSettingsToggleRow(_settingsCard.transform, "VolumeRow", -65, "Master Audio", (AudioListener.volume > 0.05f) ? "Normal" : "Muted", () =>
        {
            float nextVol = (AudioListener.volume > 0.05f) ? 0f : 0.9f;
            sm.SetMasterVolume(nextVol);
            BuildSettingsCardRowsRefresh();
        });

        _settingsCard.SetActive(false);
    }

    private void CreateSettingsToggleRow(Transform parent, string name, float y, string label, string state, UnityEngine.Events.UnityAction toggleAction)
    {
        var rowObj = new GameObject(name);
        rowObj.transform.SetParent(parent, false);
        var rRect = rowObj.AddComponent<RectTransform>();
        rRect.anchoredPosition = new Vector2(0, y);
        rRect.sizeDelta = new Vector2(540, 42);

        var bg = rowObj.AddComponent<Image>();
        bg.color = ThemeTokens.SurfaceInactive;

        var lbl = ThemeTokens.CreateLabel(rowObj.transform, "Label", label, 14, ThemeTokens.TextPrimary, TextAlignmentOptions.Left, true);
        var lRect = lbl.GetComponent<RectTransform>();
        lRect.anchoredPosition = new Vector2(20, 0);
        lRect.sizeDelta = new Vector2(340, 30);

        var btnObj = new GameObject("ToggleBtn");
        btnObj.transform.SetParent(rowObj.transform, false);
        var btnImg = btnObj.AddComponent<Image>();
        btnImg.color = ThemeTokens.SurfaceBorder;
        var btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = btnImg;
        btn.onClick.AddListener(() =>
        {
            TherapyAudioManager.Instance.PlayFeedbackClick();
            toggleAction?.Invoke();
        });

        var bRect = btnObj.GetComponent<RectTransform>();
        bRect.anchoredPosition = new Vector2(200, 0);
        bRect.sizeDelta = new Vector2(100, 30);

        var bText = ThemeTokens.CreateLabel(btnObj.transform, "Text", state, 13, ThemeTokens.TextPrimary, TextAlignmentOptions.Center, true);
        var btRect = bText.GetComponent<RectTransform>();
        btRect.anchorMin = Vector2.zero; btRect.anchorMax = Vector2.one;
        btRect.offsetMin = Vector2.zero; btRect.offsetMax = Vector2.zero;
    }

    private void BuildSettingsCardRowsRefresh()
    {
        if (_settingsCard != null)
        {
            Destroy(_settingsCard);
        }
        BuildSettingsCard();
        _settingsCard.SetActive(true);
    }

    private Button CreateBackButton(Transform parent, UnityEngine.Events.UnityAction action)
    {
        var backObj = new GameObject("BackButton");
        backObj.transform.SetParent(parent, false);

        var btn = backObj.AddComponent<Button>();
        btn.onClick.AddListener(() =>
        {
            TherapyAudioManager.Instance.PlayFeedbackClick();
            action?.Invoke();
        });

        var text = ThemeTokens.CreateLabel(backObj.transform, "Text", "← Back to Sessions", 13, ThemeTokens.AccentSage, TextAlignmentOptions.Left, true);
        var tRect = text.GetComponent<RectTransform>();
        tRect.sizeDelta = new Vector2(160, 30);

        var rect = backObj.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(-220, 215);
        rect.sizeDelta = new Vector2(160, 30);

        return btn;
    }

    public void ShowSettingsView()
    {
        _currentState = MenuState.Settings;
        _mainCard.SetActive(false);
        _historyCard.SetActive(false);
        _settingsCard.SetActive(true);
    }

    public void ShowMainPortal()
    {
        _currentState = MenuState.Portal;
        _historyCard.SetActive(false);
        _settingsCard.SetActive(false);
        _mainCard.SetActive(true);
    }

    // -------------------------------------------------------------
    // Legacy compatibility redirects
    // -------------------------------------------------------------
    public void PlayClaus() { SelectTab(0); LaunchCurrentSession(); }
    public void PlayAqua() { SelectTab(1); LaunchCurrentSession(); }
    public void PlayAcro() { SelectTab(2); LaunchCurrentSession(); }
    public void CLAUSSTAT() { SelectTab(0); ShowHistoryView(); }
    public void AQUASTAT() { SelectTab(1); ShowHistoryView(); }
    public void ACROSTAT() { SelectTab(2); ShowHistoryView(); }
    public void SelectGentlePreset(string phobiaType) { SelectPreset("Gentle"); }
    public void SelectStandardPreset(string phobiaType) { SelectPreset("Standard"); }
    public void SelectIntensePreset(string phobiaType) { SelectPreset("Deep"); }
    public void ToggleSnapTurn() { TherapySettingsManager.Instance.SetSnapTurn(!TherapySettingsManager.Instance.snapTurn); }
    public void ToggleComfortVignette() { TherapySettingsManager.Instance.SetComfortVignette(!TherapySettingsManager.Instance.comfortVignetteEnabled); }
    public void ToggleSeatedMode() { TherapySettingsManager.Instance.SetSeatedMode(!TherapySettingsManager.Instance.seatedMode); }
    public void ToggleSubtitles() { TherapySettingsManager.Instance.SetSubtitlesEnabled(!TherapySettingsManager.Instance.subtitlesEnabled); }

    public void Quit()
    {
        TherapyAudioManager.Instance.PlayFeedbackClick();

        if (VRFadeManager.Instance != null)
        {
            VRFadeManager.Instance.FadeOut(0.6f, () =>
            {
                Debug.Log("[Kanso VR] Application quit requested safely.");
                Application.Quit();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
            });
        }
        else
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
