using UnityEngine;
using Meta.XR;
using Meta.XR.MRUtilityKit;

public class PlacementManager : MonoBehaviour
{
    public Transform rayOrigin;
    public EnvironmentRaycastManager raycastManager;
    public LineRenderer lineRenderer;

    public GameObject previewPlanetPrefab; 
    public GameObject planetPrefab; 
    public GameObject placementVisualizerPrefab; 

    private GameObject visualizerInstance; 
    private GameObject previewInstance;

    public float planetSize;
    public float planetHeight = 0.15f;

    void Start()
    {
        StartPlacement();
    }

    public void StartPlacement()
    {
        if (previewInstance != null) Destroy(previewInstance);
        if (visualizerInstance != null) Destroy(visualizerInstance);

        previewInstance = Instantiate(previewPlanetPrefab);
        previewInstance.transform.localScale = new Vector3(planetSize, planetSize, planetSize);
        
        visualizerInstance = Instantiate(placementVisualizerPrefab); 
    }

    void Update()
    {
        if (previewInstance == null || visualizerInstance == null)
        {
            lineRenderer.enabled = false;
            return;
        }

        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);
        bool didHit = raycastManager.Raycast(ray, out var hitInfo);

        if (didHit == true)
        {
            // 1. Überprüfen, ob die Fläche flach genug ist
            bool canPlace = IsHorizontal(hitInfo.normal);

            // 2. Linie einfärben: grün = ok, rot = nope
            if (canPlace == true)
            {
                lineRenderer.startColor = Color.green;
                lineRenderer.endColor = Color.green;
            }
            else
            {
                lineRenderer.startColor = Color.red;
                lineRenderer.endColor = Color.red;
            }

            // 3. Objekte sichtbar machen und positionieren
            previewInstance.SetActive(true);
            visualizerInstance.SetActive(true);
            lineRenderer.enabled = true;
            
            lineRenderer.SetPosition(0, rayOrigin.position);
            lineRenderer.SetPosition(1, hitInfo.point);

            previewInstance.transform.position = hitInfo.point + Vector3.up * planetHeight;
            visualizerInstance.transform.position = hitInfo.point;

            // 4. Platzieren NUR wenn horizontal UND Trigger gedrückt
            if (canPlace == true && OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger))
            {
                // Das ECHTE Objekt spawnen
                GameObject spawnedPlanet = Instantiate(planetPrefab, previewInstance.transform.position, Quaternion.identity);
                spawnedPlanet.transform.localScale = new Vector3(planetSize, planetSize, planetSize);

                // Die Vorschau zerstören
                Destroy(previewInstance);
                Destroy(visualizerInstance);
                
                previewInstance = null;
                visualizerInstance = null;
                
                lineRenderer.enabled = false;
            }
        }
        else
        {
            lineRenderer.enabled = false;
            previewInstance.SetActive(false);
            visualizerInstance.SetActive(false);
        }
    }

    // --- NEUE METHODE ---
    // Diese Methode prüft, ob die getroffene Fläche horizontal ist
    bool IsHorizontal(Vector3 normal)
    {
        // Vergleiche die Richtung der Normale mit "nach oben"
        float similarity = Vector3.Dot(normal.normalized, Vector3.up);
        
        // 0.85 = ca. 32° Toleranz. Schiefe Tische sind noch ok, Wände nicht.
        if (similarity > 0.85f)
        {
            return true;
        }
        return false;
    }
}