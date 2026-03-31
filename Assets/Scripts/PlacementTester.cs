using UnityEngine;
using Oculus.Interaction;

/// <summary>
/// Spawnt ein Prefab an der Spitze eines Ray Interactors.
/// </summary>
public class PlacementTester : MonoBehaviour
{
    [SerializeField] private GameObject _prefabToSpawn;
    [SerializeField] private RayInteractor _rayInteractor;
    
    // ISelector triggert das Event (z.B. ActiveStateSelector des Controllers)
    [SerializeField, Interface(typeof(ISelector))] 
    private MonoBehaviour _selector;

    private ISelector Selector => _selector as ISelector;

    private void OnEnable()
    {
        if (Selector != null) Selector.WhenSelected += HandleSelect;
    }

    private void OnDisable()
    {
        if (Selector != null) Selector.WhenSelected -= HandleSelect;
    }

    private void HandleSelect()
    {
        if (_rayInteractor == null || _prefabToSpawn == null) return;

        Vector3 spawnPos;

        if (_rayInteractor.CollisionInfo.HasValue)
        {
            // Platziert am Treffpunkt einer Kollision
            spawnPos = _rayInteractor.CollisionInfo.Value.Point;
        }
        else
        {
            // Platziert am Ende des Rays, falls nichts getroffen wurde
            spawnPos = _rayInteractor.Ray.origin + _rayInteractor.Ray.direction * _rayInteractor.MaxRayLength;
        }

        Instantiate(_prefabToSpawn, spawnPos, Quaternion.identity);
    }
}