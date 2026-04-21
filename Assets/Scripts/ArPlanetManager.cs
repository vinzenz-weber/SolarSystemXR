using System.Collections.Generic;
using UnityEngine;

// Verwaltet alle im AR-Modus (PLANET_SCHWEBEND) platzierten Planeten.
// Erster Planet bestimmt die Basisgröße; alle weiteren werden relativ dazu skaliert.
public class ArPlanetManager : MonoBehaviour
{
    // ─── Singleton ────────────────────────────────────────────────────────
    public static ArPlanetManager Instance { get; private set; }

    // ─── Inspector-Felder ─────────────────────────────────────────────────
    [Header("Größe")]
    [Tooltip("Durchmesser des ersten platzierten Planeten in Metern")]
    public float basisGroesse = 0.3f;

    [Header("Spawn")]
    [Tooltip("Wie weit vor der Kamera der Planet erscheint (in Metern)")]
    public float spawnAbstand = 1.5f;

    [Header("GrabHandle")]
    [Tooltip("Prefab des GrabHandle (Assets/Prefabs/GrabHandle.prefab). Enthält Meta Interaction SDK Komponenten + Visual.")]
    public GameObject grabHandlePrefab;
    [Tooltip("Lokale Position des GrabHandle unter dem Wrapper (Y negativ = unter dem Planeten)")]
    public Vector3 grabHandleOffset = new Vector3(0f, -0.8f, 0f);

    // ─── Private Variablen ────────────────────────────────────────────────
    private List<ArPlanetInstanz> _platziertePlaneten = new();
    private PlanetData _referenzData;   // Erster Planet — Grundlage für relative Skalierung

    // ══════════════════════════════════════════════════════════════════════
    // UNITY LIFECYCLE
    // ══════════════════════════════════════════════════════════════════════

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // ══════════════════════════════════════════════════════════════════════
    // ÖFFENTLICHE METHODEN
    // ══════════════════════════════════════════════════════════════════════

    // Spawnt einen neuen AR-Planeten vor der Kamera und fügt ihn zur Liste hinzu.
    public void PlanetSpawnen(PlanetData data, GameObject prefab)
    {
        if (data == null || prefab == null) return;

        // Wrapper-GameObject: enthält GrabHandle und Info-Button als Kinder
        GameObject wrapper = new GameObject("ArPlanet_" + data.planetName);

        // Position: spawnAbstand Meter vor der Kamera
        Transform kamera = Camera.main.transform;
        wrapper.transform.position = kamera.position + kamera.forward * spawnAbstand;
        wrapper.transform.rotation = Quaternion.identity;

        // Planeten-Prefab als Kind des Wrappers instanziieren
        GameObject planet = Instantiate(prefab, wrapper.transform);
        planet.transform.localPosition = Vector3.zero;
        planet.transform.localRotation = Quaternion.identity;

        // Skalierung auf das Prefab anwenden, nicht auf den Wrapper
        float skalierung = BerechneSkalierung(data);
        planet.transform.localScale = Vector3.one * skalierung;

        // Erster Planet setzt die Referenz für alle nachfolgenden
        if (_referenzData == null)
            _referenzData = data;

        // ArPlanetInstanz verwaltet Gaze, Info-Button und GrabHandle
        ArPlanetInstanz instanz = wrapper.AddComponent<ArPlanetInstanz>();
        instanz.Initialisieren(data, planet, grabHandlePrefab, grabHandleOffset);

        _platziertePlaneten.Add(instanz);
    }

    // Löscht alle platzierten Planeten und setzt den Zustand zurück.
    public void AlleEntfernen()
    {
        foreach (ArPlanetInstanz instanz in _platziertePlaneten)
        {
            if (instanz != null)
                Destroy(instanz.gameObject);
        }

        _platziertePlaneten.Clear();
        _referenzData = null;
    }

    // ══════════════════════════════════════════════════════════════════════
    // HILFSMETHODEN
    // ══════════════════════════════════════════════════════════════════════

    // Berechnet die Skalierung relativ zum ersten gesetzten Referenzplaneten.
    private float BerechneSkalierung(PlanetData data)
    {
        if (_referenzData == null || _referenzData.diameter <= 0)
            return basisGroesse;

        float relativ = basisGroesse * (data.diameter / _referenzData.diameter);
        return Mathf.Clamp(relativ, 0.02f, 3f);
    }
}
