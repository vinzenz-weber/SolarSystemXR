using UnityEngine;

// Verwaltet den Lifecycle der Raumstation-Szene im Immersive Mode.
// Wird aktiv wenn GameManager die Raumstation-Szene additiv lädt.
//
// SETUP in Raumstation.unity:
// 1. Leeres GameObject "RaumstationController" erstellen
// 2. Dieses Script darauf ziehen
// 3. SpawnPunkt-Transform (Startposition Spieler in der Station) zuweisen
// 4. PlanetSpawnPunkt-Transform (wo der Planet außen erscheint) zuweisen
// 5. MiniMenuCanvas (WorldSpace Canvas mit LazyFollowUI) zuweisen + initial deaktivieren
public class RaumstationController : MonoBehaviour
{
    // ─── Singleton ────────────────────────────────────────────────────────
    // Ermöglicht GameManager.Update() direkten Zugriff ohne FindObjectOfType
    public static RaumstationController Instance { get; private set; }

    // ─── Inspector-Felder ─────────────────────────────────────────────────
    [Header("Spawn")]
    [Tooltip("Startposition des Spielers in der Station")]
    public Transform spielerSpawnPunkt;

    [Tooltip("Position des Planeten außerhalb der Station (z.B. vor dem Fenster)")]
    public Transform planetSpawnPunkt;

    [Header("Größen-Einstellungen")]
    [Tooltip("VR-Radius der Erde in Metern — muss mit ImmersivePlanetView.erdeReferenzRadius übereinstimmen")]
    public float erdeReferenzRadius = 10f;

    [Header("Mini-Menü")]
    [Tooltip("WorldSpace Canvas mit LazyFollowUI und 'Verlassen'-Button. Im Inspector deaktiviert lassen.")]
    public GameObject miniMenuCanvas;

    // ─── Interne Variablen ────────────────────────────────────────────────
    private Vector3 _spielerUrsprungsPosition;
    private Quaternion _spielerUrsprungsRotation;
    private GameObject _planetInstanz;
    private bool _miniMenuSichtbar = false;

    // ══════════════════════════════════════════════════════════════════════
    // UNITY LIFECYCLE
    // ══════════════════════════════════════════════════════════════════════

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Mini-Menü zu Beginn verstecken
        if (miniMenuCanvas != null)
            miniMenuCanvas.SetActive(false);

        // Spieler-Position speichern (für Rückkehr beim Verlassen)
        Transform spielerRoot = GameManager.Instance?.spielerRoot;
        if (spielerRoot != null)
        {
            _spielerUrsprungsPosition = spielerRoot.position;
            _spielerUrsprungsRotation = spielerRoot.rotation;

            // Spieler zum SpawnPunkt in der Station teleportieren.
            // Rotation NICHT übernehmen — OVR-Tracking läuft relativ zur Root-Rotation,
            // eine geänderte Rotation würde die Szene "am Kopf kleben" lassen.
            if (spielerSpawnPunkt != null)
                SpielerTeleportieren(spielerRoot, spielerSpawnPunkt.position, spielerRoot.rotation);
        }
        else
        {
            Debug.LogWarning("[RaumstationController] spielerRoot ist null — bitte in GameManager zuweisen.");
        }

        PlanetPlatzieren();
    }

    void OnDestroy()
    {
        // Planet-Instanz aufräumen
        if (_planetInstanz != null)
            Destroy(_planetInstanz);

        // Spieler zurückteleportieren (zurück zur Position vor der Station)
        Transform spielerRoot = GameManager.Instance?.spielerRoot;
        if (spielerRoot != null)
            SpielerTeleportieren(spielerRoot, _spielerUrsprungsPosition, spielerRoot.rotation);

        Instance = null;
    }

    // ══════════════════════════════════════════════════════════════════════
    // PLANET PLATZIEREN
    // ══════════════════════════════════════════════════════════════════════

    // Instantiiert das gewählte Planeten-Prefab außerhalb der Station in echter VR-Größe.
    // Gleiche Skalierungsformel wie ImmersivePlanetView.
    private void PlanetPlatzieren()
    {
        PlanetData planet  = GameManager.Instance?.ausgewaehlterPlanet;
        GameObject prefab  = GameManager.Instance?.GetAusgewaehltesPrefab();

        if (planet == null || prefab == null)
        {
            Debug.LogWarning("[RaumstationController] Kein Planet oder Prefab gefunden.");
            return;
        }

        // VR-Durchmesser berechnen: Erde = 20m, Jupiter = ~224m, Mars = ~5.3m
        float vrRadius      = (planet.diameter / 12756f) * erdeReferenzRadius;
        float vrDurchmesser = vrRadius * 2f;

        // Spawn-Position: dedizierter Punkt oder Fallback 200m vor der Station
        Vector3 position = planetSpawnPunkt != null
            ? planetSpawnPunkt.position
            : transform.position + transform.forward * (vrRadius + 50f);

        _planetInstanz = Instantiate(prefab, position, Quaternion.identity);

        // Skalierung über Renderer-Bounds (analog ImmersivePlanetView)
        Renderer renderer = _planetInstanz.GetComponentInChildren<Renderer>();
        if (renderer != null && renderer.bounds.size.x > 0.001f)
        {
            float faktor = vrDurchmesser / renderer.bounds.size.x;
            _planetInstanz.transform.localScale = Vector3.one * faktor;
        }
        else
        {
            // Fallback: direkte Skalierung (geht davon aus dass Scale 1 = 1m)
            _planetInstanz.transform.localScale = Vector3.one * vrDurchmesser;
        }

        Debug.Log($"[RaumstationController] {planet.planetName} platziert: " +
                  $"VR-Radius={vrRadius:F1}m, Position={position}");
    }

    // ══════════════════════════════════════════════════════════════════════
    // MINI-MENÜ
    // ══════════════════════════════════════════════════════════════════════

    // Wird von GameManager.Update() aufgerufen wenn Start-Button gedrückt wird
    public void MiniMenuToggle()
    {
        if (miniMenuCanvas == null) return;
        _miniMenuSichtbar = !_miniMenuSichtbar;
        miniMenuCanvas.SetActive(_miniMenuSichtbar);
    }

    // Wird vom "Verlassen"-Button im Mini-Menü aufgerufen (OnClick verdrahten)
    public void VerlassenKlicken()
    {
        GameManager.Instance?.ZurueckZuSchwebend();
    }

    // ══════════════════════════════════════════════════════════════════════
    // TELEPORT
    // ══════════════════════════════════════════════════════════════════════

    // Teleportiert den Spieler-Root sicher: CharacterController kurz deaktivieren,
    // Position setzen, wieder aktivieren (CC würde sonst die Bewegung blockieren)
    private void SpielerTeleportieren(Transform spielerRoot, Vector3 zielPosition, Quaternion zielRotation)
    {
        CharacterController cc = spielerRoot.GetComponentInChildren<CharacterController>();
        if (cc != null) cc.enabled = false;

        spielerRoot.SetPositionAndRotation(zielPosition, zielRotation);

        if (cc != null) cc.enabled = true;
    }
}
