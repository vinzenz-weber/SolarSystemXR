using UnityEngine;
using TMPro;

// Skaliert den aktuell angezeigten Planeten auf echte Größenverhältnisse zum Menschen.
// Erde = 10m Radius in VR → Jupiter = ~109m, Merkur = ~2.9m
// Der Spieler steht "an der Oberfläche" und blickt auf den riesigen Planeten.
//
// Wird aktiv wenn GameManager in den Zustand PLANET_IMMERSIV wechselt.
// Stellt die ursprüngliche Ansicht wieder her wenn der Zustand verlassen wird.
public class ImmersivePlanetView : MonoBehaviour
{
    // Erddurchmesser in km — Referenz für die Größenverhältnisse aller Planeten
    private const float ErddurchmesserKm = 12756f;
    [Header("Größen-Einstellungen")]
    [Tooltip("VR-Radius der Erde in Metern. Alle anderen Planeten skalieren relativ dazu.")]
    public float erdeReferenzRadius = 10f;

    [Tooltip("Abstand des Spielers von der Planetenoberfläche (in Metern)")]
    public float abstandVonOberflaeche = 5f;

    [Header("UI")]
    [Tooltip("TMP_Text für den Maßstabs-Hinweis (optional)")]
    public TMP_Text massstabLabel;

    // ─── Interne Variablen ────────────────────────────────────────────────
    private bool _istAktiv = false;
    private Vector3 _originalScale;
    private Vector3 _originalPosition;

    // ══════════════════════════════════════════════════════════════════════
    // UNITY LIFECYCLE
    // ══════════════════════════════════════════════════════════════════════

    void Update()
    {
        // Im Raumstation-Modus übernimmt RaumstationController die Planetendarstellung.
        // GameManager deaktiviert diese Komponente bereits beim Laden der Station,
        // aber als zusätzliche Absicherung prüfen wir hier nochmal.
        if (GameManager.Instance != null && GameManager.Instance.IstRaumstationAktiv()) return;

        bool sollteAktivSein = GameManager.Instance != null &&
                               GameManager.Instance.aktuellerZustand == SpielZustand.PLANET_IMMERSIV;

        // Zustand hat sich geändert → Ansicht umschalten
        if (sollteAktivSein && !_istAktiv)
        {
            _istAktiv = true;
            AktiviereImmersiveAnsicht();
        }
        else if (!sollteAktivSein && _istAktiv)
        {
            _istAktiv = false;
            WiederherstelleNormaleAnsicht();
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    // IMMERSIVE ANSICHT
    // ══════════════════════════════════════════════════════════════════════

    private void AktiviereImmersiveAnsicht()
    {
        PlanetData planet = GameManager.Instance?.ausgewaehlterPlanet;
        GameObject obj    = GameManager.Instance?.GetAktuellesObjekt();

        if (planet == null || obj == null)
        {
            Debug.LogWarning("[ImmersivePlanetView] Kein Planet oder Objekt gefunden.");
            return;
        }

        // Originale Größe und Position merken (für die Rückkehr)
        _originalScale    = obj.transform.localScale;
        _originalPosition = obj.transform.position;

        // ─── Neue Größe berechnen ──────────────────────────────────────
        // VR-Radius = (Planetendurchmesser / Erddurchmesser) * Erde-VR-Radius
        float vrRadius      = (planet.diameter / ErddurchmesserKm) * erdeReferenzRadius;
        float vrDurchmesser = vrRadius * 2f;

        // Aktuelle Weltgröße des Planeten ermitteln (Bounds des Renderers)
        float aktuellerWeltDurchmesser = HoleWeltDurchmesser(obj);

        if (aktuellerWeltDurchmesser > 0.001f)
        {
            // Skalierungsfaktor: wie viel mal größer soll der Planet werden?
            float faktor = vrDurchmesser / aktuellerWeltDurchmesser;
            obj.transform.localScale = _originalScale * faktor;
        }
        else
        {
            // Fallback: lokale Scale direkt setzen (geht davon aus, dass Scale 1 = 1m)
            obj.transform.localScale = Vector3.one * vrDurchmesser;
        }

        // ─── Position: Spieler steht "an der Oberfläche" ──────────────
        Transform kamera = Camera.main.transform;

        // Horizontale Richtung (kein Y-Anteil — Spieler steht aufrecht)
        Vector3 richtung = new Vector3(kamera.forward.x, 0, kamera.forward.z).normalized;
        float abstand    = vrRadius + abstandVonOberflaeche;

        obj.transform.position = kamera.position + richtung * abstand;

        // ─── Maßstabs-Label befüllen ───────────────────────────────────
        if (massstabLabel != null)
        {
            // Wie viele echte Kilometer entsprechen 1 VR-Meter?
            float kmProMeter = planet.diameter / vrDurchmesser;
            massstabLabel.text =
                $"{planet.planetName}\n" +
                $"Ø {planet.diameter:N0} km\n" +
                $"1 m ≈ {kmProMeter:N0} km";
        }

        Debug.Log($"[ImmersivePlanetView] {planet.planetName}: " +
                  $"VR-Radius = {vrRadius:F1}m, Abstand = {abstand:F1}m");
    }

    private void WiederherstelleNormaleAnsicht()
    {
        GameObject obj = GameManager.Instance?.GetAktuellesObjekt();
        if (obj == null) return;

        obj.transform.localScale = _originalScale;
        obj.transform.position   = _originalPosition;
    }

    // ══════════════════════════════════════════════════════════════════════
    // HILFSMETHODEN
    // ══════════════════════════════════════════════════════════════════════

    // Gibt den ungefähren Weltdurchmesser des Objekts zurück (X-Achse der Bounds)
    private float HoleWeltDurchmesser(GameObject obj)
    {
        Renderer renderer = obj.GetComponentInChildren<Renderer>();
        if (renderer != null)
            return renderer.bounds.size.x;

        return 0f;
    }
}
