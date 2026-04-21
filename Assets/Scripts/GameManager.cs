using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Die Zustände des Spiels — ähnlich wie "modes" in Processing
public enum SpielZustand
{
    START,
    AUSWAHL,          // Menü: Spieler wählt was angezeigt werden soll
    PLACEMENT,        // AR: Sonnensystem im Raum platzieren
    SONNENSYSTEM,     // Sonnensystem steht, Parameter-UI sichtbar
    PLANET_SCHWEBEND, // Einzelplanet schwebt 1.5m vor dem Spieler
    PLANET_IMMERSIV   // Planet in echter Größenrelation zum Menschen
}

public class GameManager : MonoBehaviour
{
    // ─── Singleton ────────────────────────────────────────────────────────
    // Zugriff von anderen Scripts: GameManager.Instance.ZustandWechseln(...)
    public static GameManager Instance { get; private set; }

    // ─── Zustand ──────────────────────────────────────────────────────────
    public SpielZustand aktuellerZustand;

    // ─── XR ───────────────────────────────────────────────────────────────
    [Header("XR")]
    [Tooltip("OVRCameraRig → TrackingSpace → RightHandAnchor")]
    public Transform rightControllerAnchor;
    [Tooltip("OVRCameraRig → TrackingSpace → LeftHandAnchor")]
    public Transform leftControllerAnchor;
    [Tooltip("OVRHand-Komponente der rechten Hand")]
    public OVRHand rightHand;

    // ─── Passthrough ──────────────────────────────────────────────────────
    [Header("Passthrough")]
    [Tooltip("OVRPassthroughLayer-Komponente auf dem OVRCameraRig")]
    public OVRPassthroughLayer passthroughLayer;

    // ─── UI ───────────────────────────────────────────────────────────────
    [Header("UI – Haupt-Canvas")]
    [Tooltip("Das gesamte World-Space-Canvas")]
    public GameObject auswahlCanvas;
    [Tooltip("Hauptmenü: Sonnensystem / Planeten / ...")]
    public GameObject hauptPanel;
    [Tooltip("Planeten-Untermenü mit einem Button pro Planet")]
    public GameObject planetenPanel;

    [Header("UI – Modus-Panels")]
    [Tooltip("Parameter-Slider für den Sonnensystem-Modus (eigener Canvas)")]
    public GameObject sonnensystemPanel;
    [Tooltip("Daten-Panel für die schwebende Planetenansicht (eigener Canvas)")]
    public GameObject planetDetailPanel;
    [Tooltip("UI für die immersive Planetenansicht (eigener Canvas)")]
    public GameObject immersivPanel;

    // ─── Prefabs ──────────────────────────────────────────────────────────
    [Header("Prefabs")]
    [Tooltip("Das komplette Sonnensystem (Assets/Prefabs/SolarSystem.prefab)")]
    public GameObject sonnensystemPrefab;
    [Tooltip("Wie weit vor dem Spieler das 3D-Objekt erscheint (in Metern)")]
    public float spawnAbstand = 1.5f;
    [Tooltip("Wie weit vor dem Spieler das UI-Menü erscheint (in Metern)")]
    public float uiAbstand = 1.2f;

    // ─── Raumstation ──────────────────────────────────────────────────────
    [Header("Raumstation")]
    [Tooltip("Exakter Name der Raumstation-Szene (muss in Build Settings stehen)")]
    public string raumstationSzeneName = "Raumstation";
    [Tooltip("Root-Transform des Spielers (OVRPlayerController — das GO mit CharacterController, NICHT OVRCameraRig)")]
    public Transform spielerRoot;
    [Tooltip("ImmersivePlanetView-Komponente — wird im Raumstation-Modus deaktiviert")]
    public ImmersivePlanetView immersivePlanetView;

    // ─── Interne Variablen ────────────────────────────────────────────────
    private GameObject _aktuellesObjekt;
    private bool _raumstationAktiv = false;
    [HideInInspector] public PlanetData ausgewaehlterPlanet;
    [HideInInspector] public GameObject ausgewaehltesPrefab;

    // ══════════════════════════════════════════════════════════════════════
    // UNITY LIFECYCLE
    // ══════════════════════════════════════════════════════════════════════

    void Awake()
    {
        // Singleton: nur eine Instanz des GameManagers zulassen
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        ZustandWechseln(SpielZustand.START);
        StartCoroutine(AutoStartNachDelay());
    }

    void Update()
    {
        // ☰ Menü-Button: wechselt zwischen Menü und aktuellem Inhalt
        if (MenueInputDown())
        {
            if (aktuellerZustand == SpielZustand.PLANET_IMMERSIV)
            {
                // Im Raumstation-Modus nur Mini-Menü ein-/ausblenden — KEIN Hauptmenü
                RaumstationController.Instance?.MiniMenuToggle();
                return;
            }

            if (aktuellerZustand == SpielZustand.SONNENSYSTEM  ||
                aktuellerZustand == SpielZustand.PLANET_SCHWEBEND ||
                aktuellerZustand == SpielZustand.PLACEMENT)
            {
                ZustandWechseln(SpielZustand.AUSWAHL);
            }
            else if (aktuellerZustand == SpielZustand.AUSWAHL && _aktuellesObjekt != null)
            {
                // Zurück zum letzten Inhalt, falls vorhanden
                ZustandWechseln(SpielZustand.SONNENSYSTEM);
            }
        }
    }

    // ☰ Menü-Button links am Controller
    private bool MenueInputDown()
    {
        return OVRInput.GetDown(OVRInput.Button.Start);
    }

    IEnumerator AutoStartNachDelay()
    {
        yield return new WaitForSeconds(1f);
        ZustandWechseln(SpielZustand.AUSWAHL);
    }

    // ══════════════════════════════════════════════════════════════════════
    // ZUSTANDSWECHSEL
    // ══════════════════════════════════════════════════════════════════════

    public void ZustandWechseln(SpielZustand neuerZustand)
    {
        aktuellerZustand = neuerZustand;
        Debug.Log("Spielzustand: " + aktuellerZustand);

        // Erst alle Panels ausblenden, dann den richtigen State aktivieren
        AlleUIPanelsVerstecken();

        switch (neuerZustand)
        {
            case SpielZustand.START:
                ArPlanetManager.Instance?.AlleEntfernen();
                break;

            case SpielZustand.AUSWAHL:
                PassthroughEinschalten();
                CanvasVorSpielerPositionieren();
                auswahlCanvas.SetActive(true);
                ZeigeHauptmenue();
                break;

            case SpielZustand.PLACEMENT:
                PassthroughEinschalten();
                ArPlanetManager.Instance?.AlleEntfernen();
                // PlatzierungManager.cs reagiert auf diesen Zustand und übernimmt
                break;

            case SpielZustand.SONNENSYSTEM:
                PassthroughEinschalten();
                ArPlanetManager.Instance?.AlleEntfernen();
                if (sonnensystemPanel != null)
                    sonnensystemPanel.SetActive(true);
                break;

            case SpielZustand.PLANET_SCHWEBEND:
                PassthroughEinschalten();
                // Kein Panel automatisch — ArPlanetInstanz übernimmt via Gaze-Erkennung
                break;

            case SpielZustand.PLANET_IMMERSIV:
                PassthroughAusschalten();   // Dunkler Weltraum statt Passthrough
                // Mini-Menü lebt in der Raumstation-Szene — immersivPanel bleibt versteckt
                StartCoroutine(RaumstationLaden());
                break;
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    // PANEL-STEUERUNG
    // ══════════════════════════════════════════════════════════════════════

    private void AlleUIPanelsVerstecken()
    {
        auswahlCanvas.SetActive(false);
        if (sonnensystemPanel  != null) sonnensystemPanel.SetActive(false);
        if (planetDetailPanel  != null) planetDetailPanel.SetActive(false);
        if (immersivPanel      != null) immersivPanel.SetActive(false);
    }

    private void CanvasVorSpielerPositionieren()
    {
        Transform kamera = Camera.main.transform;
        Vector3 position  = kamera.position + kamera.forward * uiAbstand;
        Quaternion rotation = Quaternion.LookRotation(position - kamera.position);
        auswahlCanvas.transform.SetPositionAndRotation(position, rotation);
    }

    private void ZeigeHauptmenue()
    {
        hauptPanel.SetActive(true);
        planetenPanel.SetActive(false);
    }

    private void ZeigePlanetenmenue()
    {
        hauptPanel.SetActive(false);
        planetenPanel.SetActive(true);
    }

    // ══════════════════════════════════════════════════════════════════════
    // PASSTHROUGH
    // ══════════════════════════════════════════════════════════════════════

    public void PassthroughEinschalten()
    {
        if (passthroughLayer != null)
            passthroughLayer.enabled = true;
    }

    public void PassthroughAusschalten()
    {
        if (passthroughLayer != null)
            passthroughLayer.enabled = false;
    }

    // Gibt den rechten Controller zurück — oder die Kamera als Fallback (Editor/Desktop)
    public Transform HoleControllerTransform()
    {
        if (rightControllerAnchor != null)
            return rightControllerAnchor;
        return Camera.main.transform;
    }

    // Gibt alle verfügbaren Controller zurück (für Dual-Hand-Raycast und -Input)
    public Transform[] HoleAlleControllerTransforms()
    {
        var liste = new System.Collections.Generic.List<Transform>();
        if (rightControllerAnchor != null) liste.Add(rightControllerAnchor);
        if (leftControllerAnchor  != null) liste.Add(leftControllerAnchor);
        if (liste.Count == 0)              liste.Add(Camera.main.transform);
        return liste.ToArray();
    }

    // ══════════════════════════════════════════════════════════════════════
    // BUTTON-CALLBACKS  (im Inspector unter OnClick() verdrahten)
    // ══════════════════════════════════════════════════════════════════════

    // Hauptmenü → "Sonnensystem" → startet Platzierungsmodus
    public void AufSonnensystemKlicken()
    {
        ZustandWechseln(SpielZustand.PLACEMENT);
    }

    // Hauptmenü → "Planeten"
    public void AufPlanetenMenuKlicken()
    {
        ZeigePlanetenmenue();
    }

    // Planeten-Untermenü → "Zurück"
    public void AufZurueckKlicken()
    {
        ZeigeHauptmenue();
    }

    // Wird von PlanetButton aufgerufen (aus dem Planeten-Untermenü)
    public void PlanetAuswaehlen(PlanetData planet, GameObject prefab)
    {
        ausgewaehlterPlanet = planet;
        ausgewaehltesPrefab = prefab;  // Prefab-Referenz für RaumstationController speichern
        Debug.Log("Planet ausgewählt: " + planet.planetName);

        // ArPlanetManager übernimmt Skalierung, Spawn und Verwaltung
        ArPlanetManager.Instance?.PlanetSpawnen(planet, prefab);

        ZustandWechseln(SpielZustand.PLANET_SCHWEBEND);
    }

    // Immersive Ansicht anzeigen — Button "Immersiv ansehen" ruft das auf
    public void ImmersivAnzeigen()
    {
        ZustandWechseln(SpielZustand.PLANET_IMMERSIV);
    }

    // Zurück von immersiv zur schwebenden Ansicht
    public void ZurueckZuSchwebend()
    {
        if (_raumstationAktiv)
            StartCoroutine(RaumstationEntladen());
        ZustandWechseln(SpielZustand.PLANET_SCHWEBEND);
    }

    // Gibt zurück ob die Raumstation-Szene gerade geladen ist
    public bool IstRaumstationAktiv() => _raumstationAktiv;

    // Gibt das zuletzt gewählte Planeten-Prefab zurück (für RaumstationController)
    public GameObject GetAusgewaehltesPrefab() => ausgewaehltesPrefab;

    // Wird von PlatzierungManager aufgerufen, wenn die Platzierung bestätigt wurde
    public void SonnensystemPlatziert(GameObject sonnensystem)
    {
        _aktuellesObjekt = sonnensystem;
        ZustandWechseln(SpielZustand.SONNENSYSTEM);
    }

    // Gibt das aktuelle 3D-Objekt zurück (z.B. für ImmersivePlanetView)
    public GameObject GetAktuellesObjekt() => _aktuellesObjekt;

    // ══════════════════════════════════════════════════════════════════════
    // RAUMSTATION – SZENE LADEN / ENTLADEN
    // ══════════════════════════════════════════════════════════════════════

    // Lädt die Raumstation-Szene additiv (beide Szenen laufen gleichzeitig).
    // ImmersivePlanetView wird deaktiviert, der schwebende Planet versteckt.
    // RaumstationController.Start() übernimmt nach dem Laden.
    private IEnumerator RaumstationLaden()
    {
        _raumstationAktiv = true;

        // ImmersivePlanetView sofort im gleichen Frame deaktivieren,
        // damit sie nicht versucht den Planet zu skalieren
        if (immersivePlanetView != null) immersivePlanetView.enabled = false;

        // Schwebenden Planeten verstecken (RaumstationController zeigt eigene Version)
        if (_aktuellesObjekt != null) _aktuellesObjekt.SetActive(false);

        yield return SceneManager.LoadSceneAsync(raumstationSzeneName, LoadSceneMode.Additive);
        Debug.Log("[GameManager] Raumstation geladen.");
    }

    // Entlädt die Raumstation-Szene und stellt den vorherigen Zustand wieder her.
    // RaumstationController.OnDestroy() teleportiert den Spieler zurück, bevor die Szene entladen wird.
    private IEnumerator RaumstationEntladen()
    {
        yield return SceneManager.UnloadSceneAsync(raumstationSzeneName);

        _raumstationAktiv = false;

        // Schwebenden Planeten und ImmersivePlanetView wiederherstellen
        if (_aktuellesObjekt != null) _aktuellesObjekt.SetActive(true);
        if (immersivePlanetView != null) immersivePlanetView.enabled = true;

        Debug.Log("[GameManager] Raumstation entladen.");
    }

}
