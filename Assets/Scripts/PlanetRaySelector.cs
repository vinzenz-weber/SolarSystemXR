using UnityEngine;
using System;

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
    public OVRInput.Button alternateSelectButton = OVRInput.Button.PrimaryIndexTrigger;
    public bool acceptRawIndexTriggers = true;

    private void Update()
    {
        Transform origin = GetRayOrigin();
        if (origin == null) return;

        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.WORLD)
        {
            return;
        }

        if (HasSelectInputDown())
        {
            TrySelectPlanet(origin);
        }
    }

    private void TrySelectPlanet(Transform origin)
    {
        Ray ray = new Ray(origin.position, origin.forward);

        if (TryGetPlanetFromRay(ray, out PlanetData selectedPlanetData, out PlanetSelectable selectedPlanet) == false)
        {
            return;
        }

        if (PlanetInfoPanelManager.Instance == null)
        {
            Debug.LogWarning("PlanetRaySelector: Kein PlanetInfoPanelManager in der Szene gefunden.");
            return;
        }

        PlanetInfoPanelManager.Instance.ShowPlanet(selectedPlanetData, selectedPlanet);
    }

    private bool HasSelectInputDown()
    {
        if (OVRInput.GetDown(selectButton) || OVRInput.GetDown(alternateSelectButton))
        {
            return true;
        }

        if (acceptRawIndexTriggers == false) return false;

        return OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger)
            || OVRInput.GetDown(OVRInput.RawButton.LIndexTrigger);
    }

    private bool TryGetPlanetFromRay(Ray ray, out PlanetData selectedPlanetData, out PlanetSelectable selectedPlanet)
    {
        if (TryGetPlanetFromRaycast(ray, planetLayers, out selectedPlanetData, out selectedPlanet))
        {
            return true;
        }

        // Falls die Szene noch eine alte Layer-Maske hat, z.B. Layer 6,
        // pruefen wir beim Klick zusaetzlich alle Layer und filtern weiter auf PlanetData.
        if (planetLayers.value != ~0)
        {
            return TryGetPlanetFromRaycast(ray, ~0, out selectedPlanetData, out selectedPlanet);
        }

        selectedPlanetData = null;
        selectedPlanet = null;
        return false;
    }

    private bool TryGetPlanetFromRaycast(Ray ray, int layerMask, out PlanetData selectedPlanetData, out PlanetSelectable selectedPlanet)
    {
        selectedPlanetData = null;
        selectedPlanet = null;

        RaycastHit[] hits = Physics.RaycastAll(ray, maxDistance, layerMask, QueryTriggerInteraction.Collide);
        if (hits.Length == 0) return false;

        Array.Sort(hits, (first, second) => first.distance.CompareTo(second.distance));

        for (int i = 0; i < hits.Length; i++)
        {
            PlanetData hitPlanetData = GetPlanetDataFromHit(hits[i].collider, out PlanetSelectable hitPlanet);
            if (hitPlanetData == null) continue;

            selectedPlanetData = hitPlanetData;
            selectedPlanet = hitPlanet;
            return true;
        }

        return false;
    }

    private PlanetData GetPlanetDataFromHit(Collider hitCollider, out PlanetSelectable selectedPlanet)
    {
        selectedPlanet = null;
        if (hitCollider == null) return null;

        PlanetSelectable selectable = hitCollider.GetComponentInParent<PlanetSelectable>();
        if (selectable != null)
        {
            selectedPlanet = selectable;
            return selectable.GetPlanetData();
        }

        InteractablePlanetVisual interactableVisual = hitCollider.GetComponentInParent<InteractablePlanetVisual>();
        if (interactableVisual != null && interactableVisual.PlanetData != null)
        {
            return interactableVisual.PlanetData;
        }

        PlanetBody planetBody = hitCollider.GetComponentInParent<PlanetBody>();
        if (planetBody != null)
        {
            return planetBody.data;
        }

        return null;
    }

    private Transform GetRayOrigin()
    {
        if (rayOrigin != null) return rayOrigin;
        if (Camera.main != null) return Camera.main.transform;
        return null;
    }
}
