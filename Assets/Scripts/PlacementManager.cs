using System.Collections.Generic;
using UnityEngine;
using Meta.XR;
using Meta.XR.MRUtilityKit;

public class PlacementManager : MonoBehaviour
{
    public Transform rayOrigin;
    public EnvironmentRaycastManager raycastManager;
    public LineRenderer lineRenderer;

    public GameObject placementVisualizerPrefab;

    // Aktuell zu platzierender Planet (null wenn Sonnensystem-Modus)
    private PlanetData currentPlanetData;

    // Sonnensystem-Modus: hier merken wir uns das Prefab, das stattdessen platziert wird
    private GameObject _currentSolarSystemPrefab;
    private bool _isSolarSystemMode;

    private GameObject previewInstance;
    private GameObject visualizerInstance;

    // Liste aller bereits in der Welt platzierten Objekte (Planeten oder Sonnensystem),
    // damit wir sie beim Wechsel des Modus wieder aufräumen können.
    private List<GameObject> _placedObjects = new List<GameObject>();

    public float planetHeight = 0.15f;

    [Header("Scaling")]
    public float earthDiameterInVR = 0.2f; // 0.2 Meter = 20 cm für die Erde
    private const float earthDiameterInKm = 12742f;

    // ----------- AUSWAHL: einzelner Planet -----------
    public void SelectPlanet(PlanetData data)
    {
        ClearPreview();

        _isSolarSystemMode = false;
        _currentSolarSystemPrefab = null;
        currentPlanetData = data;

        previewInstance = Instantiate(currentPlanetData.previewPrefab);
        float vrScale = GetScaledSize(currentPlanetData.diameter);
        previewInstance.transform.localScale = new Vector3(vrScale, vrScale, vrScale);

        visualizerInstance = Instantiate(placementVisualizerPrefab);
    }

    // ----------- AUSWAHL: Sonnensystem-Prefab -----------
    public void SelectSolarSystem(GameObject solarSystemPrefab)
    {
        ClearPreview();

        _isSolarSystemMode = true;
        currentPlanetData = null;
        _currentSolarSystemPrefab = solarSystemPrefab;

        // Vorschau ist hier dasselbe Prefab. Falls du eine eigene Vorschau willst,
        // kannst du im MainMenuController ein zusätzliches "previewPrefab" durchreichen.
        previewInstance = Instantiate(_currentSolarSystemPrefab);

        visualizerInstance = Instantiate(placementVisualizerPrefab);
    }

    // ----------- AUFRÄUMEN -----------
    // Zerstört alle bisher platzierten Objekte. Wird vom MainMenuController
    // aufgerufen, wenn der User in den Sonnensystem-Modus wechselt.
    public void ClearPlacedObjects()
    {
        for (int i = 0; i < _placedObjects.Count; i++)
        {
            if (_placedObjects[i] != null)
            {
                Destroy(_placedObjects[i]);
            }
        }
        _placedObjects.Clear();
    }

    private void ClearPreview()
    {
        if (previewInstance != null) Destroy(previewInstance);
        if (visualizerInstance != null) Destroy(visualizerInstance);
        previewInstance = null;
        visualizerInstance = null;
        lineRenderer.enabled = false;
    }

    void Update()
    {
        // Wenn weder Planet noch Sonnensystem ausgewählt ist, machen wir nichts.
        if (previewInstance == null || visualizerInstance == null)
        {
            lineRenderer.enabled = false;
            return;
        }

        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);
        bool didHit = raycastManager.Raycast(ray, out var hitInfo);

        if (didHit == true)
        {
            bool canPlace = IsHorizontal(hitInfo.normal);

            if (canPlace == true)
            {
                lineRenderer.startColor = Color.green;
                lineRenderer.endColor = Color.green;
            }
            else
            {
                lineRenderer.startColor = Color.red;
                lineRenderer.endColor = Color.red;
            }

            previewInstance.SetActive(true);
            visualizerInstance.SetActive(true);
            lineRenderer.enabled = true;

            lineRenderer.SetPosition(0, rayOrigin.position);
            lineRenderer.SetPosition(1, hitInfo.point);

            // Beim Sonnensystem heben wir die Vorschau nicht an - es steht direkt auf dem Boden.
            float lift = _isSolarSystemMode ? 0f : planetHeight;
            previewInstance.transform.position = hitInfo.point + Vector3.up * lift;
            visualizerInstance.transform.position = hitInfo.point;

            // --- PLATZIEREN ---
            if (canPlace == true && OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger))
            {
                GameObject spawned;

                if (_isSolarSystemMode == true)
                {
                    spawned = Instantiate(_currentSolarSystemPrefab, previewInstance.transform.position, Quaternion.identity);
                }
                else
                {
                    spawned = Instantiate(currentPlanetData.planetPrefab, previewInstance.transform.position, Quaternion.identity);
                    float vrScale = GetScaledSize(currentPlanetData.diameter);
                    spawned.transform.localScale = new Vector3(vrScale, vrScale, vrScale);
                }

                // In die Liste, damit wir das Objekt später wieder aufräumen können
                _placedObjects.Add(spawned);

                Destroy(previewInstance);
                Destroy(visualizerInstance);
                previewInstance = null;
                visualizerInstance = null;

                currentPlanetData = null;
                _currentSolarSystemPrefab = null;
                _isSolarSystemMode = false;

                lineRenderer.enabled = false;

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.SetState(GameState.WORLD);
                }
            }
        }
        else
        {
            lineRenderer.enabled = false;
            previewInstance.SetActive(false);
            visualizerInstance.SetActive(false);
        }
    }

    bool IsHorizontal(Vector3 normal)
    {
        float similarity = Vector3.Dot(normal.normalized, Vector3.up);
        if (similarity > 0.85f)
        {
            return true;
        }
        return false;
    }

    float GetScaledSize(float realSizeInKm)
    {
        return (realSizeInKm / earthDiameterInKm) * earthDiameterInVR;
    }
}
