using UnityEngine;

// Richtet die wichtigsten Size-Minispiel-Elemente nach dem Prefab-Spawn im Raum aus.
// Das Prefab selbst darf weiter als Ganzes vom MinigameManager gespawnt werden.
public class SizeMinigameWorldLayout : MonoBehaviour
{
    [Header("Referenzen")]
    [Tooltip("Das List-GameObject aus dem Size-Prefab.")]
    public Transform ListRoot;

    [Tooltip("Parent der einzelnen Snap-Ringe.")]
    public Transform OrbitsRoot;

    [Header("Position vor dem Spieler")]
    [Tooltip("Welt-Hoehe fuer Liste und Snap-Ringe in Metern.")]
    public float WorldHeight = 1.1f;

    [Tooltip("Abstand der Planeten-Liste vor dem Kopf des Spielers.")]
    public float ListDistance = 1.05f;

    [Tooltip("Abstand der Snap-Ringe vor dem Kopf des Spielers.")]
    public float OrbitsDistance = 1.35f;

    [Tooltip("Seitlicher Offset der Liste. Positiv = rechts vom Spieler.")]
    public float ListSideOffset = 0f;

    [Tooltip("Seitlicher Offset der Snap-Ringe. Positiv = rechts vom Spieler.")]
    public float OrbitsSideOffset = 0f;

    [Tooltip("Wenn aktiv, wird beim Start automatisch anhand der Kamera ausgerichtet.")]
    public bool AlignOnStart = true;

    private void Reset()
    {
        CollectReferences();
    }

    private void OnValidate()
    {
        CollectReferences();
    }

    private void Start()
    {
        if (AlignOnStart)
        {
            AlignToPlayer();
        }
    }

    [ContextMenu("Size-Layout vor Spieler ausrichten")]
    public void AlignToPlayer()
    {
        Transform cameraTransform = Camera.main != null ? Camera.main.transform : null;
        if (cameraTransform == null)
        {
            Debug.LogWarning("SizeMinigameWorldLayout: Keine Main Camera gefunden.");
            return;
        }

        CollectReferences();

        Vector3 forward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up);
        if (forward.sqrMagnitude < 0.0001f)
        {
            forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
        }

        if (forward.sqrMagnitude < 0.0001f)
        {
            forward = Vector3.forward;
        }

        forward.Normalize();

        Quaternion floorParallelRotation = Quaternion.LookRotation(forward, Vector3.up);
        Vector3 right = floorParallelRotation * Vector3.right;

        Vector3 basePosition = cameraTransform.position;
        basePosition.y = WorldHeight;

        PlaceRoot(ListRoot, basePosition + forward * ListDistance + right * ListSideOffset, floorParallelRotation);
        PlaceRoot(OrbitsRoot, basePosition + forward * OrbitsDistance + right * OrbitsSideOffset, floorParallelRotation);
    }

    private void PlaceRoot(Transform root, Vector3 position, Quaternion rotation)
    {
        if (root == null) return;

        root.SetPositionAndRotation(position, rotation);
    }

    private void CollectReferences()
    {
        if (ListRoot == null)
        {
            ListRoot = FindDirectChild("List");
        }

        if (OrbitsRoot == null)
        {
            OrbitsRoot = FindDirectChild("Orbits");
        }
    }

    private Transform FindDirectChild(string childName)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.name == childName)
            {
                return child;
            }
        }

        return null;
    }
}
