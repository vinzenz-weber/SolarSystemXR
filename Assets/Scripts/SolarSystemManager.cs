using UnityEngine;

public class SolarSystemManager : MonoBehaviour
{
    [Header("XR-Skalierung (Zimmer-Maßstab)")]
    [Tooltip("1 AU in Meter. Für max 1.5m Radius (Neptun bei ~30 AU) muss dieser Wert ca. 0.05f sein.")]
    public float distanceScale = 0.05f;

    [Tooltip("Planetengröße relativ zur Erde (12742 km). 0.002 = Erde ~2mm Durchmesser.")]
    public float planetSizeScale = 0.002f;

    [Header("Sonne")]
    [Tooltip("Sonnen-Radius als Bruchteil von Merkurs Perihelabstand (0.307 AU). " +
             "1.0 = Sonne würde Merkur-Orbit gerade berühren. 0.5 = halb so groß.")]
    [Range(0.01f, 1f)]
    public float sunSizeRatio = 0.5f;

    public float SunDiameter => sunSizeRatio * 0.307f * distanceScale * 2f;

    [Header("Simulation & Zeit")]
    [Tooltip("Simulationsgeschwindigkeit in Tagen pro Sekunde.")]
    public float timeScale = 1f;
    
    [Tooltip("Die aktuell simulierte Zeit in Tagen seit dem Start (Epoche). Wichtig für die korrekte Planetenposition.")]
    public double currentSimulationDays = 0.0;

    [Header("Lernmodus (Übertreibungen)")]
    [Tooltip("Multiplikator für alle Exzentrizitäten. 1 = realistisch, >1 = übertriebene Ellipsen.")]
    [Range(0f, 5f)]
    public float exzentrizitaetMultiplikator = 1f;

    [Tooltip("Multiplikator für Inklination (Bahnneigung) zur besseren Visualisierung der schiefen Bahnen.")]
    [Range(0f, 5f)]
    public float inklinationMultiplikator = 1f;

    private void Update()
    {
        // Lässt die Zeit im Sonnensystem basierend auf timeScale vergehen.
        // double wird verwendet, um Präzisionsverluste bei sehr großen Werten zu vermeiden.
        currentSimulationDays += timeScale * Time.deltaTime;
    }
}