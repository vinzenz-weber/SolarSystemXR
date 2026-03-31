using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    // === "Globale" Variablen ===


    public GameObject solarSystemPrefab; // Referenz auf unser Sonnensystem-Prefab
    public TextMeshProUGUI debugState;
    public GameObject startButtonUI;


    // 1. Wir definieren unsere eigenen Zustände mit aussagekräftigen Namen
    public enum GameState
    {
        Start,          // Z.B. für ein Hauptmenü oder Willkommens-UI
        Placement,      // Das Sonnensystem wird im Raum/auf dem Tisch platziert
        Exploration     // Das System ist platziert, man kann Planeten anschauen/steuern
    }

    // 2. Wir legen eine Variable von diesem neuen Typ an
    public GameState currentState = GameState.Start;


    // Referenz auf unser Platzierungs-Skript
    public DesktopPlacement placementScript;
    public StartPhase startPhaseScript;

    // === Setup ===
    private void Start()
    {
        // Wir setzen den Startzustand explizit fest
        currentState = GameState.Start;
        solarSystemPrefab.SetActive(false); // Das Sonnensystem ist am Anfang unsichtbar
    }

    // === Draw / Loop ===
    private void Update()
    {
        // switch prüft den aktuellen Wert von "currentState" und springt 
        // direkt in den passenden "case" (Fall) Block.
        switch (currentState)
        {
            case GameState.Start:
                debugState.text = "State: START";
                HandleStartPhase();
                break;

            case GameState.Placement:
                debugState.text = "State: PLACEMENT";
                HandlePlacementPhase();
                break;

            case GameState.Exploration:
                HandleExplorationPhase();
                break;
        }
    }

    private void HandleStartPhase()
    {
        placementScript.enabled = false;
        startButtonUI.SetActive(true);
        if (startPhaseScript != null && startPhaseScript.startButtonClicked == true)
        {
            currentState = GameState.Placement;
            Debug.Log("[GameManager] Start-Button geklickt. Starte Phase: Placement!");
            startPhaseScript.startButtonClicked = false; // Reset des Flags, damit es nicht ständig true bleibt
            startButtonUI.SetActive(false);
        }
    }

    // === Eigene Funktionen für die Phasen ===
    private void HandlePlacementPhase()
    {
        if (placementScript != null)
        {
            placementScript.enabled = true;
        }
        // Wir fragen das andere Skript: "Bist du schon gelockt?"
        if (placementScript != null && placementScript.isLocked == true)
        {
            // Wenn ja: Wechsle den Zustand durch Zuweisung des Enums
            currentState = GameState.Exploration;

            // Schalte das Platzierungs-Skript ab
            placementScript.enabled = false;

            Debug.Log("[GameManager] Placement abgeschlossen. Starte Phase: Exploration!");
        }
    }

    private void HandleExplorationPhase()
    {
        // Hier passiert später alles, was nach dem Platzieren kommt.
        // UI einblenden, Zeit/Skalierung manipulieren etc.
    }
}