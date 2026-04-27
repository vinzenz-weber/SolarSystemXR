using UnityEngine;

public enum MinigameType
{
    Reihenfolge,
    Size,
    Gravity
}

// Verwaltet immer genau ein aktives Minispiel.
public class MinigameManager : MonoBehaviour
{
    public static MinigameManager Instance { get; private set; }

    [Header("Optionale Prefabs")]
    [Tooltip("Platzhalter- oder echtes Prefab fuer das Reihenfolge-Minispiel.")]
    public GameObject reihenfolgePrefab;

    [Tooltip("Platzhalter- oder echtes Prefab fuer das Size-Minispiel.")]
    public GameObject sizePrefab;

    [Header("Referenzen")]
    public PlacementManager placementManager;
    public MainMenuController mainMenuController;

    [Header("Positionierung")]
    [Tooltip("Abstand des Minigame-Prefabs vor der Kamera.")]
    public float panelDistance = 1.4f;

    [Tooltip("Hoehe relativ zur Kamera.")]
    public float panelHeightOffset = -0.05f;

    private GameObject _activeMinigame;
    private MinigameType _currentType;
    private bool _hasActiveMinigame;

    public MinigameType CurrentType => _currentType;
    public bool HasActiveMinigame => _hasActiveMinigame;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void StartMinigame(MinigameType type)
    {
        Debug.Log("MinigameManager: Starte Minispiel " + type + ".");
        EndActiveInstanceOnly();

        _currentType = type;
        _hasActiveMinigame = true;

        if (placementManager != null)
        {
            placementManager.ClearPlacedObjects();
        }

        GameObject prefab = GetPrefab(type);
        if (prefab == null)
        {
            Debug.LogWarning("MinigameManager: Kein Prefab fuer " + type + " zugewiesen. Es wird keine UI automatisch erzeugt.");
            _hasActiveMinigame = false;
            return;
        }

        _activeMinigame = Instantiate(prefab);

        PositionMinigameInFrontOfUser(_activeMinigame);
        SetGameState(type);
    }

    public void EndMinigame()
    {
        EndActiveInstanceOnly();
        _hasActiveMinigame = false;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetState(GameState.MAIN_MENU);
        }

        MainMenuController menuController = mainMenuController != null
            ? mainMenuController
            : FindFirstObjectByType<MainMenuController>();

        if (menuController != null)
        {
            menuController.ShowTestTab();
        }
    }

    public void ResetMinigame()
    {
        if (_hasActiveMinigame == false)
        {
            Debug.LogWarning("MinigameManager: Kein aktives Minispiel zum Zuruecksetzen.");
            return;
        }

        StartMinigame(_currentType);
    }

    public void CompleteCurrentMinigame()
    {
        if (_hasActiveMinigame == false)
        {
            Debug.LogWarning("MinigameManager: Kein aktives Minispiel zum Abschliessen.");
            return;
        }

        SetMinigameCompleted(_currentType, true);
    }

    public void SetMinigameCompleted(MinigameType type, bool isCompleted)
    {
        string key = GetCompletedKey(type);
        PlayerPrefs.SetInt(key, isCompleted ? 1 : 0);
        PlayerPrefs.Save();

        MainMenuController menuController = mainMenuController != null
            ? mainMenuController
            : FindFirstObjectByType<MainMenuController>();

        if (menuController != null)
        {
            menuController.RefreshMinigameButtons();
        }
    }

    public bool IsMinigameCompleted(MinigameType type)
    {
        return PlayerPrefs.GetInt(GetCompletedKey(type), 0) == 1;
    }

    public void ResetQuizProgress()
    {
        PlayerPrefs.DeleteKey(GetCompletedKey(MinigameType.Reihenfolge));
        PlayerPrefs.DeleteKey(GetCompletedKey(MinigameType.Size));
        PlayerPrefs.Save();

        MainMenuController menuController = mainMenuController != null
            ? mainMenuController
            : FindFirstObjectByType<MainMenuController>();

        if (menuController != null)
        {
            menuController.RefreshMinigameButtons();
        }

        Debug.Log("MinigameManager: Quiz-Fortschritt wurde zurueckgesetzt.");
    }

    private GameObject GetPrefab(MinigameType type)
    {
        if (type == MinigameType.Reihenfolge) return reihenfolgePrefab;
        if (type == MinigameType.Size) return sizePrefab;
        return null;
    }

    private void SetGameState(MinigameType type)
    {
        if (GameManager.Instance == null) return;

        if (type == MinigameType.Reihenfolge)
        {
            GameManager.Instance.SetState(GameState.TEST_REIHENFOLGE);
            return;
        }

        if (type == MinigameType.Size)
        {
            GameManager.Instance.SetState(GameState.TEST_SIZE);
            return;
        }

        GameManager.Instance.SetState(GameState.TEST_GRAVITY_PLACEHOLDER);
    }

    private void EndActiveInstanceOnly()
    {
        if (_activeMinigame != null)
        {
            Destroy(_activeMinigame);
            _activeMinigame = null;
        }
    }

    private void PositionMinigameInFrontOfUser(GameObject minigameRoot)
    {
        if (minigameRoot == null) return;

        Transform cameraTransform = Camera.main != null ? Camera.main.transform : null;
        if (cameraTransform == null) return;

        Vector3 position = cameraTransform.position
            + cameraTransform.forward * panelDistance
            + Vector3.up * panelHeightOffset;

        // Unity-World-Space-Canvas zeigt seine Vorderseite in lokaler +Z-Richtung.
        Quaternion rotation = Quaternion.LookRotation(position - cameraTransform.position);
        minigameRoot.transform.SetPositionAndRotation(position, rotation);

        Debug.Log("MinigameManager: Minispiel positioniert bei " + position + ".");
    }

    private string GetCompletedKey(MinigameType type)
    {
        return "MinigameCompleted_" + type;
    }
}
