using MRMotifs.PassthroughTransitioning;
using UnityEngine;
using UnityEngine.UI;

// Schaltet den vorbereiteten ImmersiveMode in der Szene ein und aus.
// Detail-Panel und platzierte Planeten werden vom PlanetInfoPanelManager passend ein- und ausgeblendet.
public class ImmersiveModeController : MonoBehaviour
{
    [Header("Szene")]
    [Tooltip("Vorhandenes GameObject in der Szene, z.B. 'ImmersiveMode'.")]
    [SerializeField] private GameObject immersiveModeRoot;

    [Tooltip("Optional: Wenn aktiv, wird das ImmersiveMode-Objekt beim Einschalten zur Blickrichtung des Users ausgerichtet.")]
    [SerializeField] private bool alignRootToUserOnEnter;

    [Tooltip("Wenn leer, wird Camera.main benutzt.")]
    [SerializeField] private Transform userCamera;

    [Header("UI")]
    [SerializeField] private Button exitButton;

    [Header("Passthrough / VR")]
    [Tooltip("Passthrough-Fade aus dem MR Motif. Wird automatisch gesucht, wenn leer.")]
    [SerializeField] private PassthroughDissolver passthroughDissolver;

    [Tooltip("Beim Verlassen wird der vorherige Passthrough-Zustand wiederhergestellt.")]
    [SerializeField] private bool restorePassthroughOnExit = true;

    private bool _isImmersiveActive;
    private bool _wasPassthroughActive;

    public bool IsImmersiveActive => _isImmersiveActive;

    private void Awake()
    {
        FindMissingReferences();

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

    public void EnterImmersiveMode(PlanetData planetData)
    {
        EnterImmersiveMode();
    }

    // Kann direkt im Toggle unter On Value Changed (bool) eingetragen werden.
    public void SetImmersiveModeActive(bool isActive)
    {
        if (isActive)
        {
            EnterImmersiveMode();
            return;
        }

        ExitImmersiveMode();
    }

    public void EnterImmersiveMode()
    {
        FindMissingReferences();

        if (immersiveModeRoot == null)
        {
            Debug.LogWarning("ImmersiveModeController: Kein ImmersiveMode-GameObject in der Szene gefunden.");
            return;
        }

        _wasPassthroughActive = passthroughDissolver != null && passthroughDissolver.IsPassthroughActive;

        if (alignRootToUserOnEnter)
        {
            PositionSceneRelativeToUser();
        }

        immersiveModeRoot.SetActive(true);
        Debug.Log("ImmersiveModeController: ImmersiveMode-GameObject aktiviert: " + immersiveModeRoot.name);

        if (passthroughDissolver != null)
        {
            if (passthroughDissolver.gameObject.activeSelf == false)
            {
                passthroughDissolver.gameObject.SetActive(true);
            }

            if (passthroughDissolver.enabled == false)
            {
                passthroughDissolver.enabled = true;
            }

            Debug.Log("ImmersiveModeController: Fade zu VR wird ueber PassthroughDissolver gestartet.");
            passthroughDissolver.SetPassthroughActive(false);
            PlanetFactsVisibility.Refresh();
        }
        else
        {
            Debug.LogWarning("ImmersiveModeController: Kein PassthroughDissolver in der Szene gefunden.");
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetState(GameState.IMMERSIVE);
        }

        _isImmersiveActive = true;
    }

    public void ExitImmersiveMode()
    {
        _isImmersiveActive = false;

        if (immersiveModeRoot != null)
        {
            immersiveModeRoot.SetActive(false);
            Debug.Log("ImmersiveModeController: ImmersiveMode-GameObject deaktiviert: " + immersiveModeRoot.name);
        }

        if (restorePassthroughOnExit && passthroughDissolver != null)
        {
            passthroughDissolver.SetPassthroughActive(_wasPassthroughActive);
            PlanetFactsVisibility.Refresh();
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
        if (immersiveModeRoot == null)
        {
            Transform foundRoot = FindFirstTransformByNameIncludingInactive(
                "ImmersiveMode",
                "ImmersiveModeRoot",
                "ImmersiveSceneRoot");

            if (foundRoot != null)
            {
                immersiveModeRoot = foundRoot.gameObject;
            }
        }

        if (userCamera == null && Camera.main != null)
        {
            userCamera = Camera.main.transform;
        }

        if (passthroughDissolver == null)
        {
            passthroughDissolver = FindPassthroughDissolverIncludingInactive();
        }
    }

    private PassthroughDissolver FindPassthroughDissolverIncludingInactive()
    {
        PassthroughDissolver[] dissolvers = FindObjectsByType<PassthroughDissolver>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        return dissolvers.Length > 0 ? dissolvers[0] : null;
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
        if (immersiveModeRoot == null || userCamera == null) return;

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
    }
}
