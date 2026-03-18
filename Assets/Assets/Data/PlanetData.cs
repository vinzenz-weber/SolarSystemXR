using UnityEngine;

// Dieser Befehl erlaubt es dir, die Daten per Rechtsklick im Projektfenster zu erstellen
[CreateAssetMenu(fileName = "NeuerPlanet", menuName = "Sonnensystem/Planeten Daten")]
public class PlanetData : ScriptableObject
{
    [Header("Physikalische Werte")]
    public string planetName;
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

    [Header("Visualisierung")]
    public Color planetLineColor; // Farbe des Planeten
}