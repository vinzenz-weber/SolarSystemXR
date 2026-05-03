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

    [Header("Einzelplanet Prefabs")]
    [Tooltip("Generisches Interactable-Prefab mit InteractablePlanetVisual/VisualRoot. Leer = PlanetData.planetPrefab wird direkt platziert.")]
    public GameObject interactablePlanetPrefab;

    [Header("Planet Info Panel")]
    public PlanetInfoPanelManager planetInfoPanelManager;

    [Header("Sonnensystem UI")]
    [Tooltip("World-Space-UI mit SonnensystemUI und Sonnensystem-Steuerung. Optional, wenn das UI schon im Sonnensystem-Prefab liegt.")]
    public SonnensystemUI sonnensystemUIPrefab;
    private SonnensystemUI _currentSonnensystemUI;

    [Header("Sonnensystem Ausrichtung")]
    [Tooltip("Neigt das platzierte Sonnensystem in Grad zum Betrachter. 0 = flach auf der Flaeche.")]
    public float solarSystemTiltTowardsUserDegrees = 20f;

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

    [Header("Platzieren")]
    [Tooltip("Standard-Button zum Platzieren. SecondaryIndexTrigger ist normalerweise der rechte Zeigefinger-Trigger.")]
    public OVRInput.Button placeButton = OVRInput.Button.SecondaryIndexTrigger;

    [Tooltip("Zweiter Button zum Platzieren. PrimaryIndexTrigger deckt die andere Controller-Hand ab.")]
    public OVRInput.Button alternatePlaceButton = OVRInput.Button.PrimaryIndexTrigger;

    [Tooltip("Zusaetzlich RawButtons pruefen. Hilft, wenn OVRInput.Button je nach Rig/Hand nicht sauber feuert.")]
    public bool acceptRawIndexTriggers = true;

    [Tooltip("Wie stark die Flaeche nach oben zeigen muss. 1 = exakt horizontal, 0.75 erlaubt leicht schraege Flaechen.")]
    [Range(0f, 1f)]
    public float horizontalSurfaceThreshold = 0.75f;

    [Header("Editor-Fallback ohne Depth API")]
    [Tooltip("Im Unity Editor wird die Depth API komplett umgangen. Das Objekt erscheint stattdessen in fixer Distanz am Controller-Ray.")]
    public bool useEditorFallbackPlacement = true;

    [Tooltip("Distanz vor dem Ray-Origin, in der die Vorschau im Editor platziert wird.")]
    public float editorPlacementDistance = 1.5f;

    [Header("Scaling")]
    public float earthDiameterInVR = 0.2f; // 0.2 Meter = 20 cm fuer die Erde
    private const float earthDiameterInKm = 12742f;

    private void Start()
    {
        DisableDepthApiInEditorFallback();
    }

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

        if (planetInfoPanelManager != null)
        {
            planetInfoPanelManager.HidePanel();
        }
    }

    public void SetPlacedContentVisible(bool isVisible)
    {
        for (int i = 0; i < _placedPlanetObjects.Count; i++)
        {
            if (_placedPlanetObjects[i] != null)
            {
                _placedPlanetObjects[i].SetActive(isVisible);
            }
        }

        if (_placedSolarSystemObject != null)
        {
            _placedSolarSystemObject.SetActive(isVisible);
        }

        if (_currentSonnensystemUI != null)
        {
            _currentSonnensystemUI.gameObject.SetActive(isVisible);
        }
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

        if (planetInfoPanelManager != null)
        {
            planetInfoPanelManager.HidePanel();
        }
    }

    private void ClearPlacedSolarSystem()
    {
        if (_currentSonnensystemUI != null)
        {
            Destroy(_currentSonnensystemUI.gameObject);
            _currentSonnensystemUI = null;
        }

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
        bool didHit = TryGetPlacementPoint(ray, out Vector3 hitPoint, out Vector3 hitNormal);

        if (didHit == true)
        {
            bool canPlace = IsEditorFallbackActive() || IsHorizontal(hitNormal);

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
            lineRenderer.SetPosition(1, hitPoint);

            // Beim Sonnensystem heben wir die Vorschau nicht an - es steht direkt auf dem Boden.
            float lift = _isSolarSystemMode ? 0f : planetHeight;
            Vector3 previewPosition = hitPoint + Vector3.up * lift;
            Quaternion previewRotation = _isSolarSystemMode == true
                ? GetSolarSystemPlacementRotation(previewPosition)
                : Quaternion.identity;

            previewInstance.transform.SetPositionAndRotation(previewPosition, previewRotation);
            visualizerInstance.transform.position = hitPoint;

            // --- PLATZIEREN ---
            if (HasPlaceInputDown())
            {
                if (canPlace == false)
                {
                    Debug.Log("PlacementManager: Platzieren blockiert, weil die getroffene Flaeche nicht horizontal genug ist. Normal=" + hitNormal);
                    return;
                }

                if (_isSolarSystemMode == true)
                {
                    // Neu: Sonnensystem ersetzt alle einzeln platzierten Planeten.
                    ClearPlacedPlanets();
                    ClearPlacedSolarSystem();

                    _placedSolarSystemObject = Instantiate(_currentSolarSystemPrefab, previewInstance.transform.position, previewInstance.transform.rotation);
                    SetupSonnensystemUI(_placedSolarSystemObject);
                }
                else
                {
                    // Neu: Ein einzelner Planet ersetzt ein bereits platziertes Sonnensystem.
                    ClearPlacedSolarSystem();

                    GameObject spawnedPlanet = Instantiate(GetPlacementPrefab(currentPlanetData), previewInstance.transform.position, Quaternion.identity);
                    SetupPlacedPlanetVisual(spawnedPlanet, currentPlanetData);
                    float rootScale = GetInteractableRootScale(spawnedPlanet, currentPlanetData);
                    spawnedPlanet.transform.localScale = new Vector3(rootScale, rootScale, rootScale);
                    RegisterSelectablePlanet(spawnedPlanet, currentPlanetData);
                    _placedPlanetObjects.Add(spawnedPlanet);

                    ShowPlanetInfo(currentPlanetData);
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
        if (similarity >= horizontalSurfaceThreshold)
        {
            return true;
        }
        return false;
    }

    private bool HasPlaceInputDown()
    {
        if (OVRInput.GetDown(placeButton) || OVRInput.GetDown(alternatePlaceButton))
        {
            return true;
        }

        if (acceptRawIndexTriggers == false) return false;

        return OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger)
            || OVRInput.GetDown(OVRInput.RawButton.LIndexTrigger);
    }

    private bool TryGetPlacementPoint(Ray ray, out Vector3 point, out Vector3 normal)
    {
        if (IsEditorFallbackActive() == true)
        {
            // Wichtig fuer den Editor: keine Depth-API-Abfrage.
            point = ray.origin + ray.direction * editorPlacementDistance;
            normal = Vector3.up;
            return true;
        }

        point = Vector3.zero;
        normal = Vector3.up;

        if (raycastManager == null)
        {
            Debug.LogWarning("PlacementManager: Kein EnvironmentRaycastManager zugewiesen.");
            return false;
        }

        bool didHit = raycastManager.Raycast(ray, out var hitInfo);
        if (didHit == false) return false;

        point = hitInfo.point;
        normal = hitInfo.normal;
        return true;
    }

    private bool IsEditorFallbackActive()
    {
        return Application.isEditor == true && useEditorFallbackPlacement == true;
    }

    private Quaternion GetSolarSystemPlacementRotation(Vector3 placementPosition)
    {
        if (Mathf.Approximately(solarSystemTiltTowardsUserDegrees, 0f) == true)
        {
            return Quaternion.identity;
        }

        Transform viewerTransform = Camera.main != null ? Camera.main.transform : rayOrigin;
        if (viewerTransform == null)
        {
            return Quaternion.identity;
        }

        Vector3 directionToViewer = viewerTransform.position - placementPosition;
        directionToViewer.y = 0f;

        if (directionToViewer.sqrMagnitude < 0.0001f)
        {
            return Quaternion.identity;
        }

        // Lokale Up-Achse des Sonnensystems wird leicht zum User gekippt.
        Vector3 tiltAxis = Vector3.Cross(Vector3.up, directionToViewer.normalized);
        if (tiltAxis.sqrMagnitude < 0.0001f)
        {
            return Quaternion.identity;
        }

        return Quaternion.AngleAxis(solarSystemTiltTowardsUserDegrees, tiltAxis.normalized);
    }

    private void DisableDepthApiInEditorFallback()
    {
        if (IsEditorFallbackActive() == false) return;
        if (raycastManager == null) return;

        // Der Editor-Fallback soll die Depth API nicht nur nicht abfragen,
        // sondern den Runtime-Manager im Playmode auch deaktivieren.
        raycastManager.enabled = false;
        Debug.Log("PlacementManager: Editor-Fallback aktiv - Depth API RaycastManager deaktiviert.");
    }

    float GetScaledSize(float realSizeInKm)
    {
        return (realSizeInKm / earthDiameterInKm) * earthDiameterInVR;
    }

    private GameObject GetPlacementPrefab(PlanetData data)
    {
        if (data == null) return null;

        return data.interactablePlanetPrefab != null
            ? data.interactablePlanetPrefab
            : interactablePlanetPrefab != null
                ? interactablePlanetPrefab
            : data.planetPrefab;
    }

    private void SetupPlacedPlanetVisual(GameObject planetObject, PlanetData data)
    {
        if (planetObject == null || data == null) return;

        InteractablePlanetVisual visualLoader = planetObject.GetComponentInChildren<InteractablePlanetVisual>(true);
        if (visualLoader == null) return;

        visualLoader.PlanetData = data;
        visualLoader.FitVisualToTargetSize = false;
        visualLoader.RefreshVisual();
        visualLoader.RefreshOnStart = false;
    }

    private float GetInteractableRootScale(GameObject planetObject, PlanetData data)
    {
        float targetWorldSize = GetScaledSize(data.diameter);
        float visualSizeAtRootScaleOne = GetVisualSizeAtRootScaleOne(planetObject);

        if (visualSizeAtRootScaleOne <= 0.0001f)
        {
            return targetWorldSize;
        }

        return targetWorldSize / visualSizeAtRootScaleOne;
    }

    private float GetVisualSizeAtRootScaleOne(GameObject planetObject)
    {
        if (planetObject == null) return 0f;

        Vector3 originalScale = planetObject.transform.localScale;
        planetObject.transform.localScale = Vector3.one;

        InteractablePlanetVisual visualLoader = planetObject.GetComponentInChildren<InteractablePlanetVisual>(true);
        Transform visualRoot = visualLoader != null && visualLoader.VisualRoot != null
            ? visualLoader.VisualRoot
            : planetObject.transform;

        Renderer[] renderers = visualRoot.GetComponentsInChildren<Renderer>(true);
        Bounds visualBounds = new Bounds();
        bool hasBounds = false;

        foreach (Renderer visualRenderer in renderers)
        {
            if (visualRenderer == null) continue;
            if (visualRenderer.GetComponentInParent<PlanetFactAnchor>(true) != null) continue;

            if (hasBounds == false)
            {
                visualBounds = visualRenderer.bounds;
                hasBounds = true;
            }
            else
            {
                visualBounds.Encapsulate(visualRenderer.bounds);
            }
        }

        planetObject.transform.localScale = originalScale;

        if (hasBounds == false) return 0f;

        return Mathf.Max(visualBounds.size.x, visualBounds.size.y, visualBounds.size.z);
    }



    private void RegisterSelectablePlanet(GameObject planetObject, PlanetData data)
    {
        if (planetObject == null || data == null) return;

        PlanetSelectable selectable = planetObject.GetComponent<PlanetSelectable>();
        if (selectable == null)
        {
            selectable = planetObject.AddComponent<PlanetSelectable>();
        }

        selectable.planetData = data;
    }

    private void ShowPlanetInfo(PlanetData data)
    {
        PlanetInfoPanelManager manager = planetInfoPanelManager != null
            ? planetInfoPanelManager
            : PlanetInfoPanelManager.Instance;

        if (manager != null)
        {
            manager.ShowPlanet(data);
        }
    }

    private void SetupSonnensystemUI(GameObject solarSystemObject)
    {
        if (solarSystemObject == null) return;

        if (planetInfoPanelManager != null)
        {
            planetInfoPanelManager.HidePanel();
        }

        SolarSystemManager solarSystemManager = solarSystemObject.GetComponentInChildren<SolarSystemManager>();
        if (solarSystemManager == null)
        {
            Debug.LogWarning("PlacementManager: Kein SolarSystemManager im platzierten Sonnensystem gefunden.");
            return;
        }

        SonnensystemUI ui = solarSystemObject.GetComponentInChildren<SonnensystemUI>(true);

        if (ui == null && sonnensystemUIPrefab != null)
        {
            ui = Instantiate(sonnensystemUIPrefab);
        }

        if (ui == null)
        {
            Debug.LogWarning("PlacementManager: Kein SonnensystemUI gefunden oder zugewiesen.");
            return;
        }

        _currentSonnensystemUI = ui;
        ui.gameObject.SetActive(true);
        ui.Bind(solarSystemManager);
    }
}
