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

    [Tooltip("Farbe fuer noch nicht abgeschlossene Quiz-Buttons.")]
    public Color quizOpenColor = new Color(0.12f, 0.32f, 0.62f, 0.95f);

    [Tooltip("Farbe fuer abgeschlossene Quiz-Buttons.")]
    public Color quizCompletedColor = new Color(0.12f, 0.55f, 0.28f, 0.95f);

    [Tooltip("Farbe fuer Coming-Soon-Eintraege.")]
    public Color quizDisabledColor = new Color(0.23f, 0.23f, 0.23f, 0.8f);

    // ----------- TAB-BUTTONS -----------
    [Header("Tabs (Learn / Sonnensystem)")]
    [Tooltip("Visuelles Highlight fuer den aktiven/inaktiven Tab.")]
    public Image learnTabBackground;
    public Image testTabBackground;
    public Color activeTabColor = new Color(0.7f, 0.7f, 0.7f, 1f);
    public Color inactiveTabColor = new Color(1f, 1f, 1f, 0f);

    // ----------- REFERENZEN -----------
    [Header("Referenzen")]
    public PlacementManager placementManager;
    public MinigameManager minigameManager;

    private PlanetData _currentPlanet;
    private ExperienceSelection _currentSelection = ExperienceSelection.None;
    private string _reihenfolgeBaseLabel = "Reihenfolge";
    private string _sizeBaseLabel = "Size";
    private string _gravityBaseLabel = "Gravity (Coming Soon)";

    void Start()
    {
        CacheQuizButtonReferences();
        ShowLearnTab();
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
        if (learnTabBackground != null)
            learnTabBackground.color = learnActive ? activeTabColor : inactiveTabColor;

        if (testTabBackground != null)
            testTabBackground.color = learnActive ? inactiveTabColor : activeTabColor;
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

        SetActionButtonText("Discover " + data.planetName);
    }

    // Wird vom Sonnensystem-Button/Tab aufgerufen.
    public void SelectSolarSystem()
    {
        _currentPlanet = null;
        _currentSelection = ExperienceSelection.SolarSystem;

        if (sonnensystemHeadline != null && menuHeadline != null)
        {
            menuHeadline.text = sonnensystemHeadline.text;
        }

        if (sonnensystemDescription != null && descriptionText != null)
        {
            descriptionText.text = sonnensystemDescription.text;
        }

        SetActionButtonText("Discover Solar System");
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
        UpdateQuizButton(gravityButton, gravityButtonLabel, _gravityBaseLabel, false, false);
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
            label.text = isCompleted ? baseLabel + " (geschafft)" : baseLabel;
        }

        if (button == null) return;

        button.interactable = canStart;

        Image image = button.targetGraphic as Image;
        if (image == null)
        {
            image = button.GetComponent<Image>();
        }

        if (image != null)
        {
            image.color = canStart == false
                ? quizDisabledColor
                : isCompleted ? quizCompletedColor : quizOpenColor;
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
}
