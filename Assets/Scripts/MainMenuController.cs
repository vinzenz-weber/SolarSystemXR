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

    private PlanetData _currentPlanet;
    private ExperienceSelection _currentSelection = ExperienceSelection.None;

    void Start()
    {
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

        // Neu: Platzierte Objekte bleiben beim Tabwechsel erhalten.
        // Geloescht wird erst beim echten Platzieren im PlacementManager.
        UpdateTabHighlight(false);
        SelectSolarSystem();
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
