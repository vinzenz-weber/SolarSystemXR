using UnityEngine;

public class PlanetBody : MonoBehaviour {
    public PlanetData data;
    private SolarSystemManager manager;
    private float currentTheta = 0;
    private LineRenderer lineRenderer;
    private int orbitResolution = 360;

    [Header("Visualisierung")]
    public float lineWidth = 0.01f;


    void Start() {
        manager = FindObjectOfType<SolarSystemManager>();
        // Initialer Scale (Durchmesser km in Meter umrechnen * Scale)
        float scaledSize = (data.diameter / 12756f) * manager.sizeScale; 
        transform.localScale = new Vector3(scaledSize, scaledSize, scaledSize);
        
        // LineRenderer für Umlaufbahn hinzufügen
        InitializeOrbitLineRenderer();
    }
    
    void InitializeOrbitLineRenderer() {
        // Neues GameObject für die Umlaufbahn erstellen
        GameObject orbitLine = new GameObject("Orbit_" + data.name);
        orbitLine.transform.SetParent(transform.parent);
        orbitLine.transform.localPosition = Vector3.zero;
        
        // LineRenderer hinzufügen
        lineRenderer = orbitLine.AddComponent<LineRenderer>();
        lineRenderer.positionCount = orbitResolution;
        
        // LineRenderer Material und Einstellungen
        Material lineMaterial = new Material(Shader.Find("Sprites/Default"));
        lineMaterial.color = new Color(data.planetLineColor.r, data.planetLineColor.g, data.planetLineColor.b, 0.3f); // Weiß mit Transparenz
        lineRenderer.material = lineMaterial;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        
        // Umlaufbahn zeichnen
        DrawOrbit();
    }
    
    void DrawOrbit() {
        float a = data.semiMajorAxis;
        float e = data.eccentricity;
        
        for (int i = 0; i < orbitResolution; i++) {
            float angle = (360f / orbitResolution) * i;
            float r = (a * (1 - e * e)) / (1 + e * Mathf.Cos(angle * Mathf.Deg2Rad));
            
            float x = r * Mathf.Cos(angle * Mathf.Deg2Rad) * manager.distanceScale;
            float z = r * Mathf.Sin(angle * Mathf.Deg2Rad) * manager.distanceScale;
            
            lineRenderer.SetPosition(i, new Vector3(x, 0, z));
        }
    }

    void Update() {
        // Zeitfortschritt (Winkelgeschwindigkeit)
        float speed = (360f / data.orbitalPeriod) * manager.timeScale;
        currentTheta += speed * Time.deltaTime;

        // Ellipsen-Berechnung (in AU)
        float a = data.semiMajorAxis;
        float e = data.eccentricity;
        float r = (a * (1 - e * e)) / (1 + e * Mathf.Cos(currentTheta * Mathf.Deg2Rad));

        // Umrechnung in Unity-Meter
        float x = r * Mathf.Cos(currentTheta * Mathf.Deg2Rad) * manager.distanceScale;
        float z = r * Mathf.Sin(currentTheta * Mathf.Deg2Rad) * manager.distanceScale;

        transform.localPosition = new Vector3(x, 0, z);
    }
}
