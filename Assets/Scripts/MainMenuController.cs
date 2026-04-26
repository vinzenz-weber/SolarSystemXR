using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    // ----------- PLANETEN-PANEL -----------
    [Header("Planeten-Panel: Root + UI-Texte")]
    [Tooltip("Das ganze linke Panel mit Planet-Auswahl, Beschreibung und Planet-Buttons.")]
    public GameObject planetenPanel;

    public TextMeshProUGUI menuHeadline;
    //public TextMeshProUGUI subHeadline;
    public TextMeshProUGUI descriptionText;
    public Image backgroundImage;

    [Tooltip("Text auf dem Start-Experience-Button im Planeten-Panel.")]
    public TextMeshProUGUI startButtonLabel;

    // ----------- SONNENSYSTEM-PANEL -----------
    [Header("Sonnensystem-Panel: Root + Daten")]
    [Tooltip("Das ganze rechte Panel.")]
    public GameObject sonnensystemPanel;

    [Tooltip("Das Prefab des kompletten Sonnensystems, das platziert werden soll.")]
    public GameObject sonnensystemPrefab;

    public TextMeshProUGUI sonnensystemHeadline;
    //public TextMeshProUGUI sonnensystemSubHeadline;
    public TextMeshProUGUI sonnensystemDescription;

    // ----------- TAB-BUTTONS -----------
    [Header("Tabs unten (Planeten / Sonnensystem)")]
    [Tooltip("Optional: visuelles Highlight für aktive/inaktive Tabs.")]
    public Image planetenTabBackground;
    public Image sonnensystemTabBackground;
    public Color activeTabColor = new Color(0.7f, 0.7f, 0.7f, 1f);
    public Color inactiveTabColor = new Color(1f, 1f, 1f, 0f);

    // ----------- REFERENZEN -----------
    [Header("Referenzen")]
    public PlacementManager placementManager;

    private PlanetData _currentPlanet;

    void Start()
    {
        // App startet im Planeten-Tab
        ShowPlanetenTab();
    }

    // =================================================================
    //                          TAB-WECHSEL
    // =================================================================

    // Wird vom Tab-Button "Planeten" aufgerufen
    public void ShowPlanetenTab()
    {
        if (planetenPanel != null) planetenPanel.SetActive(true);
        if (sonnensystemPanel != null) sonnensystemPanel.SetActive(false);

        // Wenn der User in den Planeten-Modus wechselt, soll das bereits
        // platzierte Sonnensystem (oder alte Planeten) verschwinden.
        if (placementManager != null)
        {
            placementManager.ClearPlacedObjects();
        }

        UpdateTabHighlight(true);
    }

    // Wird vom Tab-Button "Sonnensystem" aufgerufen
    public void ShowSonnensystemTab()
    {
        if (planetenPanel != null) planetenPanel.SetActive(false);
        if (sonnensystemPanel != null) sonnensystemPanel.SetActive(true);

        // Wenn der User in den Sonnensystem-Modus wechselt, sollen alle
        // bereits platzierten Planeten verschwinden.
        if (placementManager != null)
        {
            placementManager.ClearPlacedObjects();
        }

        UpdateTabHighlight(false);
    }

    private void UpdateTabHighlight(bool planetenActive)
    {
        if (planetenTabBackground != null)
            planetenTabBackground.color = planetenActive ? activeTabColor : inactiveTabColor;

        if (sonnensystemTabBackground != null)
            sonnensystemTabBackground.color = planetenActive ? inactiveTabColor : activeTabColor;
    }

    // =================================================================
    //                       PLANETEN-AUSWAHL
    // =================================================================

    // Wird vom PlanetMenuButton aufgerufen, wenn ein Planet ausgewählt wird
    public void SelectPlanet(PlanetData data)
    {
        _currentPlanet = data;

        menuHeadline.text = data.planetName;
        //subHeadline.text = data.subHeadline;

        if (descriptionText != null)
        {
            descriptionText.text = data.beschreibung;
        }

        if (backgroundImage != null && data.planetImage != null)
        {
            backgroundImage.sprite = data.planetImage.sprite;
        }

        if (startButtonLabel != null)
        {
            startButtonLabel.text = data.planetName + " hinzufügen";
        }
    }

    // Wird vom Start-Experience-Button im Planeten-Panel aufgerufen
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
    //                       SONNENSYSTEM-AUSWAHL
    // =================================================================

    // Wird vom Start-Experience-Button im Sonnensystem-Panel aufgerufen
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
