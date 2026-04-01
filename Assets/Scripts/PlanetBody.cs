using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class PlanetBody : MonoBehaviour
{
    public PlanetData data;

    [Header("Trail")]
    [Tooltip("Anzahl gespeicherter Positionen. Mehr = längerer Schweif.")]
    public int trailPoints = 200;
    [Tooltip("Maximale Linienbreite des Trails in Metern.")]
    public float trailWidth = 0.001f;
    [Tooltip("HDR-Multiplikator für Bloom-Glow (>1 aktiviert Bloom).")]
    public float glowIntensity = 4f;
    [Tooltip("Optional: eigenes transparentes Material. Wird sonst automatisch erstellt.")]
    public Material trailMaterial;

    private SolarSystemManager manager;
    private float currentAngle = 0f;
    private float degreesPerDay;
    private Transform _transform;

    // Größen-Änderungs-Tracking
    private float lastDistanceScale = -1f;
    private float lastPlanetSizeScale = -1f;

    // Trail-Ringpuffer
    private LineRenderer lineRenderer;
    private GameObject trailObj;
    private Vector3[] trailBuffer;
    private Vector3[] renderBuffer;   // vorab alloziert, kein Alloc pro Frame
    private int writeIndex = 0;
    private bool bufferFull = false;
    private float angleStep;
    private float lastRecordedAngle;

    void Start()
    {
        _transform = transform;
        manager = FindObjectOfType<SolarSystemManager>();

        if (manager == null)
        {
            Debug.LogError($"[{name}] SolarSystemManager nicht gefunden!");
            return;
        }
        if (data == null)
        {
            Debug.LogError($"[{name}] Kein PlanetData zugewiesen!");
            return;
        }

        degreesPerDay = 360f / data.orbitalPeriod;

        ApplySize();
        ApplyPosition();

        if (data.semiMajorAxis > 0)
            InitializeTrail();
    }

    void Update()
    {
        if (manager == null || data == null) return;

        if (data.semiMajorAxis <= 0)
        {
            ApplySizeIfChanged();
            return;
        }

        currentAngle += degreesPerDay * manager.timeScale * Time.deltaTime;

        ApplyPosition();
        ApplySizeIfChanged();
        UpdateTrail();

        if (data.rotationSpeed != 0f)
            _transform.Rotate(Vector3.up, data.rotationSpeed * manager.timeScale * Time.deltaTime);
    }

    void OnDestroy()
    {
        if (trailObj != null)
            Destroy(trailObj);
    }

    // --- Größe ---

    private void ApplySizeIfChanged()
    {
        if (manager.distanceScale == lastDistanceScale && manager.planetSizeScale == lastPlanetSizeScale)
            return;

        ApplySize();
    }

    private void ApplySize()
    {
        lastDistanceScale = manager.distanceScale;
        lastPlanetSizeScale = manager.planetSizeScale;

        float size = data.semiMajorAxis <= 0
            ? manager.SunDiameter
            : data.diameter / 12756f * manager.planetSizeScale;

        _transform.localScale = Vector3.one * size;
    }

    // --- Position ---

    private void ApplyPosition()
    {
        if (data.semiMajorAxis <= 0)
        {
            _transform.localPosition = Vector3.zero;
            return;
        }

        float a   = data.semiMajorAxis;
        float e   = data.eccentricity;
        float rad = currentAngle * Mathf.Deg2Rad;
        float r   = a * (1f - e * e) / (1f + e * Mathf.Cos(rad));

        _transform.localPosition = new Vector3(
            r * Mathf.Cos(rad) * manager.distanceScale,
            0f,
            r * Mathf.Sin(rad) * manager.distanceScale
        );
    }

    // --- Trail ---

    private void InitializeTrail()
    {
        angleStep = 360f / trailPoints;
        lastRecordedAngle = currentAngle;

        trailBuffer  = new Vector3[trailPoints];
        renderBuffer = new Vector3[trailPoints];
        for (int i = 0; i < trailPoints; i++)
            trailBuffer[i] = _transform.localPosition;

        trailObj = new("Trail_" + data.planetName);
        trailObj.transform.SetParent(_transform.parent);
        trailObj.transform.localPosition = Vector3.zero;
        trailObj.transform.localRotation = Quaternion.identity;

        lineRenderer = trailObj.AddComponent<LineRenderer>();
        lineRenderer.useWorldSpace = false;
        lineRenderer.positionCount  = trailPoints;
        lineRenderer.loop           = false;

        lineRenderer.widthCurve = new AnimationCurve(
            new Keyframe(0f, 0f),
            new Keyframe(1f, 1f)
        );
        lineRenderer.widthMultiplier = trailWidth;

        Gradient gradient = new();
        gradient.SetKeys(
            new GradientColorKey[] { new(Color.white, 0f), new(Color.white, 1f) },
            new GradientAlphaKey[] { new(0f, 0f), new(1f, 1f) }
        );
        lineRenderer.colorGradient = gradient;
        lineRenderer.material = CreateTrailMaterial();
    }

    private Material CreateTrailMaterial()
    {
        Material mat = trailMaterial != null
            ? new Material(trailMaterial)
            : new Material(Shader.Find("Universal Render Pipeline/Unlit"));

        mat.SetFloat("_Surface", 1f);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.SetInt("_Cull", 0);
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

        Color col = data.planetLineColor;
        mat.SetColor("_BaseColor", new Color(
            col.r * glowIntensity,
            col.g * glowIntensity,
            col.b * glowIntensity,
            1f
        ));

        return mat;
    }

    private void UpdateTrail()
    {
        if (lineRenderer == null || trailBuffer == null) return;

        if (Mathf.Abs(Mathf.DeltaAngle(lastRecordedAngle, currentAngle)) < angleStep)
            return;

        lastRecordedAngle = currentAngle;

        trailBuffer[writeIndex] = _transform.localPosition;
        writeIndex = (writeIndex + 1) % trailPoints;
        if (writeIndex == 0) bufferFull = true;

        int count = bufferFull ? trailPoints : writeIndex;
        lineRenderer.positionCount = count;

        for (int i = 0; i < count; i++)
            renderBuffer[i] = trailBuffer[bufferFull ? (writeIndex + i) % trailPoints : i];

        lineRenderer.SetPositions(renderBuffer);
    }
}
