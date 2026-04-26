using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlanetMenuButton : MonoBehaviour
{
    public PlanetData planetData;
    public MainMenuController mainMenuController;

    [Header("Beschriftung auf diesem Button")]
    public TextMeshProUGUI Label;
    public TextMeshProUGUI subHeadline;

    void Start()
    {
        // Beschriftung des Buttons aus den Planeten-Daten setzen
        Label.text = planetData.planetName;
        subHeadline.text = planetData.subHeadline;

        // Click-Listener auf den Button hängen
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(OnPlanetButtonClicked);
        }
        else
        {
            Debug.LogError("Fehler: Auf dem Objekt " + gameObject.name + " fehlt die Button-Komponente!");
        }
    }

    // Wird beim Klick auf den Planeten-Button aufgerufen
    public void OnPlanetButtonClicked()
    {
        mainMenuController.SelectPlanet(planetData);
    }
}
