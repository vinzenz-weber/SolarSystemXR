using UnityEngine;

// Verwaltet die AR-Platzierung des Sonnensystems im PLACEMENT-Zustand.
// Der Spieler zeigt mit dem rechten Controller in den Raum,
// ein Indikator-Ring zeigt die Zielposition an.
// Index-Trigger bestätigt die Platzierung.
public class PlatzierungManager : MonoBehaviour
{
    [Header("Platzierungs-Einstellungen")]
    [Tooltip("Startabstand des Indikators vom Controller (in Metern)")]
    public float platzierungsAbstand = 2f;
    [Tooltip("Minimaler Abstand per Thumbstick")]
    public float minAbstand = 0.3f;
    [Tooltip("Maximaler Abstand per Thumbstick")]
    public float maxAbstand = 5f;
    [Tooltip("Wie schnell der Abstand per Thumbstick-Y verändert wird")]
    public float abstandsGeschwindigkeit = 2f;

    // ─── Interne Variablen ────────────────────────────────────────────────
    private GameObject _indikator;       // Leuchtender Ring als Positionsanzeige
    private float _aktuellerAbstand;
    private bool _istAktiv = false;

    // ══════════════════════════════════════════════════════════════════════
    // UNITY LIFECYCLE
    // ══════════════════════════════════════════════════════════════════════

    void Start()
    {
        _aktuellerAbstand = platzierungsAbstand;
        ErstellePlatzierungsindikator();
    }

    void Update()
    {
        // Nur aktiv wenn GameManager im PLACEMENT-Zustand ist
        bool sollteAktivSein = GameManager.Instance != null &&
                               GameManager.Instance.aktuellerZustand == SpielZustand.PLACEMENT;

        // Indikator ein- oder ausblenden wenn sich der Zustand ändert
        if (sollteAktivSein != _istAktiv)
        {
            _istAktiv = sollteAktivSein;
            if (_indikator != null)
                _indikator.SetActive(_istAktiv);
        }

        if (!_istAktiv) return;

        Transform zeigeTransform = GameManager.Instance.HoleControllerTransform();

        // Rechter Thumbstick Y → Abstand vergrößern / verkleinern
        float thumbstickY = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).y;
        _aktuellerAbstand = Mathf.Clamp(
            _aktuellerAbstand + thumbstickY * abstandsGeschwindigkeit * Time.deltaTime,
            minAbstand,
            maxAbstand
        );

        // Indikator-Position: Strahl vom Controller aus in Zeigerichtung
        Vector3 zielPosition = zeigeTransform.position + zeigeTransform.forward * _aktuellerAbstand;
        _indikator.transform.position = zielPosition;

        // Indikator soll immer nach oben zeigen (kein Kippen)
        _indikator.transform.rotation = Quaternion.identity;

        // Index-Trigger → Platzierung bestätigen
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger))
        {
            PlatzierungBestaetigen(zielPosition);
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    // PLATZIERUNG
    // ══════════════════════════════════════════════════════════════════════

    private void PlatzierungBestaetigen(Vector3 position)
    {
        // Indikator verstecken
        if (_indikator != null)
            _indikator.SetActive(false);

        // Sonnensystem an der Zielposition spawnen
        GameObject sonnensystem = Instantiate(
            GameManager.Instance.sonnensystemPrefab,
            position,
            Quaternion.identity
        );

        // GameManager informieren — wechselt zu SONNENSYSTEM-Zustand
        GameManager.Instance.SonnensystemPlatziert(sonnensystem);
    }

    // ══════════════════════════════════════════════════════════════════════
    // HILFSMETHODEN
    // ══════════════════════════════════════════════════════════════════════

    // Erstellt einen leuchtenden Ring als Platzierungsindikator
    private void ErstellePlatzierungsindikator()
    {
        _indikator = new GameObject("PlatzierungsIndikator");

        LineRenderer linie = _indikator.AddComponent<LineRenderer>();

        // Ring mit 32 Segmenten
        int segmente    = 32;
        float radius    = 0.25f;  // 25cm Radius — sichtbar, aber nicht aufdringlich
        linie.positionCount  = segmente + 1;
        linie.loop           = true;
        linie.useWorldSpace  = false;
        linie.widthMultiplier = 0.015f;

        for (int i = 0; i <= segmente; i++)
        {
            float winkel = i * 2f * Mathf.PI / segmente;
            linie.SetPosition(i, new Vector3(
                Mathf.Cos(winkel) * radius,
                0f,
                Mathf.Sin(winkel) * radius
            ));
        }

        // Leuchtendes Cyan (HDR > 1 aktiviert Bloom)
        Material material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        Color cyan = Color.cyan;
        material.SetColor("_BaseColor", new Color(cyan.r * 4f, cyan.g * 4f, cyan.b * 4f, 1f));
        linie.material = material;

        _indikator.SetActive(false);
    }
}
