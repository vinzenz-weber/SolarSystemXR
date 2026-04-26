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

    // Sonnensystem-Modus: hier merken wir uns das Prefab, das stattdessen platziert wird.
    private GameObject _currentSolarSystemPrefab;
    private bool _isSolarSystemMode;

    private GameObject previewInstance;
    private GameObject visualizerInstance;

    // Neu: Planeten und Sonnensystem getrennt merken, damit beim Platzieren
    // gezielt nur das jeweils andere System geloescht wird.
    private List<GameObject> _placedPlanetObjects = new List<GameObject>();
    private GameObject _placedSolarSystemObject;

    public float planetHeight = 0.15f;

    [Header("Scaling")]
    public float earthDiameterInVR = 0.2f; // 0.2 Meter = 20 cm fuer die Erde
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
        // kannst du im MainMenuController ein zusaetzliches previewPrefab durchreichen.
        previewInstance = Instantiate(_currentSolarSystemPrefab);

        visualizerInstance = Instantiate(placementVisualizerPrefab);
    }

    // ----------- AUFRAEUMEN -----------
    // Zerstoert alle bisher platzierten Objekte, wenn das Menue einen kompletten Reset braucht.
    public void ClearPlacedObjects()
    {
        ClearPlacedPlanets();
        ClearPlacedSolarSystem();
    }

    private void ClearPlacedPlanets()
    {
        for (int i = 0; i < _placedPlanetObjects.Count; i++)
        {
            if (_placedPlanetObjects[i] != null)
            {
                Destroy(_placedPlanetObjects[i]);
            }
        }

        _placedPlanetObjects.Clear();
    }

    private void ClearPlacedSolarSystem()
    {
        if (_placedSolarSystemObject != null)
        {
            Destroy(_placedSolarSystemObject);
            _placedSolarSystemObject = null;
        }
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
        // Wenn weder Planet noch Sonnensystem ausgewaehlt ist, machen wir nichts.
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
                if (_isSolarSystemMode == true)
                {
                    // Neu: Sonnensystem ersetzt alle einzeln platzierten Planeten.
                    ClearPlacedPlanets();
                    ClearPlacedSolarSystem();

                    _placedSolarSystemObject = Instantiate(_currentSolarSystemPrefab, previewInstance.transform.position, Quaternion.identity);
                }
                else
                {
                    // Neu: Ein einzelner Planet ersetzt ein bereits platziertes Sonnensystem.
                    ClearPlacedSolarSystem();

                    GameObject spawnedPlanet = Instantiate(currentPlanetData.planetPrefab, previewInstance.transform.position, Quaternion.identity);
                    float vrScale = GetScaledSize(currentPlanetData.diameter);
                    spawnedPlanet.transform.localScale = new Vector3(vrScale, vrScale, vrScale);
                    _placedPlanetObjects.Add(spawnedPlanet);
                }

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
