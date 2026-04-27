using Oculus.Interaction;
using System;
using UnityEngine;

// Ein Snap-Ziel fuer genau eine Umlaufbahn.
// CorrectOrbitIndex: 0=Merkur, 1=Venus, ... 7=Neptun.
public class ReihenfolgeOrbitSlot : MonoBehaviour
{
    [Header("Daten")]
    public int CorrectOrbitIndex;

    [Header("Meta SDK")]
    public SnapInteractable SnapInteractable;

    [Header("Feedback")]
    public Renderer FeedbackRenderer;
    public RoundedBoxProperties FeedbackRoundedBox;

    public ReihenfolgePlanet CurrentPlanet { get; private set; }
    public bool HasPlanet => CurrentPlanet != null;
    public bool HasCorrectPlanet => CurrentPlanet != null && CurrentPlanet.OrbitIndex == CorrectOrbitIndex;
    public event Action<ReihenfolgeOrbitSlot> WhenSlotChanged;

    private Color _neutralColor = Color.white;
    private Material _feedbackMaterial;
    private bool _isListening;

    private void Reset()
    {
        AutoFillReferences();
    }

    private void OnValidate()
    {
        AutoFillReferences();
    }

    public void Initialize(int correctOrbitIndex, Color neutralColor)
    {
        CorrectOrbitIndex = correctOrbitIndex;
        _neutralColor = neutralColor;

        CacheFeedbackMaterial();
        StartListeningToSnapEvents();
        ClearCurrentPlanet();
    }

    public void SetCurrentPlanet(ReihenfolgePlanet planet)
    {
        CurrentPlanet = planet;
        WhenSlotChanged?.Invoke(this);
    }

    public void ClearCurrentPlanet()
    {
        CurrentPlanet = null;
        SetFeedbackColor(_neutralColor);
    }

    public bool RefreshCurrentPlanetFromSnapInteractable()
    {
        if (SnapInteractable == null)
        {
            CurrentPlanet = null;
            return false;
        }

        foreach (IInteractorView interactorView in SnapInteractable.SelectingInteractorViews)
        {
            ReihenfolgePlanet planet = ResolvePlanetFromInteractor(interactorView);
            if (planet == null) continue;

            CurrentPlanet = planet;
            return true;
        }

        CurrentPlanet = null;
        return false;
    }

    private void OnEnable()
    {
        StartListeningToSnapEvents();
    }

    private void OnDisable()
    {
        StopListeningToSnapEvents();
    }

    public void SetFeedbackColor(Color color)
    {
        CacheFeedbackMaterial();

        if (FeedbackRoundedBox != null)
        {
            FeedbackRoundedBox.BorderColor = color;
            FeedbackRoundedBox.Color = new Color(color.r, color.g, color.b, Mathf.Min(color.a, 0.25f));
            FeedbackRoundedBox.Width = FeedbackRoundedBox.Width;
        }

        if (_feedbackMaterial == null) return;

        if (_feedbackMaterial.HasProperty("_BaseColor"))
        {
            _feedbackMaterial.SetColor("_BaseColor", color);
            return;
        }

        if (_feedbackMaterial.HasProperty("_Color"))
        {
            _feedbackMaterial.SetColor("_Color", color);
            return;
        }

        if (_feedbackMaterial.HasProperty("_BorderColor"))
        {
            _feedbackMaterial.SetColor("_BorderColor", color);
        }
    }

    private void CacheFeedbackMaterial()
    {
        if (_feedbackMaterial != null) return;

        if (FeedbackRenderer == null)
        {
            FeedbackRenderer = FindFeedbackRenderer();
        }

        if (FeedbackRoundedBox == null)
        {
            FeedbackRoundedBox = FindFeedbackRoundedBox();
        }

        if (FeedbackRenderer != null)
        {
            _feedbackMaterial = FeedbackRenderer.material;
        }
    }

    private void StartListeningToSnapEvents()
    {
        if (_isListening || SnapInteractable == null) return;

        SnapInteractable.WhenSelectingInteractorViewAdded += HandleSnapSelected;
        SnapInteractable.WhenSelectingInteractorViewRemoved += HandleSnapUnselected;
        _isListening = true;
    }

    private void StopListeningToSnapEvents()
    {
        if (_isListening == false || SnapInteractable == null) return;

        SnapInteractable.WhenSelectingInteractorViewAdded -= HandleSnapSelected;
        SnapInteractable.WhenSelectingInteractorViewRemoved -= HandleSnapUnselected;
        _isListening = false;
    }

    private void HandleSnapSelected(IInteractorView interactorView)
    {
        ReihenfolgePlanet planet = ResolvePlanetFromInteractor(interactorView);
        if (planet == null) return;

        CurrentPlanet = planet;
        WhenSlotChanged?.Invoke(this);
    }

    private void HandleSnapUnselected(IInteractorView interactorView)
    {
        if (CurrentPlanet == null) return;

        ReihenfolgePlanet planet = ResolvePlanetFromInteractor(interactorView);
        if (planet != CurrentPlanet) return;

        CurrentPlanet = null;
        WhenSlotChanged?.Invoke(this);
    }

    private ReihenfolgePlanet ResolvePlanetFromInteractor(IInteractorView interactorView)
    {
        Component component = interactorView as Component;
        if (component == null) return null;

        ReihenfolgePlanet planet = component.GetComponent<ReihenfolgePlanet>();
        if (planet != null) return planet;

        return component.GetComponentInParent<ReihenfolgePlanet>();
    }

    private void AutoFillReferences()
    {
        SnapInteractable localSnapInteractable = GetComponent<SnapInteractable>();
        if (localSnapInteractable == null)
        {
            localSnapInteractable = GetComponentInChildren<SnapInteractable>(true);
        }

        if (localSnapInteractable != null)
        {
            SnapInteractable = localSnapInteractable;
        }

        if (FeedbackRenderer == null)
        {
            FeedbackRenderer = FindFeedbackRenderer();
        }

        if (FeedbackRoundedBox == null)
        {
            FeedbackRoundedBox = FindFeedbackRoundedBox();
        }
    }

    private Renderer FindFeedbackRenderer()
    {
        Renderer ownRenderer = GetComponentInChildren<Renderer>(true);
        if (ownRenderer != null) return ownRenderer;

        Transform searchRoot = transform;
        for (int i = 0; i < 4 && searchRoot != null; i++)
        {
            Renderer[] renderers = searchRoot.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                if (renderer.GetComponentInParent<ReihenfolgePlanet>() != null) continue;

                string objectName = renderer.gameObject.name;
                if (objectName.Contains("Ring") || objectName.Contains("Visual"))
                {
                    return renderer;
                }
            }

            searchRoot = searchRoot.parent;
        }

        return null;
    }

    private RoundedBoxProperties FindFeedbackRoundedBox()
    {
        RoundedBoxProperties ownRoundedBox = GetComponentInChildren<RoundedBoxProperties>(true);
        if (ownRoundedBox != null) return ownRoundedBox;

        Transform searchRoot = transform;
        for (int i = 0; i < 4 && searchRoot != null; i++)
        {
            RoundedBoxProperties[] roundedBoxes = searchRoot.GetComponentsInChildren<RoundedBoxProperties>(true);
            foreach (RoundedBoxProperties roundedBox in roundedBoxes)
            {
                if (roundedBox.GetComponentInParent<ReihenfolgePlanet>() != null) continue;

                string objectName = roundedBox.gameObject.name;
                if (objectName.Contains("Ring") || objectName.Contains("Visual"))
                {
                    return roundedBox;
                }
            }

            searchRoot = searchRoot.parent;
        }

        return null;
    }
}
