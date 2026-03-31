using UnityEngine;

/// <summary>
/// Steuert die Bewegung (Rotation/Orbit) und die visuelle Darstellung der Umlaufbahn in XR.
/// Unterstützt sowohl Planeten (elliptisch) als auch die Sonne (statisch).
/// </summary>
[RequireComponent(typeof(MeshRenderer))]
public class PlanetBody : MonoBehaviour 
{
    public PlanetData data;
    
    [Header("Simulation")]
    private SolarSystemManager manager;
    private GameManager gameManager; // NEU: Referenz auf den GameManager
    private float currentTheta = 0;
    private float lastDistanceScale = -1f;
    private float lastSizeScale = -1f;

    [Header("Orbit Visualisierung")]
    public Material orbitMaterial;
    public float lineWidth = 0.002f;
    [Range(64, 512)] public int orbitResolution = 360;

    private LineRenderer lineRenderer;
    private Transform _transform;
    private Transform _mainCamTransform;

    void Start() 
    {
        _transform = transform;
        manager = FindObjectOfType<SolarSystemManager>();
        gameManager = FindObjectOfType<GameManager>(); // NEU: Dynamisches Finden des Managers
        
        if (Camera.main != null)
            _mainCamTransform = Camera.main.transform;

        if (manager == null || data == null) return;

        UpdateScale();
        
        // Orbit nur initialisieren, wenn es kein Zentralkörper ist (semiMajorAxis > 0)
        if (data.semiMajorAxis > 0)
        {
            InitializeOrbitLineRenderer();
        }
    }
    
    void InitializeOrbitLineRenderer() 
    {
        GameObject orbitObj = new GameObject("OrbitLine_" + data.name);
        orbitObj.transform.SetParent(_transform.parent); 
        orbitObj.transform.localPosition = Vector3.zero;
        orbitObj.transform.localRotation = Quaternion.identity;
        
        lineRenderer = orbitObj.AddComponent<LineRenderer>();
        lineRenderer.useWorldSpace = false; 
        lineRenderer.loop = true;
        lineRenderer.positionCount = orbitResolution;
        
        if (orbitMaterial != null)
            lineRenderer.material = orbitMaterial;
        else
            lineRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));

        lineRenderer.startColor = data.planetLineColor;
        lineRenderer.endColor = data.planetLineColor;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        
        DrawOrbitStatic();
    }
    
    void DrawOrbitStatic() 
    {
        if (lineRenderer == null) return;

        float a = data.semiMajorAxis;
        float e = data.eccentricity;
        
        for (int i = 0; i < orbitResolution; i++) 
        {
            float angle = (360f / orbitResolution) * i;
            // Kepler-Ellipse in Polarkoordinaten
            float r = (a * (1 - e * e)) / (1 + e * Mathf.Cos(angle * Mathf.Deg2Rad));
            
            float x = r * Mathf.Cos(angle * Mathf.Deg2Rad) * manager.distanceScale;
            float z = r * Mathf.Sin(angle * Mathf.Deg2Rad) * manager.distanceScale;
            
            lineRenderer.SetPosition(i, new Vector3(x, 0, z));
        }
    }

    void Update() 
    {
        if (manager == null || data == null) return;

        // Skalierungen werden immer berechnet, falls sich das System beim Platzieren ändert
        if (manager.distanceScale != lastDistanceScale) {
            lastDistanceScale = manager.distanceScale;
            DrawOrbitStatic();
        }

        if (manager.sizeScale != lastSizeScale) {
            UpdateScale();
        }

        // NEU: Bewegung/Rotation nur zulassen, wenn GameManager im Exploration-State ist
        if (gameManager != null && gameManager.currentState == GameManager.GameState.Exploration)
        {
            HandleRotation();
            HandleMovement();
        }

        HandleLineVisibility();
    }

    private void UpdateScale()
    {
        lastSizeScale = manager.sizeScale;
        // Skalierung relativ zur Erde (12756 km)
        float scaledSize = (data.diameter / 12756f) * manager.sizeScale;
        _transform.localScale = new Vector3(scaledSize, scaledSize, scaledSize);
    }

    private void HandleRotation()
    {
        // Eigenrotation um die Y-Achse
        _transform.Rotate(Vector3.up, data.rotationSpeed * manager.timeScale * Time.deltaTime);
    }

    private void HandleMovement()
    {
        // Wenn semiMajorAxis 0 ist, ist es die Sonne -> Bewegung überspringen
        if (data.semiMajorAxis <= 0)
        {
            _transform.localPosition = Vector3.zero;
            return;
        }

        float speed = (360f / data.orbitalPeriod) * manager.timeScale;
        currentTheta += speed * Time.deltaTime;

        float a = data.semiMajorAxis;
        float e = data.eccentricity;
        float r = (a * (1 - e * e)) / (1 + e * Mathf.Cos(currentTheta * Mathf.Deg2Rad));

        float x = r * Mathf.Cos(currentTheta * Mathf.Deg2Rad) * manager.distanceScale;
        float z = r * Mathf.Sin(currentTheta * Mathf.Deg2Rad) * manager.distanceScale;

        _transform.localPosition = new Vector3(x, 0, z);
    }

    private void HandleLineVisibility()
    {
        if (lineRenderer == null || _mainCamTransform == null) return;

        // Dynamische Linienbreite für VR/MR (Anti-Aliasing Effekt durch Distanz)
        float dist = Vector3.Distance(_mainCamTransform.position, _transform.position);
        lineRenderer.widthMultiplier = Mathf.Clamp(dist * 0.005f, manager.OrbitalLineWidth, manager.OrbitalLineWidth * 5f);
    }
}