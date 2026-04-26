using UnityEngine;
using UnityEngine.UI;

// Dieser Befehl erlaubt es dir, die Daten per Rechtsklick im Projektfenster zu erstellen
[CreateAssetMenu(fileName = "NeuerPlanet", menuName = "Sonnensystem/Planeten Daten")]
public class PlanetData : ScriptableObject
{
    [Header("Physikalische Werte")]
    public string planetName;
    public string subHeadline;
    public string shortDescription;

    public Image planetImage;

    [Tooltip("in km")]
    public float diameter;        // Durchmesser in km (z.B. 12756 für Erde)
    
    [Header("Umlaufbahn")]
    [Tooltip("in AU")]
    public float semiMajorAxis;   // Große Halbachse in AU (z.B. 1.0 für Erde)

    public float eccentricity;      // Exzentrizität (0 = Kreis, >0 = Ellipse)
    [Tooltip("in Days")]
    public float orbitalPeriod; // Dauer eines Umlaufs in Tagen (z.B. 365.25)
    [Tooltip("in Degrees")]
    public float inclination;       // Bahnneigung in Grad
    [Tooltip("in Days")]
    public float rotationSpeed;

    [Header("Visualisierung")]
    public Color planetLineColor; // Farbe des Planeten

    [Header("Umgebungszone")]
    [Tooltip("Schwerkraft in m/s² (Erde = 9.81, Jupiter = 24.79, Mars = 3.71)")]
    public float schwerkraft = 9.81f;

    [Tooltip("Farbe der Atmosphären-Partikel (Nebel/Wolken in der Zone)")]
    public Color atmosphaereFarbe = new Color(0.5f, 0.5f, 0.5f, 0.3f);

    [Range(0f, 1f), Tooltip("0 = keine Atmosphäre sichtbar, 1 = sehr dichter Nebel")]
    public float nebelDichte = 0f;

    [Tooltip("Hat der Planet Wind und Turbulenzen?")]
    public bool hatWind = false;

    [Range(0f, 30f), Tooltip("Windpartikel-Geschwindigkeit in m/s")]
    public float windStaerke = 0f;

    [Range(0f, 1f), Tooltip("Turbulenzstärke (0 = gleichmäßig, 1 = chaotisch)")]
    public float windTurbulenz = 0f;

    [Tooltip("Farbe der Windpartikel (z.B. Roter Staub bei Mars)")]
    public Color windPartikelFarbe = Color.white;

    [Header("Informationen")]
    [Tooltip("Kurze Beschreibung des Planeten, erscheint im Detail-Panel.")]
    [TextArea(2, 4)]
    public string beschreibung;

    [Tooltip("3–5 kurze Stichpunkte (z.B. Durchmesser, Atmosphäre, Besonderheiten).")]
    public string[] fakten;

    public GameObject planetPrefab;
    public GameObject previewPrefab;
}