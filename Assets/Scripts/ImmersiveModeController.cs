using MRMotifs.PassthroughTransitioning;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Schaltet eine einfache VR-Naturszene ein und zeigt den gewaehlten Planeten
// an der Stelle, an der sonst der Mond am Himmel stehen wuerde.
public class ImmersiveModeController : MonoBehaviour
{
    [Header("Szene")]
    [SerializeField] private GameObject immersiveModeRoot;
    [SerializeField] private Transform planetSpawnPoint;
    [SerializeField] private Transform moonAnchor;
    [SerializeField] private Transform planetContainer;
    [SerializeField] private Transform userCamera;
    [SerializeField] private PlacementManager placementManager;
    [SerializeField] private bool alignRootToUserOnEnter;
    [SerializeField] private bool createFallbackEnvironment = true;

    [Header("UI")]
    [SerializeField] private Button exitButton;

    [Header("Passthrough / VR")]
    [SerializeField] private PassthroughDissolver passthroughDissolver;
    [SerializeField] private bool restorePassthroughOnExit = true;

    [Header("Mond-Vergleich")]
    [Tooltip("Sichtbarer Mond-Durchmesser in Metern bei 12m Distanz.")]
    [SerializeField] private float moonVisualDiameter = 0.109f;

    [Tooltip("Wenn aktiv, wird die Mond-Groesse passend zur Entfernung des SpawnPoints berechnet.")]
    [SerializeField] private bool calculateMoonDiameterFromSpawnDistance = true;

    [Tooltip("Scheinbarer Durchmesser des echten Mondes am Himmel in Grad.")]
    [SerializeField] private float moonAngularDiameterDegrees = 0.5f;

    [Tooltip("Lokale Position des Mond-Ankers relativ zum User-Startpunkt.")]
    [SerializeField] private Vector3 moonLocalPosition = new Vector3(0f, 4.4f, 12f);

    [Tooltip("Langsame Rotation des angezeigten Planeten in Grad pro Sekunde.")]
    [SerializeField] private float planetRotationSpeed = 6f;

    private const float MoonDiameterKm = 3474.8f;
    private GameObject _spawnedPlanet;
    private bool _isImmersiveActive;
    private bool _wasPassthroughActive;
    private Camera _userCameraComponent;
    private float _originalFarClipPlane;
    private bool _hasOriginalFarClipPlane;

    public bool IsImmersiveActive => _isImmersiveActive;

    private void Awake()
    {
        FindMissingReferences();
        EnsureImmersiveScene();

        if (immersiveModeRoot != null)
        {
            immersiveModeRoot.SetActive(false);
        }

        if (exitButton != null)
        {
            exitButton.onClick.AddListener(ExitImmersiveMode);
        }
    }

    private void OnDestroy()
    {
        if (exitButton != null)
        {
            exitButton.onClick.RemoveListener(ExitImmersiveMode);
        }
    }

    private void Update()
    {
        if (_isImmersiveActive == false || _spawnedPlanet == null) return;

        _spawnedPlanet.transform.Rotate(Vector3.up, planetRotationSpeed * Time.deltaTime, Space.World);
    }

    public void EnterImmersiveMode(PlanetData planetData)
    {
        if (planetData == null)
        {
            Debug.LogWarning("ImmersiveModeController: Kein PlanetData uebergeben.");
            return;
        }

        if (planetData.planetName == "Sonne")
        {
            Debug.LogWarning("ImmersiveModeController: Die Sonne wird im Mond-Vergleich nicht angezeigt.");
            return;
        }

        if (planetData.planetPrefab == null)
        {
            Debug.LogWarning("ImmersiveModeController: Kein planetPrefab in PlanetData hinterlegt: " + planetData.planetName);
            return;
        }

        FindMissingReferences();
        EnsureImmersiveScene();

        if (immersiveModeRoot == null || planetSpawnPoint == null || planetContainer == null)
        {
            Debug.LogWarning("ImmersiveModeController: ImmersiveModeRoot, PlanetSpawnPoint oder PlanetContainer fehlt.");
            return;
        }

        _wasPassthroughActive = passthroughDissolver != null && passthroughDissolver.IsPassthroughActive;

        if (alignRootToUserOnEnter)
        {
            PositionSceneRelativeToUser();
        }

        immersiveModeRoot.SetActive(true);

        ClearSpawnedPlanet();
        _spawnedPlanet = Instantiate(planetData.planetPrefab, planetContainer);
        _spawnedPlanet.name = "ImmersivePlanet_" + planetData.planetName;
        _spawnedPlanet.transform.localPosition = Vector3.zero;
        _spawnedPlanet.transform.localRotation = Quaternion.Euler(0f, 0f, -planetData.axialTilt);
        _spawnedPlanet.transform.localScale = Vector3.one * GetPlanetWorldDiameter(planetData);

        AdjustCameraFarClipForSpawnedPlanet();
        DisableRuntimePlanetSystems(_spawnedPlanet);

        if (passthroughDissolver != null)
        {
            passthroughDissolver.SetPassthroughActive(false);
        }

        if (placementManager != null)
        {
            placementManager.DeactivatePlanetSun();
            placementManager.SetPlacedContentVisible(false);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetState(GameState.IMMERSIVE);
        }

        _isImmersiveActive = true;
    }

    public void ExitImmersiveMode()
    {
        ClearSpawnedPlanet();
        _isImmersiveActive = false;

        if (immersiveModeRoot != null)
        {
            immersiveModeRoot.SetActive(false);
        }

        if (restorePassthroughOnExit && passthroughDissolver != null)
        {
            passthroughDissolver.SetPassthroughActive(_wasPassthroughActive);
        }

        RestoreCameraFarClip();

        if (placementManager != null)
        {
            placementManager.SetPlacedContentVisible(true);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetState(GameState.WORLD);
        }

        if (PlanetInfoPanelManager.Instance != null)
        {
            PlanetInfoPanelManager.Instance.NotifyImmersiveModeClosed();
        }
    }

    private void FindMissingReferences()
    {
        if (userCamera == null && Camera.main != null)
        {
            userCamera = Camera.main.transform;
        }

        if (_userCameraComponent == null)
        {
            if (userCamera != null)
            {
                _userCameraComponent = userCamera.GetComponent<Camera>();
            }

            if (_userCameraComponent == null && Camera.main != null)
            {
                _userCameraComponent = Camera.main;
            }
        }

        if (passthroughDissolver == null)
        {
            passthroughDissolver = FindFirstObjectByType<PassthroughDissolver>();
        }

        if (placementManager == null)
        {
            placementManager = FindFirstObjectByType<PlacementManager>();
        }

        if (planetSpawnPoint == null)
        {
            planetSpawnPoint = FindFirstTransformByNameIncludingInactive(
                "PlanetSpawnPoint",
                "ImmersivePlanetSpawnPoint",
                "SpawnPoint",
                "MoonAnchor");
        }
    }

    private void EnsureImmersiveScene()
    {
        if (immersiveModeRoot == null)
        {
            Transform foundRoot = FindFirstTransformByNameIncludingInactive(
                "ImmersiveModeRoot",
                "ImmersiveSceneRoot",
                "OutdoorScene");

            if (foundRoot != null)
            {
                immersiveModeRoot = foundRoot.gameObject;
            }
        }

        if (immersiveModeRoot == null)
        {
            immersiveModeRoot = new GameObject("ImmersiveModeRoot");
        }

        if (planetSpawnPoint == null && moonAnchor != null)
        {
            planetSpawnPoint = moonAnchor;
        }

        if (planetSpawnPoint == null)
        {
            planetSpawnPoint = FindOrCreateChild(immersiveModeRoot.transform, "PlanetSpawnPoint");
            planetSpawnPoint.localPosition = moonLocalPosition;
        }

        if (moonAnchor == null)
        {
            moonAnchor = planetSpawnPoint;
        }

        if (planetContainer == null)
        {
            planetContainer = FindOrCreateChild(planetSpawnPoint, "PlanetContainer");
            planetContainer.localPosition = Vector3.zero;
        }

        if (createFallbackEnvironment && HasCustomImmersiveContent() == false)
        {
            EnsureRuntimeEnvironment();
        }

        EnsureExitButton();
    }

    private Transform FindOrCreateChild(Transform parent, string childName)
    {
        Transform child = parent.Find(childName);
        if (child != null) return child;

        GameObject childObject = new GameObject(childName);
        childObject.transform.SetParent(parent, false);
        return childObject.transform;
    }

    private Transform FindFirstTransformByNameIncludingInactive(params string[] objectNames)
    {
        Transform[] transforms = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (string objectName in objectNames)
        {
            foreach (Transform targetTransform in transforms)
            {
                if (targetTransform.name == objectName)
                {
                    return targetTransform;
                }
            }
        }

        return null;
    }

    private void PositionSceneRelativeToUser()
    {
        if (userCamera == null) return;

        Vector3 cameraPosition = userCamera.position;
        Vector3 forward = userCamera.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude < 0.001f)
        {
            forward = Vector3.forward;
        }

        immersiveModeRoot.transform.SetPositionAndRotation(
            new Vector3(cameraPosition.x, 0f, cameraPosition.z),
            Quaternion.LookRotation(forward.normalized, Vector3.up));

        planetSpawnPoint.localPosition = moonLocalPosition;
    }

    private float GetPlanetWorldDiameter(PlanetData planetData)
    {
        return GetCurrentMoonVisualDiameter() * (planetData.diameter / MoonDiameterKm);
    }

    private float GetCurrentMoonVisualDiameter()
    {
        if (calculateMoonDiameterFromSpawnDistance == false)
        {
            return moonVisualDiameter;
        }

        if (userCamera == null || planetSpawnPoint == null)
        {
            return moonVisualDiameter;
        }

        float distanceToSpawnPoint = Vector3.Distance(userCamera.position, planetSpawnPoint.position);
        float halfAngleRadians = moonAngularDiameterDegrees * 0.5f * Mathf.Deg2Rad;
        return 2f * distanceToSpawnPoint * Mathf.Tan(halfAngleRadians);
    }

    private void ClearSpawnedPlanet()
    {
        if (_spawnedPlanet == null) return;

        Destroy(_spawnedPlanet);
        _spawnedPlanet = null;
    }

    private void DisableRuntimePlanetSystems(GameObject planetObject)
    {
        PlanetBody[] planetBodies = planetObject.GetComponentsInChildren<PlanetBody>(true);
        foreach (PlanetBody planetBody in planetBodies)
        {
            planetBody.enabled = false;
        }

        PlanetSelectable[] selectables = planetObject.GetComponentsInChildren<PlanetSelectable>(true);
        foreach (PlanetSelectable selectable in selectables)
        {
            selectable.enabled = false;
        }
    }

    private void AdjustCameraFarClipForSpawnedPlanet()
    {
        if (_userCameraComponent == null || _spawnedPlanet == null) return;

        if (_hasOriginalFarClipPlane == false)
        {
            _originalFarClipPlane = _userCameraComponent.farClipPlane;
            _hasOriginalFarClipPlane = true;
        }

        float distanceToPlanet = Vector3.Distance(_userCameraComponent.transform.position, _spawnedPlanet.transform.position);
        float planetDiameter = GetWorldBoundsSize(_spawnedPlanet);
        float neededFarClip = distanceToPlanet + planetDiameter + 100f;

        if (_userCameraComponent.farClipPlane < neededFarClip)
        {
            _userCameraComponent.farClipPlane = neededFarClip;
        }
    }

    private void RestoreCameraFarClip()
    {
        if (_userCameraComponent == null || _hasOriginalFarClipPlane == false) return;

        _userCameraComponent.farClipPlane = _originalFarClipPlane;
        _hasOriginalFarClipPlane = false;
    }

    private float GetWorldBoundsSize(GameObject targetObject)
    {
        Renderer[] renderers = targetObject.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0) return targetObject.transform.lossyScale.magnitude;

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        return bounds.size.magnitude;
    }

    private void EnsureRuntimeEnvironment()
    {
        Transform existingEnvironment = immersiveModeRoot.transform.Find("RuntimeEnvironment");
        if (existingEnvironment != null) return;

        Transform environment = FindOrCreateChild(immersiveModeRoot.transform, "RuntimeEnvironment");

        Material groundMaterial = CreateMaterial("Immersive_Ground", new Color(0.025f, 0.05f, 0.035f, 1f));
        Material skyMaterial = CreateMaterial("Immersive_Sky", new Color(0.005f, 0.008f, 0.018f, 1f));
        Material silhouetteMaterial = CreateMaterial("Immersive_Silhouette", new Color(0.01f, 0.018f, 0.012f, 1f));

        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.SetParent(environment, false);
        ground.transform.localScale = new Vector3(8f, 1f, 8f);
        ground.GetComponent<Renderer>().sharedMaterial = groundMaterial;

        GameObject skyDome = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        skyDome.name = "SkyDome";
        skyDome.transform.SetParent(environment, false);
        skyDome.transform.localPosition = new Vector3(0f, 8f, 0f);
        skyDome.transform.localScale = Vector3.one * 50f;
        skyDome.GetComponent<Renderer>().sharedMaterial = skyMaterial;
        Destroy(skyDome.GetComponent<Collider>());

        CreateTree(environment, new Vector3(-5f, 0f, 8f), 2.8f, silhouetteMaterial);
        CreateTree(environment, new Vector3(-2f, 0f, 10f), 3.4f, silhouetteMaterial);
        CreateTree(environment, new Vector3(3.5f, 0f, 9f), 3.1f, silhouetteMaterial);
        CreateTree(environment, new Vector3(6f, 0f, 7f), 2.6f, silhouetteMaterial);
        CreateTree(environment, new Vector3(-7f, 0f, 4f), 2.4f, silhouetteMaterial);
        CreateTree(environment, new Vector3(7f, 0f, 3f), 2.2f, silhouetteMaterial);

        GameObject lightObject = new GameObject("ImmersiveMoonLight", typeof(Light));
        lightObject.transform.SetParent(environment, false);
        lightObject.transform.localRotation = Quaternion.Euler(45f, -30f, 0f);

        Light light = lightObject.GetComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 0.35f;
        light.color = new Color(0.68f, 0.78f, 1f, 1f);
    }

    private bool HasCustomImmersiveContent()
    {
        if (immersiveModeRoot == null) return false;

        foreach (Transform child in immersiveModeRoot.transform)
        {
            string childName = child.name;
            if (childName == "PlanetSpawnPoint"
                || childName == "MoonAnchor"
                || childName == "PlanetContainer"
                || childName == "ExitCanvas"
                || childName == "RuntimeEnvironment")
            {
                continue;
            }

            return true;
        }

        return false;
    }

    private void EnsureExitButton()
    {
        if (exitButton != null) return;

        Transform existingCanvas = immersiveModeRoot.transform.Find("ExitCanvas");
        if (existingCanvas != null)
        {
            exitButton = existingCanvas.GetComponentInChildren<Button>(true);
            if (exitButton != null) return;
        }

        GameObject canvasObject = new GameObject("ExitCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(immersiveModeRoot.transform, false);
        canvasObject.transform.localPosition = new Vector3(0f, 1.35f, 2.4f);
        canvasObject.transform.localRotation = Quaternion.identity;
        canvasObject.transform.localScale = Vector3.one * 0.0015f;

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(360f, 90f);

        GameObject buttonObject = new GameObject("Button_ExitImmersiveMode", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(canvasObject.transform, false);

        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = Vector2.zero;
        buttonRect.anchorMax = Vector2.one;
        buttonRect.offsetMin = Vector2.zero;
        buttonRect.offsetMax = Vector2.zero;

        Image buttonImage = buttonObject.GetComponent<Image>();
        buttonImage.color = new Color(0.08f, 0.08f, 0.1f, 0.9f);

        exitButton = buttonObject.GetComponent<Button>();

        GameObject textObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(buttonObject.transform, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TMP_Text label = textObject.GetComponent<TMP_Text>();
        label.text = "Exit";
        label.fontSize = 34f;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
    }

    private void CreateTree(Transform parent, Vector3 localPosition, float height, Material material)
    {
        GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cube);
        trunk.name = "Tree_Trunk";
        trunk.transform.SetParent(parent, false);
        trunk.transform.localPosition = localPosition + Vector3.up * (height * 0.25f);
        trunk.transform.localScale = new Vector3(0.18f, height * 0.5f, 0.18f);
        trunk.GetComponent<Renderer>().sharedMaterial = material;

        GameObject crown = new GameObject("Tree_Crown", typeof(MeshFilter), typeof(MeshRenderer));
        crown.transform.SetParent(parent, false);
        crown.transform.localPosition = localPosition + Vector3.up * (height * 0.72f);
        crown.transform.localScale = Vector3.one * height;
        crown.GetComponent<MeshFilter>().sharedMesh = CreatePyramidMesh();
        crown.GetComponent<MeshRenderer>().sharedMaterial = material;
    }

    private Mesh CreatePyramidMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "LowPolyTreeCrown";

        Vector3[] vertices =
        {
            new Vector3(-0.35f, 0f, -0.35f),
            new Vector3(0.35f, 0f, -0.35f),
            new Vector3(0.35f, 0f, 0.35f),
            new Vector3(-0.35f, 0f, 0.35f),
            new Vector3(0f, 0.7f, 0f)
        };

        int[] triangles =
        {
            0, 4, 1,
            1, 4, 2,
            2, 4, 3,
            3, 4, 0,
            0, 1, 2,
            0, 2, 3
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        return mesh;
    }

    private Material CreateMaterial(string materialName, Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
        {
            shader = Shader.Find("Unlit/Color");
        }

        Material material = new Material(shader);
        material.name = materialName;

        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }
        else if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", color);
        }

        if (material.HasProperty("_Cull"))
        {
            material.SetFloat("_Cull", 0f);
        }

        return material;
    }
}
