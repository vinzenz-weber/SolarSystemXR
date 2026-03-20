using UnityEngine;

/// <summary>
/// Steuert die elliptische Bewegung und die visuelle Darstellung der Umlaufbahn in XR.
/// </summary>
[RequireComponent(typeof(MeshRenderer))]
public class PlanetBody : MonoBehaviour 
{
    public PlanetData data;
    
    [Header("Simulation")]
    private SolarSystemManager manager;
    private float currentTheta = 0;
    private float lastDistanceScale = -1f;
    private float lastSizeScale = -1f;

    [Header("Orbit Visualisierung")]
    public Material orbitMaterial;
    public float lineWidth;
    [Range(64, 512)] public int orbitResolution = 360;

    private LineRenderer lineRenderer;
    private Transform _transform;
    private Transform _mainCamTransform;

    void Start() 
    {
        _transform = transform;
        manager = FindObjectOfType<SolarSystemManager>();
        
        if (Camera.main != null)
            _mainCamTransform = Camera.main.transform;

        // Initialer Scale basierend auf Erddurchmesser (Vergleichswert)
        float scaledSize = (data.diameter / 12756f) * manager.sizeScale; 
        _transform.localScale = new Vector3(scaledSize, scaledSize, scaledSize);

        lastSizeScale = manager.sizeScale;
        lastDistanceScale = manager.distanceScale;

        InitializeOrbitLineRenderer();
    }
    
    void InitializeOrbitLineRenderer() 
    {
        // Wir hängen den LineRenderer an ein Kind-Objekt, um Unabhängigkeit zu bewahren
        GameObject orbitObj = new GameObject("OrbitLine_" + data.name);
        orbitObj.transform.SetParent(_transform.parent); // Gleicher Parent wie Planet (Sonne)
        orbitObj.transform.localPosition = Vector3.zero;
        orbitObj.transform.localRotation = Quaternion.identity;
        
        lineRenderer = orbitObj.AddComponent<LineRenderer>();
        lineRenderer.useWorldSpace = false; // ESSENTIELL: Orbits müssen mit dem System mitwandern (Grabbing)
        lineRenderer.loop = true;
        lineRenderer.positionCount = orbitResolution;
        
        // Zuweisung des Materials (Im Inspector ein Unlit/Emission Material wählen!)
        if (orbitMaterial != null)
        {
            lineRenderer.material = orbitMaterial;
        }
        else
        {
            // Fallback falls Material vergessen wurde
            lineRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        }

        lineRenderer.startColor = data.planetLineColor;
        lineRenderer.endColor = data.planetLineColor;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        
        DrawOrbitStatic();
    }
    
    /// <summary>
    /// Zeichnet die elliptische Bahn einmalig vorab.
    /// </summary>
    void DrawOrbitStatic() 
    {
        float a = data.semiMajorAxis;
        float e = data.eccentricity;
        
        for (int i = 0; i < orbitResolution; i++) 
        {
            float angle = (360f / orbitResolution) * i;
            float r = (a * (1 - e * e)) / (1 + e * Mathf.Cos(angle * Mathf.Deg2Rad));
            
            float x = r * Mathf.Cos(angle * Mathf.Deg2Rad) * manager.distanceScale;
            float z = r * Mathf.Sin(angle * Mathf.Deg2Rad) * manager.distanceScale;
            
            lineRenderer.SetPosition(i, new Vector3(x, 0, z));
        }
    }

    void Update() 
    {
        if (manager == null || data == null) return;

        // Wenn sich die Skalierung im Manager während Play ändert, sofort anpassen
        if (manager.distanceScale != lastDistanceScale) {
            lastDistanceScale = manager.distanceScale;
            DrawOrbitStatic();
        }

        if (manager.sizeScale != lastSizeScale) {
            lastSizeScale = manager.sizeScale;
            float scaledSize = (data.diameter / 12756f) * manager.sizeScale;
            _transform.localScale = new Vector3(scaledSize, scaledSize, scaledSize);
        }

        HandleMovement();
        HandleLineVisibility();
    }

    private void HandleMovement()
    {
        // Winkelgeschwindigkeit (basierend auf orbitalPeriod in Tagen)
        float speed = (360f / data.orbitalPeriod) * manager.timeScale;
        currentTheta += speed * Time.deltaTime;

        // Ellipsen-Berechnung (Polarkoordinaten)
        float a = data.semiMajorAxis;
        float e = data.eccentricity;
        float r = (a * (1 - e * e)) / (1 + e * Mathf.Cos(currentTheta * Mathf.Deg2Rad));

        // Umrechnung AU -> Unity Meter -> DistanceScale
        float x = r * Mathf.Cos(currentTheta * Mathf.Deg2Rad) * manager.distanceScale;
        float z = r * Mathf.Sin(currentTheta * Mathf.Deg2Rad) * manager.distanceScale;

        _transform.localPosition = new Vector3(x, 0, z);
    }

    private void HandleLineVisibility()
    {
        if (lineRenderer == null || _mainCamTransform == null) return;

        lineWidth = manager.OrbitalLineWidth; // Synchronisation mit Manager-Einstellung

        // Dynamische Dicke: Verhindert, dass Linien in der Ferne flimmern oder verschwinden
        float dist = Vector3.Distance(_mainCamTransform.position, _transform.position);
        
        // Faktor 0.005f ist ein Erfahrungswert für VR; anpassen falls nötig
        lineRenderer.widthMultiplier = Mathf.Clamp(dist * 0.005f, lineWidth, lineWidth * 10f);
    }
}