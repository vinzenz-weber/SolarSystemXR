using UnityEngine;

// Stellt die Umgebungsbedingungen des aktuell gewählten Planeten in einem Raumbereich dar.
// Wird aktiviert wenn der Spieler die Zone betritt, deaktiviert wenn er sie verlässt.
//
// SETUP:
// 1. Leeres GameObject "UmgebungsZone" in der Raumstation erstellen
// 2. Dieses Script darauf ziehen → BoxCollider wird automatisch hinzugefügt
// 3. Ein Child-GameObject "AtmosphaerePartikel" mit ParticleSystem → in atmosphaerePartikel ziehen
// 4. Ein Child-GameObject "WindPartikel" mit ParticleSystem → in windPartikel ziehen
// 5. Beide Particle Systems: Shape = Box, Größe auf zonenGroesse einstellen
//
// Die Zone liest automatisch den aktuell gewählten Planeten aus dem GameManager.
[RequireComponent(typeof(BoxCollider))]
public class PlanetUmgebungsZone : MonoBehaviour
{
    // ─── Inspector-Felder ─────────────────────────────────────────────────
    [Header("Zonen-Größe")]
    [Tooltip("Breite × Höhe × Tiefe der Zone in Metern")]
    public Vector3 zonenGroesse = new Vector3(5f, 3f, 20f);

    [Header("Referenzen")]
    [Tooltip("ParticleSystem für Atmosphäre/Nebel/Wolken")]
    public ParticleSystem atmosphaerePartikel;

    [Tooltip("ParticleSystem für Wind und Turbulenzen")]
    public ParticleSystem windPartikel;

    // ─── Interne Variablen ────────────────────────────────────────────────
    private bool _spielerDrinnen = false;
    private Vector3 _originalGravity;
    private float _originalSpielerSchwerkraft = -9.81f;
    private SpielerBewegung _spielerBewegung;

    // ══════════════════════════════════════════════════════════════════════
    // UNITY LIFECYCLE
    // ══════════════════════════════════════════════════════════════════════

    void Awake()
    {
        // BoxCollider als Trigger konfigurieren
        BoxCollider col = GetComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = zonenGroesse;

        // Original-Schwerkraft merken (für Rückkehr)
        _originalGravity = Physics.gravity;
    }

    void Start()
    {
        // SpielerBewegung suchen (Desktop-Modus, kann null sein bei Quest)
        _spielerBewegung = FindObjectOfType<SpielerBewegung>();
        if (_spielerBewegung != null)
            _originalSpielerSchwerkraft = _spielerBewegung.schwerkraft;

        // Partikel-Systeme zu Beginn stoppen
        if (atmosphaerePartikel != null) atmosphaerePartikel.Stop();
        if (windPartikel        != null) windPartikel.Stop();
    }

    // ══════════════════════════════════════════════════════════════════════
    // TRIGGER: SPIELER BETRITT / VERLÄSST DIE ZONE
    // ══════════════════════════════════════════════════════════════════════

    void OnTriggerEnter(Collider other)
    {
        // Nur auf CharacterController (Spieler) reagieren
        if (other.GetComponent<CharacterController>() == null) return;
        if (_spielerDrinnen) return;

        PlanetData planet = GameManager.Instance?.ausgewaehlterPlanet;
        if (planet == null)
        {
            Debug.LogWarning("[UmgebungsZone] Kein Planet ausgewählt — Zone inaktiv.");
            return;
        }

        _spielerDrinnen = true;
        UmgebungAktivieren(planet);
    }

    void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<CharacterController>() == null) return;
        if (!_spielerDrinnen) return;

        _spielerDrinnen = false;
        UmgebungDeaktivieren();
    }

    // ══════════════════════════════════════════════════════════════════════
    // UMGEBUNG EIN / AUS
    // ══════════════════════════════════════════════════════════════════════

    private void UmgebungAktivieren(PlanetData planet)
    {
        // ─── Schwerkraft ──────────────────────────────────────────────
        // Physics.gravity: wirkt auf OVRPlayerController (Quest)
        Physics.gravity = new Vector3(0f, -planet.schwerkraft, 0f);

        // SpielerBewegung.schwerkraft: wirkt auf Desktop-CharacterController
        if (_spielerBewegung != null)
            _spielerBewegung.schwerkraft = -planet.schwerkraft;

        // ─── Atmosphäre / Nebel ───────────────────────────────────────
        if (atmosphaerePartikel != null)
        {
            ParticleSystem.MainModule main = atmosphaerePartikel.main;
            main.startColor = planet.atmosphaereFarbe;

            ParticleSystem.EmissionModule emission = atmosphaerePartikel.emission;
            // Emissionsrate skaliert mit Nebeldichte (0 = aus, 1 = 200 Partikel/s)
            emission.rateOverTime = planet.nebelDichte * 200f;

            if (planet.nebelDichte > 0f)
                atmosphaerePartikel.Play();
        }

        // ─── Wind / Turbulenzen ───────────────────────────────────────
        if (windPartikel != null && planet.hatWind)
        {
            ParticleSystem.MainModule main = windPartikel.main;
            main.startColor   = planet.windPartikelFarbe;
            main.startSpeed   = new ParticleSystem.MinMaxCurve(
                planet.windStaerke * 0.5f,
                planet.windStaerke
            );

            // Noise-Modul für Turbulenzen
            ParticleSystem.NoiseModule noise = windPartikel.noise;
            noise.enabled  = planet.windTurbulenz > 0f;
            noise.strength = new ParticleSystem.MinMaxCurve(planet.windTurbulenz * 3f);
            noise.frequency = 0.5f;

            windPartikel.Play();
        }

        Debug.Log($"[UmgebungsZone] Aktiv: {planet.planetName} | " +
                  $"Schwerkraft={planet.schwerkraft:F1} m/s² | " +
                  $"Nebel={planet.nebelDichte:F2} | " +
                  $"Wind={planet.hatWind}");
    }

    private void UmgebungDeaktivieren()
    {
        // Schwerkraft zurücksetzen
        Physics.gravity = _originalGravity;

        if (_spielerBewegung != null)
            _spielerBewegung.schwerkraft = _originalSpielerSchwerkraft;

        // Partikel stoppen
        if (atmosphaerePartikel != null) atmosphaerePartikel.Stop();
        if (windPartikel        != null) windPartikel.Stop();

        Debug.Log("[UmgebungsZone] Zurückgesetzt auf Normalzustand.");
    }

    // ══════════════════════════════════════════════════════════════════════
    // EDITOR-GIZMO: Zone sichtbar machen im Scene-View
    // ══════════════════════════════════════════════════════════════════════

    void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;

        // Gefüllter Quader (halbtransparent)
        Gizmos.color = new Color(0f, 1f, 1f, 0.08f);
        Gizmos.DrawCube(Vector3.zero, zonenGroesse);

        // Umriss
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(Vector3.zero, zonenGroesse);
    }
}
