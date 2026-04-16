using UnityEngine;
using System.Collections.Generic;

[ExecuteAlways]
public class SunPasser : MonoBehaviour
{
    [Tooltip("Alle Planeten-GameObjects hier zuweisen (inkl. LOD-Kinder werden automatisch gefunden)")]
    public GameObject[] planets;

    private static readonly int SunPosID = Shader.PropertyToID("_SunPosition");

    // Alle gefundenen Materialien werden hier gecacht
    private Material[] _alleMaterialien;

    void OnEnable()
    {
        MaterialienCachen();
    }

    // Sammelt alle Materialien von allen Renderern inklusive LOD-Kinder
    void MaterialienCachen()
    {
        var gefunden = new List<Material>();

        foreach (var planet in planets)
        {
            if (planet == null) continue;

            // GetComponentsInChildren findet auch Renderer in LOD0, LOD1, LOD2 etc.
            var renderer = planet.GetComponentsInChildren<Renderer>();
            foreach (var r in renderer)
            {
                foreach (var mat in r.sharedMaterials)
                {
                    // Nur einmal hinzufuegen, auch wenn mehrere Renderer dasselbe Material nutzen
                    if (mat != null && !gefunden.Contains(mat))
                        gefunden.Add(mat);
                }
            }
        }

        _alleMaterialien = gefunden.ToArray();
    }

    void Update()
    {
        if (_alleMaterialien == null) return;

        Vector3 worldPos = transform.position;
        foreach (var mat in _alleMaterialien)
        {
            if (mat != null)
                mat.SetVector(SunPosID, worldPos);
        }
    }
}