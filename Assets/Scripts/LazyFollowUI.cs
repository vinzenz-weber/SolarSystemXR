using UnityEngine;

// Lässt ein UI-Panel dem Spieler träge folgen — immer auf Augenhöhe,
// immer in einem festen Abstand, aber mit sanftem Nachlaufen.
//
// Einfach auf einen World-Space Canvas ziehen, fertig.
public class LazyFollowUI : MonoBehaviour
{
    [Header("Abstand & Position")]
    [Tooltip("Wie weit das Panel vor dem Spieler schwebt (in Metern)")]
    public float abstand = 1.2f;

    [Tooltip("Vertikaler Versatz von der Augenhöhe. 0 = genau auf Augenhöhe, negativ = etwas tiefer")]
    public float hoehenVersatz = -0.05f;

    [Header("Geschwindigkeit")]
    [Tooltip("Wie schnell das Panel der Position folgt. Kleiner = träger, größer = direkter")]
    [Range(0.5f, 10f)]
    public float folgeGeschwindigkeit = 2f;

    [Tooltip("Wie schnell das Panel sich zur Kamera dreht")]
    [Range(0.5f, 10f)]
    public float rotationsGeschwindigkeit = 3f;

    [Header("Verhalten")]
    [Tooltip("Beim Einblenden sofort an die Zielposition springen (kein Einflug-Effekt)")]
    public bool sofortBeimEinblenden = true;

    // ══════════════════════════════════════════════════════════════════════
    // UNITY LIFECYCLE
    // ══════════════════════════════════════════════════════════════════════

    void OnEnable()
    {
        // Beim ersten Einblenden sofort positionieren, damit das Panel
        // nicht von irgendwo hereinfliegt
        if (sofortBeimEinblenden)
            transform.SetPositionAndRotation(ZielPosition(), ZielRotation());
    }

    void Update()
    {
        // Position träge zur Zielposition bewegen
        transform.position = Vector3.Lerp(
            transform.position,
            ZielPosition(),
            Time.deltaTime * folgeGeschwindigkeit
        );

        // Rotation träge zur Kamera drehen
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            ZielRotation(),
            Time.deltaTime * rotationsGeschwindigkeit
        );
    }

    // ══════════════════════════════════════════════════════════════════════
    // HILFSMETHODEN
    // ══════════════════════════════════════════════════════════════════════

    // Zielposition: immer vor der Kamera, auf Augenhöhe
    // Nur die horizontale Blickrichtung wird genutzt — das Panel
    // folgt nicht wenn der Kopf nach oben/unten schaut
    private Vector3 ZielPosition()
    {
        Transform kamera = Camera.main.transform;

        Vector3 horizontaleRichtung = new Vector3(kamera.forward.x, 0f, kamera.forward.z).normalized;
        Vector3 position = kamera.position + horizontaleRichtung * abstand;
        position.y = kamera.position.y + hoehenVersatz;

        return position;
    }

    // Zielrotation: Panel schaut immer zur Kamera
    private Quaternion ZielRotation()
    {
        Vector3 richtungZurKamera = transform.position - Camera.main.transform.position;
        richtungZurKamera.y = 0f; // Panel bleibt aufrecht, kippt nicht
        return Quaternion.LookRotation(richtungZurKamera);
    }
}
