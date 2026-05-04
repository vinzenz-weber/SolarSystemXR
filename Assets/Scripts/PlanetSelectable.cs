using UnityEngine;

// Liegt auf einem Planet-Objekt und macht dessen PlanetData per Ray-Auswahl verfuegbar.
public class PlanetSelectable : MonoBehaviour
{
    public PlanetData planetData;

    public void Select()
    {
        PlanetData selectedPlanetData = GetPlanetData();

        if (selectedPlanetData == null)
        {
            Debug.LogWarning("PlanetSelectable: Kein PlanetData auf " + gameObject.name + " zugewiesen.");
            return;
        }

        if (PlanetInfoPanelManager.Instance == null)
        {
            Debug.LogWarning("PlanetSelectable: Kein PlanetInfoPanelManager in der Szene gefunden.");
            return;
        }

        PlanetInfoPanelManager.Instance.ShowPlanet(selectedPlanetData, this);
    }

    public PlanetData GetPlanetData()
    {
        if (planetData != null)
        {
            return planetData;
        }

        InteractablePlanetVisual interactableVisual = GetComponentInChildren<InteractablePlanetVisual>(true);
        if (interactableVisual != null && interactableVisual.PlanetData != null)
        {
            return interactableVisual.PlanetData;
        }

        PlanetBody planetBody = GetComponentInChildren<PlanetBody>(true);
        if (planetBody != null && planetBody.data != null)
        {
            return planetBody.data;
        }

        return null;
    }
}
