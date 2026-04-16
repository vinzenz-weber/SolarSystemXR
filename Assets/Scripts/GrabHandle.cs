using UnityEngine;

// Griffelement (Pille) unterhalb eines World-Space Canvas.
// Auf die Pille zeigen + Index-Trigger halten → Canvas frei im Raum verschieben.
// Beim Loslassen dreht sich der Canvas zur Kamera und bleibt dort stehen.
//
// SETUP:
// 1. Leeres GameObject "GrabHandle" als Kind des Canvas erstellen
// 2. Dieses Script auf "GrabHandle" ziehen
// 3. CapsuleCollider hinzufügen (isTrigger=true, Direction=X-Axis, Radius=0.025, Height=0.12)
// 4. Als Kind von "GrabHandle" ein Capsule-Primitiv erstellen → "GrabHandle_Visual"
//    - Rotation Z=90° (liegende Pille), Scale z.B. (0.025, 0.06, 0.025)
//    - Den auto-erzeugten CapsuleCollider vom Capsule-Primitiv entfernen!
// 5. "GrabHandle" unterhalb der Canvas-Unterkante positionieren (LocalPos y ≈ -0.23)
// 6. Inspector-Felder setzen (zielCanvas, Ray-Origins, Materialien)
[RequireComponent(typeof(CapsuleCollider))]
public class GrabHandle : MonoBehaviour
{
    // ─── Inspector-Felder ─────────────────────────────────────────────────
    [Header("Referenzen")]
    [Tooltip("Der Canvas-Root, der beim Greifen verschoben wird")]
    public Transform zielCanvas;

    [Tooltip("LazyFollowUI auf dem Canvas — wird beim Greifen pausiert (optional)")]
    public LazyFollowUI lazyFollow;

    [Header("Ray-Origins")]
    [Tooltip("Transform des RayInteractor-GameObjects des rechten Controllers (aus OVRInteractionComprehensive). " +
             "Damit stimmt der Raycast mit dem sichtbaren Meta-Ray überein.")]
    public Transform rightRayOrigin;

    [Tooltip("Transform des RayInteractor-GameObjects des linken Controllers.")]
    public Transform leftRayOrigin;

    [Header("Visuelle Rückmeldung")]
    [Tooltip("Material der Pille im Normalzustand (z.B. halbtransparent weiß)")]
    public Material normalMaterial;

    [Tooltip("Material wenn der Controller draufzeigt oder greift (z.B. HDR-Cyan)")]
    public Material hervorgehobenMaterial;

    [Header("Einstellungen")]
    [Tooltip("Maximale Raycast-Distanz in Metern")]
    public float maxRaycastDistanz = 5f;

    // ─── Private Variablen ────────────────────────────────────────────────
    private MeshRenderer  _renderer;           // Renderer der Pille (Kind-Objekt)
    private bool          _wirdGezeigt;        // Ray trifft gerade die Pille
    private bool          _wirdGegriffen;      // Trigger gehalten, Canvas folgt Controller
    private Transform     _aktiverController;  // Welcher Controller greift gerade
    private Vector3       _grabOffset;         // Offset Canvas-Position ↔ Controller-Position

    // ══════════════════════════════════════════════════════════════════════
    // UNITY LIFECYCLE
    // ══════════════════════════════════════════════════════════════════════

    void Awake()
    {
        GetComponent<CapsuleCollider>().isTrigger = true;

        // Renderer im Kind-Objekt suchen (GrabHandle_Visual)
        _renderer = GetComponentInChildren<MeshRenderer>();
    }

    void Update()
    {
        if (_wirdGegriffen)
        {
            // Canvas der Controller-Position folgen (plus gespeicherter Offset)
            CanvasVerschieben();

            // Trigger losgelassen → Greifen beenden
            bool losgelassen =
                OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch) ||
                OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch);

            if (losgelassen)
                GreifenBeenden();
        }
        else
        {
            // Raycast: zeigt ein Controller auf die Pille?
            RaycastAufGriff();

            // Controller zeigt auf Pille + Trigger gedrückt → Greifen starten
            if (_wirdGezeigt)
            {
                bool gedrueckt =
                    OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch) ||
                    OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch);

                if (gedrueckt)
                    GreifenStarten();
            }
        }
    }

    void OnDisable()
    {
        // Wenn der Canvas ausgeblendet wird (Zustandswechsel im GameManager):
        // Grab-Zustand zurücksetzen und LazyFollow wieder aktivieren,
        // damit es beim nächsten Einblenden korrekt vor dem Spieler positioniert.
        _wirdGegriffen     = false;
        _aktiverController = null;
        HervorhebungSetzen(false);

        if (lazyFollow != null)
            lazyFollow.enabled = true;
    }

    // ══════════════════════════════════════════════════════════════════════
    // GREIFEN
    // ══════════════════════════════════════════════════════════════════════

    private void GreifenStarten()
    {
        // Welcher Controller hat den Trigger gedrückt?
        bool rechtsGedrueckt = OVRInput.GetDown(
            OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch);

        if (rechtsGedrueckt && rightRayOrigin != null)
            _aktiverController = rightRayOrigin;
        else if (!rechtsGedrueckt && leftRayOrigin != null)
            _aktiverController = leftRayOrigin;
        else
            _aktiverController = GameManager.Instance.HoleControllerTransform();

        // Offset merken: wo ist der Canvas relativ zum Controller?
        if (zielCanvas != null)
            _grabOffset = zielCanvas.position - _aktiverController.position;

        // LazyFollow pausieren — sonst überschreibt es die Bewegung in Update()
        if (lazyFollow != null)
            lazyFollow.enabled = false;

        _wirdGegriffen = true;
        HervorhebungSetzen(true);
    }

    private void GreifenBeenden()
    {
        _wirdGegriffen     = false;
        _aktiverController = null;

        // Canvas zur Kamera ausrichten (aufrecht, kein Kippen)
        CanvasZurKameraAusrichten();

        // LazyFollow bleibt deaktiviert — Canvas bleibt wo abgelegt (wie Meta Quest OS).
        // OnDisable() reaktiviert LazyFollow wenn der Canvas das nächste Mal
        // ausgeblendet wird, sodass er beim Neuöffnen wieder korrekt positioniert.

        HervorhebungSetzen(false);
    }

    private void CanvasVerschieben()
    {
        if (zielCanvas == null || _aktiverController == null) return;
        zielCanvas.position = _aktiverController.position + _grabOffset;
    }

    // Dreht den Canvas so, dass er zur Kamera schaut (nur horizontal, kein Kippen)
    private void CanvasZurKameraAusrichten()
    {
        if (zielCanvas == null || Camera.main == null) return;

        Vector3 richtung = zielCanvas.position - Camera.main.transform.position;
        richtung.y = 0f; // Panel bleibt aufrecht
        if (richtung.sqrMagnitude < 0.001f) return;

        zielCanvas.rotation = Quaternion.LookRotation(richtung);
    }

    // ══════════════════════════════════════════════════════════════════════
    // RAYCAST
    // ══════════════════════════════════════════════════════════════════════

    private void RaycastAufGriff()
    {
        bool trifft = false;

        foreach (Transform origin in HoleRayOrigins())
        {
            Ray strahl = new Ray(origin.position, origin.forward);

            // QueryTriggerInteraction.Collide: Raycast trifft auch Trigger-Collider
            if (Physics.Raycast(strahl, out RaycastHit treffer, maxRaycastDistanz,
                                ~0, QueryTriggerInteraction.Collide))
            {
                // Nur reagieren wenn der Ray genau unseren Griff-Collider trifft
                if (treffer.collider.gameObject == gameObject)
                {
                    trifft = true;
                    break;
                }
            }
        }

        if (trifft != _wirdGezeigt)
        {
            _wirdGezeigt = trifft;
            HervorhebungSetzen(_wirdGezeigt);
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    // HILFSMETHODEN
    // ══════════════════════════════════════════════════════════════════════

    // Material der Pille wechseln (Normal ↔ Hervorgehoben)
    private void HervorhebungSetzen(bool aktiv)
    {
        if (_renderer == null) return;
        Material zielMaterial = aktiv ? hervorgehobenMaterial : normalMaterial;
        if (zielMaterial != null)
            _renderer.material = zielMaterial;
    }

    // Ray-Origins: bevorzugt RayInteractor-Transforms,
    // fällt auf Controller-Anchors aus GameManager zurück
    private Transform[] HoleRayOrigins()
    {
        var liste = new System.Collections.Generic.List<Transform>();
        if (rightRayOrigin != null) liste.Add(rightRayOrigin);
        if (leftRayOrigin  != null) liste.Add(leftRayOrigin);

        if (liste.Count == 0)
            liste.AddRange(GameManager.Instance.HoleAlleControllerTransforms());

        return liste.ToArray();
    }
}
