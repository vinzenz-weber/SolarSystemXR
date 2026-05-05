using UnityEngine;

// Rudimentaerer Detector fuer den Menue-Schritt.
// Primaerer Pfad: Quest-Start/Menu-Input. Optional kann ein Palm/Wrist-Transform
// zugewiesen werden, um eine Handflaechen-Drehung zu erkennen.
public class TutorialMenuGestureDetector : MonoBehaviour
{
    [Header("Referenzen")]
    [SerializeField] private TutorialStepSignal stepSignal;

    [Tooltip("Optional: Handgelenk- oder Palm-Transform aus dem Rig fuer Hand-Tracking.")]
    [SerializeField] private Transform palmTransform;

    [Tooltip("Leer = Camera.main.")]
    [SerializeField] private Transform userCamera;

    [Header("Erkennung")]
    [Tooltip("Controller/Menu-Button als Fallback akzeptieren.")]
    [SerializeField] private bool acceptStartButton = true;

    [Tooltip("Wie lange die Handpose gehalten werden muss.")]
    [SerializeField] private float requiredHoldTime = 0.4f;

    [Tooltip("Schwellwert fuer Handflaeche nach oben.")]
    [Range(-1f, 1f)]
    [SerializeField] private float palmUpThreshold = 0.65f;

    [Tooltip("Schwellwert fuer Handflaeche grob Richtung Kopf.")]
    [Range(-1f, 1f)]
    [SerializeField] private float palmToUserThreshold = 0.25f;

    private float _holdTimer;

    private void Awake()
    {
        if (stepSignal == null)
        {
            stepSignal = GetComponent<TutorialStepSignal>();
        }

        if (stepSignal == null)
        {
            stepSignal = GetComponentInParent<TutorialStepSignal>();
        }

        if (userCamera == null && Camera.main != null)
        {
            userCamera = Camera.main.transform;
        }
    }

    private void Update()
    {
        if (stepSignal == null) return;

        if (acceptStartButton && OVRInput.GetDown(OVRInput.Button.Start))
        {
            stepSignal.CompleteStep();
            return;
        }

        if (IsPalmMenuPose())
        {
            _holdTimer += Time.deltaTime;
            if (_holdTimer >= requiredHoldTime)
            {
                stepSignal.CompleteStep();
            }

            return;
        }

        _holdTimer = 0f;
    }

    private bool IsPalmMenuPose()
    {
        if (palmTransform == null || userCamera == null) return false;

        Vector3 directionToUser = userCamera.position - palmTransform.position;
        if (directionToUser.sqrMagnitude < 0.001f) return false;

        float palmUpAmount = Vector3.Dot(palmTransform.up, Vector3.up);
        float palmToUserAmount = Vector3.Dot(palmTransform.up, directionToUser.normalized);

        return palmUpAmount >= palmUpThreshold
            && palmToUserAmount >= palmToUserThreshold;
    }
}
