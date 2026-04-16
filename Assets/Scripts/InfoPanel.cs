using UnityEngine;
using TMPro;
using UnityEngine.UI;

// Singleton für das Info-Panel-System.
//
// WICHTIG: Dieses Script NICHT auf den Canvas legen, sondern auf ein
// eigenes leeres GameObject "InfoSystem" in der Szene. Sonst stoppt
// Update() sobald der Canvas versteckt wird und der Raycast funktioniert nicht.
//
// SETUP:
// 1. Leeres GameObject "InfoSystem" in der Szene erstellen
// 2. Dieses Script auf "InfoSystem" ziehen
// 3. World-Space Canvas "InfoPanel" erstellen (mit TitelText, InhaltText, Schließen-Button)
// 4. Den Canvas in das Feld "panelCanvas" ziehen
// 5. Canvas initial auf SetActive(false) setzen
// 6. Die RayInteractor-GameObjects aus OVRInteractionComprehensive in
//    rightRayOrigin und leftRayOrigin ziehen (damit der Raycast mit dem
//    Meta-Standard-Ray übereinstimmt)
public class InfoPanel : MonoBehaviour
{
    public static InfoPanel Instance { get; private set; }

    [Header("Canvas-Referenz")]
    [Tooltip("Der World-Space Canvas mit dem UI — NICHT das Objekt auf dem dieses Script liegt")]
    public GameObject panelCanvas;

    [Header("UI-Elemente")]
    public TMP_Text titelText;
    public TMP_Text inhaltText;
    [Tooltip("Schließen-Button (optional)")]
    public Button schliessenButton;

    [Header("Ray-Origins")]
    [Tooltip("Transform des RayInteractor-GameObjects des rechten Controllers (aus OVRInteractionComprehensive). " +
             "Damit stimmt der Raycast exakt mit dem sichtbaren Meta-Ray überein.")]
    public Transform rightRayOrigin;
    [Tooltip("Transform des RayInteractor-GameObjects des linken Controllers (aus OVRInteractionComprehensive).")]
    public Transform leftRayOrigin;

    [Header("Interaktion")]
    [Tooltip("Maximale Raycast-Distanz in Metern")]
    public float maxRaycastDistanz = 5f;
    [Tooltip("Auf welchem Layer sind die InfoPunkte? (default: alles)")]
    public LayerMask infoPunktLayer = ~0;

    [Header("Positionierung")]
    [Tooltip("Abstand des Panels vom InfoPunkt (in Richtung Kamera)")]
    public float panelAbstandVomPunkt = 0.35f;

    private InfoPunkt _aktuellerPunkt = null;

    // ══════════════════════════════════════════════════════════════════════
    // UNITY LIFECYCLE
    // ══════════════════════════════════════════════════════════════════════

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        if (schliessenButton != null)
            schliessenButton.onClick.AddListener(Verstecke);

        // Canvas verstecken — aber dieses Script bleibt aktiv!
        if (panelCanvas != null)
            panelCanvas.SetActive(false);
    }

    void Update()
    {
        // Raycast nur wenn wir im richtigen Zustand sind
        if (GameManager.Instance == null ||
            GameManager.Instance.aktuellerZustand != SpielZustand.PLANET_SCHWEBEND)
        {
            if (panelCanvas != null && panelCanvas.activeSelf)
                Verstecke();
            return;
        }

        RaycastAufInfoPunkte();

        // Index-Trigger (rechts ODER links) → InfoPunkt öffnen / Panel schließen
        bool triggerGedrueckt =
            OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch) ||
            OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch);

        if (triggerGedrueckt)
        {
            if (_aktuellerPunkt != null)
                Zeige(_aktuellerPunkt);
            else if (panelCanvas != null && panelCanvas.activeSelf)
                Verstecke();
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    // INFO ANZEIGEN / VERSTECKEN
    // ══════════════════════════════════════════════════════════════════════

    public void Zeige(InfoPunkt punkt)
    {
        if (titelText  != null) titelText.text  = punkt.titel;
        if (inhaltText != null) inhaltText.text = punkt.inhalt;

        PanelNebenPunktPositionieren(punkt.transform.position);

        if (panelCanvas != null)
            panelCanvas.SetActive(true);
    }

    public void Verstecke()
    {
        if (panelCanvas != null)
            panelCanvas.SetActive(false);
    }

    // ══════════════════════════════════════════════════════════════════════
    // RAYCAST-ERKENNUNG
    // ══════════════════════════════════════════════════════════════════════

    private void RaycastAufInfoPunkte()
    {
        InfoPunkt neuerPunkt = null;

        // Ray-Origins bestimmen: RayInteractor-Transforms wenn gesetzt,
        // sonst Fallback auf Controller-Anchors aus dem GameManager
        Transform[] origins = HoleRayOrigins();

        foreach (Transform origin in origins)
        {
            Ray strahl = new Ray(origin.position, origin.forward);

            // QueryTriggerInteraction.Collide: Raycast trifft auch Trigger-Collider
            if (Physics.Raycast(strahl, out RaycastHit treffer, maxRaycastDistanz,
                                infoPunktLayer, QueryTriggerInteraction.Collide))
            {
                treffer.collider.TryGetComponent(out neuerPunkt);
                if (neuerPunkt != null) break; // erster Treffer reicht
            }
        }

        if (neuerPunkt != _aktuellerPunkt)
        {
            if (_aktuellerPunkt != null) _aktuellerPunkt.Hervorheben(false);
            _aktuellerPunkt = neuerPunkt;
            if (_aktuellerPunkt != null) _aktuellerPunkt.Hervorheben(true);
        }
    }

    // Gibt die Ray-Origins zurück: bevorzugt die RayInteractor-Transforms,
    // fällt auf die Controller-Anchors aus dem GameManager zurück
    private Transform[] HoleRayOrigins()
    {
        var liste = new System.Collections.Generic.List<Transform>();

        if (rightRayOrigin != null) liste.Add(rightRayOrigin);
        if (leftRayOrigin  != null) liste.Add(leftRayOrigin);

        // Fallback: Controller-Anchors aus GameManager (z.B. im Editor ohne OVR-Setup)
        if (liste.Count == 0)
            liste.AddRange(GameManager.Instance.HoleAlleControllerTransforms());

        return liste.ToArray();
    }

    // ══════════════════════════════════════════════════════════════════════
    // HILFSMETHODEN
    // ══════════════════════════════════════════════════════════════════════

    private void PanelNebenPunktPositionieren(Vector3 punktPosition)
    {
        if (panelCanvas == null || Camera.main == null) return;

        Vector3 richtungZurKamera = (Camera.main.transform.position - punktPosition).normalized;
        Vector3 panelPosition = punktPosition + richtungZurKamera * panelAbstandVomPunkt;
        panelCanvas.transform.SetPositionAndRotation(
            panelPosition,
            Quaternion.LookRotation(-richtungZurKamera)
        );
    }
}
