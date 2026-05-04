using TMPro;
using UnityEngine;

// Schaltet Planet-Labels nur in den echten Minispielen sichtbar.
// Wichtig: UI-Labels heissen oft ebenfalls "Label", deshalb wird zusaetzlich
// geprueft, ob das Objekt zu einem Planet-Root gehoert.
public static class PlanetLabelVisibility
{
    private const string LabelObjectName = "Label";

    public static void Refresh()
    {
        bool canShowLabels = CanShowLabels();

        Transform[] allTransforms = Object.FindObjectsByType<Transform>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        foreach (Transform currentTransform in allTransforms)
        {
            if (currentTransform == null) continue;
            if (IsPlanetLabelObject(currentTransform) == false) continue;

            currentTransform.gameObject.SetActive(canShowLabels);
        }
    }

    public static void RefreshInRoot(Transform root)
    {
        if (root == null) return;

        bool canShowLabels = CanShowLabels();
        Transform[] childTransforms = root.GetComponentsInChildren<Transform>(true);

        foreach (Transform currentTransform in childTransforms)
        {
            if (currentTransform == null) continue;
            if (IsPlanetLabelObject(currentTransform) == false) continue;

            currentTransform.gameObject.SetActive(canShowLabels);
        }
    }

    private static bool CanShowLabels()
    {
        if (GameManager.Instance == null) return false;

        return GameManager.Instance.CurrentState == GameState.TEST_REIHENFOLGE
            || GameManager.Instance.CurrentState == GameState.TEST_SIZE;
    }

    private static bool IsPlanetLabelObject(Transform currentTransform)
    {
        if (string.Equals(currentTransform.name, LabelObjectName, System.StringComparison.OrdinalIgnoreCase) == false)
        {
            return false;
        }

        if (currentTransform.GetComponentInChildren<TMP_Text>(true) == null)
        {
            return false;
        }

        return currentTransform.GetComponentInParent<PlanetBody>(true) != null
            || currentTransform.GetComponentInParent<PlanetSelectable>(true) != null
            || currentTransform.GetComponentInParent<InteractablePlanetVisual>(true) != null
            || currentTransform.GetComponentInParent<ReihenfolgePlanet>(true) != null;
    }
}
