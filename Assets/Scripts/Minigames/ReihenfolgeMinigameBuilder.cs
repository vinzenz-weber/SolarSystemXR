using System.Collections.Generic;
using Oculus.Interaction;
using Oculus.Interaction.Grab;
using Oculus.Interaction.GrabAPI;
using Oculus.Interaction.HandGrab;
using TMPro;
using UnityEngine;

// Baut das Reihenfolge-Minispiel aus PlanetData zur Laufzeit auf.
// Dadurch bleiben die Daten generisch und das Prefab muss nicht 8x von Hand dupliziert werden.
public class ReihenfolgeMinigameBuilder : MonoBehaviour
{
    [Header("PlanetData in Reihenfolge")]
    public PlanetData[] PlanetenDaten;

    [Header("UI")]
    public TMP_Text SuccessText;

    [Header("Art Direction (optional)")]
    [Tooltip("Wenn gesetzt, kommen UI-/Layout-Anker aus diesem Prefab-Script statt nur aus Zahlenwerten.")]
    public ReihenfolgeMinigameLayout Layout;

    [Header("Layout")]
    public Vector3 OrbitCenterLocal = new Vector3(0f, -0.08f, -0.08f);
    public Vector3 BenchCenterLocal = new Vector3(0f, -0.53f, -0.08f);
    public float FirstOrbitRadius = 0.1f;
    public float OrbitRadiusStep = 0.045f;
    public float BenchSpacing = 0.12f;
    public float SlotTriggerRadius = 0.075f;
    public float RingLineWidth = 0.006f;

    [Header("Planetengroessen")]
    public float SmallestPlanetSize = 0.055f;
    public float LargestPlanetSize = 0.15f;

    [Header("Farben")]
    public Color NeutralColor = new Color(0.25f, 0.65f, 1f, 0.65f);
    public Color CorrectColor = new Color(0.1f, 0.9f, 0.35f, 1f);
    public Color WrongColor = new Color(1f, 0.18f, 0.12f, 1f);

    private ReihenfolgeChecker _checker;
    private Transform _contentParent;
    private Transform _generatedRoot;

    private void Start()
    {
        Build();
    }

    public void Build()
    {
        if (PlanetenDaten == null || PlanetenDaten.Length == 0)
        {
            Debug.LogWarning("ReihenfolgeMinigameBuilder: Keine PlanetData-Assets zugewiesen.");
            return;
        }

        ClearGeneratedRoot();
        ResolveLayout();

        _generatedRoot = new GameObject("Reihenfolge_GeneratedWorld").transform;
        _generatedRoot.SetParent(_contentParent, false);

        if (SuccessText == null)
        {
            SuccessText = CreateSuccessText();
        }

        List<ReihenfolgeOrbitSlot> slots = new();
        List<ReihenfolgePlanet> planets = new();
        List<Transform> benchPositions = new();

        float largestDiameter = GetLargestDiameter();

        for (int i = 0; i < PlanetenDaten.Length; i++)
        {
            float radius = FirstOrbitRadius + OrbitRadiusStep * i;
            ReihenfolgeOrbitSlot slot = CreateOrbitSlot(i, radius);
            slots.Add(slot);
        }

        for (int i = 0; i < PlanetenDaten.Length; i++)
        {
            Transform benchPosition = GetBenchPosition(i, PlanetenDaten.Length);
            benchPositions.Add(benchPosition);
        }

        for (int i = 0; i < PlanetenDaten.Length; i++)
        {
            ReihenfolgePlanet planet = CreatePlanet(PlanetenDaten[i], i, largestDiameter, benchPositions[i]);
            planets.Add(planet);
        }

        _checker = GetComponent<ReihenfolgeChecker>();
        if (_checker == null)
        {
            _checker = gameObject.AddComponent<ReihenfolgeChecker>();
        }

        _checker.Slots = slots.ToArray();
        _checker.Planets = planets.ToArray();
        _checker.BenchPositions = benchPositions.ToArray();
        _checker.SuccessText = SuccessText;
        _checker.NeutralColor = NeutralColor;
        _checker.CorrectColor = CorrectColor;
        _checker.WrongColor = WrongColor;
        _checker.SnapCheckDistance = SlotTriggerRadius * 1.35f;
        _checker.ResetGame();
    }

    private ReihenfolgeOrbitSlot CreateOrbitSlot(int orbitIndex, float radius)
    {
        GameObject ringObject = new GameObject("OrbitRing_" + orbitIndex);
        ringObject.transform.SetParent(_generatedRoot, false);
        ringObject.transform.localPosition = GetOrbitCenterLocal();

        LineRenderer lineRenderer = ringObject.AddComponent<LineRenderer>();
        ConfigureRingRenderer(lineRenderer, radius);

        GameObject slotObject = new GameObject("SnapSlot_" + orbitIndex);
        slotObject.transform.SetParent(_generatedRoot, false);
        slotObject.transform.localPosition = GetOrbitCenterLocal() + new Vector3(radius, 0f, 0f);
        slotObject.transform.localRotation = Quaternion.identity;

        Rigidbody slotRigidbody = slotObject.AddComponent<Rigidbody>();
        slotRigidbody.isKinematic = true;
        slotRigidbody.useGravity = false;

        SphereCollider trigger = slotObject.AddComponent<SphereCollider>();
        trigger.isTrigger = true;
        trigger.radius = SlotTriggerRadius;

        SnapInteractable snapInteractable = slotObject.AddComponent<SnapInteractable>();
        snapInteractable.InjectAllSnapInteractable(slotRigidbody);

        ReihenfolgeOrbitSlot slot = slotObject.AddComponent<ReihenfolgeOrbitSlot>();
        slot.SnapInteractable = snapInteractable;
        slot.FeedbackRenderer = lineRenderer;
        slot.Initialize(orbitIndex, NeutralColor);

        return slot;
    }

    private Transform GetBenchPosition(int index, int count)
    {
        if (Layout != null
            && Layout.BenchPositions != null
            && index < Layout.BenchPositions.Length
            && Layout.BenchPositions[index] != null)
        {
            return Layout.BenchPositions[index];
        }

        return CreateBenchPosition(index, count);
    }

    private Transform CreateBenchPosition(int index, int count)
    {
        GameObject benchObject = new GameObject("BenchPosition_" + index);
        benchObject.transform.SetParent(_generatedRoot, false);

        float centeredIndex = index - (count - 1) * 0.5f;
        benchObject.transform.localPosition = BenchCenterLocal + new Vector3(centeredIndex * BenchSpacing, 0f, 0f);
        benchObject.transform.localRotation = Quaternion.identity;

        return benchObject.transform;
    }

    private ReihenfolgePlanet CreatePlanet(PlanetData data, int orbitIndex, float largestDiameter, Transform startPosition)
    {
        GameObject planetRoot = new GameObject("Planet_" + data.planetName);
        planetRoot.transform.SetParent(_generatedRoot, false);
        planetRoot.transform.SetPositionAndRotation(startPosition.position, startPosition.rotation);

        Rigidbody planetRigidbody = planetRoot.AddComponent<Rigidbody>();
        planetRigidbody.useGravity = false;
        planetRigidbody.isKinematic = true;
        planetRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

        Grabbable grabbable = planetRoot.AddComponent<Grabbable>();
        grabbable.InjectOptionalRigidbody(planetRigidbody);
        grabbable.InjectOptionalThrowWhenUnselected(false);
        grabbable.InjectOptionalKinematicWhileSelected(true);

        GameObject visual = CreatePlanetVisual(data, planetRoot.transform);
        float targetSize = GetTargetPlanetSize(data, largestDiameter);
        ScaleVisualToTargetSize(visual, targetSize);

        SphereCollider collider = planetRoot.AddComponent<SphereCollider>();
        collider.radius = targetSize * 0.62f;

        HandGrabInteractable handGrab = HandGrabUtils.CreateHandGrabInteractable(planetRoot.transform, "HandGrabInteractable");
        handGrab.InjectAllHandGrabInteractable(
            GrabTypeFlags.All,
            planetRigidbody,
            GrabbingRule.DefaultPinchRule,
            GrabbingRule.DefaultPalmRule);
        handGrab.InjectOptionalPointableElement(grabbable);

        DistanceHandGrabInteractable distanceHandGrab = planetRoot.AddComponent<DistanceHandGrabInteractable>();
        distanceHandGrab.InjectAllDistanceHandGrabInteractable(
            GrabTypeFlags.All,
            planetRigidbody,
            GrabbingRule.DefaultPinchRule,
            GrabbingRule.DefaultPalmRule);
        distanceHandGrab.InjectOptionalPointableElement(grabbable);

        DistanceGrabInteractable distanceGrab = planetRoot.AddComponent<DistanceGrabInteractable>();
        distanceGrab.InjectAllGrabInteractable(planetRigidbody);
        distanceGrab.InjectOptionalPointableElement(grabbable);

        SnapInteractor snapInteractor = planetRoot.AddComponent<SnapInteractor>();
        snapInteractor.InjectAllSnapInteractor(grabbable, planetRigidbody);
        snapInteractor.DistanceThreshold = 0.03f;

        ReihenfolgePlanet planet = planetRoot.AddComponent<ReihenfolgePlanet>();
        planet.Rigidbody = planetRigidbody;
        planet.Grabbable = grabbable;
        planet.SnapInteractor = snapInteractor;
        planet.FeedbackRenderer = visual.GetComponentInChildren<Renderer>();
        planet.Initialize(data, orbitIndex, NeutralColor);
        planet.ResetToBench(startPosition);

        return planet;
    }

    private TMP_Text CreateSuccessText()
    {
        GameObject textObject = new GameObject("SuccessText_Geschafft");
        textObject.transform.SetParent(_generatedRoot, false);
        textObject.transform.localPosition = GetOrbitCenterLocal() + new Vector3(0f, FirstOrbitRadius + OrbitRadiusStep * PlanetenDaten.Length + 0.08f, 0f);
        textObject.transform.localRotation = Quaternion.identity;
        textObject.transform.localScale = Vector3.one * 0.08f;

        TextMeshPro text = textObject.AddComponent<TextMeshPro>();
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = 2.8f;
        text.text = "Geschafft!";
        text.color = CorrectColor;
        textObject.SetActive(false);

        return text;
    }

    private GameObject CreatePlanetVisual(PlanetData data, Transform parent)
    {
        GameObject prefab = data != null ? data.planetPrefab : null;
        GameObject visual = prefab != null
            ? Instantiate(prefab, parent)
            : GameObject.CreatePrimitive(PrimitiveType.Sphere);

        visual.name = data != null ? "Visual_" + data.planetName : "Visual_Planet";
        visual.transform.SetParent(parent, false);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = Quaternion.identity;

        DisableRuntimeSolarSystemScripts(visual);
        DisableCollidersOnVisual(visual);
        DisableNestedRigidbodiesOnVisual(visual);
        PlanetFactsVisibility.Refresh();
        PlanetLabelVisibility.RefreshInRoot(visual.transform);

        return visual;
    }

    private void ResolveLayout()
    {
        if (Layout == null)
        {
            Layout = GetComponentInChildren<ReihenfolgeMinigameLayout>(true);
        }

        _contentParent = transform;

        if (Layout != null)
        {
            if (Layout.GeneratedContentParent != null)
            {
                _contentParent = Layout.GeneratedContentParent;
            }

            if (SuccessText == null && Layout.SuccessText != null)
            {
                SuccessText = Layout.SuccessText;
            }
        }
    }

    private Vector3 GetOrbitCenterLocal()
    {
        if (Layout != null && Layout.OrbitCenter != null)
        {
            return _generatedRoot.InverseTransformPoint(Layout.OrbitCenter.position);
        }

        return OrbitCenterLocal;
    }

    private void ConfigureRingRenderer(LineRenderer lineRenderer, float radius)
    {
        int points = 96;
        lineRenderer.useWorldSpace = false;
        lineRenderer.loop = true;
        lineRenderer.positionCount = points;
        lineRenderer.widthMultiplier = RingLineWidth;
        lineRenderer.material = CreateUnlitMaterial(NeutralColor);

        for (int i = 0; i < points; i++)
        {
            float angle = (i / (float)points) * Mathf.PI * 2f;
            lineRenderer.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f));
        }
    }

    private Material CreateUnlitMaterial(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
        {
            shader = Shader.Find("Sprites/Default");
        }

        Material material = new Material(shader);
        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }
        else if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", color);
        }

        return material;
    }

    private void ScaleVisualToTargetSize(GameObject visual, float targetSize)
    {
        if (visual == null) return;

        if (PlanetVisualBoundsUtility.TryGetPlanetVisualBounds(visual.transform, true, out Bounds bounds) == false)
        {
            visual.transform.localScale = Vector3.one * targetSize;
            return;
        }

        float currentSize = PlanetVisualBoundsUtility.GetLargestWorldSize(bounds);
        if (currentSize <= 0.0001f)
        {
            visual.transform.localScale = Vector3.one * targetSize;
            return;
        }

        float scaleFactor = targetSize / currentSize;
        visual.transform.localScale *= scaleFactor;
    }

    private float GetTargetPlanetSize(PlanetData data, float largestDiameter)
    {
        if (data == null || largestDiameter <= 0f) return SmallestPlanetSize;

        float normalized = Mathf.Sqrt(Mathf.Clamp01(data.diameter / largestDiameter));
        return Mathf.Lerp(SmallestPlanetSize, LargestPlanetSize, normalized);
    }

    private float GetLargestDiameter()
    {
        float largestDiameter = 1f;

        foreach (PlanetData data in PlanetenDaten)
        {
            if (data != null)
            {
                largestDiameter = Mathf.Max(largestDiameter, data.diameter);
            }
        }

        return largestDiameter;
    }

    private void DisableRuntimeSolarSystemScripts(GameObject visual)
    {
        PlanetBody[] planetBodies = visual.GetComponentsInChildren<PlanetBody>(true);
        foreach (PlanetBody planetBody in planetBodies)
        {
            planetBody.enabled = false;
        }
    }

    private void DisableCollidersOnVisual(GameObject visual)
    {
        Collider[] colliders = visual.GetComponentsInChildren<Collider>(true);
        foreach (Collider visualCollider in colliders)
        {
            visualCollider.enabled = false;
        }
    }

    private void DisableNestedRigidbodiesOnVisual(GameObject visual)
    {
        Rigidbody[] rigidbodies = visual.GetComponentsInChildren<Rigidbody>(true);
        foreach (Rigidbody visualRigidbody in rigidbodies)
        {
            visualRigidbody.isKinematic = true;
            visualRigidbody.useGravity = false;
            visualRigidbody.detectCollisions = false;
        }
    }

    private void ClearGeneratedRoot()
    {
        if (_generatedRoot == null) return;

        Destroy(_generatedRoot.gameObject);
        _generatedRoot = null;
    }
}
