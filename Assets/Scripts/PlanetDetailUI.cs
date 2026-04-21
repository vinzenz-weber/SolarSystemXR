using UnityEngine;
using TMPro;
using System.Text;

// Zeigt die Daten des aktuell ausgewählten Planeten an.
// Wird aktiv wenn GameManager in den PLANET_SCHWEBEND-Zustand wechselt.
//
// SETUP im Unity-Editor:
// 1. World-Space Canvas "PlanetDetailPanel" erstellen
// 2. Dieses Script auf den Canvas ziehen
// 3. Die TMP_Text-Felder in den Inspector-Slots verdrahten
// 4. Den "Immersiv ansehen"-Button mit GameManager.ImmersivAnzeigen() verbinden
public class PlanetDetailUI : MonoBehaviour
{
    [Header("Text-Felder")]
    [Tooltip("Großer Titel — zeigt den Planetennamen")]
    public TMP_Text nameText;

    [Tooltip("Kurze Beschreibung des Planeten (PlanetData.beschreibung)")]
    public TMP_Text beschreibungText;

    [Tooltip("Stichpunkte (PlanetData.fakten[])")]
    public TMP_Text faktenText;

    [Tooltip("Technische Daten: Durchmesser, Abstand, Umlaufzeit")]
    public TMP_Text statsText;

    // ══════════════════════════════════════════════════════════════════════
    // UNITY LIFECYCLE
    // ══════════════════════════════════════════════════════════════════════

    void OnEnable()
    {
        // Panel wird sichtbar → Daten des aktuellen Planeten eintragen
        PlanetData planet = GameManager.Instance?.ausgewaehlterPlanet;

        if (planet == null)
        {
            Debug.LogWarning("[PlanetDetailUI] Kein Planet ausgewählt.");
            return;
        }

        BefuellePanel(planet);

        // Panel vor dem Spieler positionieren
        PanelPositionieren();
    }

    // ══════════════════════════════════════════════════════════════════════
    // PANEL BEFÜLLEN
    // ══════════════════════════════════════════════════════════════════════

    internal void BefuellePanel(PlanetData planet)
    {
        // Planetenname
        if (nameText != null)
            nameText.text = planet.planetName;

        // Beschreibung
        if (beschreibungText != null)
            beschreibungText.text = string.IsNullOrEmpty(planet.beschreibung)
                ? "Keine Beschreibung vorhanden."
                : planet.beschreibung;

        // Fakten als Stichpunkte
        if (faktenText != null)
        {
            if (planet.fakten != null && planet.fakten.Length > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (string fakt in planet.fakten)
                    sb.AppendLine("• " + fakt);
                faktenText.text = sb.ToString();
            }
            else
            {
                faktenText.text = "";
            }
        }

        // Technische Daten aus dem ScriptableObject
        if (statsText != null)
        {
            string abstand = planet.semiMajorAxis > 0
                ? $"{planet.semiMajorAxis:F3} AU"
                : "Zentrum";

            string umlaufzeit = planet.orbitalPeriod > 0
                ? $"{planet.orbitalPeriod:F0} Tage"
                : "—";

            statsText.text =
                $"Durchmesser:   {planet.diameter:N0} km\n" +
                $"Abstand Sonne: {abstand}\n" +
                $"Umlaufzeit:    {umlaufzeit}\n" +
                $"Exzentrizität: {planet.eccentricity:F3}";
        }
    }

    // Panel rechts neben dem Planeten, zur Kamera gedreht
    private void PanelPositionieren()
    {
        if (Camera.main == null) return;

        GameObject planet = GameManager.Instance?.GetAktuellesObjekt();
        if (planet == null) return;

        Transform kamera = Camera.main.transform;
        Vector3 planetPos = planet.transform.position;

        // Rechts vom Planeten (aus Kamera-Perspektive gesehen)
        Vector3 richtungZurKamera = (kamera.position - planetPos).normalized;
        Vector3 rechts = Vector3.Cross(Vector3.up, richtungZurKamera).normalized;

        // Abstand vom Planetenzentrum — passt sich an Planetengröße an
        float planetRadius = HolePlanetRadius(planet);
        float seitenAbstand = planetRadius + 0.25f;  // 25cm Luft nach dem Rand

        Vector3 position = planetPos + rechts * seitenAbstand;

        // Panel schaut zur Kamera
        Quaternion rotation = Quaternion.LookRotation(-richtungZurKamera);
        transform.SetPositionAndRotation(position, rotation);
    }

    // Wird von ArPlanetInstanz aufgerufen — positioniert das Panel neben dem AR-Planeten.
    public void ZeigeFuerArPlanet(PlanetData planet, Vector3 planetPosition, float planetRadius)
    {
        BefuellePanel(planet);

        if (Camera.main == null) return;

        Vector3 richtungZurKamera = (Camera.main.transform.position - planetPosition).normalized;
        Vector3 rechts = Vector3.Cross(Vector3.up, richtungZurKamera).normalized;
        float seitenAbstand = planetRadius + 0.25f;

        Vector3 position = planetPosition + rechts * seitenAbstand;
        Quaternion rotation = Quaternion.LookRotation(-richtungZurKamera);
        transform.SetPositionAndRotation(position, rotation);
    }

    // Ungefährer Radius des Planeten aus seinen Renderer-Bounds
    private float HolePlanetRadius(GameObject planet)
    {
        Renderer renderer = planet.GetComponentInChildren<Renderer>();
        if (renderer != null)
            return renderer.bounds.extents.x;

        return 0.15f; // Fallback: 15cm
    }
}
