using TMPro;
using UnityEngine;

// Optionale Art-Direction-Anker fuer das Reihenfolge-Minispiel.
// Wenn dieses Script im Prefab liegt, nutzt der Builder diese Transforms statt harter Zahlen.
public class ReihenfolgeMinigameLayout : MonoBehaviour
{
    [Header("Spawn-Ausrichtung")]
    [Tooltip("Wenn aktiv, wird das komplette Minigame beim Start auf den Kopf-/Kamera-Ursprung gesetzt.")]
    public bool PlaceAtCameraOnStart = true;

    [Tooltip("Lokaler Offset relativ zum Kopf. Z ist nach vorne, X zur Seite, Y nach oben.")]
    public Vector3 CameraLocalOffset = Vector3.zero;

    [Tooltip("Wenn aktiv, wird das komplette Minigame beim Start in Blickrichtung der Kamera gedreht.")]
    public bool RotateToPlayerOnStart = true;

    [Header("Generierter Inhalt")]
    [Tooltip("Parent fuer Planeten, Orbits und Snap-Slots. Leer = Builder-GameObject.")]
    public Transform GeneratedContentParent;

    [Tooltip("Mittelpunkt der Orbit-Ringe. Leer = Builder nutzt OrbitCenterLocal.")]
    public Transform OrbitCenter;

    [Tooltip("Startpositionen der Planeten. Wenn 8 gesetzt sind, kann die Bench komplett per Editor gestaltet werden.")]
    public Transform[] BenchPositions;

    [Header("UI")]
    [Tooltip("Optionales Anweisungs-Panel im Prefab. Der Builder laesst es nur stehen; Layout passiert im Editor.")]
    public GameObject InstructionPanel;

    [Tooltip("Optionaler Geschafft-Text. Leer = Builder erzeugt einfachen 3D-Text.")]
    public TMP_Text SuccessText;

    private void Start()
    {
        if (PlaceAtCameraOnStart || RotateToPlayerOnStart)
        {
            AlignRootToCamera();
        }
    }

    [ContextMenu("Reihenfolge-Minispiel am Kopf ausrichten")]
    public void AlignRootToCamera()
    {
        Transform cameraTransform = Camera.main != null ? Camera.main.transform : null;
        if (cameraTransform == null)
        {
            Debug.LogWarning("ReihenfolgeMinigameLayout: Keine Main Camera gefunden.");
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
