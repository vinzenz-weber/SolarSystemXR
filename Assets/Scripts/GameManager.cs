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

    [Header("Panels beim Hauptmenue")]
    [Tooltip("PlacementManager versteckt Planet-Info-Panel und Sonnensystem-Panel, solange das Hauptmenue sichtbar ist.")]
    [SerializeField] private PlacementManager placementManager;

    [Tooltip("Optionale weitere Panel-Roots, die beim Hauptmenue versteckt und danach wiederhergestellt werden.")]
    [SerializeField] private GameObject[] panelsToHideWhileMainMenuOpen;

    public GameState CurrentState { get; private set; }

    private GameState _startButtonReturnState = GameState.WORLD;
    private bool _hasStartButtonReturnState;
    private bool[] _panelVisibilityBeforeMainMenu;
    private bool _hasStoredExtraPanelVisibility;

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
        if (OVRInput.GetDown(OVRInput.Button.Start) == false) return;

        if (CurrentState == GameState.MAIN_MENU)
        {
            CloseMainMenuWithStartButton();
            return;
        }

        // Im WORLD-State oeffnet die Options-Taste links das Hauptmenue als Overlay.
        if (CurrentState == GameState.WORLD)
        {
            OpenMainMenuWithStartButton();
            return;
        }

        // In Immersive/Test bleiben die bisherigen Ruecksprung-Regeln erhalten.
        if (CanReturnToMainMenuWithStartButton())
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
        GameState previousState = CurrentState;
        CurrentState = newState;

        if (newState == GameState.MAIN_MENU && previousState != GameState.MAIN_MENU)
        {
            SetNonMenuPanelsHiddenByMainMenu(true);
        }
        else if (previousState == GameState.MAIN_MENU && newState != GameState.MAIN_MENU)
        {
            SetNonMenuPanelsHiddenByMainMenu(false);
        }

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

    private void OpenMainMenuWithStartButton()
    {
        _startButtonReturnState = CurrentState;
        _hasStartButtonReturnState = true;
        SetState(GameState.MAIN_MENU);
    }

    private void CloseMainMenuWithStartButton()
    {
        GameState targetState = _hasStartButtonReturnState
            ? _startButtonReturnState
            : GameState.WORLD;

        if (targetState == GameState.MAIN_MENU)
        {
            targetState = GameState.WORLD;
        }

        _hasStartButtonReturnState = false;
        SetState(targetState);
    }

    private void Show(GameObject go, bool visible)
    {
        if (go == null) return;
        go.SetActive(visible);
    }

    private void SetNonMenuPanelsHiddenByMainMenu(bool isHidden)
    {
        PlacementManager manager = GetPlacementManager();
        if (manager != null)
        {
            manager.SetWorldPanelsHiddenByMainMenu(isHidden);
        }

        SetExtraPanelsHiddenByMainMenu(isHidden);
    }

    private PlacementManager GetPlacementManager()
    {
        if (placementManager != null) return placementManager;

        placementManager = FindFirstObjectByType<PlacementManager>();
        return placementManager;
    }

    private void SetExtraPanelsHiddenByMainMenu(bool isHidden)
    {
        if (panelsToHideWhileMainMenuOpen == null) return;

        if (isHidden)
        {
            StoreExtraPanelVisibility();

            for (int i = 0; i < panelsToHideWhileMainMenuOpen.Length; i++)
            {
                GameObject panel = panelsToHideWhileMainMenuOpen[i];
                if (panel == null || IsMainMenuObject(panel)) continue;

                panel.SetActive(false);
            }

            return;
        }

        RestoreExtraPanelVisibility();
    }

    private void StoreExtraPanelVisibility()
    {
        if (_hasStoredExtraPanelVisibility) return;

        _panelVisibilityBeforeMainMenu = new bool[panelsToHideWhileMainMenuOpen.Length];

        for (int i = 0; i < panelsToHideWhileMainMenuOpen.Length; i++)
        {
            GameObject panel = panelsToHideWhileMainMenuOpen[i];
            _panelVisibilityBeforeMainMenu[i] = panel != null && panel.activeSelf;
        }

        _hasStoredExtraPanelVisibility = true;
    }

    private void RestoreExtraPanelVisibility()
    {
        if (_hasStoredExtraPanelVisibility == false || _panelVisibilityBeforeMainMenu == null) return;

        int count = Mathf.Min(panelsToHideWhileMainMenuOpen.Length, _panelVisibilityBeforeMainMenu.Length);
        for (int i = 0; i < count; i++)
        {
            GameObject panel = panelsToHideWhileMainMenuOpen[i];
            if (panel == null || IsMainMenuObject(panel)) continue;

            panel.SetActive(_panelVisibilityBeforeMainMenu[i]);
        }

        _hasStoredExtraPanelVisibility = false;
    }

    private bool IsMainMenuObject(GameObject target)
    {
        if (target == null || mainMenuCanvas == null) return false;
        if (target == mainMenuCanvas) return true;

        return target.transform.IsChildOf(mainMenuCanvas.transform);
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
