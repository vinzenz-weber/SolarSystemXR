using UnityEngine;
using UnityEngine.InputSystem;

public class DesktopPlacement : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject solarSystemPrefab;
    public LayerMask placementLayer;

    [Header("Distanz → Skalierung")]
    [Tooltip("Raycast-Distanz (m) bei der das kleinste System erscheint, z.B. Tisch.")]
    public float minDistance = 0.3f;
    [Tooltip("Raycast-Distanz (m) bei der das größte System erscheint, z.B. Boden/Raum.")]
    public float maxDistance = 3.0f;
    [Tooltip("distanceScale bei minDistance (Neptune ~18cm Radius).")]
    public float minDistanceScale = 0.006f;
    [Tooltip("distanceScale bei maxDistance (Neptune ~1.5m Radius).")]
    public float maxDistanceScale = 0.05f;

    [Header("Höhe")]
    [Tooltip("Höhe des System-Zentrums über der Oberfläche, als Bruchteil des Neptune-Orbit-Radius.")]
    public float heightRatio = 0.5f;

    private GameObject currentInstance;
    private SolarSystemManager solarSystemManager;
    private Transform systemContainer;
    private Camera mainCam;
    private bool isLocked = false;

    // Neptune semi-major axis in AU — bestimmt den Außenradius des Systems
    private const float NeptuneAU = 30.07f;

    private void Start()
    {
        mainCam = Camera.main;
    }

    private void Update()
    {
        if (Mouse.current == null || Keyboard.current == null) return;

        // Rechtsklick: Platzieren oder Verschieben (nur wenn nicht gesperrt)
        if (Mouse.current.rightButton.wasPressedThisFrame && !isLocked)
            PlaceSystem();

        // Leertaste: Platzierung fixieren
        if (Keyboard.current.spaceKey.wasPressedThisFrame && currentInstance != null)
        {
            isLocked = true;
            Debug.Log("Sonnensystem fixiert.");
        }

        // R: Platzierung wieder freigeben
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            isLocked = false;
            Debug.Log("Platzierung freigegeben — Rechtsklick zum Neu-Platzieren.");
        }
    }

    private void PlaceSystem()
    {
        Ray ray = mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (!Physics.Raycast(ray, out RaycastHit hit, 100f, placementLayer))
            return;

        // Distanz → Skala interpolieren
        float t = Mathf.InverseLerp(minDistance, maxDistance, hit.distance);
        float newDistanceScale = Mathf.Lerp(minDistanceScale, maxDistanceScale, t);

        // Höhe über der Oberfläche skaliert mit dem System
        float yOffset = NeptuneAU * newDistanceScale * heightRatio;

        if (currentInstance == null)
        {
            currentInstance = Instantiate(solarSystemPrefab, hit.point, Quaternion.identity);
            solarSystemManager = currentInstance.GetComponentInChildren<SolarSystemManager>();
            if (solarSystemManager == null)
            {
                Debug.LogError("SolarSystemManager nicht im Prefab gefunden!");
                Destroy(currentInstance);
                currentInstance = null;
                return;
            }
            systemContainer = solarSystemManager.transform;
        }
        else
        {
            currentInstance.transform.position = hit.point;
        }

        solarSystemManager.distanceScale = newDistanceScale;
        systemContainer.localPosition = new Vector3(0f, yOffset, 0f);
    }
}
