using System.Collections.Generic;
using UnityEngine;

public class SolarSystemManager : MonoBehaviour
{
    public enum DistanceScaleMode
    {
        LinearRealistisch,
        WurzelKompakt_XR
    }

    [Header("Planeten Daten")]
    [Tooltip("Sonne an Stelle 0, danach die 8 Planeten.")]
    public List<PlanetData> planetenDaten;

    [Header("Darstellung & Skalierung")]
    public DistanceScaleMode darstellungsModus = DistanceScaleMode.WurzelKompakt_XR;
    public float distanceScale = 0.5f; 
    public float planetSizeScale = 0.002f;

    [Header("Sonne")]
    [Range(0.01f, 1f)]
    public float sunSizeRatio = 0.5f;
    public float SunDiameter => sunSizeRatio * 0.307f * distanceScale * 2f;

    [Header("Simulation & Zeit")]
    public float timeScale = 1f;
    public double currentSimulationDays = 0.0;

    [Header("Lernmodus (Übertreibungen)")]
    [Range(0f, 5f)]
    public float exzentrizitaetMultiplikator = 1f;
    [Range(0f, 5f)]
    public float inklinationMultiplikator = 1f;

    [Header("Orbit Linien (Bahnen)")]
    public int orbitResolution = 100;
    public float lineWidth = 0.002f;
    [Tooltip("Wie viel Grad um den Planeten herum die Linie ausgeblendet wird.")]
    [Range(1f, 45f)]
    public float orbitGapAngle = 10f;
    public Material lineMaterial;

    private class PlanetInstance
    {
        public PlanetData data;
        public Transform planetTransform;
        public LineRenderer orbitLine;
        public float currentRotationAngle;
    }

    private List<PlanetInstance> instances = new List<PlanetInstance>();

    // Tracking für Live-Updates im Editor
    private float lastDistScale, lastSizeScale, lastEccMult, lastIncMult;
    private DistanceScaleMode lastMode;

    void Start()
    {
        InitializeSystem();
        SaveCurrentScales();
    }

    void Update()
    {
        currentSimulationDays += timeScale * Time.deltaTime;
        bool scalesChanged = CheckForScaleChanges();

        foreach (var instance in instances)
        {
            // A) Eigenrotation
            if (instance.data.rotationSpeed != 0f)
            {
                float rotationDegreesPerDay = 360f / instance.data.rotationSpeed;
                instance.currentRotationAngle += rotationDegreesPerDay * timeScale * Time.deltaTime;
                instance.currentRotationAngle %= 360f;

                Quaternion tilt = Quaternion.Euler(instance.data.axialTilt, 0f, 0f);
                Quaternion spin = Quaternion.Euler(0f, -instance.currentRotationAngle, 0f);
                instance.planetTransform.localRotation = tilt * spin;
            }

            // B) Umlaufbahn & Position & Dynamische Linie
            if (instance.data.semiMajorAxis > 0)
            {
                Vector3 newPos = CalculateKeplerPosition(instance.data, currentSimulationDays);
                instance.planetTransform.localPosition = newPos;

                // Zeichnet die Linie jeden Frame neu, damit die Lücke mit dem Planeten mitwandert!
                UpdateOrbitLine(instance, currentSimulationDays);
            }

            // C) Größe updaten
            if (scalesChanged)
            {
                UpdatePlanetSize(instance);
            }
        }
    }

    private void InitializeSystem()
    {
        foreach (var data in planetenDaten)
        {
            if (data == null || data.planetPrefab == null) continue;

            PlanetInstance instance = new PlanetInstance { data = data };

            GameObject planetObj = Instantiate(data.planetPrefab, transform);
            planetObj.name = data.planetName;
            instance.planetTransform = planetObj.transform;

            UpdatePlanetSize(instance);

            if (data.semiMajorAxis > 0)
            {
                GameObject lineObj = new GameObject($"OrbitLine_{data.planetName}");
                lineObj.transform.SetParent(transform);
                lineObj.transform.localPosition = Vector3.zero;
                // Rotation wird pro Frame in UpdateOrbitLine an die Bahnebene angepasst.

                LineRenderer lr = lineObj.AddComponent<LineRenderer>();
                lr.useWorldSpace = true;
                // TransformZ: Ribbon-Normale = transform.forward. Wir richten die Transform
                // so aus, dass forward genau senkrecht auf der Bahnebene steht -> Ribbon
                // liegt flach in der Bahnebene (mit deren Inklination).
                lr.alignment = LineAlignment.TransformZ;
                lr.loop = false; // Kein Loop mehr, wegen der Lücke am Planeten
                lr.positionCount = orbitResolution;
                lr.startWidth = lineWidth;
                lr.endWidth = lineWidth;

                if (lineMaterial != null) lr.material = lineMaterial;
                else lr.material = new Material(Shader.Find("Sprites/Default"));

                // Weicher Fade-Out an den Enden der Linie (bei der Lücke)
                Gradient fadeGradient = new Gradient();
                fadeGradient.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                    new GradientAlphaKey[] {
                        new GradientAlphaKey(0f, 0f),       // Start (unsichtbar)
                        new GradientAlphaKey(1f, 0.05f),    // Schnell voll sichtbar werden
                        new GradientAlphaKey(1f, 0.95f),    // Voll sichtbar bleiben
                        new GradientAlphaKey(0f, 1f)        // Ende (wieder unsichtbar am Planeten)
                    }
                );
                lr.colorGradient = fadeGradient;

                instance.orbitLine = lr;
            }

            instances.Add(instance);
        }
    }

    private void UpdateOrbitLine(PlanetInstance instance, double timeInDays)
    {
        if (instance.orbitLine == null) return;

        // Ribbon in die Bahnebene legen: Lokale Z-Achse des LineObjects = Normale der Bahnebene.
        // Argument-of-Periapsis (w) braucht es hier nicht – das dreht nur INNERHALB der Ebene.
        float inc = instance.data.inclination * inklinationMultiplikator;
        Quaternion planeRotation = Quaternion.Euler(0f, instance.data.longitudeOfAscendingNode, 0f)
                                 * Quaternion.Euler(inc, 0f, 0f);
        instance.orbitLine.transform.localRotation = planeRotation * Quaternion.Euler(90f, 0f, 0f);

        double meanMotion = 360.0 / instance.data.orbitalPeriod;
        double currentAnomaly = instance.data.meanAnomalyAtEpoch + (meanMotion * timeInDays);
        float currentAngle = (float)(currentAnomaly % 360.0);

        Vector3[] points = new Vector3[orbitResolution];
        
        // Berechne Start und Ende der Linie so, dass beim Planeten eine Lücke entsteht
        float startAngle = currentAngle + orbitGapAngle;
        float endAngle = currentAngle + 360f - orbitGapAngle;
        float angleStep = (endAngle - startAngle) / (orbitResolution - 1);

        for (int i = 0; i < orbitResolution; i++)
        {
            float angle = startAngle + (i * angleStep);
            // Da wir useWorldSpace = true nutzen, müssen wir transform.TransformPoint anwenden,
            // falls das gesamte Sonnensystem irgendwo im Raum platziert wird.
            Vector3 localPos = CalculateKeplerPositionByAnomaly(instance.data, angle);
            points[i] = transform.TransformPoint(localPos);
        }

        instance.orbitLine.SetPositions(points);
        instance.orbitLine.startWidth = lineWidth;
        instance.orbitLine.endWidth = lineWidth;
    }

    private Vector3 CalculateKeplerPosition(PlanetData data, double timeInDays)
    {
        double meanMotion = 360.0 / data.orbitalPeriod;
        double currentAnomaly = data.meanAnomalyAtEpoch + (meanMotion * timeInDays);
        return CalculateKeplerPositionByAnomaly(data, (float)(currentAnomaly % 360.0));
    }

    private Vector3 CalculateKeplerPositionByAnomaly(PlanetData data, float trueAnomalyDegrees)
    {
        float a = data.semiMajorAxis;
        float e = data.eccentricity * exzentrizitaetMultiplikator;
        
        float rad = trueAnomalyDegrees * Mathf.Deg2Rad;
        float r = a * (1f - e * e) / (1f + e * Mathf.Cos(rad));

        Vector3 orbitPos = new Vector3(r * Mathf.Cos(rad), 0f, r * Mathf.Sin(rad));

        float inc = data.inclination * inklinationMultiplikator;
        float Omega = data.longitudeOfAscendingNode;
        float w = data.argumentOfPeriapsis;

        Quaternion orbitalRotation = Quaternion.Euler(0f, Omega, 0f) 
                                   * Quaternion.Euler(inc, 0f, 0f) 
                                   * Quaternion.Euler(0f, w, 0f);

        Vector3 rawPos = orbitalRotation * orbitPos;

        if (darstellungsModus == DistanceScaleMode.WurzelKompakt_XR)
        {
            float dist = rawPos.magnitude;
            if (dist > 0.0001f)
            {
                float compressedDist = Mathf.Sqrt(dist);
                return (rawPos / dist) * compressedDist * distanceScale;
            }
        }

        return rawPos * distanceScale;
    }

    private void UpdatePlanetSize(PlanetInstance instance)
    {
        float size = instance.data.semiMajorAxis <= 0
            ? SunDiameter
            : (instance.data.diameter / 12742f) * planetSizeScale;

        instance.planetTransform.localScale = Vector3.one * size;
    }

    private bool CheckForScaleChanges()
    {
        if (distanceScale != lastDistScale || planetSizeScale != lastSizeScale || 
            exzentrizitaetMultiplikator != lastEccMult || inklinationMultiplikator != lastIncMult ||
            darstellungsModus != lastMode)
        {
            SaveCurrentScales();
            return true;
        }
        return false;
    }

    private void SaveCurrentScales()
    {
        lastDistScale = distanceScale;
        lastSizeScale = planetSizeScale;
        lastEccMult = exzentrizitaetMultiplikator;
        lastIncMult = inklinationMultiplikator;
        lastMode = darstellungsModus;
    }
}