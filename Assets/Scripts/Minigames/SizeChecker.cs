using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Prueft das Size-Minispiel.
// Meta SDK kuemmert sich weiter um Grab und Snap, dieses Script liest nur die Liste aus.
public class SizeChecker : MonoBehaviour
{
    [Header("Spielobjekte")]
    [Tooltip("Wenn aktiv, sammelt der Checker Planeten und Listen-Referenzen automatisch aus den Children ein.")]
    public bool AutoCollectChildren = true;
    public SnapInteractable ListSnapInteractable;
    public Transform ListRoot;
    public ReihenfolgeOrbitSlot[] Slots;
    public ReihenfolgePlanet[] Planets;
    public TMP_Text SuccessText;
    public GameObject CompletionMenu;
    public Button CompletionMenuButton;

    [Header("Auswertung")]
    public int ExpectedPlanetCount = 8;

    [Tooltip("Aktivieren, falls die sichtbare Listenrichtung im Headset gespiegelt ist.")]
    public bool InvertOrder = false;

    [Header("Farben")]
    public Color NeutralColor = new Color(0.25f, 0.65f, 1f, 0.65f);
    public Color CorrectColor = new Color(0.1f, 0.9f, 0.35f, 1f);
    public Color WrongColor = new Color(1f, 0.18f, 0.12f, 1f);

    [Header("Feedback")]
    public float WrongBlinkDuration = 0.45f;

    [Header("Groessen-Feedback")]
    [Tooltip("Wenn aktiv, werden alle wartenden Planeten vor Spielstart gleich gross dargestellt.")]
    public bool NormalizeWaitingPlanetSizes = true;

    [Tooltip("Sichtbare Groesse der Planeten, solange sie vor dem Spieler auf der Liste warten.")]
    public float WaitingPlanetSizeMeters = 0.03f;

    [Tooltip("So gross wird die Erde im Size-Minispiel, wenn sie richtig platziert ist.")]
    public float EarthSizeMeters = 0.04f;

    [Tooltip("Durchmesser der Erde in km. Dient als Vergleichswert fuer alle anderen Planeten.")]
    public float EarthDiameterKm = 12742f;

    [Tooltip("Nach dieser Zeit fliegt ein falsch platzierter Planet zur Liste zurueck.")]
    public float WrongReturnDelay = 2f;

    [Tooltip("Dauer der Rueckflug-Animation zur Liste.")]
    public float ReturnDuration = 0.45f;

    [Header("Korrekte Platzierung - Visual Offset")]
    [Tooltip("Optionale lokale Verschiebung einzelner Planeten-Visuals nach korrekter Platzierung.")]
    public SizePlanetVisualOffset[] CorrectPlacementVisualOffsets;

    private bool _hasCompleted;
    private string _lastWrongOrderSignature = "";
    private Coroutine _wrongBlinkCoroutine;
    private SizeSlotState[] _slotStates;
    private List<Coroutine> _runningPlanetCoroutines = new();
    private Dictionary<Transform, Vector3> _initialVisualScales = new();
    private Dictionary<Transform, Vector3> _initialVisualPositions = new();
    private bool _isReady;

    private void Reset()
    {
        CollectChildren();
    }

    private void OnValidate()
    {
        WaitingPlanetSizeMeters = Mathf.Max(0.001f, WaitingPlanetSizeMeters);
        EarthSizeMeters = Mathf.Max(0.001f, EarthSizeMeters);
        EarthDiameterKm = Mathf.Max(0.001f, EarthDiameterKm);
        WrongReturnDelay = Mathf.Max(0f, WrongReturnDelay);
        ReturnDuration = Mathf.Max(0.01f, ReturnDuration);

        if (AutoCollectChildren)
        {
            CollectChildren();
        }
    }

    private void Start()
    {
        _isReady = false;

        if (AutoCollectChildren)
        {
            CollectChildren();
        }

        BindCompletionMenuButton();
        ApplyWaitingPlanetSizeToVisualSettings();
        StartCoroutine(PrepareGameAfterVisualRefresh());
    }

    private void Update()
    {
        if (_isReady == false) return;
        if (_hasCompleted) return;

        EvaluateLive();
    }

    public void ResetGame()
    {
        _hasCompleted = false;
        _lastWrongOrderSignature = "";
        HideSuccessText();
        HideCompletionMenu();
        StopWrongBlink();
        StopPlanetCoroutines();
        SetAllPlanetsColor(NeutralColor);
        ResetPlanetScales();
    }

    private IEnumerator PrepareGameAfterVisualRefresh()
    {
        // InteractablePlanetVisual erzeugt seine Planet-Visuals ebenfalls in Start.
        // Ein Frame Wartezeit stellt sicher, dass danach die finalen Renderer existieren.
        yield return null;

        if (AutoCollectChildren)
        {
            CollectChildren();
        }

        ApplyWaitingPlanetSizeToVisualSettings();
        RefreshPlanetVisuals();
        NormalizeWaitingPlanetScales();
        CacheInitialVisualTransforms();
        ResetGame();
        _isReady = true;
    }

    public bool ValidateCurrentOrder()
    {
        List<ReihenfolgePlanet> currentOrder = Slots != null && Slots.Length > 0
            ? GetPlanetsInSlotOrder()
            : GetSnappedPlanetsInListOrder();

        if (currentOrder.Count != ExpectedPlanetCount) return false;

        List<ReihenfolgePlanet> expectedOrder = GetExpectedOrder();
        if (expectedOrder.Count != ExpectedPlanetCount) return false;

        for (int i = 0; i < ExpectedPlanetCount; i++)
        {
            if (currentOrder[i] != expectedOrder[i])
            {
                return false;
            }
        }

        return true;
    }

    public void FinishButtonPressed()
    {
        if (_hasCompleted)
        {
            EndMinigame();
            return;
        }

        if (ValidateCurrentOrder())
        {
            CompleteMinigame();
            return;
        }

        BlinkWrongOrder(true);
    }

    [ContextMenu("Planeten und Liste automatisch sammeln")]
    public void CollectChildren()
    {
        Planets = GetComponentsInChildren<ReihenfolgePlanet>(true);
        CollectCompletionMenuReferences();

        if (ListSnapInteractable == null)
        {
            ListSnapInteractable = FindListSnapInteractable();
        }

        if (ListRoot == null && ListSnapInteractable != null)
        {
            Transform listParent = FindParentNamed(ListSnapInteractable.transform, "List");
            ListRoot = listParent != null ? listParent : ListSnapInteractable.transform;
        }

        Slots = CollectSizeSlots();
    }

    private void EvaluateLive()
    {
        EvaluateSlots();

        if (ValidateCurrentOrder())
        {
            CompleteMinigame();
            return;
        }

        if (Slots != null && Slots.Length > 0) return;

        List<ReihenfolgePlanet> currentOrder = GetSnappedPlanetsInListOrder();

        if (currentOrder.Count != ExpectedPlanetCount)
        {
            _lastWrongOrderSignature = "";
            if (_wrongBlinkCoroutine == null)
            {
                SetAllPlanetsColor(NeutralColor);
            }
            return;
        }

        string currentSignature = GetOrderSignature(currentOrder);
        if (currentSignature != _lastWrongOrderSignature)
        {
            _lastWrongOrderSignature = currentSignature;
            BlinkWrongOrder(true);
        }
    }

    private void CompleteMinigame()
    {
        if (_hasCompleted) return;

        _hasCompleted = true;
        StopWrongBlink();
        SetAllPlanetsColor(CorrectColor);
        GrowAllPlanetsToRealSize();

        if (SuccessText != null)
        {
            SuccessText.gameObject.SetActive(true);
            SuccessText.text = "Geschafft!";
        }

        ShowCompletionMenu();

        if (MinigameManager.Instance != null)
        {
            MinigameManager.Instance.CompleteCurrentMinigame();
        }
    }

    private void EndMinigame()
    {
        if (MinigameManager.Instance != null)
        {
            MinigameManager.Instance.EndMinigame();
        }
    }

    private List<ReihenfolgePlanet> GetSnappedPlanetsInListOrder()
    {
        List<SizePlanetEntry> entries = new();
        if (ListSnapInteractable == null || ListRoot == null) return new List<ReihenfolgePlanet>();

        foreach (IInteractorView interactorView in ListSnapInteractable.SelectingInteractorViews)
        {
            ReihenfolgePlanet planet = ResolvePlanetFromInteractor(interactorView);
            if (planet == null || entries.Exists(entry => entry.Planet == planet)) continue;

            float localX = ListRoot.InverseTransformPoint(planet.transform.position).x;
            entries.Add(new SizePlanetEntry(planet, localX));
        }

        entries.Sort((a, b) => a.LocalX.CompareTo(b.LocalX));
        if (InvertOrder)
        {
            entries.Reverse();
        }

        List<ReihenfolgePlanet> order = new();
        foreach (SizePlanetEntry entry in entries)
        {
            order.Add(entry.Planet);
        }

        return order;
    }

    private List<ReihenfolgePlanet> GetExpectedOrder()
    {
        List<ReihenfolgePlanet> expectedOrder = new();
        if (Planets == null) return expectedOrder;

        foreach (ReihenfolgePlanet planet in Planets)
        {
            if (planet == null || planet.PlanetData == null) continue;
            expectedOrder.Add(planet);
        }

        expectedOrder.Sort((a, b) => a.PlanetData.diameter.CompareTo(b.PlanetData.diameter));
        return expectedOrder;
    }

    private void EvaluateSlots()
    {
        if (Slots == null || Planets == null || Slots.Length == 0 || Planets.Length == 0) return;

        EnsureSlotStates();
        List<ReihenfolgePlanet> expectedOrder = GetExpectedOrder();
        if (expectedOrder.Count == 0) return;

        for (int i = 0; i < Slots.Length; i++)
        {
            ReihenfolgeOrbitSlot slot = Slots[i];
            if (slot == null) continue;

            slot.RefreshCurrentPlanetFromSnapInteractable();
            ReihenfolgePlanet currentPlanet = slot.CurrentPlanet;
            SizeSlotState state = _slotStates[i];

            if (currentPlanet == null)
            {
                if (state.Planet != null && state.IsCorrect)
                {
                    ResetPlanetScale(state.Planet);
                }

                state.Planet = null;
                state.IsCorrect = false;
                state.HasReturnStarted = false;
                state.WrongSince = -1f;
                slot.SetFeedbackColor(NeutralColor);
                _slotStates[i] = state;
                continue;
            }

            bool isCorrect = i < expectedOrder.Count && currentPlanet == expectedOrder[i];
            bool wasAlreadyCorrect = state.Planet == currentPlanet && state.IsCorrect;

            if (state.Planet != currentPlanet)
            {
                if (state.Planet != null && state.IsCorrect)
                {
                    ResetPlanetScale(state.Planet);
                }

                state.Planet = currentPlanet;
                state.IsCorrect = false;
                state.HasReturnStarted = false;
                state.WrongSince = isCorrect ? -1f : Time.time;
            }

            if (isCorrect)
            {
                state.IsCorrect = true;
                state.HasReturnStarted = false;
                state.WrongSince = -1f;
                slot.SetFeedbackColor(CorrectColor);
                currentPlanet.SetFeedbackColor(CorrectColor);

                if (wasAlreadyCorrect == false)
                {
                    SetPlanetToRealSize(currentPlanet);
                    ApplyCorrectPlacementVisualOffset(currentPlanet);
                    DisablePhysicsForCorrectPlanet(currentPlanet);
                }
            }
            else
            {
                state.IsCorrect = false;
                slot.SetFeedbackColor(WrongColor);
                currentPlanet.SetFeedbackColor(WrongColor);

                if (state.WrongSince < 0f)
                {
                    state.WrongSince = Time.time;
                }

                if (state.HasReturnStarted == false && Time.time - state.WrongSince >= WrongReturnDelay)
                {
                    state.HasReturnStarted = true;
                    StartPlanetCoroutine(ReturnPlanetToList(currentPlanet, slot));
                }
            }

            _slotStates[i] = state;
        }
    }

    private List<ReihenfolgePlanet> GetPlanetsInSlotOrder()
    {
        List<ReihenfolgePlanet> order = new();
        if (Slots == null) return order;

        foreach (ReihenfolgeOrbitSlot slot in Slots)
        {
            if (slot == null) continue;

            slot.RefreshCurrentPlanetFromSnapInteractable();
            if (slot.CurrentPlanet != null)
            {
                order.Add(slot.CurrentPlanet);
            }
        }

        return order;
    }

    private ReihenfolgeOrbitSlot[] CollectSizeSlots()
    {
        ReihenfolgeOrbitSlot[] allSlots = GetComponentsInChildren<ReihenfolgeOrbitSlot>(true);
        List<ReihenfolgeOrbitSlot> uniqueSlots = new();

        foreach (ReihenfolgeOrbitSlot slot in allSlots)
        {
            if (slot == null || slot.SnapInteractable == null) continue;
            if (slot.SnapInteractable == ListSnapInteractable) continue;
            if (uniqueSlots.Exists(existingSlot => existingSlot.SnapInteractable == slot.SnapInteractable)) continue;

            uniqueSlots.Add(slot);
        }

        uniqueSlots.Sort((a, b) =>
        {
            Transform sortRoot = ListRoot != null ? ListRoot : transform;
            float aX = sortRoot.InverseTransformPoint(a.transform.position).x;
            float bX = sortRoot.InverseTransformPoint(b.transform.position).x;
            return aX.CompareTo(bX);
        });

        if (InvertOrder)
        {
            uniqueSlots.Reverse();
        }

        return uniqueSlots.ToArray();
    }

    private void EnsureSlotStates()
    {
        if (_slotStates != null && Slots != null && _slotStates.Length == Slots.Length) return;

        int slotCount = Slots != null ? Slots.Length : 0;
        _slotStates = new SizeSlotState[slotCount];
        for (int i = 0; i < _slotStates.Length; i++)
        {
            _slotStates[i].WrongSince = -1f;
        }
    }

    private void GrowAllPlanetsToRealSize()
    {
        if (Planets == null) return;

        foreach (ReihenfolgePlanet planet in Planets)
        {
            SetPlanetToRealSize(planet);
            ApplyCorrectPlacementVisualOffset(planet);
            DisablePhysicsForCorrectPlanet(planet);
        }
    }

    private void SetPlanetToRealSize(ReihenfolgePlanet planet)
    {
        if (planet == null || planet.PlanetData == null) return;

        float targetSize = GetTargetWorldSize(planet);
        if (targetSize <= 0f) return;

        ScalePlanetToWorldSize(planet, targetSize);
    }

    private float GetTargetWorldSize(ReihenfolgePlanet planet)
    {
        if (planet == null || planet.PlanetData == null || EarthDiameterKm <= 0f) return 0f;

        return (planet.PlanetData.diameter / EarthDiameterKm) * EarthSizeMeters;
    }

    private void ScalePlanetToWorldSize(ReihenfolgePlanet planet, float targetWorldSize)
    {
        if (planet == null) return;

        Transform scaleRoot = GetPlanetScaleRoot(planet);
        if (scaleRoot == null) return;

        float startSize = GetPlanetWorldSize(scaleRoot);
        if (startSize <= 0.0001f) return;

        scaleRoot.localScale *= targetWorldSize / startSize;
    }

    private void ApplyCorrectPlacementVisualOffset(ReihenfolgePlanet planet)
    {
        Vector3 localOffset = GetCorrectPlacementVisualOffset(planet);
        if (localOffset == Vector3.zero) return;

        Transform scaleRoot = GetPlanetScaleRoot(planet);
        if (scaleRoot == null) return;

        if (_initialVisualPositions.TryGetValue(scaleRoot, out Vector3 initialPosition))
        {
            scaleRoot.localPosition = initialPosition + localOffset;
            return;
        }

        scaleRoot.localPosition += localOffset;
    }

    private Vector3 GetCorrectPlacementVisualOffset(ReihenfolgePlanet planet)
    {
        if (planet == null || planet.PlanetData == null || CorrectPlacementVisualOffsets == null) return Vector3.zero;

        foreach (SizePlanetVisualOffset visualOffset in CorrectPlacementVisualOffsets)
        {
            if (visualOffset.PlanetData == planet.PlanetData)
            {
                return visualOffset.LocalOffset;
            }
        }

        return Vector3.zero;
    }

    private float GetPlanetWorldSize(Transform root)
    {
        if (root == null) return 0f;

        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0) return 0f;

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        return Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
    }

    private Transform GetPlanetScaleRoot(ReihenfolgePlanet planet)
    {
        if (planet == null) return null;

        InteractablePlanetVisual visual = planet.GetComponent<InteractablePlanetVisual>();
        if (visual != null && visual.VisualRoot != null)
        {
            return visual.VisualRoot;
        }

        return planet.transform;
    }

    private IEnumerator ReturnPlanetToList(ReihenfolgePlanet planet, ReihenfolgeOrbitSlot slot)
    {
        if (planet == null || ListRoot == null) yield break;

        if (planet.SnapInteractor != null)
        {
            planet.SnapInteractor.enabled = false;
        }

        ResetPlanetScale(planet);

        Vector3 startPosition = planet.transform.position;
        Quaternion startRotation = planet.transform.rotation;
        Vector3 targetPosition = GetListReturnPosition();
        Quaternion targetRotation = ListRoot.rotation;
        float elapsed = 0f;

        while (elapsed < ReturnDuration && planet != null)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / ReturnDuration));
            planet.transform.SetPositionAndRotation(
                Vector3.Lerp(startPosition, targetPosition, t),
                Quaternion.Slerp(startRotation, targetRotation, t));
            yield return null;
        }

        if (planet != null)
        {
            planet.transform.SetPositionAndRotation(targetPosition, targetRotation);
            planet.SetFeedbackColor(NeutralColor);

            if (planet.Rigidbody != null)
            {
                planet.Rigidbody.linearVelocity = Vector3.zero;
                planet.Rigidbody.angularVelocity = Vector3.zero;
            }

            if (planet.SnapInteractor != null)
            {
                planet.SnapInteractor.enabled = true;
                if (ListSnapInteractable != null)
                {
                    planet.SnapInteractor.InjectOptionalTimeOutInteractable(ListSnapInteractable);
                    planet.SnapInteractor.InjectOptionaTimeOut(0f);
                }
            }
        }

        if (slot != null)
        {
            slot.ClearCurrentPlanet();
        }
    }

    private Vector3 GetListReturnPosition()
    {
        if (ListRoot == null) return transform.position;

        Vector3 position = ListRoot.position;
        position += ListRoot.up * 0.08f;
        return position;
    }

    private void ResetPlanetScales()
    {
        if (Planets == null) return;

        foreach (ReihenfolgePlanet planet in Planets)
        {
            ResetPlanetScale(planet);
        }
    }

    private void ApplyWaitingPlanetSizeToVisualSettings()
    {
        if (NormalizeWaitingPlanetSizes == false || Planets == null) return;

        foreach (ReihenfolgePlanet planet in Planets)
        {
            if (planet == null) continue;

            InteractablePlanetVisual visual = planet.GetComponent<InteractablePlanetVisual>();
            if (visual == null) continue;

            visual.FitVisualToTargetSize = true;
            visual.TargetVisualSize = WaitingPlanetSizeMeters;
        }
    }

    private void RefreshPlanetVisuals()
    {
        if (Planets == null) return;

        foreach (ReihenfolgePlanet planet in Planets)
        {
            if (planet == null) continue;

            InteractablePlanetVisual visual = planet.GetComponent<InteractablePlanetVisual>();
            if (visual == null || visual.RefreshOnStart == false) continue;

            visual.RefreshVisual();
        }
    }

    private void NormalizeWaitingPlanetScales()
    {
        if (NormalizeWaitingPlanetSizes == false || Planets == null) return;

        foreach (ReihenfolgePlanet planet in Planets)
        {
            Transform scaleRoot = GetPlanetScaleRoot(planet);
            if (scaleRoot == null) continue;

            float currentSize = GetPlanetWorldSize(scaleRoot);
            if (currentSize <= 0.0001f) continue;

            scaleRoot.localScale *= WaitingPlanetSizeMeters / currentSize;
        }
    }

    private void ResetPlanetScale(ReihenfolgePlanet planet)
    {
        Transform scaleRoot = GetPlanetScaleRoot(planet);
        if (scaleRoot != null)
        {
            if (_initialVisualScales.TryGetValue(scaleRoot, out Vector3 initialScale))
            {
                scaleRoot.localScale = initialScale;
            }
            else
            {
                scaleRoot.localScale = Vector3.one;
            }

            if (_initialVisualPositions.TryGetValue(scaleRoot, out Vector3 initialPosition))
            {
                scaleRoot.localPosition = initialPosition;
            }
        }
    }

    private void CacheInitialVisualTransforms()
    {
        _initialVisualScales.Clear();
        _initialVisualPositions.Clear();
        if (Planets == null) return;

        foreach (ReihenfolgePlanet planet in Planets)
        {
            Transform scaleRoot = GetPlanetScaleRoot(planet);
            if (scaleRoot != null && _initialVisualScales.ContainsKey(scaleRoot) == false)
            {
                _initialVisualScales.Add(scaleRoot, scaleRoot.localScale);
                _initialVisualPositions.Add(scaleRoot, scaleRoot.localPosition);
            }
        }
    }

    private void DisablePhysicsForCorrectPlanet(ReihenfolgePlanet planet)
    {
        if (planet == null) return;

        Rigidbody[] rigidbodies = planet.GetComponentsInChildren<Rigidbody>(true);
        foreach (Rigidbody planetRigidbody in rigidbodies)
        {
            planetRigidbody.linearVelocity = Vector3.zero;
            planetRigidbody.angularVelocity = Vector3.zero;
            planetRigidbody.useGravity = false;
            planetRigidbody.isKinematic = true;
            planetRigidbody.detectCollisions = false;
        }

        Collider[] colliders = planet.GetComponentsInChildren<Collider>(true);
        foreach (Collider planetCollider in colliders)
        {
            planetCollider.enabled = false;
        }
    }

    private void StartPlanetCoroutine(IEnumerator routine)
    {
        _runningPlanetCoroutines.Add(StartCoroutine(routine));
    }

    private void StopPlanetCoroutines()
    {
        foreach (Coroutine coroutine in _runningPlanetCoroutines)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
        }

        _runningPlanetCoroutines.Clear();
    }

    private ReihenfolgePlanet ResolvePlanetFromInteractor(IInteractorView interactorView)
    {
        Component component = interactorView as Component;
        if (component == null) return null;

        ReihenfolgePlanet planet = component.GetComponent<ReihenfolgePlanet>();
        if (planet != null) return planet;

        return component.GetComponentInParent<ReihenfolgePlanet>();
    }

    private SnapInteractable FindListSnapInteractable()
    {
        SnapInteractable[] snapInteractables = GetComponentsInChildren<SnapInteractable>(true);

        foreach (SnapInteractable snapInteractable in snapInteractables)
        {
            if (snapInteractable.GetComponent<ListSnapPoseDelegate>() != null)
            {
                return snapInteractable;
            }
        }

        foreach (SnapInteractable snapInteractable in snapInteractables)
        {
            if (FindParentNamed(snapInteractable.transform, "List") != null)
            {
                return snapInteractable;
            }
        }

        return snapInteractables.Length > 0 ? snapInteractables[0] : null;
    }

    private void CollectCompletionMenuReferences()
    {
        if (CompletionMenu == null)
        {
            Transform completionMenuTransform = FindDirectChild("Congrats");
            if (completionMenuTransform == null)
            {
                completionMenuTransform = FindDirectChild("CompletionMenu");
            }

            if (completionMenuTransform != null)
            {
                CompletionMenu = completionMenuTransform.gameObject;
            }
        }

        if (CompletionMenuButton == null && CompletionMenu != null)
        {
            CompletionMenuButton = CompletionMenu.GetComponentInChildren<Button>(true);
        }
    }

    private void BindCompletionMenuButton()
    {
        CollectCompletionMenuReferences();
        if (CompletionMenuButton == null) return;

        CompletionMenuButton.onClick.RemoveListener(EndMinigame);
        CompletionMenuButton.onClick.AddListener(EndMinigame);
    }

    private void ShowCompletionMenu()
    {
        if (CompletionMenu != null)
        {
            CompletionMenu.SetActive(true);
        }
    }

    private void HideCompletionMenu()
    {
        if (CompletionMenu != null)
        {
            CompletionMenu.SetActive(false);
        }
    }

    private Transform FindDirectChild(string childName)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.name == childName)
            {
                return child;
            }
        }

        return null;
    }

    private Transform FindParentNamed(Transform start, string targetName)
    {
        Transform current = start;
        while (current != null)
        {
            if (current.name == targetName)
            {
                return current;
            }

            current = current.parent;
        }

        return null;
    }

    private void BlinkWrongOrder(bool forceBlink)
    {
        if (forceBlink == false && _wrongBlinkCoroutine != null) return;

        StopWrongBlink();
        _wrongBlinkCoroutine = StartCoroutine(BlinkWrongRoutine());
    }

    private IEnumerator BlinkWrongRoutine()
    {
        SetAllPlanetsColor(WrongColor);
        yield return new WaitForSeconds(WrongBlinkDuration);

        _wrongBlinkCoroutine = null;
        if (_hasCompleted == false)
        {
            SetAllPlanetsColor(NeutralColor);
        }
    }

    private void StopWrongBlink()
    {
        if (_wrongBlinkCoroutine == null) return;

        StopCoroutine(_wrongBlinkCoroutine);
        _wrongBlinkCoroutine = null;
    }

    private void SetAllPlanetsColor(Color color)
    {
        if (Planets == null) return;

        foreach (ReihenfolgePlanet planet in Planets)
        {
            if (planet != null)
            {
                planet.SetFeedbackColor(color);
            }
        }
    }

    private void HideSuccessText()
    {
        if (SuccessText != null)
        {
            SuccessText.gameObject.SetActive(false);
        }
    }

    private string GetOrderSignature(List<ReihenfolgePlanet> order)
    {
        string signature = "";
        foreach (ReihenfolgePlanet planet in order)
        {
            signature += planet.GetInstanceID() + "|";
        }

        return signature;
    }

    private readonly struct SizePlanetEntry
    {
        public readonly ReihenfolgePlanet Planet;
        public readonly float LocalX;

        public SizePlanetEntry(ReihenfolgePlanet planet, float localX)
        {
            Planet = planet;
            LocalX = localX;
        }
    }

    private struct SizeSlotState
    {
        public ReihenfolgePlanet Planet;
        public bool IsCorrect;
        public bool HasReturnStarted;
        public float WrongSince;
    }

    [System.Serializable]
    public struct SizePlanetVisualOffset
    {
        public PlanetData PlanetData;
        public Vector3 LocalOffset;
    }
}
