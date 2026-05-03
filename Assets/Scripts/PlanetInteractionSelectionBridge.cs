using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using UnityEngine;

// Verbindet Meta-Interaction-SDK-Auswahl/Grab mit unserem PlanetInfoPanel.
// So aktualisiert ein DistanceGrab-Klick denselben Weg wie die normale Ray-Auswahl.
public class PlanetInteractionSelectionBridge : MonoBehaviour
{
    private PlanetSelectable _planetSelectable;
    private DistanceGrabInteractable[] _distanceGrabInteractables;
    private DistanceHandGrabInteractable[] _distanceHandGrabInteractables;

    private void Awake()
    {
        CacheReferences();
    }

    private void OnEnable()
    {
        CacheReferences();
        AddListeners();
    }

    private void OnDisable()
    {
        RemoveListeners();
    }

    public void Refresh()
    {
        RemoveListeners();
        CacheReferences();
        AddListeners();
    }

    private void CacheReferences()
    {
        if (_planetSelectable == null)
        {
            _planetSelectable = GetComponent<PlanetSelectable>();
        }

        _distanceGrabInteractables = GetComponentsInChildren<DistanceGrabInteractable>(true);
        _distanceHandGrabInteractables = GetComponentsInChildren<DistanceHandGrabInteractable>(true);
    }

    private void AddListeners()
    {
        if (_distanceGrabInteractables != null)
        {
            foreach (DistanceGrabInteractable interactable in _distanceGrabInteractables)
            {
                if (interactable == null) continue;
                interactable.WhenSelectingInteractorViewAdded -= HandleSelectedByInteractionSdk;
                interactable.WhenSelectingInteractorViewAdded += HandleSelectedByInteractionSdk;
            }
        }

        if (_distanceHandGrabInteractables != null)
        {
            foreach (DistanceHandGrabInteractable interactable in _distanceHandGrabInteractables)
            {
                if (interactable == null) continue;
                interactable.WhenSelectingInteractorViewAdded -= HandleSelectedByInteractionSdk;
                interactable.WhenSelectingInteractorViewAdded += HandleSelectedByInteractionSdk;
            }
        }
    }

    private void RemoveListeners()
    {
        if (_distanceGrabInteractables != null)
        {
            foreach (DistanceGrabInteractable interactable in _distanceGrabInteractables)
            {
                if (interactable == null) continue;
                interactable.WhenSelectingInteractorViewAdded -= HandleSelectedByInteractionSdk;
            }
        }

        if (_distanceHandGrabInteractables != null)
        {
            foreach (DistanceHandGrabInteractable interactable in _distanceHandGrabInteractables)
            {
                if (interactable == null) continue;
                interactable.WhenSelectingInteractorViewAdded -= HandleSelectedByInteractionSdk;
            }
        }
    }

    private void HandleSelectedByInteractionSdk(IInteractorView interactorView)
    {
        SelectPlanet();
    }

    private void SelectPlanet()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.WORLD)
        {
            return;
        }

        if (_planetSelectable == null)
        {
            _planetSelectable = GetComponent<PlanetSelectable>();
        }

        if (_planetSelectable == null)
        {
            Debug.LogWarning("PlanetInteractionSelectionBridge: Kein PlanetSelectable auf " + name + " gefunden.");
            return;
        }

        _planetSelectable.Select();
    }
}
