using MRMotifs.PassthroughTransitioning;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    private enum ExperienceSelection
    {
        None,
        Planet,
        SolarSystem
    }

    // ----------- LEARN-PANEL -----------
    [Header("Learn-Panel: Root + UI-Texte")]
    [Tooltip("Das linke Panel mit Planet-Auswahl, Beschreibung und Planet-Buttons.")]
    public GameObject learnPanel;

    public TextMeshProUGUI menuHeadline;
    public TextMeshProUGUI descriptionText;
    public Image backgroundImage;

    [Tooltip("Text auf dem Start-Experience-Button im Learn-Panel.")]
    public TextMeshProUGUI startButtonLabel;

    // Neu: Optionaler Text fuer den Action-Button. Wenn leer, wird startButtonLabel benutzt.
    [SerializeField] private TMP_Text actionButtonText;

    // ----------- TEST-PANEL -----------
    [Header("Test-Panel: Root + Daten")]
    [Tooltip("Das rechte Panel fuer das Sonnensystem.")]
    public GameObject testPanel;

    [Tooltip("Das Prefab des kompletten Sonnensystems, das platziert werden soll.")]
    public GameObject sonnensystemPrefab;

    public TextMeshProUGUI sonnensystemHeadline;
    public TextMeshProUGUI sonnensystemDescription;

    [Header("Test-Panel: Quiz-Buttons")]
    [Tooltip("Optional: Button fuer das Reihenfolge-Quiz. Wird sonst ueber den Namen Button_Reihenfolge gesucht.")]
    public Button reihenfolgeButton;

    [Tooltip("Optional: Text auf dem Reihenfolge-Button.")]
    public TextMeshProUGUI reihenfolgeButtonLabel;

    [Tooltip("Optional: Button fuer das Size-Quiz. Wird sonst ueber den Namen Button_Size gesucht.")]
    public Button sizeButton;

    [Tooltip("Optional: Text auf dem Size-Button.")]
    public TextMeshProUGUI sizeButtonLabel;

    [Tooltip("Optional: Button fuer Gravity. Wird sonst ueber den Namen Button_Gravity gesucht.")]
    public Button gravityButton;

    [Tooltip("Optional: Text auf dem Gravity-Button.")]
    public TextMeshProUGUI gravityButtonLabel;

    // ----------- TAB-BUTTONS -----------
    [Header("Tabs (Learn / Sonnensystem)")]
    [Tooltip("Visuelles Highlight fuer den aktiven/inaktiven Tab.")]
    public Image learnTabBackground;
    public Image testTabBackground;
    public Color activeTabColor = new Color(1f, 1f, 1f, 0.4f);
    public Color inactiveTabColor = new Color(1f, 1f, 1f, 0f);

    [Header("Neues MenuRoot")]
    [Tooltip("Root des neuen finalen Menues. Wenn leer, wird GameManager.mainMenuCanvas oder ein Objekt mit dem Namen MenuRoot verwendet.")]
    [SerializeField] private GameObject menuRoot;

    [Tooltip("Verdrahtet Buttons im neuen MenuRoot automatisch mit diesem Controller.")]
    [SerializeField] private bool autoBindMenuRoot = true;

    [Tooltip("PlanetData-Assets in der Reihenfolge der Planet-Buttons im neuen MenuRoot.")]
    [SerializeField] private List<PlanetData> menuPlanetData = new List<PlanetData>();

    // ----------- REFERENZEN -----------
    [Header("Referenzen")]
    public PlacementManager placementManager;
    public MinigameManager minigameManager;

    [Header("Passthrough / VR")]
    [Tooltip("Toggle im MainMenu, der zwischen Passthrough und VR wechselt.")]
    [SerializeField] private Toggle _passthroughModeToggle;

    [Tooltip("Dissolver aus MR Motif #1. Wenn leer, wird er automatisch in der Szene gesucht.")]
    [SerializeField] private PassthroughDissolver _passthroughDissolver;

    [Tooltip("Startzustand: aktiv = Passthrough, inaktiv = VR.")]
    [SerializeField] private bool _isPassthroughOnAtStart;

    [Header("Planetengroesse im Placement")]
    [Tooltip("Toggle im MainMenu: aktiv = relative Planetengroessen, inaktiv = alle Einzelplaneten 50 cm Durchmesser.")]
    [SerializeField] private Toggle _planetSizeModeToggle;
    [Tooltip("Fallback, falls der Planetengroessen-Modus als Button statt Toggle gebaut ist.")]
    [SerializeField] private Button _planetSizeModeButton;
    [Tooltip("Hintergrund des selbstgebauten Toggles, z.B. ToggleBG.")]
    [SerializeField] private Graphic _planetSizeModeBackground;
    [Tooltip("Beweglicher Knopf des selbstgebauten Toggles, z.B. ToggleHandle.")]
    [SerializeField] private RectTransform _planetSizeModeHandle;
    [Tooltip("Position des ToggleHandle, wenn relative Planetengroessen aktiv sind.")]
    [SerializeField] private float _planetSizeHandleOnX = -4f;
    [Tooltip("Position des ToggleHandle, wenn alle Planeten 50 cm Durchmesser haben.")]
    [SerializeField] private float _planetSizeHandleOffX = -24f;
    [SerializeField] private Color _planetSizeModeOnColor = new Color(0.188f, 0.82f, 0.345f, 1f);
    [SerializeField] private Color _planetSizeModeOffColor = new Color(1f, 1f, 1f, 0.4f);
    [Tooltip("Startzustand: aktiv = Planetengroessen bleiben wie bisher.")]
    [SerializeField] private bool _useRelativePlanetSizesAtStart = true;

    private PlanetData _currentPlanet;
    private ExperienceSelection _currentSelection = ExperienceSelection.None;
    private string _reihenfolgeBaseLabel = "Reihenfolge";
    private string _sizeBaseLabel = "Size";
    private string _gravityBaseLabel = "Gravity (Coming Soon)";
    private Color _learnTabVisibleColor;
    private Color _testTabVisibleColor;
    private bool _hasCachedTabColors;
    private Transform _startExperienceButton;
    private Transform _startExperiencePanelRoot;
    private Transform _planetSizeModeRoot;

    void Start()
    {
        AutoBindMenuRoot();
        CacheQuizButtonReferences();
        CacheTabVisibleColors();
        SetupPassthroughToggle();
        SetupPlanetSizeModeToggle();
        ShowLearnTab();
    }

    private void OnDestroy()
    {
        if (_passthroughModeToggle != null)
        {
            _passthroughModeToggle.onValueChanged.RemoveListener(SetPassthroughMode);
        }

        if (_planetSizeModeToggle != null)
        {
            _planetSizeModeToggle.onValueChanged.RemoveListener(SetRelativePlanetSizeMode);
        }

        if (_planetSizeModeButton != null)
        {
            _planetSizeModeButton.onClick.RemoveListener(ToggleRelativePlanetSizeMode);
        }
    }

    // =================================================================
    //                           TAB-WECHSEL
    // =================================================================

    // Wird vom Tab-Button "Learn" aufgerufen.
    public void ShowLearnTab()
    {
        if (learnPanel != null) learnPanel.SetActive(true);
        if (testPanel != null) testPanel.SetActive(false);

        // Neu: Platzierte Objekte bleiben beim Tabwechsel erhalten.
        // Geloescht wird erst beim echten Platzieren im PlacementManager.
        UpdateTabHighlight(true);
    }

    // Wird vom Tab-Button "Sonnensystem" aufgerufen.
    public void ShowTestTab()
    {
        if (learnPanel != null) learnPanel.SetActive(false);
        if (testPanel != null) testPanel.SetActive(true);

        UpdateTabHighlight(false);
        RefreshMinigameButtons();
    }

    private void UpdateTabHighlight(bool learnActive)
    {
        CacheTabVisibleColors();
        SetTabBackgroundVisible(learnTabBackground, _learnTabVisibleColor, learnActive);
        SetTabBackgroundVisible(testTabBackground, _testTabVisibleColor, learnActive == false);
    }

    private void CacheTabVisibleColors()
    {
        if (_hasCachedTabColors) return;

        _learnTabVisibleColor = GetVisibleTabColor(learnTabBackground);
        _testTabVisibleColor = GetVisibleTabColor(testTabBackground);
        _hasCachedTabColors = true;
    }

    private Color GetVisibleTabColor(Image tabBackground)
    {
        // Active State in der finalen Navigation: #FFFFFF mit 40 Prozent Deckkraft.
        return activeTabColor;
    }

    private void SetTabBackgroundVisible(Image tabBackground, Color visibleColor, bool isVisible)
    {
        if (tabBackground == null) return;

        Color color = visibleColor;
        color.a = isVisible ? visibleColor.a : inactiveTabColor.a;
        tabBackground.color = color;

        // Wichtig: Das Image bleibt aktiv, damit der Button weiter Ray/Poke-Events bekommt.
        tabBackground.raycastTarget = true;
    }

    // =================================================================
    //                       PLANETEN-AUSWAHL (Learn)
    // =================================================================

    // Wird vom PlanetMenuButton aufgerufen, wenn ein Planet ausgewaehlt wird.
    public void SelectPlanet(PlanetData data)
    {
        if (data == null)
        {
            Debug.LogWarning("MainMenuController: Kein PlanetData am Button hinterlegt.");
            return;
        }

        _currentPlanet = data;
        _currentSelection = ExperienceSelection.Planet;

        if (menuHeadline != null)
        {
            menuHeadline.text = data.planetName;
        }

        if (descriptionText != null)
        {
            descriptionText.text = data.beschreibung;
        }

        if (backgroundImage != null && data.planetImage != null)
        {
            backgroundImage.sprite = data.planetImage;
        }

        ShowStartExperienceButton();
        SetActionButtonText("Discover " + data.planetName);
    }

    // Wird vom Sonnensystem-Button/Tab aufgerufen.
    public void SelectSolarSystem()
    {
        _currentPlanet = null;
        _currentSelection = ExperienceSelection.SolarSystem;

        if (placementManager != null)
        {
            placementManager.DeactivatePlanetSun();
        }

        if (menuHeadline != null)
        {
            menuHeadline.text = sonnensystemHeadline != null ? sonnensystemHeadline.text : "Sonnensystem";
        }

        if (descriptionText != null)
        {
            descriptionText.text = sonnensystemDescription != null
                ? sonnensystemDescription.text
                : "Platziere das komplette Sonnensystem im Raum.";
        }

        ShowStartExperienceButton();
        SetActionButtonText("Discover Solar System");
    }

    private void AutoBindMenuRoot()
    {
        if (autoBindMenuRoot == false) return;

        Transform root = GetMenuRootTransform();
        if (root == null) return;

        Transform mainMenu = FindDeepChild(root, "MainMenu");
        Transform mainPanel = FindDirectChild(mainMenu, "MainPanel");
        Transform learnPanelTransform = FindDirectChild(mainMenu, "LearnPanel");

        if (mainPanel != null)
        {
            learnPanel = mainPanel.gameObject;
            menuHeadline = FindTextMeshPro(mainPanel, "Headline", menuHeadline);
            descriptionText = FindTextMeshPro(mainPanel, "Description", descriptionText);
            _startExperienceButton = FindDeepChild(mainPanel, "Button_Primary");
            if (_startExperienceButton == null)
            {
                _startExperienceButton = FindDeepChild(mainPanel, "PrimaryButton_IconAndLabel_UnityUIButton");
            }

            _startExperiencePanelRoot = FindStartExperiencePanelRoot(mainPanel, _startExperienceButton);
            AutoBindPlanetSizeModeToggle(_startExperiencePanelRoot);
            actionButtonText = FindText(_startExperienceButton, null, actionButtonText);

            TextMeshProUGUI actionButtonLabel = actionButtonText as TextMeshProUGUI;
            if (actionButtonLabel != null)
            {
                startButtonLabel = actionButtonLabel;
            }

            BindStartExperienceButton();
            HideStartExperienceButton();
            BindPlanetButtons(mainPanel);
            BindSolarSystemButton(mainPanel);
        }

        if (learnPanelTransform != null)
        {
            testPanel = learnPanelTransform.gameObject;
            BindChallengeButtons(learnPanelTransform);
        }

        Transform navigation = FindDeepChild(root, "Navigation");
        learnTabBackground = FindImage(FindDeepChild(navigation, "Tab Button Home"), learnTabBackground);
        testTabBackground = FindImage(FindDeepChild(navigation, "Tab Button Learn"), testTabBackground);
        BindButtonByName(navigation, "Tab Button Home", ShowLearnTab);
        BindButtonByName(navigation, "Tab Button Learn", ShowTestTab);

        if (_passthroughModeToggle == null)
        {
            _passthroughModeToggle = FindDeepChild(root, "Passthrough")?.GetComponent<Toggle>();
        }
    }

    private Transform GetMenuRootTransform()
    {
        if (menuRoot != null) return menuRoot.transform;

        if (GameManager.Instance != null && GameManager.Instance.mainMenuCanvas != null)
        {
            menuRoot = GameManager.Instance.mainMenuCanvas;
            return menuRoot.transform;
        }

        GameObject foundRoot = GameObject.Find("MenuRoot");
        if (foundRoot == null) return null;

        menuRoot = foundRoot;
        return menuRoot.transform;
    }

    private void BindStartExperienceButton()
    {
        if (_startExperienceButton == null) return;

        Button button = _startExperienceButton.GetComponent<Button>();
        if (button == null) button = _startExperienceButton.GetComponentInChildren<Button>(true);

        BindClick(button, StartExperience);
    }

    private void HideStartExperienceButton()
    {
        SetStartExperienceButtonVisible(false);
    }

    private void ShowStartExperienceButton()
    {
        SetStartExperienceButtonVisible(true);
    }

    private void SetStartExperienceButtonVisible(bool isVisible)
    {
        if (_startExperienceButton == null) return;

        Transform visibleRoot = _startExperiencePanelRoot != null
            ? _startExperiencePanelRoot
            : _startExperienceButton;

        visibleRoot.gameObject.SetActive(isVisible);

        if (visibleRoot != _startExperienceButton)
        {
            _startExperienceButton.gameObject.SetActive(true);
        }

        SetPlanetSizeModeVisible(isVisible && _currentSelection == ExperienceSelection.Planet);
    }

    private void BindPlanetButtons(Transform mainPanel)
    {
        if (menuPlanetData == null || menuPlanetData.Count == 0) return;

        Transform planetsContainer = FindDeepChild(mainPanel, "PlanetsContainer");
        Transform content = FindDeepChild(planetsContainer, "Content");
        if (content == null) return;

        Button[] buttons = GetDirectChildButtons(content);
        int count = Mathf.Min(buttons.Length, menuPlanetData.Count);

        for (int i = 0; i < count; i++)
        {
            PlanetData data = menuPlanetData[i];
            Button button = buttons[i];
            if (button == null || data == null) continue;

            SetButtonTexts(button.transform, data.planetName, data.subHeadline);
            button.onClick.AddListener(() => SelectPlanet(data));
        }
    }

    private void BindSolarSystemButton(Transform mainPanel)
    {
        Transform solarSystemContainer = FindDeepChild(mainPanel, "PlanetsContainer (1)");
        Transform content = FindDeepChild(solarSystemContainer, "Content");
        if (content == null) return;

        Button[] buttons = GetDirectChildButtons(content);
        if (buttons.Length == 0) return;

        Button button = buttons[0];
        SetButtonTexts(button.transform, "Sonnensystem", "Alle Planeten");
        BindClick(button, SelectSolarSystem);
    }

    private Transform FindStartExperiencePanelRoot(Transform mainPanel, Transform startButton)
    {
        if (startButton == null) return null;

        Transform current = startButton.parent;
        while (current != null && current != mainPanel)
        {
            if (ContainsToggle(current) || FindDeepChild(current, "ToggleHandle") != null)
            {
                return current;
            }

            current = current.parent;
        }

        return startButton;
    }

    private bool ContainsToggle(Transform root)
    {
        if (root == null) return false;

        Toggle toggle = root.GetComponentInChildren<Toggle>(true);
        return toggle != null;
    }

    private void AutoBindPlanetSizeModeToggle(Transform actionRoot)
    {
        if (actionRoot == null) return;

        if (_planetSizeModeToggle == null)
        {
            _planetSizeModeToggle = actionRoot.GetComponentInChildren<Toggle>(true);
        }

        if (_planetSizeModeRoot == null)
        {
            _planetSizeModeRoot = FindPlanetSizeModeRoot(actionRoot);
        }

        Transform visualRoot = _planetSizeModeRoot != null
            ? _planetSizeModeRoot
            : _planetSizeModeToggle != null
                ? _planetSizeModeToggle.transform
                : actionRoot;

        if (_planetSizeModeBackground == null)
        {
            _planetSizeModeBackground = FindGraphicWithName(visualRoot, "ToggleBG");
        }

        if (_planetSizeModeHandle == null)
        {
            _planetSizeModeHandle = FindRectTransformWithName(visualRoot, "ToggleHandle");
        }

        if (_planetSizeModeToggle == null && _planetSizeModeButton == null)
        {
            _planetSizeModeButton = FindButtonForPlanetSizeToggle(visualRoot);
        }
    }

    private Transform FindPlanetSizeModeRoot(Transform actionRoot)
    {
        if (_planetSizeModeToggle != null)
        {
            return FindClosestToggleVisualRoot(actionRoot, _planetSizeModeToggle.transform);
        }

        RectTransform handle = FindRectTransformWithName(actionRoot, "ToggleHandle");
        if (handle != null)
        {
            return FindClosestToggleVisualRoot(actionRoot, handle);
        }

        return null;
    }

    private Transform FindClosestToggleVisualRoot(Transform limitRoot, Transform child)
    {
        Transform current = child;
        while (current != null && current != limitRoot.parent)
        {
            if (current == limitRoot) break;

            if (FindDeepChild(current, "ToggleHandle") != null || FindDeepChild(current, "ToggleBG") != null)
            {
                return current;
            }

            current = current.parent;
        }

        return child;
    }

    private Graphic FindGraphicWithName(Transform root, string objectName)
    {
        RectTransform rectTransform = FindRectTransformWithName(root, objectName);
        if (rectTransform == null) return null;

        return rectTransform.GetComponent<Graphic>();
    }

    private RectTransform FindRectTransformWithName(Transform root, string objectName)
    {
        if (root == null || string.IsNullOrWhiteSpace(objectName)) return null;

        RectTransform[] rectTransforms = root.GetComponentsInChildren<RectTransform>(true);
        for (int i = 0; i < rectTransforms.Length; i++)
        {
            if (rectTransforms[i] != null && rectTransforms[i].name == objectName)
            {
                return rectTransforms[i];
            }
        }

        return null;
    }

    private Button FindButtonForPlanetSizeToggle(Transform visualRoot)
    {
        if (visualRoot == null) return null;

        Button button = visualRoot.GetComponent<Button>();
        if (button != null) return button;

        if (_planetSizeModeHandle != null)
        {
            button = _planetSizeModeHandle.GetComponent<Button>();
            if (button != null) return button;
            button = _planetSizeModeHandle.GetComponentInParent<Button>();
            if (button != null && button.transform.IsChildOf(visualRoot)) return button;
        }

        return visualRoot.GetComponentInChildren<Button>(true);
    }

    private void BindChallengeButtons(Transform learnPanelTransform)
    {
        Transform challengesContainer = FindDeepChild(learnPanelTransform, "ChallengesContainer");
        if (challengesContainer == null) return;

        Button[] buttons = GetDirectChildButtons(challengesContainer);

        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = buttons[i];
            if (button == null) continue;

            string text = GetCombinedButtonText(button.transform).ToLowerInvariant();

            if (IsDebugResetChallenge(button.transform, text, i))
            {
                gravityButton = button;
                gravityButtonLabel = button.GetComponentInChildren<TextMeshProUGUI>(true);
                CacheChallengeBaseLabel(gravityButtonLabel, ref _gravityBaseLabel);
                BindChallengeStart(button, ResetQuizProgress);
                continue;
            }

            if (text.Contains("size") || text.Contains("groesse") || text.Contains("größe"))
            {
                sizeButton = button;
                sizeButtonLabel = button.GetComponentInChildren<TextMeshProUGUI>(true);
                CacheChallengeBaseLabel(sizeButtonLabel, ref _sizeBaseLabel);
                BindChallengeStart(button, StartSizeMinigame);
                continue;
            }

            if (text.Contains("arrange") || text.Contains("order") || text.Contains("reihenfolge"))
            {
                reihenfolgeButton = button;
                reihenfolgeButtonLabel = button.GetComponentInChildren<TextMeshProUGUI>(true);
                CacheChallengeBaseLabel(reihenfolgeButtonLabel, ref _reihenfolgeBaseLabel);
                BindChallengeStart(button, StartReihenfolgeMinigame);
                continue;
            }

            gravityButton = button;
            gravityButtonLabel = button.GetComponentInChildren<TextMeshProUGUI>(true);
            CacheChallengeBaseLabel(gravityButtonLabel, ref _gravityBaseLabel);
            BindChallengeStart(button, ResetQuizProgress);
        }
    }

    private void CacheChallengeBaseLabel(TextMeshProUGUI label, ref string baseLabel)
    {
        if (label == null || string.IsNullOrWhiteSpace(label.text)) return;

        baseLabel = label.text.Replace(" (geschafft)", "");
    }

    private bool IsDebugResetChallenge(Transform challengeTransform, string combinedText, int index)
    {
        if (index == 2) return true;
        if (challengeTransform == null) return false;

        string objectName = challengeTransform.name.ToLowerInvariant();
        return objectName.Contains("challengebutton (3)")
            || combinedText.Contains("gravity")
            || combinedText.Contains("schwerkraft")
            || combinedText.Contains("test 3");
    }

    private void BindChallengeStart(Button challengeButton, UnityEngine.Events.UnityAction action)
    {
        if (challengeButton == null || action == null) return;

        BindClick(challengeButton, action);

        Transform startButtonTransform = FindDeepChild(challengeButton.transform, "StartButton");
        if (startButtonTransform == null) return;

        Button startButton = startButtonTransform.GetComponent<Button>();
        if (startButton == null) startButton = startButtonTransform.GetComponentInChildren<Button>(true);

        BindClick(startButton, action);
    }

    private void BindButtonByName(Transform root, string buttonName, UnityEngine.Events.UnityAction action)
    {
        Transform buttonTransform = FindDeepChild(root, buttonName);
        if (buttonTransform == null) return;

        Button button = buttonTransform.GetComponent<Button>();
        if (button == null) button = buttonTransform.GetComponentInChildren<Button>(true);
        if (button == null) return;

        BindClick(button, action);
    }

    private void BindClick(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null || action == null) return;

        button.onClick.RemoveListener(action);
        button.onClick.AddListener(action);
    }

    private Button[] GetDirectChildButtons(Transform parent)
    {
        List<Button> buttons = new List<Button>();
        if (parent == null) return buttons.ToArray();

        for (int i = 0; i < parent.childCount; i++)
        {
            Button button = parent.GetChild(i).GetComponent<Button>();
            if (button != null)
            {
                buttons.Add(button);
            }
        }

        return buttons.ToArray();
    }

    private void SetButtonTexts(Transform buttonTransform, string title, string subtitle)
    {
        if (buttonTransform == null) return;

        TMP_Text[] texts = buttonTransform.GetComponentsInChildren<TMP_Text>(true);
        if (texts.Length > 0) texts[0].text = title;
        if (texts.Length > 1) texts[1].text = subtitle;
    }

    private string GetCombinedButtonText(Transform buttonTransform)
    {
        if (buttonTransform == null) return "";

        TMP_Text[] texts = buttonTransform.GetComponentsInChildren<TMP_Text>(true);
        string combinedText = "";

        for (int i = 0; i < texts.Length; i++)
        {
            if (texts[i] != null)
            {
                combinedText += " " + texts[i].text;
            }
        }

        return combinedText;
    }

    private TMP_Text FindText(Transform root, string childName, TMP_Text fallback)
    {
        Transform target = string.IsNullOrWhiteSpace(childName) ? root : FindDeepChild(root, childName);
        if (target == null) return fallback;

        TMP_Text text = target.GetComponent<TMP_Text>();
        if (text == null) text = target.GetComponentInChildren<TMP_Text>(true);
        return text != null ? text : fallback;
    }

    private TextMeshProUGUI FindTextMeshPro(Transform root, string childName, TextMeshProUGUI fallback)
    {
        TMP_Text text = FindText(root, childName, fallback);
        TextMeshProUGUI textMeshPro = text as TextMeshProUGUI;
        return textMeshPro != null ? textMeshPro : fallback;
    }

    private Image FindImage(Transform root, Image fallback)
    {
        if (root == null) return fallback;

        Image image = root.GetComponent<Image>();
        return image != null ? image : fallback;
    }

    private Transform FindDirectChild(Transform parent, string childName)
    {
        if (parent == null) return null;

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.name == childName) return child;
        }

        return null;
    }

    private Transform FindDeepChild(Transform parent, string childName)
    {
        if (parent == null || string.IsNullOrWhiteSpace(childName)) return null;

        if (parent.name == childName) return parent;

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform result = FindDeepChild(parent.GetChild(i), childName);
            if (result != null) return result;
        }

        return null;
    }

    // =================================================================
    //                       EXPERIENCE STARTEN
    // =================================================================

    // Wird vom Start-Experience-Button aufgerufen.
    public void StartExperience()
    {
        if (placementManager == null)
        {
            Debug.LogWarning("MainMenuController: PlacementManager ist nicht zugewiesen.");
            return;
        }

        if (_currentSelection == ExperienceSelection.SolarSystem)
        {
            StartSolarSystemPlacement();
            return;
        }

        if (_currentSelection == ExperienceSelection.Planet && _currentPlanet != null)
        {
            placementManager.SelectPlanet(_currentPlanet);
            StartPlacementState();
            return;
        }

        Debug.LogWarning("Keine Experience ausgewaehlt - bitte erst Planet oder Sonnensystem antippen.");
    }

    // Alte Button-Verknuepfung bleibt funktionsfaehig.
    public void StartSolarSystemExperience()
    {
        SelectSolarSystem();
        StartExperience();
    }

    // =================================================================
    //                       MINISPIELE (Test)
    // =================================================================

    public void StartReihenfolgeMinigame()
    {
        Debug.Log("MainMenuController: Reihenfolge-Button wurde geklickt.");
        StartMinigame(MinigameType.Reihenfolge);
    }

    public void StartSizeMinigame()
    {
        Debug.Log("MainMenuController: Size-Button wurde geklickt.");
        StartMinigame(MinigameType.Size);
    }

    public void ShowGravityComingSoon()
    {
        Debug.Log("MainMenuController: Gravity-Button wurde geklickt.");
        StartMinigame(MinigameType.Gravity);
    }

    // Wird vom Reset-Button im Hauptmenue aufgerufen.
    // Setzt den Quiz-Fortschritt zurueck, als waeren beide Quizzes noch nicht geloest.
    public void ResetActiveMinigame()
    {
        ResetQuizProgress();
    }

    public void ResetQuizProgress()
    {
        MinigameManager manager = GetMinigameManager();
        if (manager == null) return;

        manager.ResetQuizProgress();
    }

    // Optionaler Zurueck-Button im Hauptmenue, falls das Minispiel beendet werden soll.
    public void EndActiveMinigame()
    {
        MinigameManager manager = GetMinigameManager();
        if (manager == null) return;

        manager.EndMinigame();
    }

    // Wird spaeter von den Checker-Scripts aufgerufen, wenn ein Quiz geloest wurde.
    public void CompleteActiveMinigame()
    {
        MinigameManager manager = GetMinigameManager();
        if (manager == null) return;

        manager.CompleteCurrentMinigame();
    }

    public void RefreshMinigameButtons()
    {
        CacheQuizButtonReferences();

        MinigameManager manager = GetMinigameManager();
        bool isReihenfolgeCompleted = manager != null && manager.IsMinigameCompleted(MinigameType.Reihenfolge);
        bool isSizeCompleted = manager != null && manager.IsMinigameCompleted(MinigameType.Size);

        UpdateQuizButton(reihenfolgeButton, reihenfolgeButtonLabel, _reihenfolgeBaseLabel, isReihenfolgeCompleted, true);
        UpdateQuizButton(sizeButton, sizeButtonLabel, _sizeBaseLabel, isSizeCompleted, true);
        UpdateDebugResetButton(gravityButton, gravityButtonLabel, _gravityBaseLabel);
    }

    private void StartMinigame(MinigameType type)
    {
        MinigameManager manager = GetMinigameManager();
        if (manager == null)
        {
            Debug.LogWarning("MainMenuController: Kein MinigameManager in der Szene gefunden.");
            return;
        }

        manager.StartMinigame(type);
    }

    private MinigameManager GetMinigameManager()
    {
        return minigameManager != null
            ? minigameManager
            : MinigameManager.Instance;
    }

    private void CacheQuizButtonReferences()
    {
        if (testPanel == null) return;

        CacheButtonReference("Button_Reihenfolge", ref reihenfolgeButton, ref reihenfolgeButtonLabel, ref _reihenfolgeBaseLabel);
        CacheButtonReference("Button_Size", ref sizeButton, ref sizeButtonLabel, ref _sizeBaseLabel);
        CacheButtonReference("Button_Gravity", ref gravityButton, ref gravityButtonLabel, ref _gravityBaseLabel);
    }

    private void CacheButtonReference(string buttonName, ref Button button, ref TextMeshProUGUI label, ref string baseLabel)
    {
        if (button == null)
        {
            Transform buttonTransform = testPanel.transform.Find(buttonName);
            if (buttonTransform != null)
            {
                button = buttonTransform.GetComponent<Button>();
            }
        }

        if (label == null && button != null)
        {
            label = button.GetComponentInChildren<TextMeshProUGUI>(true);
        }

        if (label != null && string.IsNullOrWhiteSpace(label.text) == false)
        {
            string currentText = label.text.Replace(" (geschafft)", "");
            baseLabel = currentText;
        }
    }

    private void UpdateQuizButton(Button button, TextMeshProUGUI label, string baseLabel, bool isCompleted, bool canStart)
    {
        if (label != null)
        {
            label.text = baseLabel;
        }

        if (button == null) return;

        button.interactable = canStart;
        SetChallengeCompletionVisual(button.transform, isCompleted, canStart);
    }

    private void UpdateDebugResetButton(Button button, TextMeshProUGUI label, string baseLabel)
    {
        if (label != null)
        {
            label.text = baseLabel;
        }

        if (button == null) return;

        button.gameObject.SetActive(true);
        button.interactable = true;
        SetChallengeCompletionVisual(button.transform, false, true);
    }

    private void SetChallengeCompletionVisual(Transform challengeTransform, bool isCompleted, bool canStart)
    {
        if (challengeTransform == null) return;

        Transform startButton = FindDirectChild(challengeTransform, "StartButton");
        if (startButton == null) startButton = FindDeepChild(challengeTransform, "StartButton");

        Transform check = FindDirectChild(challengeTransform, "Check");
        if (check == null) check = FindDeepChild(challengeTransform, "Check");

        if (startButton != null)
        {
            startButton.gameObject.SetActive(canStart && isCompleted == false);
        }

        if (check != null)
        {
            check.gameObject.SetActive(canStart && isCompleted);
        }
    }

    private void StartSolarSystemPlacement()
    {
        if (sonnensystemPrefab == null)
        {
            Debug.LogWarning("Kein Sonnensystem-Prefab im MainMenuController hinterlegt.");
            return;
        }

        placementManager.SelectSolarSystem(sonnensystemPrefab);
        StartPlacementState();
    }

    private void StartPlacementState()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetState(GameState.PLACEMENT);
        }
    }

    private void SetActionButtonText(string newText)
    {
        // Neu: Der Button-Text kann ueber die neue Referenz oder ueber die alte Referenz laufen.
        TMP_Text targetText = actionButtonText != null ? actionButtonText : startButtonLabel;

        if (targetText != null)
        {
            targetText.text = newText;
        }
    }

    private void SetupPassthroughToggle()
    {
        if (_passthroughDissolver == null)
        {
            _passthroughDissolver = FindFirstObjectByType<PassthroughDissolver>();
        }

        if (_passthroughModeToggle != null)
        {
            _passthroughModeToggle.SetIsOnWithoutNotify(_isPassthroughOnAtStart);
            _passthroughModeToggle.onValueChanged.AddListener(SetPassthroughMode);
        }

        SetPassthroughMode(_isPassthroughOnAtStart);
    }

    // Kann direkt im Toggle unter On Value Changed (bool) eingetragen werden.
    public void SetPassthroughMode(bool isActive)
    {
        if (_passthroughDissolver == null)
        {
            _passthroughDissolver = FindFirstObjectByType<PassthroughDissolver>();
        }

        if (_passthroughDissolver == null)
        {
            Debug.LogWarning("MainMenuController: Kein PassthroughDissolver in der Szene gefunden.");
            return;
        }

        _passthroughDissolver.SetPassthroughActive(isActive);
    }

    private void SetupPlanetSizeModeToggle()
    {
        if (placementManager == null)
        {
            placementManager = FindFirstObjectByType<PlacementManager>();
        }

        bool useRelativeSizes = _planetSizeModeToggle != null
            ? _planetSizeModeToggle.isOn
            : _useRelativePlanetSizesAtStart;

        if (placementManager != null)
        {
            placementManager.SetUseRelativePlanetSizes(useRelativeSizes);
        }

        if (_planetSizeModeToggle != null)
        {
            _planetSizeModeToggle.SetIsOnWithoutNotify(useRelativeSizes);
            _planetSizeModeToggle.onValueChanged.RemoveListener(SetRelativePlanetSizeMode);
            _planetSizeModeToggle.onValueChanged.AddListener(SetRelativePlanetSizeMode);
        }

        if (_planetSizeModeToggle == null && _planetSizeModeButton != null)
        {
            _planetSizeModeButton.onClick.RemoveListener(ToggleRelativePlanetSizeMode);
            _planetSizeModeButton.onClick.AddListener(ToggleRelativePlanetSizeMode);
        }

        UpdatePlanetSizeModeVisual(useRelativeSizes);
        SetPlanetSizeModeVisible(false);
    }

    // Kann direkt im Toggle unter On Value Changed (bool) eingetragen werden.
    public void SetRelativePlanetSizeMode(bool useRelativeSizes)
    {
        if (placementManager == null)
        {
            placementManager = FindFirstObjectByType<PlacementManager>();
        }

        if (placementManager != null)
        {
            placementManager.SetUseRelativePlanetSizes(useRelativeSizes);
        }

        if (_planetSizeModeToggle != null)
        {
            _planetSizeModeToggle.SetIsOnWithoutNotify(useRelativeSizes);
        }

        UpdatePlanetSizeModeVisual(useRelativeSizes);
    }

    public void ToggleRelativePlanetSizeMode()
    {
        bool useRelativeSizes = _planetSizeModeToggle != null
            ? _planetSizeModeToggle.isOn == false
            : placementManager == null || placementManager.UsesRelativePlanetSizes() == false;

        SetRelativePlanetSizeMode(useRelativeSizes);
    }

    private void SetPlanetSizeModeVisible(bool isVisible)
    {
        Transform visibleRoot = _planetSizeModeRoot != null
            ? _planetSizeModeRoot
            : _planetSizeModeToggle != null
                ? _planetSizeModeToggle.transform
                : null;

        if (visibleRoot == null) return;

        visibleRoot.gameObject.SetActive(isVisible);
    }

    private void UpdatePlanetSizeModeVisual(bool useRelativeSizes)
    {
        if (_planetSizeModeHandle != null)
        {
            Vector2 position = _planetSizeModeHandle.anchoredPosition;
            position.x = useRelativeSizes ? _planetSizeHandleOnX : _planetSizeHandleOffX;
            _planetSizeModeHandle.anchoredPosition = position;
        }

        if (_planetSizeModeBackground != null)
        {
            _planetSizeModeBackground.color = useRelativeSizes
                ? _planetSizeModeOnColor
                : _planetSizeModeOffColor;
        }
    }
}
