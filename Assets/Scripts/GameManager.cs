using System.Collections;
using MRMotifs.PassthroughTransitioning;
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

    [Header("Startsequenz / Splash")]
    [Tooltip("Wenn aktiv, startet die App kurz in VR mit Partikeln und blendet danach ins Passthrough-Hauptmenue.")]
    [SerializeField] private bool playStartupSequence = true;

    [Tooltip("Wie lange die Startsequenz in VR sichtbar bleibt. Spaeter kann hier Logo/Splashscreen ergaenzt werden.")]
    [SerializeField] private float startupDuration = 4f;

    [Tooltip("Zusaetzliche Sekunden nach der Startsequenz, bevor das Hauptmenue sichtbar wird.")]
    [SerializeField] private float mainMenuSpawnDelay = 0f;

    [Tooltip("Objekte, die nur waehrend der Startsequenz sichtbar sein sollen, z.B. ein Splashscreen-Root.")]
    [SerializeField] private GameObject[] startupOnlyObjects;

    [Tooltip("Partikelsysteme fuer die Startsequenz. Wenn leer, werden Partikelsysteme in der Szene automatisch gesucht.")]
    [SerializeField] private ParticleSystem[] startupParticleSystems;

    [Tooltip("Passthrough-Dissolver aus MR Motif #1. Wenn leer, wird er automatisch gesucht.")]
    [SerializeField] private PassthroughDissolver startupPassthroughDissolver;

    [Tooltip("MainMenuController, damit der Passthrough-Toggle nach der Startsequenz korrekt synchronisiert ist.")]
    [SerializeField] private MainMenuController mainMenuController;

    public GameState CurrentState { get; private set; }
    public bool IsStartupSequenceActive => _isStartupSequenceActive;

    private GameState _startButtonReturnState = GameState.WORLD;
    private bool _hasStartButtonReturnState;
    private bool[] _panelVisibilityBeforeMainMenu;
    private bool _hasStoredExtraPanelVisibility;
    private bool _isStartupSequenceActive;

    void Awake()
    {
        // Wir haben nur eine Szene -> einfaches Singleton ohne DontDestroyOnLoad
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        _isStartupSequenceActive = playStartupSequence;
    }

    void Start()
    {
        if (mainMenuCanvas == null) Debug.LogWarning("GameManager: mainMenuCanvas ist nicht zugewiesen.");

        if (playStartupSequence)
        {
            StartCoroutine(RunStartupSequence());
            return;
        }

        // App startet sonst direkt im Hauptmenue.
        SetState(GameState.MAIN_MENU);
    }

    void Update()
    {
        if (_isStartupSequenceActive) return;
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

        PlanetFactsVisibility.Refresh();
        Debug.Log("GameState: " + newState);
    }

    private IEnumerator RunStartupSequence()
    {
        _isStartupSequenceActive = true;
        Show(mainMenuCanvas, false);
        SetStartupOnlyObjectsVisible(true);
        SetStartupParticlesPlaying(true);
        SetPassthroughImmediate(false);

        yield return new WaitForSeconds(Mathf.Max(0f, startupDuration));

        SetStartupOnlyObjectsVisible(false);
        SetPassthroughWithMenuSync(true);

        yield return new WaitForSeconds(Mathf.Max(0f, mainMenuSpawnDelay));

        _isStartupSequenceActive = false;
        SetState(GameState.MAIN_MENU);
    }

    private void SetStartupOnlyObjectsVisible(bool isVisible)
    {
        if (startupOnlyObjects == null) return;

        for (int i = 0; i < startupOnlyObjects.Length; i++)
        {
            if (startupOnlyObjects[i] != null)
            {
                startupOnlyObjects[i].SetActive(isVisible);
            }
        }
    }

    private void SetStartupParticlesPlaying(bool isPlaying)
    {
        ParticleSystem[] particles = GetStartupParticleSystems();

        for (int i = 0; i < particles.Length; i++)
        {
            ParticleSystem particleSystem = particles[i];
            if (particleSystem == null) continue;

            if (isPlaying)
            {
                particleSystem.gameObject.SetActive(true);
                particleSystem.Clear(true);
                particleSystem.Play(true);
            }
            else
            {
                particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                particleSystem.gameObject.SetActive(false);
            }
        }
    }

    private ParticleSystem[] GetStartupParticleSystems()
    {
        if (startupParticleSystems != null && startupParticleSystems.Length > 0)
        {
            return startupParticleSystems;
        }

        startupParticleSystems = FindObjectsByType<ParticleSystem>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        return startupParticleSystems;
    }

    private void SetPassthroughImmediate(bool isActive)
    {
        PassthroughDissolver dissolver = GetStartupPassthroughDissolver();
        if (dissolver == null) return;

        dissolver.SetPassthroughActiveImmediate(isActive);
        PlanetFactsVisibility.Refresh();
    }

    private void SetPassthroughWithMenuSync(bool isActive)
    {
        MainMenuController controller = GetMainMenuController();
        if (controller != null)
        {
            controller.SetPassthroughMode(isActive);
            return;
        }

        PassthroughDissolver dissolver = GetStartupPassthroughDissolver();
        if (dissolver != null)
        {
            dissolver.SetPassthroughActive(isActive);
        }

        PlanetFactsVisibility.Refresh();
    }

    private PassthroughDissolver GetStartupPassthroughDissolver()
    {
        if (startupPassthroughDissolver != null) return startupPassthroughDissolver;

        PassthroughDissolver[] dissolvers = FindObjectsByType<PassthroughDissolver>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        startupPassthroughDissolver = dissolvers.Length > 0 ? dissolvers[0] : null;
        return startupPassthroughDissolver;
    }

    private MainMenuController GetMainMenuController()
    {
        if (mainMenuController != null) return mainMenuController;

        MainMenuController[] controllers = FindObjectsByType<MainMenuController>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        mainMenuController = controllers.Length > 0 ? controllers[0] : null;
        return mainMenuController;
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
