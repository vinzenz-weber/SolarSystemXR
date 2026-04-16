using UnityEngine;

public class SpielerBewegung : MonoBehaviour
{
    // --- Variablen für den Inspector ---
    // public bedeutet, du kannst diese Werte später direkt in Unity ändern, 
    // ohne den Code neu öffnen zu müssen.
    public float laufGeschwindigkeit = 5.0f;
    public float mausEmpfindlichkeit = 2.0f;
    
    // Hier ziehen wir später unsere Kamera rein
    public Transform spielerKamera;

    // --- Interne Variablen (nur für das Skript) ---
    private CharacterController controller;
    private float vertikaleKameraRotation = 0f;
    // public damit PlanetUmgebungsZone die Schwerkraft von außen ändern kann
    public float schwerkraft = -9.81f;
    private float fallGeschwindigkeit = 0f;

    // Start entspricht setup() in Processing
    void Start()
    {
        // Wir holen uns die CharacterController Komponente, die auf dem gleichen Objekt liegt
        controller = GetComponent<CharacterController>();

        // Versteckt den Mauszeiger im Spiel und sperrt ihn in der Mitte des Bildschirms
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update entspricht draw() in Processing
    void Update()
    {
        // ==========================================
        // 1. UMSCHAUEN (Maus)
        // ==========================================
        float mausX = Input.GetAxis("Mouse X") * mausEmpfindlichkeit;
        float mausY = Input.GetAxis("Mouse Y") * mausEmpfindlichkeit;

        // Spieler-Kapsel dreht sich links/rechts (Y-Achse)
        transform.Rotate(0, mausX, 0);

        // Kamera dreht sich hoch/runter (X-Achse). 
        // Wir begrenzen das (Clamp), damit man sich nicht überschlägt.
        vertikaleKameraRotation -= mausY;
        vertikaleKameraRotation = Mathf.Clamp(vertikaleKameraRotation, -90f, 90f);
        spielerKamera.localRotation = Quaternion.Euler(vertikaleKameraRotation, 0f, 0f);


        // ==========================================
        // 2. LAUFEN (Tastatur: W, A, S, D)
        // ==========================================
        float bewegenX = Input.GetAxis("Horizontal"); // A und D Tasten
        float bewegenZ = Input.GetAxis("Vertical");   // W und S Tasten

        // Wir berechnen die Richtung relativ zu der Richtung, in die wir gerade schauen
        Vector3 bewegenRichtung = transform.right * bewegenX + transform.forward * bewegenZ;


        // ==========================================
        // 3. SCHWERKRAFT (Damit wir nicht fliegen)
        // ==========================================
        if (controller.isGrounded) 
        {
            fallGeschwindigkeit = -2f; // Ein kleiner Minuswert hält uns fest auf dem Boden
        } 
        else 
        {
            // Wenn wir in der Luft sind, zieht uns die Schwerkraft nach unten
            // Time.deltaTime ist wichtig, damit es framerate-unabhängig passiert!
            fallGeschwindigkeit += schwerkraft * Time.deltaTime; 
        }

        // Wir fügen die Fallgeschwindigkeit zu unserer Bewegung hinzu
        bewegenRichtung.y = fallGeschwindigkeit;

        // Führt die eigentliche Bewegung aus. 
        // Auch hier * Time.deltaTime, damit schnelle PCs nicht schneller laufen als langsame.
        controller.Move(bewegenRichtung * laufGeschwindigkeit * Time.deltaTime);
    }
}