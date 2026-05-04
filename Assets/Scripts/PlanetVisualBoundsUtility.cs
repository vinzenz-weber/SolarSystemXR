using TMPro;
using UnityEngine;

// Gemeinsame Bounds-Hilfe fuer Planet-Visuals.
// Wichtig: Lern-Facts, Linien und Text duerfen die Planetengroesse nicht beeinflussen.
public static class PlanetVisualBoundsUtility
{
    private const string FactsObjectName = "Facts";

    public static bool TryGetPlanetVisualBounds(Transform root, bool includeInactive, out Bounds bounds)
    {
        bounds = new Bounds();
        if (root == null) return false;

        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(includeInactive);
        bool hasBounds = false;

        foreach (Renderer renderer in renderers)
        {
            if (renderer == null) continue;
            if (ShouldIgnoreRenderer(renderer)) continue;

            if (hasBounds == false)
            {
                bounds = renderer.bounds;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }

        return hasBounds;
    }

    public static float GetLargestWorldSize(Bounds bounds)
    {
        return Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
    }

    public static bool ShouldIgnoreRenderer(Renderer renderer)
    {
        if (renderer == null) return true;

        if (renderer is LineRenderer)
        {
            return true;
        }

        if (renderer.GetComponentInParent<TMP_Text>(true) != null)
        {
            return true;
        }

        if (renderer.GetComponentInParent<PlanetFactAnchor>(true) != null)
        {
            return true;
        }

        return HasParentNamed(renderer.transform, FactsObjectName);
    }

    private static bool HasParentNamed(Transform current, string objectName)
    {
        while (current != null)
        {
            if (string.Equals(current.name, objectName, System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }
}
