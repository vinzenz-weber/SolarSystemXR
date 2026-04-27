using UnityEngine;

// Schaltet den Meta-Controller-als-Hand-Modus nur fuer das Reihenfolge-Minispiel ein.
// Die Controller-Modelle/Rays bleiben davon unberuehrt.
public class ReihenfolgeControllerHandMode : MonoBehaviour
{
    [Header("Meta XR")]
    [Tooltip("SnapExamples nutzt Natural. Dadurch kann der Controller Hand-Grabs ausloesen.")]
    public OVRManager.ControllerDrivenHandPosesType ActivePoseType = OVRManager.ControllerDrivenHandPosesType.Natural;

    private OVRManager.ControllerDrivenHandPosesType _originalPoseType;
    private bool _hasOriginalPoseType;
    private bool _isEnabled;

    public void SetControllerHandMode(bool isEnabled)
    {
        OVRManager manager = OVRManager.instance;
        if (manager == null)
        {
            if (isEnabled)
            {
                Debug.LogWarning("ReihenfolgeControllerHandMode: Kein OVRManager gefunden.");
            }

            return;
        }

        if (isEnabled)
        {
            EnableMode(manager);
            return;
        }

        DisableMode(manager);
    }

    private void EnableMode(OVRManager manager)
    {
        if (_isEnabled) return;

        _originalPoseType = manager.controllerDrivenHandPosesType;
        _hasOriginalPoseType = true;

        manager.controllerDrivenHandPosesType = ActivePoseType;
        _isEnabled = true;

        Debug.Log("ReihenfolgeControllerHandMode: Controller-Hand-Modus aktiv (" + ActivePoseType + ").");
    }

    private void DisableMode(OVRManager manager)
    {
        if (_isEnabled == false) return;

        if (_hasOriginalPoseType)
        {
            manager.controllerDrivenHandPosesType = _originalPoseType;
        }

        _isEnabled = false;

        Debug.Log("ReihenfolgeControllerHandMode: Controller-Hand-Modus wiederhergestellt (" + manager.controllerDrivenHandPosesType + ").");
    }

    private void OnDisable()
    {
        if (_isEnabled && OVRManager.instance != null)
        {
            DisableMode(OVRManager.instance);
        }
    }
}
