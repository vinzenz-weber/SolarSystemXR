using UnityEngine;

// Die drei States, die der App-Flow aktuell braucht:
// MAIN_MENU  -> Hauptmenü ist sichtbar, User wählt einen Planeten aus
// PLACEMENT  -> Hauptmenü zu, User platziert den Planeten in der Welt
// WORLD      -> Planet steht; per Options-Taste links lässt sich das Hauptmenü wieder öffnen
public enum GameState
{
    MAIN_MENU,
    PLACEMENT,
    WORLD
}

public class GameManager : MonoBehaviour
{
    // Globaler Zugriffspunkt – andere Scripts rufen GameManager.Instance.SetState(...)
    public static GameManager Instance;

    [Header("Canvas-Referenz")]
    [Tooltip("Hauptmenü-Canvas: sichtbar in MAIN_MENU, in PLACEMENT/WORLD ausgeblendet")]
    public GameObject mainMenuCanvas;

    public GameState CurrentState { get; private set; }

    void Awake()
    {
        // Wir haben nur eine Szene -> einfaches Singleton ohne DontDestroyOnLoad
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (mainMenuCanvas == null) Debug.LogWarning("GameManager: mainMenuCanvas ist nicht zugewiesen.");

        // App startet immer im Hauptmenü
        SetState(GameState.MAIN_MENU);
    }

    void Update()
    {
        // Im WORLD-State öffnet die Options-Taste links das Hauptmenü wieder
        if (CurrentState == GameState.WORLD && OVRInput.GetDown(OVRInput.Button.Start))
        {
            SetState(GameState.MAIN_MENU);
        }
    }

    // Zentrale Stelle für State-Wechsel – schaltet das Hauptmenü-Canvas
    public void SetState(GameState newState)
    {
        CurrentState = newState;

        switch (newState)
        {
            case GameState.MAIN_MENU:
                Show(mainMenuCanvas, true);
                break;

            case GameState.PLACEMENT:
                Show(mainMenuCanvas, false);
                break;

            case GameState.WORLD:
                Show(mainMenuCanvas, false);
                break;
        }

        Debug.Log("GameState: " + newState);
    }

    private void Show(GameObject go, bool visible)
    {
        if (go == null) return;
        go.SetActive(visible);
    }
}
