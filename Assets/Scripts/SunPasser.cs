using UnityEngine;


[ExecuteAlways]
public class SunPasser : MonoBehaviour
{
    [Tooltip("Alle Planeten-Materialien hier zuweisen")]
    public Material[] planetMaterials; 
    private static readonly int SunPosID = Shader.PropertyToID("_SunPosition");

    void Update()
    {
        Vector3 worldPos = transform.position;
        
        // Zwingt den Vektor direkt in die Instanzen der Shared Materials, 
        // ohne das SRP Batching aufzubrechen.
        foreach (var mat in planetMaterials)
        {
            if (mat != null)
            {
                mat.SetVector(SunPosID, worldPos);
            }
        }
    }
}