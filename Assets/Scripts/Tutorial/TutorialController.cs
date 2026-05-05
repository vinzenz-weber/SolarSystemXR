using System.Collections;
using UnityEngine;

// Steuert die Tutorial-Schritte in der Startsequenz.
// Die einzelnen Step-Prefabs enthalten nur UI, Video und kleine Trigger-Scripts.
public class TutorialController : MonoBehaviour
{
    [Header("Tutorial-Schritte")]
    [Tooltip("Prefab-Reihenfolge: z.B. Pinch, Menue-Geste, UI greifen.")]
    [SerializeField] private GameObject[] stepPrefabs;

    [Tooltip("Parent fuer die erzeugten Step-Prefabs. Leer = dieses GameObject.")]
    [SerializeField] private Transform stepParent;

    [Header("Timing")]
    [Tooltip("Kurze Pause zwischen zwei Schritten.")]
    [SerializeField] private float pauseBetweenSteps = 0.25f;

    [Header("Positionierung")]
    [Tooltip("Tutorial-Panels werden einmalig in diesem Abstand vor dem User platziert.")]
    [SerializeField] private float spawnDistance = 1f;

    [Tooltip("Vertikaler Versatz relativ zur Augenhoehe.")]
    [SerializeField] private float heightOffset = 0f;

    [Tooltip("Wenn aktiv, wird das TutorialRoot beim Start versteckt.")]
    [SerializeField] private bool hideOnAwake = true;

    private TutorialStepSignal _activeStepSignal;
    private GameObject _activeStepObject;
    private bool _isStepCompleted;
    private bool _isRunning;

    public bool HasSteps => stepPrefabs != null && stepPrefabs.Length > 0;
    public bool IsRunning => _isRunning;

    private void Awake()
    {
        if (stepParent == null)
        {
            stepParent = transform;
        }

        if (hideOnAwake)
        {
            gameObject.SetActive(false);
        }
    }

    public IEnumerator RunTutorial()
    {
        if (HasSteps == false)
        {
            yield break;
        }

        _isRunning = true;
        gameObject.SetActive(true);

        for (int i = 0; i < stepPrefabs.Length; i++)
        {
            yield return RunStep(stepPrefabs[i], i);
        }

        ClearActiveStep();
        gameObject.SetActive(false);
        _isRunning = false;
    }

    public void CompleteCurrentStep()
    {
        if (_isRunning == false) return;

        _isStepCompleted = true;
    }

    private IEnumerator RunStep(GameObject stepPrefab, int stepIndex)
    {
        ClearActiveStep();
        _isStepCompleted = false;

        if (stepPrefab == null)
        {
            Debug.LogWarning("TutorialController: Tutorial-Schritt " + stepIndex + " hat kein Prefab.");
            yield break;
        }

        _activeStepObject = Instantiate(stepPrefab, stepParent);
        PlaceStepInFrontOfUser(_activeStepObject);
        _activeStepSignal = _activeStepObject.GetComponentInChildren<TutorialStepSignal>(true);

        if (_activeStepSignal == null)
        {
            _activeStepSignal = _activeStepObject.AddComponent<TutorialStepSignal>();
        }

        _activeStepSignal.Initialize(this);

        while (_isStepCompleted == false)
        {
            yield return null;
        }

        ClearActiveStep();

        if (pauseBetweenSteps > 0f)
        {
            yield return new WaitForSeconds(pauseBetweenSteps);
        }
    }

    private void PlaceStepInFrontOfUser(GameObject stepObject)
    {
        if (stepObject == null) return;

        Transform cameraTransform = Camera.main != null ? Camera.main.transform : null;
        if (cameraTransform == null) return;

        Vector3 forward = new Vector3(cameraTransform.forward.x, 0f, cameraTransform.forward.z).normalized;
        if (forward.sqrMagnitude < 0.001f)
        {
            forward = Vector3.forward;
        }

        Vector3 targetPosition = cameraTransform.position + forward * spawnDistance;
        targetPosition.y = cameraTransform.position.y + heightOffset;
        Quaternion targetRotation = Quaternion.LookRotation(forward, Vector3.up);

        LazyFollowUI[] lazyFollowPanels = stepObject.GetComponentsInChildren<LazyFollowUI>(true);
        if (lazyFollowPanels.Length == 0)
        {
            stepObject.transform.SetPositionAndRotation(targetPosition, targetRotation);
            return;
        }

        for (int i = 0; i < lazyFollowPanels.Length; i++)
        {
            LazyFollowUI lazyFollow = lazyFollowPanels[i];
            if (lazyFollow == null) continue;

            lazyFollow.enabled = false;
            lazyFollow.transform.SetPositionAndRotation(targetPosition, targetRotation);
        }
    }

    private void ClearActiveStep()
    {
        if (_activeStepSignal != null)
        {
            _activeStepSignal.Initialize(null);
            _activeStepSignal = null;
        }

        if (_activeStepObject != null)
        {
            Destroy(_activeStepObject);
            _activeStepObject = null;
        }
    }
}
