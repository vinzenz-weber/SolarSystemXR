using UnityEngine;

// Liegt auf einem Planet-Objekt und macht dessen PlanetData per Ray-Auswahl verfuegbar.
public class PlanetSelectable : MonoBehaviour
{
    public PlanetData planetData;

    public void Select()
    {
        if (planetData == null)
        {
            Debug.LogWarning("PlanetSelectable: Kein PlanetData auf " + gameObject.name + " zugewiesen.");
            return;
        }

        if (PlanetInfoPanelManager.Instance == null)
        {
            Debug.LogWarning("PlanetSelectable: Kein PlanetInfoPanelManager in der Szene gefunden.");
            return;
        }

        PlanetInfoPanelManager.Instance.ShowPlanet(planetData);
    }
}
