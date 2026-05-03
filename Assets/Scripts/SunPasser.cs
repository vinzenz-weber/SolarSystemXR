using UnityEngine;
using System.Collections.Generic;

[ExecuteAlways]
public class SunPasser : MonoBehaviour
{
    [Header("Automatisches Sammeln")]
    [Tooltip("Wenn aktiv, werden alle GameObjects auf dem angegebenen Layer automatisch als Planeten verwendet.")]
    public bool usePlanetLayer = true;

    [Tooltip("Layername fuer Planeten-GameObjects. LOD-Kinder und Renderer darunter werden automatisch mitgenommen.")]
    public string planetLayerName = "Planet";

    [Tooltip("Wenn aktiv, wird der Material-Cache regelmaessig erneuert. Das ist hilfreich, wenn Planeten erst zur Laufzeit gespawnt werden.")]
    public bool refreshAutomatically = true;

    [Tooltip("Zeit in Sekunden zwischen automatischen Cache-Aktualisierungen.")]
    public float refreshInterval = 1f;

    [Header("Manuelle Zusatzobjekte")]
    [Tooltip("Optional: Sonderobjekte manuell ergaenzen. Fuer normale Planeten reicht der Layer.")]
    public GameObject[] planets;

    private static readonly int SunPosID = Shader.PropertyToID("_SunPosition");

    // Alle gefundenen Materialien werden hier gecacht
    private Material[] _alleMaterialien;
    private float _nextRefreshTime;

    private void OnEnable()
    {
        CacheMaterialien();
        PlaneNaechsteAktualisierung();
    }

    private void Start()
    {
        CacheMaterialien();
        PlaneNaechsteAktualisierung();
    }

    private void OnValidate()
    {
        if (refreshInterval < 0.1f)
        {
            refreshInterval = 0.1f;
        }
    }

    private void Update()
    {
        if (refreshAutomatically && Time.realtimeSinceStartup >= _nextRefreshTime)
        {
            CacheMaterialien();
            PlaneNaechsteAktualisierung();
        }

        if (_alleMaterialien == null) return;

        Vector3 worldPos = transform.position;
        foreach (Material mat in _alleMaterialien)
        {
            if (mat != null)
            {
                mat.SetVector(SunPosID, worldPos);
            }
        }
    }

    // Sammelt alle Materialien von allen Planeten-Renderern inklusive LOD-Kinder.
    private void CacheMaterialien()
    {
        HashSet<Material> gefunden = new HashSet<Material>();

        if (usePlanetLayer)
        {
            SammleMaterialienVomPlanetLayer(gefunden);
        }

        SammleMaterialienVonManuellenPlaneten(gefunden);

        _alleMaterialien = new Material[gefunden.Count];
        gefunden.CopyTo(_alleMaterialien);
    }

    private void SammleMaterialienVomPlanetLayer(HashSet<Material> gefunden)
    {
        int planetLayer = LayerMask.NameToLayer(planetLayerName);
        if (planetLayer < 0)
        {
            Debug.LogWarning("SunPasser: Layer '" + planetLayerName + "' wurde nicht gefunden.");
            return;
        }

        Transform[] alleTransforms = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Transform transformInScene in alleTransforms)
        {
            if (transformInScene.gameObject.layer != planetLayer) continue;

            SammleRendererMaterialien(transformInScene.gameObject, gefunden);
        }
    }

    private void SammleMaterialienVonManuellenPlaneten(HashSet<Material> gefunden)
    {
        if (planets == null) return;

        foreach (GameObject planet in planets)
        {
            if (planet == null) continue;

            SammleRendererMaterialien(planet, gefunden);
        }
    }

    private void SammleRendererMaterialien(GameObject root, HashSet<Material> gefunden)
    {
        // GetComponentsInChildren findet auch Renderer in LOD0, LOD1, LOD2 etc.
        Renderer[] renderer = root.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer einzelnerRenderer in renderer)
        {
            foreach (Material mat in einzelnerRenderer.sharedMaterials)
            {
                if (mat != null)
                {
                    gefunden.Add(mat);
                }
            }
        }
    }

    private void PlaneNaechsteAktualisierung()
    {
        _nextRefreshTime = Time.realtimeSinceStartup + refreshInterval;
    }
}
