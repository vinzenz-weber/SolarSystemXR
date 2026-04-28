using UnityEngine;

// Die States, die der App-Flow aktuell braucht:
// MAIN_MENU  -> Hauptmenue ist sichtbar, User waehlt eine Experience aus
// PLACEMENT  -> Hauptmenue zu, User platziert einen Planeten oder das Sonnensystem
// WORLD      -> Objekt steht; per Options-Taste links laesst sich das Hauptmenue wieder oeffnen
// IMMERSIVE  -> VR-Naturszene mit gewaehltem Planeten an der Mondposition
// TEST_*     -> ein Minispiel oder Platzhalter ist aktiv
public enum GameState
{
    MAIN_MENU,
    PLACEMENT,
    WORLD,
    IMMERSIVE,
    TEST_REIHENFOLGE,
    TEST_SIZE,
    TEST_GRAVITY_PLACEHOLDER
}

public class GameManager : MonoBehaviour
{
    // Globaler Zugriffspunkt - andere Scripts rufen GameManager.Instance.SetState(...)
    public static GameManager Instance;

    [Header("Canvas-Referenz")]
    [Tooltip("Hauptmenue-Canvas: sichtbar in MAIN_MENU, in PLACEMENT/WORLD/TEST ausgeblendet")]
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

        // App startet immer im Hauptmenue
        SetState(GameState.MAIN_MENU);
    }

    void Update()
    {
        // Im WORLD- und TEST-State oeffnet die Options-Taste links das Hauptmenue wieder.
        if (CanReturnToMainMenuWithStartButton() && OVRInput.GetDown(OVRInput.Button.Start))
        {
            if (CurrentState == GameState.IMMERSIVE)
            {
                ImmersiveModeController immersiveModeController = FindFirstObjectByType<ImmersiveModeController>();
                if (immersiveModeController != null)
                {
                    immersiveModeController.ExitImmersiveMode();
                    return;
                }
            }

            if (IsTestState(CurrentState) && MinigameManager.Instance != null)
            {
                MinigameManager.Instance.EndMinigame();
                return;
            }

            SetState(GameState.MAIN_MENU);
        }
    }

    // Zentrale Stelle fuer State-Wechsel - schaltet das Hauptmenue-Canvas
    public void SetState(GameState newState)
    {
        CurrentState = newState;

        switch (newState)
        {
            case GameState.MAIN_MENU:
                Show(mainMenuCanvas, true);
                break;

            case GameState.PLACEMENT:
            case GameState.WORLD:
            case GameState.IMMERSIVE:
                Show(mainMenuCanvas, false);
                break;

            case GameState.TEST_REIHENFOLGE:
            case GameState.TEST_SIZE:
            case GameState.TEST_GRAVITY_PLACEHOLDER:
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

    private bool CanReturnToMainMenuWithStartButton()
    {
        return CurrentState == GameState.WORLD
            || CurrentState == GameState.IMMERSIVE
            || IsTestState(CurrentState);
    }

    private bool IsTestState(GameState state)
    {
        return state == GameState.TEST_REIHENFOLGE
            || state == GameState.TEST_SIZE
            || state == GameState.TEST_GRAVITY_PLACEHOLDER;
    }
}
