using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    // ----------- LEARN-PANEL -----------
    [Header("Learn-Panel: Root + UI-Texte")]
    [Tooltip("Das linke Panel mit Planet-Auswahl, Beschreibung und Planet-Buttons.")]
    public GameObject learnPanel;

    public TextMeshProUGUI menuHeadline;
    public TextMeshProUGUI descriptionText;
    public Image backgroundImage;

    [Tooltip("Text auf dem Start-Experience-Button im Learn-Panel.")]
    public TextMeshProUGUI startButtonLabel;

    // ----------- TEST-PANEL -----------
    [Header("Test-Panel: Root + Daten")]
    [Tooltip("Das rechte Panel (Test / Minispiele).")]
    public GameObject testPanel;

    [Tooltip("Das Prefab des kompletten Sonnensystems, das platziert werden soll.")]
    public GameObject sonnensystemPrefab;

    public TextMeshProUGUI sonnensystemHeadline;
    public TextMeshProUGUI sonnensystemDescription;

    // ----------- TAB-BUTTONS -----------
    [Header("Tabs (Learn / Test)")]
    [Tooltip("Visuelles Highlight für den aktiven/inaktiven Learn-Tab.")]
    public Image learnTabBackground;
    public Image testTabBackground;
    public Color activeTabColor = new Color(0.7f, 0.7f, 0.7f, 1f);
    public Color inactiveTabColor = new Color(1f, 1f, 1f, 0f);

    // ----------- REFERENZEN -----------
    [Header("Referenzen")]
    public PlacementManager placementManager;

    private PlanetData _currentPlanet;

    void Start()
    {
        ShowLearnTab();
    }

    // =================================================================
    //                           TAB-WECHSEL
    // =================================================================

    // Wird vom Tab-Button "Learn" aufgerufen
    public void ShowLearnTab()
    {
        if (learnPanel != null) learnPanel.SetActive(true);
        if (testPanel != null) testPanel.SetActive(false);

        if (placementManager != null)
        {
            placementManager.ClearPlacedObjects();
        }

        UpdateTabHighlight(true);
    }

    // Wird vom Tab-Button "Test" aufgerufen
    public void ShowTestTab()
    {
        if (learnPanel != null) learnPanel.SetActive(false);
        if (testPanel != null) testPanel.SetActive(true);

        if (placementManager != null)
        {
            placementManager.ClearPlacedObjects();
        }

        UpdateTabHighlight(false);
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

    // Wird vom PlanetMenuButton aufgerufen, wenn ein Planet ausgewählt wird
    public void SelectPlanet(PlanetData data)
    {
        _currentPlanet = data;

        menuHeadline.text = data.planetName;

        if (descriptionText != null)
        {
            descriptionText.text = data.beschreibung;
        }

        if (backgroundImage != null && data.planetImage != null)
        {
            backgroundImage.sprite = data.planetImage;
        }

        if (startButtonLabel != null)
        {
            startButtonLabel.text = data.planetName + " hinzufügen";
        }
    }

    // Wird vom Start-Experience-Button im Learn-Panel aufgerufen
    public void StartExperience()
    {
        if (_currentPlanet == null)
        {
            Debug.LogWarning("Kein Planet ausgewählt – bitte erst einen Planeten antippen.");
            return;
        }

        placementManager.SelectPlanet(_currentPlanet);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetState(GameState.PLACEMENT);
        }
    }

    // =================================================================
    //                       SONNENSYSTEM (Learn → SolarSystem)
    // =================================================================

    // Wird vom Start-Button im Sonnensystem-Bereich aufgerufen
    public void StartSolarSystemExperience()
    {
        if (sonnensystemPrefab == null)
        {
            Debug.LogWarning("Kein Sonnensystem-Prefab im MainMenuController hinterlegt.");
            return;
        }

        placementManager.SelectSolarSystem(sonnensystemPrefab);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetState(GameState.PLACEMENT);
        }
    }
}
