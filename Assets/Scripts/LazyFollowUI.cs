using UnityEngine;

// Laesst ein World-Space-UI-Panel bei Bedarf wieder vor dem Spieler erscheinen.
//
// Neues Verhalten:
// - Das Panel bleibt an seiner aktuellen Position, solange der Blick-Ray den Panel-Collider trifft.
// - Sobald der Blick-Ray den Collider kurz nicht mehr trifft, wird das Panel wieder vor der Kamera zentriert.
// - Waehrend des Zentrierens laeuft die Bewegung zu Ende, damit das Panel nicht am Rand stehen bleibt.
//
// Setup:
// 1. Dieses Script auf den Canvas- oder MenuRoot legen.
// 2. Einen BoxCollider auf das sichtbare Menue-Panel legen.
// 3. panelCollider im Inspector mit diesem BoxCollider verbinden.
public class LazyFollowUI : MonoBehaviour
{
    [Header("Abstand & Position")]
    [Tooltip("Wie weit das Panel vor dem Spieler schwebt (in Metern).")]
    public float abstand = 1.2f;

    [Tooltip("Vertikaler Versatz von der Augenhoehe. 0 = genau auf Augenhoehe, negativ = etwas tiefer.")]
    public float hoehenVersatz = -0.05f;

    [Header("Geschwindigkeit")]
    [Tooltip("Wie schnell das Panel zur Zielposition zurueckkehrt. Kleiner = traeger, groesser = direkter.")]
    [Range(0.5f, 10f)]
    public float folgeGeschwindigkeit = 2f;

    [Tooltip("Wie schnell das Panel sich zur Kamera dreht.")]
    [Range(0.5f, 10f)]
    public float rotationsGeschwindigkeit = 3f;

    [Header("Blick-Ray")]
    [Tooltip("Collider des sichtbaren Menue-Panels. Ein BoxCollider auf dem Panel ist ideal.")]
    public Collider panelCollider;

    [Tooltip("Layer, auf denen der Blick-Ray nach dem Panel sucht.")]
    public LayerMask raycastLayer = ~0;

    [Tooltip("Maximale Raycast-Distanz in Metern.")]
    public float raycastDistanz = 10f;

    [Tooltip("Wie lange der Blick-Ray das Panel verfehlen muss, bevor neu zentriert wird.")]
    public float sekundenBisRecenter = 0.15f;

    [Header("Verhalten")]
    [Tooltip("Beim Einblenden sofort an die Zielposition springen.")]
    public bool sofortBeimEinblenden = true;

    [Tooltip("Falls kein Panel-Collider gesetzt ist: altes Verhalten nutzen und immer folgen.")]
    public bool immerFolgenOhneCollider = true;

    [Tooltip("Kleine Distanz, ab der das Panel als fertig zentriert gilt.")]
    public float zielToleranz = 0.02f;

    private float _missTimer;
    private bool _isRecentering;
    private Transform _kamera;
    private readonly RaycastHit[] _raycastHits = new RaycastHit[8];

    private void OnEnable()
    {
        _kamera = GetKameraTransform();
        _missTimer = 0f;
        _isRecentering = false;

        if (sofortBeimEinblenden)
        {
            transform.SetPositionAndRotation(ZielPosition(), ZielRotation(ZielPosition()));
        }
    }

    private void Update()
    {
        _kamera = GetKameraTransform();
        if (_kamera == null) return;

        if (panelCollider == null)
        {
            if (immerFolgenOhneCollider)
            {
                BewegeZurZielposition();
            }

            return;
        }

        AktualisiereRecenterStatus();

        if (_isRecentering)
        {
            BewegeZurZielposition();
        }
    }

    private void AktualisiereRecenterStatus()
    {
        bool hasPanelHit = TrifftBlickRayPanel();

        if (_isRecentering)
        {
            if (IstAmZiel() && hasPanelHit)
            {
                _isRecentering = false;
                _missTimer = 0f;
            }

            return;
        }

        if (hasPanelHit)
        {
            _missTimer = 0f;
            return;
        }

        _missTimer += Time.deltaTime;

        if (_missTimer >= Mathf.Max(0f, sekundenBisRecenter))
        {
            _isRecentering = true;
        }
    }

    private bool TrifftBlickRayPanel()
    {
        Ray ray = new Ray(_kamera.position, _kamera.forward);

        // Direkter Test auf den eingetragenen Collider: andere Objekte blockieren die Abfrage nicht.
        if (panelCollider.Raycast(ray, out RaycastHit _, raycastDistanz))
        {
            return true;
        }

        // Fallback fuer den Fall, dass Unterobjekte eigene Collider haben.
        int hitCount = Physics.RaycastNonAlloc(
            ray,
            _raycastHits,
            raycastDistanz,
            raycastLayer,
            QueryTriggerInteraction.Collide
        );

        for (int i = 0; i < hitCount; i++)
        {
            Collider hitCollider = _raycastHits[i].collider;
            if (hitCollider == null) continue;

            if (hitCollider == panelCollider || hitCollider.transform.IsChildOf(panelCollider.transform))
            {
                return true;
            }
        }

        return false;
    }

    private void BewegeZurZielposition()
    {
        Vector3 zielPosition = ZielPosition();

        transform.position = Vector3.Lerp(
            transform.position,
            zielPosition,
            Time.deltaTime * folgeGeschwindigkeit
        );

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            ZielRotation(zielPosition),
            Time.deltaTime * rotationsGeschwindigkeit
        );
    }

    private bool IstAmZiel()
    {
        Vector3 zielPosition = ZielPosition();
        float quadratischeDistanz = (transform.position - zielPosition).sqrMagnitude;
        return quadratischeDistanz <= zielToleranz * zielToleranz;
    }

    // Zielposition: vor der Kamera, aber nur anhand der horizontalen Blickrichtung.
    private Vector3 ZielPosition()
    {
        if (_kamera == null) return transform.position;

        Vector3 horizontaleRichtung = new Vector3(_kamera.forward.x, 0f, _kamera.forward.z).normalized;

        if (horizontaleRichtung.sqrMagnitude < 0.001f)
        {
            horizontaleRichtung = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
        }

        if (horizontaleRichtung.sqrMagnitude < 0.001f)
        {
            horizontaleRichtung = Vector3.forward;
        }

        Vector3 position = _kamera.position + horizontaleRichtung * abstand;
        position.y = _kamera.position.y + hoehenVersatz;

        return position;
    }

    // Zielrotation: Panel schaut zur Kamera und bleibt dabei aufrecht.
    private Quaternion ZielRotation(Vector3 panelPosition)
    {
        if (_kamera == null) return transform.rotation;

        Vector3 richtungZurKamera = panelPosition - _kamera.position;
        richtungZurKamera.y = 0f;

        if (richtungZurKamera.sqrMagnitude < 0.001f)
        {
            return transform.rotation;
        }

        return Quaternion.LookRotation(richtungZurKamera);
    }

    private Transform GetKameraTransform()
    {
        return Camera.main != null ? Camera.main.transform : null;
    }
}
