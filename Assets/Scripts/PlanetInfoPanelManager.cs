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
    public float distanceInFront = 1.1f;
    public float sideOffset = 0.45f;
    public float heightOffset = -0.1f;

    private PlanetData _currentPlanetData;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (infoPanel != null)
        {
            infoPanel.gameObject.SetActive(false);
        }
    }

    public void ShowPlanet(PlanetData data)
    {
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
        infoPanel.Bind(data);
        infoPanel.gameObject.SetActive(true);
        PositionPanelNextToUser();
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

        Vector3 position = cameraTransform.position
            + cameraTransform.forward * distanceInFront
            + cameraTransform.right * sideOffset
            + Vector3.up * heightOffset;

        Quaternion rotation = Quaternion.LookRotation(position - cameraTransform.position);
        infoPanel.transform.SetPositionAndRotation(position, rotation);
    }

    private Transform GetUserCamera()
    {
        if (userCamera != null) return userCamera;
        if (Camera.main != null) return Camera.main.transform;
        return null;
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
