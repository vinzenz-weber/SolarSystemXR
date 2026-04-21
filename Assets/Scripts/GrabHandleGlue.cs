using UnityEngine;

// Projektspezifische Glue-Logik für das GrabHandle-GameObject.
//
// Alle Ray- und Grab-Mechanik übernimmt das Meta Interaction SDK:
//   - RayInteractable (trifft auf Collider)
//   - Grabbable       (bewegt das _targetTransform)
//   - OneGrabFreeTransformer (Bewegungs-Modus)
//
// Dieses Script kümmert sich NUR um die beiden projektspezifischen Sonderfälle,
// die kein Standard-Transformer abdeckt:
//   1. LazyFollowUI pausieren während des Greifens
//   2. Canvas nach dem Loslassen zur Kamera drehen (optional)
//
// SETUP:
// 1. Script auf dasselbe GameObject legen, das auch RayInteractable + Grabbable hat
// 2. zielTransform   = der Canvas-Root (bzw. das bewegte Objekt)
// 3. lazyFollow      = LazyFollowUI-Komponente auf dem Canvas (optional)
// 4. Im InteractableUnityEventWrapper am GrabHandle verdrahten:
//      WhenSelect   → GrabHandleGlue.OnGrabStart
//      WhenUnselect → GrabHandleGlue.OnGrabEnd
public class GrabHandleGlue : MonoBehaviour
{
    [Header("Referenzen")]
    [Tooltip("Der Canvas- oder Objekt-Transform, der nach dem Loslassen zur Kamera gedreht wird")]
    public Transform zielTransform;

    [Tooltip("LazyFollowUI auf dem Canvas — wird beim Greifen pausiert (optional)")]
    public LazyFollowUI lazyFollow;

    [Header("Einstellungen")]
    [Tooltip("Zielobjekt nach dem Loslassen zur Kamera drehen (false für Planeten)")]
    public bool ausrichtenBeimLoslassen = true;

    // ══════════════════════════════════════════════════════════════════════
    // EVENT-HANDLER (vom InteractableUnityEventWrapper aufgerufen)
    // ══════════════════════════════════════════════════════════════════════

    // WhenSelect: Ray-Trigger beginnt den Grab
    public void OnGrabStart()
    {
        // LazyFollow pausieren — sonst überschreibt es die Grabbable-Bewegung
        if (lazyFollow != null)
            lazyFollow.enabled = false;
    }

    // WhenUnselect: Grab wird beendet
    public void OnGrabEnd()
    {
        if (ausrichtenBeimLoslassen)
            ZurKameraAusrichten();

        // LazyFollow bleibt deaktiviert — Objekt bleibt wo abgelegt (wie Meta Quest OS).
        // OnDisable() reaktiviert LazyFollow beim nächsten Ausblenden des Canvas,
        // damit es beim nächsten Einblenden wieder vor dem Spieler erscheint.
    }

    // ══════════════════════════════════════════════════════════════════════
    // UNITY LIFECYCLE
    // ══════════════════════════════════════════════════════════════════════

    void OnDisable()
    {
        // Canvas wird ausgeblendet (z.B. Zustandswechsel im GameManager)
        // → LazyFollow wieder aktivieren, damit nächstes Einblenden sauber positioniert
        if (lazyFollow != null)
            lazyFollow.enabled = true;
    }

    // ══════════════════════════════════════════════════════════════════════
    // HILFSMETHODEN
    // ══════════════════════════════════════════════════════════════════════

    // Dreht das Ziel zur Kamera — nur horizontal (Panel bleibt aufrecht)
    private void ZurKameraAusrichten()
    {
        if (zielTransform == null || Camera.main == null) return;

        Vector3 richtung = zielTransform.position - Camera.main.transform.position;
        richtung.y = 0f;
        if (richtung.sqrMagnitude < 0.001f) return;

        zielTransform.rotation = Quaternion.LookRotation(richtung);
    }
}
