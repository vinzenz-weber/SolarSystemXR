using System.Collections;
using UnityEngine;

// Die Zustände des Spiels — ähnlich wie "modes" in Processing
public enum SpielZustand
{
    START,
    AUSWAHL,    // Menü: Spieler wählt was angezeigt werden soll
    EXPLORE,    // Inhalt wird angezeigt, Spieler kann erkunden
    PLACEMENT   // Reserviert für später (manuelle Platzierung)
}

public class GameManager : MonoBehaviour
{
    // ─── Zustand ──────────────────────────────────────────────────────────
    public SpielZustand aktuellerZustand;

    // ─── XR ───────────────────────────────────────────────────────────────
    [Header("XR")]
    [Tooltip("OVRCameraRig → TrackingSpace → RightHandAnchor")]
    public Transform rightControllerAnchor;
    [Tooltip("OVRHand-Komponente der rechten Hand")]
    public OVRHand rightHand;

    // ─── UI ───────────────────────────────────────────────────────────────
    [Header("UI")]
    [Tooltip("Das gesamte World-Space-Canvas")]
    public GameObject auswahlCanvas;

    [Tooltip("Hauptmenü: Sonnensystem / Planeten / ...")]
    public GameObject hauptPanel;

    [Tooltip("Planeten-Untermenü mit einem Button pro Planet")]
    public GameObject planetenPanel;

    // ─── Prefabs ──────────────────────────────────────────────────────────
    [Header("Prefabs")]
    [Tooltip("Das komplette Sonnensystem (Assets/Prefabs/SolarSystem.prefab)")]
    public GameObject sonnensystemPrefab;

    [Tooltip("Wie weit vor dem Spieler das Objekt erscheint (in Metern)")]
    public float spawnAbstand = 1.5f;

    // ─── Interne Variablen ────────────────────────────────────────────────
    private GameObject aktuellesObjekt;         // aktuell angezeigte Szene
    [HideInInspector] public PlanetData ausgewaehlterPlanet;

    // ══════════════════════════════════════════════════════════════════════
    // UNITY LIFECYCLE
    // ══════════════════════════════════════════════════════════════════════

    void Start()
    {
        ZustandWechseln(SpielZustand.START);
        StartCoroutine(AutoStartNachDelay());
    }

    void Update()
    {
        // Canvas nur im AUSWAHL-Zustand sichtbar
        auswahlCanvas.SetActive(aktuellerZustand == SpielZustand.AUSWAHL);
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

        // Einmalige Aktionen beim Wechsel in einen Zustand
        if (neuerZustand == SpielZustand.AUSWAHL)
        {
            ZeigeHauptmenue();  // immer mit Hauptmenü starten
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    // PANEL-STEUERUNG  (intern)
    // ══════════════════════════════════════════════════════════════════════

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
    // BUTTON-CALLBACKS  (im Inspector unter OnClick() verdrahten)
    // ══════════════════════════════════════════════════════════════════════

    // Hauptmenü → "Sonnensystem"
    public void AufSonnensystemKlicken()
    {
        ObjektAnzeigen(sonnensystemPrefab);
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

    // Wird von PlanetButton aufgerufen
    public void PlanetAuswaehlen(PlanetData planet, GameObject prefab)
    {
        ausgewaehlterPlanet = planet;
        Debug.Log("Planet ausgewählt: " + planet.planetName);
        ObjektAnzeigen(prefab);
    }

    // ══════════════════════════════════════════════════════════════════════
    // OBJEKT SPAWNEN
    // ══════════════════════════════════════════════════════════════════════

    private void ObjektAnzeigen(GameObject prefab)
    {
        // Altes Objekt löschen
        if (aktuellesObjekt != null)
            Destroy(aktuellesObjekt);

        // Position: spawnAbstand Meter vor der Kamera
        Transform kamera = Camera.main.transform;
        Vector3 position = kamera.position + kamera.forward * spawnAbstand;
        Quaternion rotation = Quaternion.Euler(0, kamera.eulerAngles.y + 180f, 0);

        aktuellesObjekt = Instantiate(prefab, position, rotation);

        ZustandWechseln(SpielZustand.EXPLORE);
    }
}
