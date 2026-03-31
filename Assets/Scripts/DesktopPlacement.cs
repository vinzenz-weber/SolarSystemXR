using UnityEngine;
using UnityEngine.InputSystem;

public class DesktopPlacement : MonoBehaviour
{
    // === "Globale" Variablen (wie oben in einem Processing-Sketch) ===
    public GameObject solarSystemPrefab;
    public LayerMask placementLayer;

    private GameObject currentInstance;
    private Camera mainCam;
    
    // Ein simpler Schalter, um die Platzierung zu sperren
    public bool isLocked = false; 

    // === Wird einmal am Start aufgerufen (wie void setup() in Processing) ===
    private void Start()
    {
        mainCam = Camera.main;
    }

    // === Wird jeden Frame aufgerufen (wie void draw() in Processing) ===
    private void Update()
    {
        // Wenn keine Maus oder Tastatur da ist, brich ab
        if (Mouse.current == null || Keyboard.current == null) return;
        Debug.Log("Update in DesktopPlacement läuft...");

        // Wenn das System fest platziert wurde, mach in diesem Skript nichts mehr
        if (isLocked == true) return;

        // 1. Rechtsklick: Platzieren oder Verschieben
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            PlaceSystem();
        }

        // 2. Leertaste: Platzierung fixieren
        if (Keyboard.current.spaceKey.wasPressedThisFrame && currentInstance != null)
        {
            isLocked = true;
            Debug.Log("Sonnensystem wurde fest platziert!");
            // Hier könnte dein GameManager einfach prüfen: if(placementScript.isLocked) ...
        }
    }

    // === Eigene Funktion für die Platzierungs-Logik ===
    private void PlaceSystem()
    {
        // Lese die 2D-Mausposition auf dem Bildschirm aus
        Vector2 mousePos = Mouse.current.position.ReadValue();
        
        // Wandle die 2D-Mausposition in einen 3D-Laserstrahl (Ray) aus der Kamera um
        Ray ray = mainCam.ScreenPointToRay(mousePos);
        RaycastHit hit;

        // Schieße den Strahl 100 Meter weit und prüfe, ob er den Boden (placementLayer) trifft
        if (Physics.Raycast(ray, out hit, 100f, placementLayer))
        {
            // Wenn noch kein System da ist -> Erschaffen (Instantiate)
            if (currentInstance == null)
            {
                currentInstance = Instantiate(solarSystemPrefab, hit.point, Quaternion.identity);
            }
            // Wenn schon eins da ist -> Einfach an die neue Stelle schieben
            else
            {
                currentInstance.transform.position = hit.point;
            }
        }
    }
}