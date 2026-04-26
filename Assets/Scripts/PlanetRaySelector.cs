using UnityEngine;

// Einfacher Controller-Ray fuer Planet-Auswahl per Trigger.
// Fuer XR-UI/Interaction bleibt Meta SDK zustaendig; dieses Script verbindet nur Ray-Hit mit PlanetData.
public class PlanetRaySelector : MonoBehaviour
{
    [Header("Ray")]
    public Transform rayOrigin;
    public float maxDistance = 20f;
    public LayerMask planetLayers = ~0;

    [Header("Input")]
    public OVRInput.Button selectButton = OVRInput.Button.SecondaryIndexTrigger;

    private void Update()
    {
        if (rayOrigin == null) return;

        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.WORLD)
        {
            return;
        }

        if (OVRInput.GetDown(selectButton))
        {
            TrySelectPlanet();
        }
    }

    private void TrySelectPlanet()
    {
        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, planetLayers, QueryTriggerInteraction.Collide) == false)
        {
            return;
        }

        PlanetSelectable selectable = hit.collider.GetComponentInParent<PlanetSelectable>();
        if (selectable != null)
        {
            selectable.Select();
            return;
        }

        PlanetBody planetBody = hit.collider.GetComponentInParent<PlanetBody>();
        if (planetBody != null && PlanetInfoPanelManager.Instance != null)
        {
            PlanetInfoPanelManager.Instance.ShowPlanet(planetBody.data);
        }
    }
}
