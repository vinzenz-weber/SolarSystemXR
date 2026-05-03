using UnityEngine;

// Dreht das fertig arrangierte Size-Minispiel nach dem Spawn zum Spieler.
// Die lokalen Positionen der Kinder bleiben unveraendert, damit das Prefab
// frei im Editor art-directed werden kann.
public class SizeMinigameWorldLayout : MonoBehaviour
{
    [Header("Spawn-Ausrichtung")]
    [Tooltip("Wenn aktiv, wird das komplette Minigame beim Start auf den Kopf-/Kamera-Ursprung gesetzt.")]
    public bool PlaceAtCameraOnStart = true;

    [Tooltip("Lokaler Offset relativ zum Kopf. Z ist nach vorne, X zur Seite, Y nach oben.")]
    public Vector3 CameraLocalOffset = Vector3.zero;

    [Tooltip("Wenn aktiv, wird das komplette Minigame beim Start in Blickrichtung der Kamera gedreht.")]
    public bool RotateToPlayerOnStart = true;

    private void Start()
    {
        if (PlaceAtCameraOnStart || RotateToPlayerOnStart)
        {
            AlignRootToCamera();
        }
    }

    [ContextMenu("Size-Minispiel am Kopf ausrichten")]
    public void AlignRootToCamera()
    {
        Transform cameraTransform = Camera.main != null ? Camera.main.transform : null;
        if (cameraTransform == null)
        {
            Debug.LogWarning("SizeMinigameWorldLayout: Keine Main Camera gefunden.");
            return;
        }

        if (PlaceAtCameraOnStart)
        {
            transform.position = cameraTransform.position + cameraTransform.TransformDirection(CameraLocalOffset);
        }

        if (RotateToPlayerOnStart)
        {
            RotateRootToCameraForward(cameraTransform);
        }
    }

    private void RotateRootToCameraForward(Transform cameraTransform)
    {
        Vector3 forward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up);
        if (forward.sqrMagnitude < 0.0001f)
        {
            forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
        }

        if (forward.sqrMagnitude < 0.0001f)
        {
            forward = Vector3.forward;
        }

        transform.rotation = Quaternion.LookRotation(forward.normalized, Vector3.up);
    }
}
