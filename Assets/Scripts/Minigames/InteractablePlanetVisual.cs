using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

// Editor-/Runtime-Hilfe fuer Interactable-Planet-Prefabs.
// Du waehlst ein PlanetData aus, und das visuelle Planet-Prefab wird unter VisualRoot eingesetzt.
public class InteractablePlanetVisual : MonoBehaviour
{
    private const string RetiredVisualRootName = "RuntimeRetiredVisuals";

    [Header("Daten")]
    public PlanetData PlanetData;

    [Header("Aufbau")]
    [Tooltip("Hier wird das visuelle Planet-Prefab als Child gespawnt.")]
    public Transform VisualRoot;

    [Tooltip("Lokaler Offset des gespawnten Planet-Visuals unter VisualRoot.")]
    public Vector3 VisualLocalPosition = new Vector3(0f, 0f, -0.025f);

    [Tooltip("Lokale Rotation des gespawnten Planet-Visuals unter VisualRoot in Grad.")]
    public Vector3 VisualLocalEulerAngles = Vector3.zero;

    [Tooltip("Wenn aktiv, wird beim Start automatisch das gewaehlte Planet-Prefab eingesetzt.")]
    public bool RefreshOnStart = true;

    [Header("Visual-Aufraeumen")]
    [Tooltip("PlanetBody im Visual deaktivieren, damit Minigames keine Sonnensystem-Orbitlogik starten.")]
    public bool DisablePlanetBody = true;

    [Tooltip("Collider aus dem Visual deaktivieren. Der Interactable-Root sollte eigene Grab-/Snap-Collider haben.")]
    public bool DisableVisualColliders = true;

    [Tooltip("Rigidbody aus dem Visual deaktivieren, damit nur der Interactable-Root bewegt wird.")]
    public bool DisableVisualRigidbodies = true;

    [Header("Groesse")]
    [Tooltip("Standard aus: Die Groesse wird ueber den Interactable-Root oder VisualRoot im Editor gesetzt.")]
    public bool FitVisualToTargetSize = false;
    public float TargetVisualSize = 0.12f;

    private void Reset()
    {
        VisualRoot = transform.Find("VisualRoot");
    }

    private void Awake()
    {
        if (Application.isPlaying && RefreshOnStart)
        {
            DeactivateVisualRootChildrenForRuntime();
        }
    }

    private void Start()
    {
        if (Application.isPlaying && RefreshOnStart)
        {
            RefreshVisual();
        }
    }

    [ContextMenu("Planet Visual aktualisieren")]
    public void RefreshVisual()
    {
        if (VisualRoot == null)
        {
            Debug.LogWarning("InteractablePlanetVisual: VisualRoot fehlt auf " + name + ".");
            return;
        }

        ClearVisualRoot();

        if (PlanetData == null || PlanetData.planetPrefab == null)
        {
            Debug.LogWarning("InteractablePlanetVisual: PlanetData oder planetPrefab fehlt auf " + name + ".");
            return;
        }

        GameObject visual = InstantiatePlanetPrefab();
        visual.name = "Visual_" + PlanetData.planetName;
        visual.transform.SetParent(VisualRoot, false);
        visual.transform.localPosition = VisualLocalPosition;
        visual.transform.localRotation = Quaternion.Euler(VisualLocalEulerAngles);

        CleanupVisual(visual);

        if (FitVisualToTargetSize)
        {
            ScaleVisualToTargetSize(visual, TargetVisualSize);
        }
    }

    [ContextMenu("Planet Visual leeren")]
    public void ClearVisualRoot()
    {
        if (VisualRoot == null) return;

        for (int i = VisualRoot.childCount - 1; i >= 0; i--)
        {
            GameObject child = VisualRoot.GetChild(i).gameObject;

            if (Application.isPlaying)
            {
                RetireRuntimeVisual(child);
            }
            else
            {
                DestroyImmediate(child);
            }
        }
    }

    private void DeactivateVisualRootChildrenForRuntime()
    {
        if (VisualRoot == null) return;

        for (int i = VisualRoot.childCount - 1; i >= 0; i--)
        {
            VisualRoot.GetChild(i).gameObject.SetActive(false);
        }
    }

    private void RetireRuntimeVisual(GameObject child)
    {
        if (child == null) return;

        Transform retiredRoot = GetOrCreateRetiredVisualRoot();

        child.SetActive(false);
        child.transform.SetParent(retiredRoot, false);
    }

    private Transform GetOrCreateRetiredVisualRoot()
    {
        Transform retiredRoot = transform.Find(RetiredVisualRootName);
        if (retiredRoot != null)
        {
            retiredRoot.gameObject.SetActive(false);
            return retiredRoot;
        }

        GameObject retiredObject = new GameObject(RetiredVisualRootName);
        retiredObject.transform.SetParent(transform, false);
        retiredObject.SetActive(false);
        return retiredObject.transform;
    }

    private GameObject InstantiatePlanetPrefab()
    {
#if UNITY_EDITOR
        if (Application.isPlaying == false)
        {
            GameObject editorInstance = PrefabUtility.InstantiatePrefab(PlanetData.planetPrefab) as GameObject;
            if (editorInstance != null)
            {
                return editorInstance;
            }
        }
#endif

        return Instantiate(PlanetData.planetPrefab);
    }

    private void CleanupVisual(GameObject visual)
    {
        if (DisablePlanetBody)
        {
            PlanetBody[] planetBodies = visual.GetComponentsInChildren<PlanetBody>(true);
            foreach (PlanetBody planetBody in planetBodies)
            {
                planetBody.enabled = false;
            }
        }

        if (DisableVisualColliders)
        {
            Collider[] colliders = visual.GetComponentsInChildren<Collider>(true);
            foreach (Collider visualCollider in colliders)
            {
                visualCollider.enabled = false;
            }
        }

        if (DisableVisualRigidbodies)
        {
            Rigidbody[] rigidbodies = visual.GetComponentsInChildren<Rigidbody>(true);
            foreach (Rigidbody visualRigidbody in rigidbodies)
            {
                visualRigidbody.isKinematic = true;
                visualRigidbody.useGravity = false;
                visualRigidbody.detectCollisions = false;
            }
        }
    }

    private void ScaleVisualToTargetSize(GameObject visual, float targetSize)
    {
        Renderer[] renderers = visual.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0) return;

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        float currentSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
        if (currentSize <= 0.0001f) return;

        visual.transform.localScale *= targetSize / currentSize;
    }
}
