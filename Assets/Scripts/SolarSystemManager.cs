using UnityEngine;

public class SolarSystemManager : MonoBehaviour
{
    [Header("Tisch-Skalierung")]
    [Tooltip("1 AU in Meter. 0.006 = Neptune bei ~18cm Radius.")]
    public float distanceScale = 0.006f;

    [Tooltip("Planetengröße relativ zur Erde (12756 km). 0.002 = Erde ~2mm Durchmesser.")]
    public float planetSizeScale = 0.002f;

    [Header("Sonne")]
    [Tooltip("Sonnen-Radius als Bruchteil von Merkurs Perihelabstand (0.307 AU). " +
             "1.0 = Sonne würde Merkur-Orbit gerade berühren. 0.5 = halb so groß.")]
    [Range(0.01f, 1f)]
    public float sunSizeRatio = 0.5f;

    // Sonnen-Durchmesser skaliert automatisch mit distanceScale:
    // Merkur-Perihelabstand = 0.387 * (1 - 0.205) = 0.307 AU
    public float SunDiameter => sunSizeRatio * 0.307f * distanceScale * 2f;

    [Header("Simulation")]
    [Tooltip("Simulationsgeschwindigkeit in Tagen pro Sekunde.")]
    public float timeScale = 1f;

    [Header("Lernmodus")]
    [Tooltip("Multiplikator für alle Exzentrizitäten. 1 = realistisch, >1 = übertriebene Ellipsen.")]
    [Range(0f, 5f)]
    public float exzentrizitaetMultiplikator = 1f;
}
