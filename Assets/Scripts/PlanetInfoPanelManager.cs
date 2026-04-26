using UnityEngine;

// Verwaltet genau ein globales Info-Panel und positioniert es neben dem User.
public class PlanetInfoPanelManager : MonoBehaviour
{
    public static PlanetInfoPanelManager Instance { get; private set; }

    [Header("Panel")]
    public PlanetInfoPanel infoPanel;

    [Header("Position")]
    [Tooltip("Wenn leer, wird Camera.main benutzt.")]
    public Transform userCamera;
    public float distanceInFront = 1.1f;
    public float sideOffset = 0.45f;
    public float heightOffset = -0.1f;

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
}
