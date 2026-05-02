using UnityEngine;

[CreateAssetMenu(fileName = "NeuerPlanet", menuName = "Sonnensystem/Planeten Daten")]
public class PlanetData : ScriptableObject
{
    [Header("Allgemeine Informationen")]
    public string planetName;
    public string subHeadline;

    [TextArea(2, 4)]
    public string shortDescription;

    [Tooltip("Nutze Sprite anstelle von UI.Image, da ScriptableObjects keine Szenen-Komponenten speichern koennen.")]
    public Sprite planetImage;

    [Header("Physikalische Werte")]
    [Tooltip("Durchmesser in km, z.B. 12742 fuer Erde.")]
    public float diameter;

    [Tooltip("Achsenneigung in Grad, z.B. 23.44 fuer Erde. Wichtig fuer die korrekte 3D-Drehung.")]
    public float axialTilt;

    [Tooltip("Dauer einer Eigenrotation in Tagen, z.B. 0.997 fuer Erde.")]
    public float rotationSpeed;

    [Header("Umlaufbahn (Kepler-Elemente)")]
    [Tooltip("Grosse Halbachse in AU, z.B. 1.0 fuer Erde.")]
    public float semiMajorAxis;

    [Tooltip("Exzentrizitaet: 0 = perfekter Kreis, nahe 1 = extreme Ellipse.")]
    public float eccentricity;

    [Tooltip("Bahnneigung in Grad relativ zur Ekliptik.")]
    public float inclination;

    [Tooltip("Laenge des aufsteigenden Knotens in Grad, also Drehung der Bahnebene.")]
    public float longitudeOfAscendingNode;

    [Tooltip("Argument der Periapsis in Grad, also Drehung der Ellipse in der Bahnebene.")]
    public float argumentOfPeriapsis;

    [Tooltip("Mittlere Anomalie zur Epoche in Grad, also Startposition des Planeten bei Zeit = 0.")]
    public float meanAnomalyAtEpoch;

    [Tooltip("Dauer eines kompletten Umlaufs um die Sonne in Tagen.")]
    public float orbitalPeriod;

    [Header("Visualisierung")]
    public Color planetLineColor = Color.white;
    public GameObject planetPrefab;

    [Tooltip("Optionaler Spezialfall: eigenes Interactable-Prefab nur fuer diesen Planeten. Normalerweise leer lassen und das gemeinsame Prefab im PlacementManager nutzen.")]
    public GameObject interactablePlanetPrefab;

    public GameObject previewPrefab;

    [Header("Umgebungszone")]
    [Tooltip("Schwerkraft in m/s^2, z.B. Erde = 9.81, Jupiter = 24.79, Mars = 3.71.")]
    public float schwerkraft = 9.81f;

    [Tooltip("Farbe der Atmosphaeren-Partikel, z.B. Nebel/Wolken in der Zone.")]
    public Color atmosphaereFarbe = new Color(0.5f, 0.5f, 0.5f, 0.3f);

    [Range(0f, 1f), Tooltip("0 = keine Atmosphaere sichtbar, 1 = sehr dichter Nebel.")]
    public float nebelDichte = 0f;

    [Tooltip("Hat der Planet Wind und Turbulenzen?")]
    public bool hatWind = false;

    [Range(0f, 30f), Tooltip("Windpartikel-Geschwindigkeit in m/s.")]
    public float windStaerke = 0f;

    [Range(0f, 1f), Tooltip("Turbulenzstaerke: 0 = gleichmaessig, 1 = chaotisch.")]
    public float windTurbulenz = 0f;

    [Tooltip("Farbe der Windpartikel, z.B. roter Staub bei Mars.")]
    public Color windPartikelFarbe = Color.white;

    [Header("Zusaetzliche Informationen")]
    [Tooltip("Kurze Beschreibung des Planeten, erscheint im Detail-Panel.")]
    [TextArea(3, 6)]
    public string beschreibung;

    [Header("Detail-Panel Kurztexte")]
    [Tooltip("Kurzer Vergleichs- oder Erklaertext zur Groesse, z.B. halb so gross wie die Erde.")]
    [TextArea(1, 3)]
    public string planetSizeInfoText;

    [Tooltip("Kurzer Erklaertext zur Entfernung von der Sonne.")]
    [TextArea(1, 3)]
    public string orbitalDistanceInfoText;

    [Tooltip("Kurzer Erklaertext zur Umlaufgeschwindigkeit.")]
    [TextArea(1, 3)]
    public string orbitalSpeedInfoText;

    [Tooltip("Kurzer Erklaertext zur Achsenneigung.")]
    [TextArea(1, 3)]
    public string axialTiltInfoText;

    [Tooltip("Kurzer Erklaertext zur Exzentrizitaet der Umlaufbahn.")]
    [TextArea(1, 3)]
    public string eccentricityInfoText;

    [Tooltip("Kurzer Erklaertext zur relativen Groesse im Vergleich zur Erde.")]
    [TextArea(1, 3)]
    public string planetScaleInfoText;

    [Tooltip("Kurzer Erklaertext zur Schwerkraft.")]
    [TextArea(1, 3)]
    public string gravityInfoText;

    [Tooltip("3-5 kurze Stichpunkte. PlanetFactAnchor nutzt eine 1-basierte Nummerierung: Anchor 1 zeigt fakten[0].")]
    public string[] fakten;
}
