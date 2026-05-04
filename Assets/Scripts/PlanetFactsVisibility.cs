using MRMotifs.PassthroughTransitioning;
using UnityEngine;

// Schaltet alle Child-Objekte mit dem Namen "Facts".
// Sichtbar sind Facts nur fuer den aktiv ausgewaehlten Einzelplaneten im Passthrough-WORLD-Modus.
public static class PlanetFactsVisibility
{
    private const string FactsObjectName = "Facts";

    private static PlanetSelectable _selectedPlanet;

    public static void SelectPlanet(PlanetSelectable selectedPlanet)
    {
        _selectedPlanet = selectedPlanet;
        Refresh();
    }

    public static void ClearSelection()
    {
        _selectedPlanet = null;
        Refresh();
    }

    public static void Refresh()
    {
        bool canShowFacts = CanShowFacts();
        Transform selectedRoot = _selectedPlanet != null ? _selectedPlanet.transform : null;

        Transform[] allTransforms = Object.FindObjectsByType<Transform>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        foreach (Transform currentTransform in allTransforms)
        {
            if (currentTransform == null) continue;
            if (IsFactsObject(currentTransform) == false) continue;

            bool isSelectedPlanetFacts = selectedRoot != null && currentTransform.IsChildOf(selectedRoot);
            currentTransform.gameObject.SetActive(canShowFacts && isSelectedPlanetFacts);
        }
    }

    private static bool CanShowFacts()
    {
        if (_selectedPlanet == null) return false;

        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.WORLD)
        {
            return false;
        }

        PassthroughDissolver passthroughDissolver = FindPassthroughDissolver();
        return passthroughDissolver != null && passthroughDissolver.IsPassthroughActive;
    }

    private static PassthroughDissolver FindPassthroughDissolver()
    {
        PassthroughDissolver[] dissolvers = Object.FindObjectsByType<PassthroughDissolver>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        return dissolvers.Length > 0 ? dissolvers[0] : null;
    }

    private static bool IsFactsObject(Transform currentTransform)
    {
        return string.Equals(
            currentTransform.name,
            FactsObjectName,
            System.StringComparison.OrdinalIgnoreCase);
    }
}
