using UnityEngine;
using UnityEngine.Video;

// Wird auf ein Tutorial-Step-Prefab gelegt.
// Buttons, Meta-Interactable-Events oder eigene Detector rufen hier CompleteStep() auf.
public class TutorialStepSignal : MonoBehaviour
{
    [Header("Video")]
    [Tooltip("Optionaler VideoPlayer im Step. Wird beim Einblenden automatisch auf Loop gesetzt und gestartet.")]
    [SerializeField] private VideoPlayer videoPlayer;

    private TutorialController _controller;
    private bool _hasGrabStarted;

    private void Awake()
    {
        if (videoPlayer == null)
        {
            videoPlayer = GetComponentInChildren<VideoPlayer>(true);
        }
    }

    private void OnEnable()
    {
        StartLoopVideo();
    }

    public void Initialize(TutorialController controller)
    {
        _controller = controller;
        _hasGrabStarted = false;
    }

    // Fuer Step 1: Im Button.onClick eintragen.
    public void CompleteStep()
    {
        if (_controller == null) return;

        _controller.CompleteCurrentStep();
    }

    // Fuer Step 3: In InteractableUnityEventWrapper.WhenSelect eintragen.
    public void OnGrabStart()
    {
        _hasGrabStarted = true;
    }

    // Fuer Step 3: In InteractableUnityEventWrapper.WhenUnselect eintragen.
    public void OnGrabEnd()
    {
        if (_hasGrabStarted == false) return;

        CompleteStep();
    }

    private void StartLoopVideo()
    {
        if (videoPlayer == null) return;

        videoPlayer.isLooping = true;
        videoPlayer.Play();
    }
}
