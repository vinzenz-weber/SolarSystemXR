using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Meta.XR;
using Meta.XR.MRUtilityKit;

public class PlacementManager : MonoBehaviour
{
    [Header("Placement-Ray")]
    [Tooltip("Fallback-Ray-Ursprung. Wenn hier ein HandAnchor eingetragen ist, wird automatisch ein genauerer Controller-/Hand-Ray-Child gesucht.")]
    public Transform rayOrigin;
    [Tooltip("Automatisch einen genaueren Ray-Ursprung wie RightControllerAnchor unter rayOrigin verwenden. Wichtig fuer Controller-as-Hand/Natural-Hand-Modus.")]
    [SerializeField] private bool _autoUseChildRayOrigin = true;

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

    [Header("Sonnensystem Preview")]
    [Tooltip("Ghost-Prefab fuer die Sonnensystem-Vorschau. Wenn leer, wird als Fallback das echte Sonnensystem-Prefab genutzt.")]
    public GameObject solarSystemPreviewPrefab;
    [Tooltip("Durchmesser der Sonnensystem-Vorschau in Metern.")]
    public float solarSystemPreviewDiameterMeters = 0.5f;

    [Header("Sonnensystem Ausrichtung")]
    [Tooltip("Neigt das platzierte Sonnensystem in Grad zum Betrachter. 0 = flach auf der Flaeche.")]
    public float solarSystemTiltTowardsUserDegrees = 20f;

    [Header("Sonne fuer Einzelplaneten")]
    [Tooltip("Root-GameObject, das beim ersten Einzelplaneten an die User-Position gesetzt und horizontal in User-Blickrichtung gedreht wird. SunPasser kann direkt darauf oder auf einem Child liegen.")]
    [FormerlySerializedAs("_sunObject")]
    [SerializeField] private GameObject _sunPlacementObject;

    [Tooltip("Wenn leer, wird Camera.main benutzt. Die Sonne wird beim ersten Planeten an dieser Position gespawnt.")]
    [SerializeField] private Transform _userCamera;

    [Tooltip("Wenn aktiv, wird das Sun-Objekt bis zum ersten platzierten Einzelplaneten ausgeblendet.")]
    [SerializeField] private bool _hideSunUntilFirstPlanetPlacement = true;

    // Aktuell zu platzierender Planet (null wenn Sonnensystem-Modus)
    private PlanetData currentPlanetData;

    // Sonnensystem-Modus: hier merken wir uns das Prefab, das stattdessen platziert wird.
    private GameObject _currentSolarSystemPrefab;
    private bool _isSolarSystemMode;

    private GameObject previewInstance;
    private GameObject visualizerInstance;
    private Vector3 _lastPlacementPoint;
    private Quaternion _lastPlacementRotation = Quaternion.identity;

    // Neu: Planeten und Sonnensystem getrennt merken, damit beim Platzieren
    // gezielt nur das jeweils andere System geloescht wird.
    private List<GameObject> _placedPlanetObjects = new List<GameObject>();
    private GameObject _placedSolarSystemObject;
    private bool _hasSpawnedSunForPlanets;
    private bool _hasStoredPanelVisibilityForMainMenu;
    private bool _wasPlanetInfoPanelVisibleBeforeMainMenu;
    private bool _wasSonnensystemUIVisibleBeforeMainMenu;
    private List<GameObjectState> _distanceGrabObjectStatesBeforeMainMenu = new List<GameObjectState>();
    private bool _hasStoredDistanceGrabObjectStatesForMainMenu;

    public float planetHeight = 0.15f;

    [Header("Platzieren")]
    [Tooltip("Standard-Button zum Platzieren. SecondaryIndexTrigger ist normalerweise der rechte Zeigefinger-Trigger.")]
    public OVRInput.Button placeButton = OVRInput.Button.SecondaryIndexTrigger;

    [Tooltip("Zweiter Button zum Platzieren. PrimaryIndexTrigger deckt die andere Controller-Hand ab.")]
    public OVRInput.Button alternatePlaceButton = OVRInput.Button.PrimaryIndexTrigger;

    [Tooltip("Zusaetzlich RawButtons pruefen. Hilft, wenn OVRInput.Button je nach Rig/Hand nicht sauber feuert.")]
    public bool acceptRawIndexTriggers = true;

    [Tooltip("Zusaetzlich Index-Pinch der Hand als Platzier-Geste akzeptieren.")]
    [SerializeField] private bool _acceptIndexPinchPlacement = true;

    [Tooltip("Optional: rechte OVRHand. Leer = wird zur Laufzeit automatisch gesucht.")]
    [SerializeField] private OVRHand _rightHand;

    [Tooltip("Optional: linke OVRHand. Leer = wird zur Laufzeit automatisch gesucht.")]
    [SerializeField] private OVRHand _leftHand;

    [Tooltip("Ab welcher Pinch-Staerke ein Pinch als Klick startet.")]
    [Range(0f, 1f)]
    [SerializeField] private float _pinchDownThreshold = 0.75f;

    [Tooltip("Unter welcher Pinch-Staerke der Pinch wieder als losgelassen gilt.")]
    [Range(0f, 1f)]
    [SerializeField] private float _pinchUpThreshold = 0.35f;

    [Tooltip("Wie stark die Flaeche nach oben zeigen muss. 1 = exakt horizontal, 0.75 erlaubt leicht schraege Flaechen.")]
    [Range(0f, 1f)]
    public float horizontalSurfaceThreshold = 0.75f;

    [Header("Environment Raycast / Test-Placement")]
    [Tooltip("Aktiv = EnvironmentRaycastManager nutzen. Inaktiv = Test-Placement in fixer Distanz am Controller-Ray, auch im Build.")]
    [SerializeField] private bool _useEnvironmentRaycastPlacement = false;

    [Tooltip("Optionales GameObject mit dem EnvironmentRaycastManager. Leer = GameObject der raycastManager-Komponente.")]
    [SerializeField] private GameObject _environmentRaycastManagerObject;

    [HideInInspector]
    public bool useEditorFallbackPlacement = true;

    [Tooltip("Distanz vor dem Ray-Origin, in der die Vorschau bei Test-Placement platziert wird.")]
    public float editorPlacementDistance = 1.5f;

    [Header("Scaling")]
    public float earthDiameterInVR = 0.2f; // 0.2 Meter = 20 cm fuer die Erde
    [Tooltip("Aktiv = Planeten werden relativ zur echten Groesse skaliert. Der feste 50-cm-Modus ist verworfen und wird nicht mehr genutzt.")]
    [SerializeField] private bool _useRelativePlanetSizes = true;
    [Tooltip("Veralteter Fallback-Wert aus dem verworfenen 50-cm-Modus.")]
    [SerializeField] private float fixedPlanetDiameterMeters = 0.5f;
    private const float earthDiameterInKm = 12742f;
    private Transform _resolvedRayOrigin;
    private bool _wasRightIndexPinching;
    private bool _wasLeftIndexPinching;

    private void Awake()
    {
        WarnIfOldFixedSizeModeWasSerialized();
        _useRelativePlanetSizes = true;
    }

    private void Start()
    {
        _useRelativePlanetSizes = true;
        PrepareSunObject();
        ApplyEnvironmentRaycastManagerState();
    }

    private void OnValidate()
    {
        _useRelativePlanetSizes = true;
        ApplyEnvironmentRaycastManagerState();
    }

    // ----------- AUSWAHL: einzelner Planet -----------
    public void SelectPlanet(PlanetData data)
    {
        ClearPreview();

        _isSolarSystemMode = false;
        _currentSolarSystemPrefab = null;
        currentPlanetData = data;

        previewInstance = Instantiate(currentPlanetData.previewPrefab);
        float vrScale = GetTargetPlanetWorldDiameter(currentPlanetData);
        previewInstance.transform.localScale = new Vector3(vrScale, vrScale, vrScale);
        PlanetFactsVisibility.ClearSelection();

        visualizerInstance = Instantiate(placementVisualizerPrefab);
    }

    // ----------- AUSWAHL: Sonnensystem-Prefab -----------
    public void SelectSolarSystem(GameObject solarSystemPrefab)
    {
        SelectSolarSystem(solarSystemPrefab, null);
    }

    public void SelectSolarSystem(GameObject solarSystemPrefab, GameObject previewPrefab)
    {
        ClearPreview();
        DeactivatePlanetSun();

        _isSolarSystemMode = true;
        currentPlanetData = null;
        _currentSolarSystemPrefab = solarSystemPrefab;

        GameObject selectedPreviewPrefab = previewPrefab != null
            ? previewPrefab
            : solarSystemPreviewPrefab != null
                ? solarSystemPreviewPrefab
                : _currentSolarSystemPrefab;

        previewInstance = Instantiate(selectedPreviewPrefab);
        previewInstance.transform.localScale = Vector3.one * Mathf.Max(0.001f, solarSystemPreviewDiameterMeters);
        PlanetFactsVisibility.ClearSelection();

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

    public void DeactivatePlanetSun()
    {
        _hasSpawnedSunForPlanets = false;
        SetSunVisible(false);
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

    public void SetWorldPanelsHiddenByMainMenu(bool isHidden)
    {
        if (isHidden)
        {
            StorePanelVisibilityForMainMenu();
            SetPlanetInfoPanelVisible(false);
            SetSonnensystemUIVisible(false);
            SetDistanceGrabObjectsHiddenByMainMenu(true);
            return;
        }

        RestorePanelVisibilityAfterMainMenu();
        SetDistanceGrabObjectsHiddenByMainMenu(false);
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
        DeactivatePlanetSun();

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

        Transform currentRayOrigin = GetRayOrigin();
        if (currentRayOrigin == null)
        {
            lineRenderer.enabled = false;
            return;
        }

        Ray ray = new Ray(currentRayOrigin.position, currentRayOrigin.forward);
        bool didHit = TryGetPlacementPoint(ray, out Vector3 hitPoint, out Vector3 hitNormal);

        if (didHit == true)
        {
            bool canPlace = IsSimpleRayPlacementActive() || IsHorizontal(hitNormal);

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

            lineRenderer.SetPosition(0, currentRayOrigin.position);
            lineRenderer.SetPosition(1, hitPoint);

            float lift = _isSolarSystemMode
                ? Mathf.Max(0.001f, solarSystemPreviewDiameterMeters) * 0.5f
                : planetHeight;
            Vector3 previewPosition = hitPoint + Vector3.up * lift;
            Quaternion previewRotation = _isSolarSystemMode == true
                ? GetSolarSystemPlacementRotation(previewPosition)
                : Quaternion.identity;

            _lastPlacementPoint = hitPoint;
            _lastPlacementRotation = _isSolarSystemMode == true
                ? GetSolarSystemPlacementRotation(hitPoint)
                : previewRotation;

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

                    _placedSolarSystemObject = Instantiate(_currentSolarSystemPrefab, _lastPlacementPoint, _lastPlacementRotation);
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
                    PlanetSelectable selectable = RegisterSelectablePlanet(spawnedPlanet, currentPlanetData);
                    RegisterInteractionSelectionBridge(spawnedPlanet);
                    _placedPlanetObjects.Add(spawnedPlanet);

                    SpawnSunForFirstPlacedPlanet();
                    ShowPlanetInfo(currentPlanetData, selectable);
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

        if (acceptRawIndexTriggers == true
            && (OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger)
                || OVRInput.GetDown(OVRInput.RawButton.LIndexTrigger)))
        {
            return true;
        }

        return HasIndexPinchDown();
    }

    private Transform GetRayOrigin()
    {
        if (_autoUseChildRayOrigin == false) return rayOrigin;

        if (_resolvedRayOrigin != null && _resolvedRayOrigin.gameObject.activeInHierarchy)
        {
            return _resolvedRayOrigin;
        }

        _resolvedRayOrigin = FindBetterRayOrigin(rayOrigin);
        return _resolvedRayOrigin != null ? _resolvedRayOrigin : rayOrigin;
    }

    private Transform FindBetterRayOrigin(Transform root)
    {
        if (root == null) return null;

        // Der allgemeine HandAnchor liegt oft an der Handwurzel. Fuer Raycasts ist
        // der konkrete Controller-/Pointer-Anchor genauer, auch im Natural-Hand-Modus.
        string[] candidateNames =
        {
            "RightControllerAnchor",
            "LeftControllerAnchor",
            "RightControllerInHandAnchor",
            "LeftControllerInHandAnchor",
            "RightHandOnControllerAnchor",
            "LeftHandOnControllerAnchor"
        };

        for (int i = 0; i < candidateNames.Length; i++)
        {
            Transform candidate = FindDeepChild(root, candidateNames[i]);
            if (candidate != null && candidate.gameObject.activeInHierarchy)
            {
                return candidate;
            }
        }

        return null;
    }

    private Transform FindDeepChild(Transform root, string childName)
    {
        if (root == null) return null;

        foreach (Transform child in root)
        {
            if (child.name == childName)
            {
                return child;
            }

            Transform foundChild = FindDeepChild(child, childName);
            if (foundChild != null)
            {
                return foundChild;
            }
        }

        return null;
    }

    private bool HasIndexPinchDown()
    {
        if (_acceptIndexPinchPlacement == false) return false;

        CacheHandsIfNeeded();

        bool hasRightPinchDown = HasHandIndexPinchDown(_rightHand, ref _wasRightIndexPinching);
        bool hasLeftPinchDown = HasHandIndexPinchDown(_leftHand, ref _wasLeftIndexPinching);
        return hasRightPinchDown || hasLeftPinchDown;
    }

    private void CacheHandsIfNeeded()
    {
        if (_rightHand != null && _leftHand != null) return;

        OVRHand[] hands = FindObjectsByType<OVRHand>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < hands.Length; i++)
        {
            OVRHand hand = hands[i];
            if (hand == null) continue;

            if (hand.GetHand() == OVRPlugin.Hand.HandRight && _rightHand == null)
            {
                _rightHand = hand;
            }
            else if (hand.GetHand() == OVRPlugin.Hand.HandLeft && _leftHand == null)
            {
                _leftHand = hand;
            }
        }
    }

    private bool HasHandIndexPinchDown(OVRHand hand, ref bool wasPinching)
    {
        if (hand == null || hand.IsTracked == false)
        {
            wasPinching = false;
            return false;
        }

        float pinchStrength = hand.GetFingerPinchStrength(OVRHand.HandFinger.Index);
        bool isPinching = wasPinching
            ? pinchStrength > _pinchUpThreshold
            : pinchStrength >= _pinchDownThreshold;

        bool hasPinchDown = isPinching && wasPinching == false;
        wasPinching = isPinching;
        return hasPinchDown;
    }

    private bool TryGetPlacementPoint(Ray ray, out Vector3 point, out Vector3 normal)
    {
        if (IsSimpleRayPlacementActive() == true)
        {
            // Test-Placement: keine Depth-API-Abfrage, sondern fixer Punkt am Ray.
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

    private bool IsSimpleRayPlacementActive()
    {
        return _useEnvironmentRaycastPlacement == false;
    }

    private Quaternion GetSolarSystemPlacementRotation(Vector3 placementPosition)
    {
        if (Mathf.Approximately(solarSystemTiltTowardsUserDegrees, 0f) == true)
        {
            return Quaternion.identity;
        }

        Transform viewerTransform = Camera.main != null ? Camera.main.transform : GetRayOrigin();
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

    private void ApplyEnvironmentRaycastManagerState()
    {
        if (raycastManager == null && _environmentRaycastManagerObject == null) return;

        GameObject targetObject = _environmentRaycastManagerObject != null
            ? _environmentRaycastManagerObject
            : raycastManager.gameObject;

        bool shouldUseRaycastManager = _useEnvironmentRaycastPlacement == true;

        if (targetObject != null && targetObject != gameObject)
        {
            targetObject.SetActive(shouldUseRaycastManager);
        }

        if (raycastManager != null)
        {
            raycastManager.enabled = shouldUseRaycastManager;
        }
    }

    float GetScaledSize(float realSizeInKm)
    {
        return (realSizeInKm / earthDiameterInKm) * earthDiameterInVR;
    }

    private float GetTargetPlanetWorldDiameter(PlanetData data)
    {
        _useRelativePlanetSizes = true;

        if (data == null) return Mathf.Max(0.001f, fixedPlanetDiameterMeters);

        return GetScaledSize(data.diameter);
    }

    public void SetUseRelativePlanetSizes(bool useRelativeSizes)
    {
        // Der Gleichgroessen-Modus ist verworfen. Alte Inspector-Events duerfen
        // deshalb nicht mehr auf feste 50-cm-Planeten zurueckschalten.
        _useRelativePlanetSizes = true;
        UpdateCurrentPlanetPreviewScale();

        if (useRelativeSizes == false)
        {
            Debug.Log("PlacementManager: Gleichgroessen-Modus ist verworfen. Relative Planetengroessen bleiben aktiv.");
        }
    }

    public bool UsesRelativePlanetSizes()
    {
        _useRelativePlanetSizes = true;
        return _useRelativePlanetSizes;
    }

    private void WarnIfOldFixedSizeModeWasSerialized()
    {
        if (_useRelativePlanetSizes == true) return;

        Debug.Log("PlacementManager: Alter Gleichgroessen-Modus aus der Szene wird ignoriert. Relative Planetengroessen bleiben aktiv.");
    }

    private void UpdateCurrentPlanetPreviewScale()
    {
        if (previewInstance == null || currentPlanetData == null || _isSolarSystemMode == true) return;

        float targetDiameter = GetTargetPlanetWorldDiameter(currentPlanetData);
        previewInstance.transform.localScale = Vector3.one * targetDiameter;
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
        float targetWorldSize = GetTargetPlanetWorldDiameter(data);
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
            if (PlanetVisualBoundsUtility.ShouldIgnoreRenderer(visualRenderer)) continue;

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

    private PlanetSelectable RegisterSelectablePlanet(GameObject planetObject, PlanetData data)
    {
        if (planetObject == null || data == null) return null;

        PlanetSelectable selectable = planetObject.GetComponent<PlanetSelectable>();
        if (selectable == null)
        {
            selectable = planetObject.AddComponent<PlanetSelectable>();
        }

        selectable.planetData = data;
        return selectable;
    }

    private void RegisterInteractionSelectionBridge(GameObject planetObject)
    {
        if (planetObject == null) return;

        PlanetInteractionSelectionBridge bridge = planetObject.GetComponent<PlanetInteractionSelectionBridge>();
        if (bridge == null)
        {
            bridge = planetObject.AddComponent<PlanetInteractionSelectionBridge>();
        }

        bridge.Refresh();
    }

    private void ShowPlanetInfo(PlanetData data, PlanetSelectable selectedPlanet)
    {
        PlanetInfoPanelManager manager = planetInfoPanelManager != null
            ? planetInfoPanelManager
            : PlanetInfoPanelManager.Instance;

        if (manager != null)
        {
            manager.ShowPlanet(data, selectedPlanet);
        }
    }

    private void SetupSonnensystemUI(GameObject solarSystemObject)
    {
        if (solarSystemObject == null) return;

        PlanetFactsVisibility.ClearSelection();

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

    private void PrepareSunObject()
    {
        if (_hideSunUntilFirstPlanetPlacement == false) return;

        SetSunVisible(false);
    }

    private void SpawnSunForFirstPlacedPlanet()
    {
        if (_hasSpawnedSunForPlanets) return;

        GameObject sunObjectToPlace = GetSunObjectToPlace();
        if (sunObjectToPlace == null)
        {
            Debug.LogWarning("PlacementManager: Kein GameObject fuer das Sun-Placement gefunden.");
            return;
        }

        Transform userTransform = GetUserCameraTransform();
        if (userTransform == null)
        {
            Debug.LogWarning("PlacementManager: Keine Kamera fuer das Sun-Placement gefunden.");
            return;
        }

        Vector3 horizontalForward = userTransform.forward;
        horizontalForward.y = 0f;

        if (horizontalForward.sqrMagnitude < 0.0001f)
        {
            horizontalForward = Vector3.forward;
        }

        Quaternion horizontalRotation = Quaternion.LookRotation(horizontalForward.normalized, Vector3.up);
        sunObjectToPlace.transform.SetPositionAndRotation(userTransform.position, horizontalRotation);

        SetSunVisible(true);
        _hasSpawnedSunForPlanets = true;
    }

    private Transform GetUserCameraTransform()
    {
        if (_userCamera != null) return _userCamera;
        if (Camera.main != null) return Camera.main.transform;
        return GetRayOrigin();
    }

    private void SetSunVisible(bool isVisible)
    {
        GameObject sunObjectToPlace = GetSunObjectToPlace();
        if (sunObjectToPlace == null) return;

        sunObjectToPlace.SetActive(isVisible);
    }

    private GameObject GetSunObjectToPlace()
    {
        return _sunPlacementObject;
    }

    private void StorePanelVisibilityForMainMenu()
    {
        if (_hasStoredPanelVisibilityForMainMenu) return;

        PlanetInfoPanelManager manager = GetPlanetInfoPanelManager();
        _wasPlanetInfoPanelVisibleBeforeMainMenu = manager != null
            && manager.infoPanel != null
            && manager.infoPanel.gameObject.activeSelf;

        _wasSonnensystemUIVisibleBeforeMainMenu = _currentSonnensystemUI != null
            && _currentSonnensystemUI.gameObject.activeSelf;

        _hasStoredPanelVisibilityForMainMenu = true;
    }

    private void RestorePanelVisibilityAfterMainMenu()
    {
        if (_hasStoredPanelVisibilityForMainMenu == false) return;

        SetPlanetInfoPanelVisible(_wasPlanetInfoPanelVisibleBeforeMainMenu);
        SetSonnensystemUIVisible(_wasSonnensystemUIVisibleBeforeMainMenu);

        if (_wasPlanetInfoPanelVisibleBeforeMainMenu)
        {
            PlanetInfoPanelManager manager = GetPlanetInfoPanelManager();
            if (manager != null)
            {
                manager.PositionPanelNextToUser();
            }
        }

        _hasStoredPanelVisibilityForMainMenu = false;
    }

    private void SetPlanetInfoPanelVisible(bool isVisible)
    {
        PlanetInfoPanelManager manager = GetPlanetInfoPanelManager();
        if (manager == null || manager.infoPanel == null) return;

        manager.infoPanel.gameObject.SetActive(isVisible);
    }

    private void SetSonnensystemUIVisible(bool isVisible)
    {
        if (_currentSonnensystemUI == null) return;

        _currentSonnensystemUI.gameObject.SetActive(isVisible);
    }

    private PlanetInfoPanelManager GetPlanetInfoPanelManager()
    {
        return planetInfoPanelManager != null
            ? planetInfoPanelManager
            : PlanetInfoPanelManager.Instance;
    }

    private void SetDistanceGrabObjectsHiddenByMainMenu(bool isHidden)
    {
        if (isHidden)
        {
            StoreDistanceGrabObjectStatesForMainMenu();
            SetDistanceGrabObjectsVisible(false);
            return;
        }

        RestoreDistanceGrabObjectStatesAfterMainMenu();
    }

    private void StoreDistanceGrabObjectStatesForMainMenu()
    {
        if (_hasStoredDistanceGrabObjectStatesForMainMenu) return;

        _distanceGrabObjectStatesBeforeMainMenu.Clear();
        StoreDistanceGrabObjectStates(_placedSolarSystemObject);

        for (int i = 0; i < _placedPlanetObjects.Count; i++)
        {
            StoreDistanceGrabObjectStates(_placedPlanetObjects[i]);
        }

        _hasStoredDistanceGrabObjectStatesForMainMenu = true;
    }

    private void StoreDistanceGrabObjectStates(GameObject root)
    {
        if (root == null) return;

        Transform[] children = root.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in children)
        {
            if (child == null) continue;
            if (child.name != "ISDK_DistanceHandGrabInteraction") continue;

            _distanceGrabObjectStatesBeforeMainMenu.Add(new GameObjectState(child.gameObject, child.gameObject.activeSelf));
        }
    }

    private void SetDistanceGrabObjectsVisible(bool isVisible)
    {
        SetDistanceGrabObjectsVisible(_placedSolarSystemObject, isVisible);

        for (int i = 0; i < _placedPlanetObjects.Count; i++)
        {
            SetDistanceGrabObjectsVisible(_placedPlanetObjects[i], isVisible);
        }
    }

    private void SetDistanceGrabObjectsVisible(GameObject root, bool isVisible)
    {
        if (root == null) return;

        Transform[] children = root.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in children)
        {
            if (child == null) continue;
            if (child.name != "ISDK_DistanceHandGrabInteraction") continue;

            child.gameObject.SetActive(isVisible);
        }
    }

    private void RestoreDistanceGrabObjectStatesAfterMainMenu()
    {
        if (_hasStoredDistanceGrabObjectStatesForMainMenu == false) return;

        for (int i = 0; i < _distanceGrabObjectStatesBeforeMainMenu.Count; i++)
        {
            GameObjectState state = _distanceGrabObjectStatesBeforeMainMenu[i];
            if (state.GameObject != null)
            {
                state.GameObject.SetActive(state.WasActive);
            }
        }

        _distanceGrabObjectStatesBeforeMainMenu.Clear();
        _hasStoredDistanceGrabObjectStatesForMainMenu = false;
    }

    private readonly struct GameObjectState
    {
        public readonly GameObject GameObject;
        public readonly bool WasActive;

        public GameObjectState(GameObject gameObject, bool wasActive)
        {
            GameObject = gameObject;
            WasActive = wasActive;
        }
    }
}
