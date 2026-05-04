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

    [Header("Labels")]
    [Tooltip("Ziehe hier das Label-GameObject aus dem SonnensystemPrefab hinein.")]
    public GameObject planetLabelTemplate;
    [Tooltip("Abstand zwischen Planet-Unterseite und Label in Weltmetern.")]
    public float planetLabelWorldGap = 0.03f;
    [Tooltip("Einheitlicher Multiplikator fuer alle Labels. 1 = Groesse des Label-Templates.")]
    public float planetLabelWorldScale = 1f;
    [Tooltip("Stabiler Radius-Faktor fuer die Label-Position. 0.5 passt zu Planeten-Prefabs mit 1 Einheit Durchmesser.")]
    public float planetLabelRadiusFactor = 0.5f;

    [Header("Sonne")]
    [Range(0.01f, 1f)]
    public float sunSizeRatio = 0.5f;
    public float SunDiameter => sunSizeRatio * 0.307f * distanceScale * 2f;

    [Header("Simulation & Zeit")]
    public float timeScale = 1f;
    [Range(0f, 1f)]
    [Tooltip("Bremst nur die sichtbare Eigenrotation der Planeten. 0.02 = 2 Prozent der normalen Simulationsgeschwindigkeit.")]
    public float EigenrotationMultiplikator = 0.02f;
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
        public Transform labelTransform;
        public LineRenderer orbitLine;
        public float currentRotationAngle;
    }

    private List<PlanetInstance> instances = new List<PlanetInstance>();
    private Vector3 labelTemplateWorldScale = Vector3.one;

    // Tracking für Live-Updates im Editor
    private float lastDistScale, lastSizeScale, lastEccMult, lastIncMult;
    private DistanceScaleMode lastMode;

    void Start()
    {
        CacheLabelTemplateScale();
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
                instance.currentRotationAngle += rotationDegreesPerDay * timeScale * EigenrotationMultiplikator * Time.deltaTime;
                instance.currentRotationAngle %= 360f;
            }

            Quaternion tilt = Quaternion.Euler(instance.data.axialTilt, 0f, 0f);
            Quaternion spin = Quaternion.Euler(0f, -instance.currentRotationAngle, 0f);
            instance.planetTransform.localRotation = tilt * spin;

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

            UpdatePlanetLabel(instance);
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
            instance.planetTransform.localPosition = data.semiMajorAxis > 0
                ? CalculateKeplerPosition(data, currentSimulationDays)
                : Vector3.zero;

            if (planetLabelTemplate != null)
            {
                GameObject labelObj = Instantiate(planetLabelTemplate, transform);
                labelObj.name = "Label_" + data.planetName;
                instance.labelTransform = labelObj.transform;
                
                var tmpText = labelObj.GetComponentInChildren<TMPro.TMP_Text>(true);
                if (tmpText != null)
                {
                    tmpText.text = data.planetName;
                }
                else
                {
                    var uiText = labelObj.GetComponentInChildren<UnityEngine.UI.Text>(true);
                    if (uiText != null)
                    {
                        uiText.text = data.planetName;
                    }
                }
                labelObj.SetActive(true);
            }

            UpdatePlanetSize(instance);
            UpdatePlanetLabel(instance);

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

        PlanetFactsVisibility.Refresh();
    }

    private void UpdatePlanetLabel(PlanetInstance instance)
    {
        if (instance == null || instance.planetTransform == null || instance.labelTransform == null) return;

        float planetRadius = GetStablePlanetWorldRadius(instance.planetTransform);
        Vector3 labelPosition = instance.planetTransform.position + Vector3.down * (planetRadius + planetLabelWorldGap);

        instance.labelTransform.position = labelPosition;
        SetWorldScale(instance.labelTransform, labelTemplateWorldScale * Mathf.Max(0.0001f, planetLabelWorldScale));
    }

    private float GetStablePlanetWorldRadius(Transform planetTransform)
    {
        if (planetTransform == null) return 0f;

        // Renderer.bounds jittert bei rotierenden Meshes, weil die Bounds weltachsen-ausgerichtet sind.
        // Die Transform-Skalierung bleibt stabil und reicht fuer die Label-Hoehe im Sonnensystem-Modus.
        Vector3 worldScale = planetTransform.lossyScale;
        float largestScale = Mathf.Max(Mathf.Abs(worldScale.x), Mathf.Abs(worldScale.y), Mathf.Abs(worldScale.z));
        return largestScale * Mathf.Max(0f, planetLabelRadiusFactor);
    }

    private void CacheLabelTemplateScale()
    {
        if (planetLabelTemplate == null)
        {
            labelTemplateWorldScale = Vector3.one;
            return;
        }

        labelTemplateWorldScale = planetLabelTemplate.transform.lossyScale;
        if (labelTemplateWorldScale.sqrMagnitude < 0.0001f)
        {
            labelTemplateWorldScale = planetLabelTemplate.transform.localScale;
        }
    }

    private void SetWorldScale(Transform targetTransform, Vector3 worldScale)
    {
        if (targetTransform == null) return;

        Transform parent = targetTransform.parent;
        if (parent == null)
        {
            targetTransform.localScale = worldScale;
            return;
        }

        Vector3 parentScale = parent.lossyScale;
        targetTransform.localScale = new Vector3(
            SafeDivide(worldScale.x, parentScale.x),
            SafeDivide(worldScale.y, parentScale.y),
            SafeDivide(worldScale.z, parentScale.z)
        );
    }

    private float SafeDivide(float value, float divisor)
    {
        if (Mathf.Abs(divisor) < 0.0001f)
        {
            return value;
        }

        return value / divisor;
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
        float e = Mathf.Clamp(data.eccentricity * exzentrizitaetMultiplikator, 0f, 0.95f);
        
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

    public void SetPlanetScale(float neuerWert)
    {
        planetSizeScale = Mathf.Max(0f, neuerWert);
    }

    public void SetOrbitalSpeed(float neuerWert)
    {
        timeScale = Mathf.Max(0f, neuerWert);
    }

    public void SetInclinationMultiplier(float neuerWert)
    {
        inklinationMultiplikator = Mathf.Max(0f, neuerWert);
    }

    public void SetOrbitalDistance(float neuerWert)
    {
        distanceScale = Mathf.Max(0f, neuerWert);
    }

    public void SetEccentricityMultiplier(float neuerWert)
    {
        exzentrizitaetMultiplikator = Mathf.Max(0f, neuerWert);
    }

    public void SetSpacingMode(bool isCompactXrMode)
    {
        darstellungsModus = isCompactXrMode
            ? DistanceScaleMode.WurzelKompakt_XR
            : DistanceScaleMode.LinearRealistisch;
    }

    public void ResetPanelSettings()
    {
        SetOrbitalDistance(0.05f);
        SetPlanetScale(0.002f);
        SetOrbitalSpeed(1f);
        SetInclinationMultiplier(1f);
        SetEccentricityMultiplier(1f);
        SetSpacingMode(true);
    }
}
