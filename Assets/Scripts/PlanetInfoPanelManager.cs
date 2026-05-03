using UnityEngine;

// Verwaltet genau ein globales Info-Panel und positioniert es neben dem User.
public class PlanetInfoPanelManager : MonoBehaviour
{
    public static PlanetInfoPanelManager Instance { get; private set; }

    [Header("Panel")]
    public PlanetInfoPanel infoPanel;

    [Header("Immersive Mode")]
    [SerializeField] private ImmersiveModeController immersiveModeController;

    [Header("Position")]
    [Tooltip("Wenn leer, wird Camera.main benutzt.")]
    public Transform userCamera;
    [Tooltip("Meter vor dem Spieler, auf Basis der horizontalen Blickrichtung.")]
    public float distanceInFront = 0.45f;

    [Tooltip("Meter zur Seite. Negative Werte liegen links vom Spieler.")]
    public float sideOffset = -0.45f;

    [Tooltip("Hoehenoffset relativ zur Kopfkamera. -0.55m liegt etwa leicht ueber Huefthoehe.")]
    public float heightOffset = -0.6f;

    private PlanetData _currentPlanetData;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        UsePlanetDetailRootIfAvailable();
    }

    private void Start()
    {
        UsePlanetDetailRootIfAvailable();

        if (infoPanel != null)
        {
            infoPanel.gameObject.SetActive(false);
        }
    }

    public void ShowPlanet(PlanetData data)
    {
        UsePlanetDetailRootIfAvailable();

        if (data == null)
        {
            Debug.LogWarning("PlanetInfoPanelManager: Kein PlanetData uebergeben.");
            return;
        }

        if (infoPanel == null)
        {
            Debug.LogWarning("PlanetInfoPanelManager: InfoPanel ist nicht zugewiesen.");
            return;
        }

        _currentPlanetData = data;
        bool wasPanelVisible = infoPanel.gameObject.activeSelf;

        infoPanel.Bind(data);
        infoPanel.gameObject.SetActive(true);

        if (wasPanelVisible == false)
        {
            PositionPanelNextToUser();
        }
    }

    public void HidePanel()
    {
        if (infoPanel != null)
        {
            infoPanel.gameObject.SetActive(false);
        }
    }

    public void ToggleImmersiveMode(PlanetData data)
    {
        ImmersiveModeController controller = GetImmersiveModeController();
        if (controller != null && controller.IsImmersiveActive)
        {
            controller.ExitImmersiveMode();
            return;
        }

        EnterImmersiveMode(data);
    }

    public void EnterImmersiveMode(PlanetData data)
    {
        PlanetData targetData = data != null ? data : _currentPlanetData;

        if (targetData == null)
        {
            Debug.LogWarning("PlanetInfoPanelManager: Kein Planet fuer den Immersive Mode vorhanden.");
            return;
        }

        ImmersiveModeController controller = GetImmersiveModeController();
        if (controller == null)
        {
            Debug.LogWarning("PlanetInfoPanelManager: Kein ImmersiveModeController gefunden.");
            return;
        }

        controller.EnterImmersiveMode(targetData);

        if (infoPanel != null)
        {
            infoPanel.gameObject.SetActive(true);
            infoPanel.Bind(targetData);
            infoPanel.SetImmersiveModeActive(true);
            PositionPanelNextToUser();
        }
    }

    public void NotifyImmersiveModeClosed()
    {
        if (infoPanel != null)
        {
            infoPanel.SetImmersiveModeActive(false);

            if (_currentPlanetData != null)
            {
                infoPanel.gameObject.SetActive(true);
                PositionPanelNextToUser();
            }
        }
    }

    public void PositionPanelNextToUser()
    {
        if (infoPanel == null) return;

        Transform cameraTransform = GetUserCamera();
        if (cameraTransform == null) return;

        Vector3 forward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
        if (forward.sqrMagnitude < 0.001f)
        {
            forward = Vector3.ProjectOnPlane(cameraTransform.parent != null ? cameraTransform.parent.forward : Vector3.forward, Vector3.up).normalized;
        }

        Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;

        Vector3 position = cameraTransform.position
            + forward * distanceInFront
            + right * sideOffset
            + Vector3.up * heightOffset;

        // Das Panel bleibt aufrecht und orientiert sich zur horizontalen Spielerposition.
        Vector3 lookDirection = Vector3.ProjectOnPlane(position - cameraTransform.position, Vector3.up).normalized;
        Quaternion rotation = Quaternion.LookRotation(lookDirection);
        infoPanel.transform.SetPositionAndRotation(position, rotation);
    }

    private Transform GetUserCamera()
    {
        if (userCamera != null) return userCamera;
        if (Camera.main != null) return Camera.main.transform;
        return null;
    }

    private void UsePlanetDetailRootIfAvailable()
    {
        GameObject detailRoot = GameObject.Find("PlanetDetailRoot");
        if (detailRoot == null) return;

        PlanetInfoPanel detailPanel = detailRoot.GetComponentInChildren<PlanetInfoPanel>(true);
        if (detailPanel == null)
        {
            detailPanel = detailRoot.AddComponent<PlanetInfoPanel>();
        }

        if (infoPanel != null && infoPanel != detailPanel)
        {
            infoPanel.gameObject.SetActive(false);
        }

        infoPanel = detailPanel;
    }

    private ImmersiveModeController GetImmersiveModeController()
    {
        if (immersiveModeController != null) return immersiveModeController;

        immersiveModeController = FindFirstObjectByType<ImmersiveModeController>();
        if (immersiveModeController != null) return immersiveModeController;

        GameObject controllerObject = new GameObject("ImmersiveModeController");
        immersiveModeController = controllerObject.AddComponent<ImmersiveModeController>();
        return immersiveModeController;
    }
}
