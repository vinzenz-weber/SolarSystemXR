using UnityEngine;

[CreateAssetMenu(fileName = "NeuerPlanet", menuName = "Sonnensystem/Planeten Daten")]
public class PlanetData : ScriptableObject
{
    [Header("Allgemeine Informationen")]
    public string planetName;
    public string subHeadline;
    
    [TextArea(2, 4)]
    public string shortDescription;

    [Tooltip("Nutze Sprite anstelle von UI.Image, da ScriptableObjects keine Szenen-Komponenten speichern können.")]
    public Sprite planetImage;

    [Header("Physikalische Werte")]
    [Tooltip("Durchmesser in km (z.B. 12742 für Erde)")]
    public float diameter;
    
    [Tooltip("Achsenneigung in Grad (z.B. 23.44 für Erde). Wichtig für die korrekte 3D-Drehung.")]
    public float axialTilt;

    [Tooltip("Dauer einer Eigenrotation in Tagen (z.B. 0.997 für Erde)")]
    public float rotationSpeed;

    [Header("Umlaufbahn (Kepler-Elemente)")]
    [Tooltip("Große Halbachse in AU (z.B. 1.0 für Erde)")]
    public float semiMajorAxis;

    [Tooltip("Exzentrizität (0 = perfekter Kreis, nahe 1 = extreme Ellipse)")]
    public float eccentricity;

    [Tooltip("Bahnneigung in Grad relativ zur Ekliptik")]
    public float inclination;

    [Tooltip("Länge des aufsteigenden Knotens in Grad (Drehung der Bahnebene)")]
    public float longitudeOfAscendingNode;

    [Tooltip("Argument der Periapsis in Grad (Drehung der Ellipse in der Bahnebene)")]
    public float argumentOfPeriapsis;

    [Tooltip("Mittlere Anomalie zur Epoche in Grad (Startposition des Planeten bei Zeit = 0)")]
    public float meanAnomalyAtEpoch;

    [Tooltip("Dauer eines kompletten Umlaufs um die Sonne in Tagen")]
    public float orbitalPeriod;

    [Header("Visualisierung")]
    public Color planetLineColor = Color.white;
    public GameObject planetPrefab;
    public GameObject previewPrefab;

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

    [Header("Zusätzliche Informationen")]
    [Tooltip("Kurze Beschreibung des Planeten, erscheint im Detail-Panel.")]
    [TextArea(3, 6)]
    public string beschreibung;

    [Tooltip("3–5 kurze Stichpunkte (z.B. Durchmesser, Atmosphäre, Besonderheiten).")]
    public string[] fakten;
}