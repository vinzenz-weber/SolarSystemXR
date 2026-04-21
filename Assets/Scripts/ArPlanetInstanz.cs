using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Oculus.Interaction;

// Sitzt auf dem Wrapper-GameObject eines AR-Planeten.
// Zuständig für: Gaze-Erkennung → Info-Button einblenden → Panel öffnen/schließen + GrabHandle.
public class ArPlanetInstanz : MonoBehaviour
{
    // ─── Inspector-Felder ─────────────────────────────────────────────────
    [Header("Gaze")]
    [Tooltip("Maximale Entfernung für den Gaze-Raycast in Metern")]
    public float gazeDistanz = 8f;

    // ─── Private Variablen ────────────────────────────────────────────────
    private PlanetData _planetData;
    private GameObject _planet;          // Das Prefab-Child mit Mesh
    private GameObject _infoButtonGO;    // World-Space Canvas mit dem ℹ-Button
    private SphereCollider _gazeKollider;
    private bool _panelOffen = false;

    // GrabHandle-Konfiguration — von ArPlanetManager bei der Initialisierung übergeben
    private GameObject _grabHandlePrefab;
    private Vector3    _grabHandleOffset;

    // ══════════════════════════════════════════════════════════════════════
    // INITIALISIERUNG  (wird von ArPlanetManager aufgerufen)
    // ══════════════════════════════════════════════════════════════════════

    public void Initialisieren(PlanetData data, GameObject planet, GameObject grabHandlePrefab, Vector3 grabHandleOffset)
    {
        _planetData       = data;
        _planet           = planet;
        _grabHandlePrefab = grabHandlePrefab;
        _grabHandleOffset = grabHandleOffset;

        // SphereCollider auf dem Planet-GO für den Gaze-Raycast
        _gazeKollider = planet.GetComponent<SphereCollider>();
        if (_gazeKollider == null)
            _gazeKollider = planet.AddComponent<SphereCollider>();

        _gazeKollider.isTrigger = true;
        // Etwas größer als 0.5 damit der Planet leichter anvisierbar ist
        _gazeKollider.radius    = 0.55f;

        InfoButtonErstellen();
        GrabHandleErstellen();
    }

    // ══════════════════════════════════════════════════════════════════════
    // UNITY LIFECYCLE
    // ══════════════════════════════════════════════════════════════════════

    void Update()
    {
        // Gaze nur prüfen wenn das Panel geschlossen ist
        if (!_panelOffen)
            GazeAktualisieren();
    }

    // ══════════════════════════════════════════════════════════════════════
    // GAZE-ERKENNUNG
    // ══════════════════════════════════════════════════════════════════════

    private void GazeAktualisieren()
    {
        if (Camera.main == null || _gazeKollider == null) return;

        Ray strahl = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        bool schautHin = _gazeKollider.Raycast(strahl, out _, gazeDistanz);

        _infoButtonGO?.SetActive(schautHin);

        // Info-Button immer zur Kamera drehen damit er lesbar bleibt
        if (schautHin && _infoButtonGO != null)
        {
            _infoButtonGO.transform.rotation = Quaternion.LookRotation(
                _infoButtonGO.transform.position - Camera.main.transform.position
            );
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    // BUTTON-CALLBACK
    // ══════════════════════════════════════════════════════════════════════

    private void InfoButtonGeklickt()
    {
        _panelOffen = !_panelOffen;

        // Info-Button ausblenden wenn Panel offen, sonst wieder zeigen (via Gaze)
        _infoButtonGO?.SetActive(!_panelOffen);

        if (_panelOffen)
            PanelOeffnen();
        else
            PanelSchliessen();
    }

    // ══════════════════════════════════════════════════════════════════════
    // PANEL
    // ══════════════════════════════════════════════════════════════════════

    private void PanelOeffnen()
    {
        if (GameManager.Instance == null) return;

        GameObject panelGO = GameManager.Instance.planetDetailPanel;
        if (panelGO == null) return;

        PlanetDetailUI ui = panelGO.GetComponent<PlanetDetailUI>();
        if (ui == null) return;

        // Radius aus der tatsächlichen Weltgröße des Planet-Meshes
        float radius = _planet != null ? _planet.transform.lossyScale.x * 0.5f : 0.15f;
        Vector3 planetPos = _planet != null ? _planet.transform.position : transform.position;

        ui.ZeigeFuerArPlanet(_planetData, planetPos, radius);
        panelGO.SetActive(true);
    }

    private void PanelSchliessen()
    {
        GameManager.Instance?.planetDetailPanel?.SetActive(false);
    }

    // ══════════════════════════════════════════════════════════════════════
    // SETUP: INFO-BUTTON (per Code erstellt)
    // ══════════════════════════════════════════════════════════════════════

    private void InfoButtonErstellen()
    {
        // Canvas als Kind des Wrappers
        _infoButtonGO = new GameObject("InfoButton_Canvas");
        _infoButtonGO.transform.SetParent(transform);
        _infoButtonGO.transform.localPosition = Vector3.up * 0.8f;
        _infoButtonGO.transform.localRotation = Quaternion.identity;

        Canvas canvas = _infoButtonGO.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;

        // Kleiner quadratischer Canvas (100x100 Einheiten bei 0.001 Scale = 10x10cm)
        RectTransform canvasRect = _infoButtonGO.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(100f, 100f);
        _infoButtonGO.transform.localScale = Vector3.one * 0.001f;

        _infoButtonGO.AddComponent<GraphicRaycaster>();

        // Panel mit halbtransparentem Hintergrund
        GameObject panelGO = new GameObject("Panel");
        panelGO.transform.SetParent(_infoButtonGO.transform, false);

        Image panelImage = panelGO.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.6f);

        RectTransform panelRect = panelGO.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // ℹ-Text
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(panelGO.transform, false);

        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text      = "ℹ";
        tmp.fontSize  = 80f;
        tmp.color     = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;

        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        // Unsichtbarer Button über dem gesamten Canvas
        Button button = panelGO.AddComponent<Button>();
        button.onClick.AddListener(InfoButtonGeklickt);

        _infoButtonGO.SetActive(false);
    }

    // ══════════════════════════════════════════════════════════════════════
    // SETUP: GRAB-HANDLE (Prefab wird instanziiert)
    // ══════════════════════════════════════════════════════════════════════

    // Spawnt das GrabHandle-Prefab unter dem Wrapper und verdrahtet die Grabbable
    // so, dass beim Greifen der Wrapper (und damit der ganze Planet) bewegt wird.
    private void GrabHandleErstellen()
    {
        if (_grabHandlePrefab == null)
        {
            Debug.LogWarning("[ArPlanetInstanz] Kein GrabHandle-Prefab zugewiesen — Planet ist nicht greifbar.");
            return;
        }

        GameObject handleGO = Instantiate(_grabHandlePrefab, transform);
        handleGO.transform.localPosition = _grabHandleOffset;
        handleGO.transform.localRotation = Quaternion.identity;

        // Grabbable auf den Wrapper-Transform umleiten:
        // Die Pille wird gegriffen, aber der ganze Wrapper (=Planet) wird bewegt.
        Grabbable grabbable = handleGO.GetComponentInChildren<Grabbable>();
        if (grabbable != null)
            grabbable.InjectOptionalTargetTransform(transform);
        else
            Debug.LogWarning("[ArPlanetInstanz] GrabHandle-Prefab enthält keine Grabbable-Komponente.");
    }
}
