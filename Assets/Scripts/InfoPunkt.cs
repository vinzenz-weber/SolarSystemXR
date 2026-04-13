using UnityEngine;

// Markiert eine interessante Stelle auf einem Planeten-Prefab.
// Wird auf leere GameObjects (Empties) auf dem Planeten platziert.
// InfoPanel.cs erkennt diesen Punkt per Raycast und zeigt die Infos an.
//
// SETUP:
// 1. Empty GameObject auf dem Planet-Prefab erstellen (z.B. "InfoPunkt_Nord")
// 2. Dieses Script draufziehen
// 3. Titel und Inhalt im Inspector befüllen
// 4. Der Collider wird automatisch hinzugefügt
[RequireComponent(typeof(SphereCollider))]
public class InfoPunkt : MonoBehaviour
{
    [Header("Inhalt")]
    [Tooltip("Kurzer Titel der Information (z.B. 'Nordpol')")]
    public string titel = "Interessanter Punkt";

    [TextArea(2, 6)]
    [Tooltip("Text der im Info-Panel angezeigt wird")]
    public string inhalt = "Hier steht eine interessante Information über den Planeten.";

    [Header("Einstellungen")]
    [Tooltip("Radius des unsichtbaren Trefferbereichs in Metern")]
    public float kolliderRadius = 0.05f;

    // ─── Visueller Indikator ─────────────────────────────────────────────
    private GameObject _visuell;        // Kleines leuchtendes Kügelchen
    private float _basisSkalierung;     // Gespeicherte Normalgröße für Hervorheben
    private bool _istHervorgehoben = false;

    // ══════════════════════════════════════════════════════════════════════
    // UNITY LIFECYCLE
    // ══════════════════════════════════════════════════════════════════════

    void Awake()
    {
        SphereCollider collider = GetComponent<SphereCollider>();
        collider.radius    = kolliderRadius;
        collider.isTrigger = true;

        ErstelleVisuell();
    }

    // ══════════════════════════════════════════════════════════════════════
    // ÖFFENTLICHE METHODEN  (aufgerufen von InfoPanel.cs)
    // ══════════════════════════════════════════════════════════════════════

    // Wird aufgerufen wenn der Controller-Ray auf diesen Punkt zeigt
    public void Hervorheben(bool aktiv)
    {
        if (_istHervorgehoben == aktiv) return;
        _istHervorgehoben = aktiv;

        // Größe des Kügelchens ändern für visuelles Feedback
        if (_visuell != null)
        {
            float faktor = aktiv ? 1.6f : 1f;
            _visuell.transform.localScale = Vector3.one * (_basisSkalierung * faktor);
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    // HILFSMETHODEN
    // ══════════════════════════════════════════════════════════════════════

    // Erstellt ein kleines leuchtendes Kügelchen als visuellen Marker
    private void ErstelleVisuell()
    {
        _visuell = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        _visuell.name = "InfoPunkt_Visual";
        _visuell.transform.SetParent(transform);
        _visuell.transform.localPosition = Vector3.zero;
        _visuell.transform.localScale    = Vector3.one;

        // Das Sphere-Primitiv bringt einen eigenen Collider mit — den entfernen,
        // damit nur der SphereCollider auf dem InfoPunkt selbst für Raycasts zählt
        Destroy(_visuell.GetComponent<Collider>());

        // Leuchtendes weißes Material (HDR für Bloom)
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        mat.SetColor("_BaseColor", new Color(3f, 3f, 3f, 1f)); // HDR weiß
        _visuell.GetComponent<MeshRenderer>().material = mat;

        // Größe: kleines Kügelchen relativ zum Kollider-Radius — als Basis für Hervorheben() speichern
        _basisSkalierung = kolliderRadius * 0.4f;
        _visuell.transform.localScale = Vector3.one * _basisSkalierung;
    }
}
