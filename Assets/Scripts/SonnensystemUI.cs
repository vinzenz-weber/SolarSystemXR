using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Steuert das Slider-Panel fuer ein platziertes Sonnensystem.
// Das Panel bekommt seinen SolarSystemManager ueber Bind(), damit es genau
// die frisch platzierte Sonnensystem-Instanz veraendert.
public class SonnensystemUI : MonoBehaviour
{
    [Header("Referenzen")]
    [Tooltip("SolarSystemManager der platzierten Sonnensystem-Instanz")]
    public SolarSystemManager solarSystemManager;

    [Header("Slider")]
    [Tooltip("Slider fuer Orbital Distance / Abstaende der Umlaufbahnen")]
    public Slider distanzSlider;
    [Tooltip("Slider fuer Planet Scale / Planetengroessen")]
    public Slider groesseSlider;
    [Tooltip("Slider fuer Orbital Speed / Simulationsgeschwindigkeit")]
    public Slider zeitSlider;
    [Tooltip("Slider fuer Inclination / Neigung der Umlaufbahnebene")]
    public Slider inklinationSlider;
    [Tooltip("Slider fuer Eccentricity / Exzentrizitaet der Umlaufbahnen")]
    public Slider exzentrizitaetSlider;

    [Header("Spacing Mode")]
    [Tooltip("Optionaler Toggle: aktiv = kompakter XR-Abstandsmodus, inaktiv = linearer Modus.")]
    public Toggle spacingModeToggle;
    [Tooltip("Fallback, falls der Spacing-Mode im UI als Button statt Toggle gebaut ist.")]
    public Button spacingModeButton;
    [Tooltip("Hintergrund des selbstgebauten Toggles, z.B. ToggleBG.")]
    public Graphic spacingModeBackground;
    [Tooltip("Beweglicher Knopf des selbstgebauten Toggles, z.B. ToggleHandle.")]
    public RectTransform spacingModeHandle;
    [Tooltip("Position des ToggleHandle, wenn Spacing Mode aktiv ist.")]
    public float spacingHandleOnX = -4f;
    [Tooltip("Position des ToggleHandle, wenn Spacing Mode aus ist.")]
    public float spacingHandleOffX = -24f;
    public Color spacingModeOnColor = new Color(0.188f, 0.82f, 0.345f, 1f);
    public Color spacingModeOffColor = new Color(1f, 1f, 1f, 0.4f);

    [Header("Wert-Labels (optional)")]
    public TMP_Text distanzLabel;
    public TMP_Text groesseLabel;
    public TMP_Text zeitLabel;
    public TMP_Text inklinationLabel;
    public TMP_Text exzentrizitaetLabel;
    public TMP_Text spacingModeLabel;

    [Header("Position")]
    [Tooltip("Wenn aktiv, stellt sich das Panel beim Anzeigen neben den User.")]
    public bool positioniereNebenUser = true;
    public float abstandVorUser = 0.3f;
    public float seitlicherAbstand = -0.35f;
    public float hoehenOffset = -0.75f;

    private bool _isInitializing;

    private void OnEnable()
    {
        if (solarSystemManager == null)
        {
            solarSystemManager = FindFirstObjectByType<SolarSystemManager>();
        }

        InitialisierePanel();
    }

    private void OnDisable()
    {
        EntferneListener();
    }

    public void Bind(SolarSystemManager neuerManager)
    {
        solarSystemManager = neuerManager;
        InitialisierePanel();
    }

    public void AufDistanzGeaendert(float wert)
    {
        if (solarSystemManager == null || _isInitializing == true) return;
        solarSystemManager.SetOrbitalDistance(wert);
        AktualisiereSliderLabel(distanzLabel, distanzSlider);
    }

    public void AufGroesseGeaendert(float wert)
    {
        if (solarSystemManager == null || _isInitializing == true) return;
        solarSystemManager.SetPlanetScale(wert);
        AktualisiereSliderLabel(groesseLabel, groesseSlider);
    }

    public void AufZeitGeaendert(float wert)
    {
        if (solarSystemManager == null || _isInitializing == true) return;
        solarSystemManager.SetOrbitalSpeed(wert);
        AktualisiereSliderLabel(zeitLabel, zeitSlider);
    }

    public void AufInklinationGeaendert(float wert)
    {
        if (solarSystemManager == null || _isInitializing == true) return;
        solarSystemManager.SetInclinationMultiplier(wert);
        AktualisiereSliderLabel(inklinationLabel, inklinationSlider);
    }

    public void AufExzentrizitaetGeaendert(float wert)
    {
        if (solarSystemManager == null || _isInitializing == true) return;
        solarSystemManager.SetEccentricityMultiplier(wert);
        AktualisiereSliderLabel(exzentrizitaetLabel, exzentrizitaetSlider);
    }

    public void AufSpacingModeGeaendert(bool isCompactXrMode)
    {
        if (solarSystemManager == null || _isInitializing == true) return;
        solarSystemManager.SetSpacingMode(isCompactXrMode);
        AktualisiereSpacingModeVisual();
        AktualisiereSpacingModeLabel();
    }

    public void AufSpacingModeKlicken()
    {
        if (solarSystemManager == null || _isInitializing == true) return;

        bool isCompactXrMode = solarSystemManager.darstellungsModus != SolarSystemManager.DistanceScaleMode.WurzelKompakt_XR;
        solarSystemManager.SetSpacingMode(isCompactXrMode);
        SetzeToggleOhneEvent(spacingModeToggle, isCompactXrMode);
        AktualisiereSpacingModeVisual();
        AktualisiereSpacingModeLabel();
    }

    public void AufResetKlicken()
    {
        if (solarSystemManager == null) return;

        UebernehmeSliderwerteInManager();
        AktualisiereLabels();
    }

    private void InitialisierePanel()
    {
        if (solarSystemManager == null)
        {
            Debug.LogWarning("[SonnensystemUI] Kein SolarSystemManager zugewiesen.");
            return;
        }

        SucheFehlendeReferenzen();
        EntferneListener();

        _isInitializing = true;
        UebernehmeSliderwerteInManager();
        _isInitializing = false;

        if (distanzSlider != null) distanzSlider.onValueChanged.AddListener(AufDistanzGeaendert);
        if (groesseSlider != null) groesseSlider.onValueChanged.AddListener(AufGroesseGeaendert);
        if (zeitSlider != null) zeitSlider.onValueChanged.AddListener(AufZeitGeaendert);
        if (inklinationSlider != null) inklinationSlider.onValueChanged.AddListener(AufInklinationGeaendert);
        if (exzentrizitaetSlider != null) exzentrizitaetSlider.onValueChanged.AddListener(AufExzentrizitaetGeaendert);
        if (spacingModeToggle != null) spacingModeToggle.onValueChanged.AddListener(AufSpacingModeGeaendert);
        if (spacingModeButton != null) spacingModeButton.onClick.AddListener(AufSpacingModeKlicken);

        if (positioniereNebenUser == true)
        {
            PositionierePanelNebenUser();
        }

        AktualisiereLabels();
    }

    private void EntferneListener()
    {
        if (distanzSlider != null) distanzSlider.onValueChanged.RemoveListener(AufDistanzGeaendert);
        if (groesseSlider != null) groesseSlider.onValueChanged.RemoveListener(AufGroesseGeaendert);
        if (zeitSlider != null) zeitSlider.onValueChanged.RemoveListener(AufZeitGeaendert);
        if (inklinationSlider != null) inklinationSlider.onValueChanged.RemoveListener(AufInklinationGeaendert);
        if (exzentrizitaetSlider != null) exzentrizitaetSlider.onValueChanged.RemoveListener(AufExzentrizitaetGeaendert);
        if (spacingModeToggle != null) spacingModeToggle.onValueChanged.RemoveListener(AufSpacingModeGeaendert);
        if (spacingModeButton != null) spacingModeButton.onClick.RemoveListener(AufSpacingModeKlicken);
    }

    private void SucheFehlendeReferenzen()
    {
        if (groesseSlider == null) groesseSlider = FindeSliderBeiLabel("Planet Scale");
        if (zeitSlider == null) zeitSlider = FindeSliderBeiLabel("Orbital Speed");
        if (inklinationSlider == null) inklinationSlider = FindeSliderBeiLabel("Inclination");
        if (inklinationSlider == null) inklinationSlider = FindeSliderBeiLabel("Axial Tilt");
        if (distanzSlider == null) distanzSlider = FindeSliderBeiLabel("Orbital Distance");
        if (exzentrizitaetSlider == null) exzentrizitaetSlider = FindeSliderBeiLabel("Eccentricity");

        if (groesseLabel == null) groesseLabel = FindeWertLabelBeiLabel("Planet Scale");
        if (zeitLabel == null) zeitLabel = FindeWertLabelBeiLabel("Orbital Speed");
        if (inklinationLabel == null) inklinationLabel = FindeWertLabelBeiLabel("Inclination");
        if (inklinationLabel == null) inklinationLabel = FindeWertLabelBeiLabel("Axial Tilt");
        if (distanzLabel == null) distanzLabel = FindeWertLabelBeiLabel("Orbital Distance");
        if (exzentrizitaetLabel == null) exzentrizitaetLabel = FindeWertLabelBeiLabel("Eccentricity");

        if (spacingModeToggle == null) spacingModeToggle = FindeToggleBeiLabel("Spacing Mode");
        if (spacingModeButton == null) spacingModeButton = FindeButtonBeiLabel("Spacing Mode");
        if (spacingModeBackground == null) spacingModeBackground = FindeGraphicMitName("ToggleBG");
        if (spacingModeHandle == null) spacingModeHandle = FindeRectTransformMitName("ToggleHandle");
        if (spacingModeButton == null && spacingModeHandle != null) spacingModeButton = spacingModeHandle.GetComponent<Button>();
    }

    private Slider FindeSliderBeiLabel(string labelText)
    {
        TMP_Text label = FindeText(labelText);
        if (label == null) return null;

        Transform current = label.transform.parent;
        while (current != null && current != transform.parent)
        {
            Slider slider = current.GetComponentInChildren<Slider>(true);
            if (slider != null) return slider;
            current = current.parent;
        }

        return null;
    }

    private TMP_Text FindeWertLabelBeiLabel(string labelText)
    {
        TMP_Text label = FindeText(labelText);
        if (label == null || label.transform.parent == null) return null;

        Transform current = label.transform.parent;
        for (int i = 0; i < 3 && current != null; i++)
        {
            TMP_Text[] texte = current.GetComponentsInChildren<TMP_Text>(true);
            foreach (TMP_Text text in texte)
            {
                if (text == label) continue;
                if (text.name.Contains("Slider Label") == true || text.text.Contains("%") == true)
                {
                    return text;
                }
            }

            current = current.parent;
        }

        return null;
    }

    private Toggle FindeToggleBeiLabel(string labelText)
    {
        TMP_Text label = FindeText(labelText);
        if (label == null) return null;

        Transform current = label.transform.parent;
        while (current != null && current != transform.parent)
        {
            Toggle toggle = current.GetComponentInChildren<Toggle>(true);
            if (toggle != null) return toggle;
            current = current.parent;
        }

        return null;
    }

    private Button FindeButtonBeiLabel(string labelText)
    {
        TMP_Text label = FindeText(labelText);
        if (label == null) return null;

        Transform current = label.transform.parent;
        while (current != null && current != transform.parent)
        {
            Button button = current.GetComponentInChildren<Button>(true);
            if (button != null) return button;
            current = current.parent;
        }

        return null;
    }

    private TMP_Text FindeText(string labelText)
    {
        TMP_Text[] texte = GetComponentsInChildren<TMP_Text>(true);
        foreach (TMP_Text text in texte)
        {
            if (text.text.Trim() == labelText)
            {
                return text;
            }
        }

        return null;
    }

    private Graphic FindeGraphicMitName(string objektName)
    {
        RectTransform rectTransform = FindeRectTransformMitName(objektName);
        if (rectTransform == null) return null;

        return rectTransform.GetComponent<Graphic>();
    }

    private RectTransform FindeRectTransformMitName(string objektName)
    {
        RectTransform[] rectTransforms = GetComponentsInChildren<RectTransform>(true);
        foreach (RectTransform rectTransform in rectTransforms)
        {
            if (rectTransform.name == objektName)
            {
                return rectTransform;
            }
        }

        return null;
    }

    private void UebernehmeSliderwerteInManager()
    {
        if (distanzSlider != null) solarSystemManager.SetOrbitalDistance(distanzSlider.value);
        if (groesseSlider != null) solarSystemManager.SetPlanetScale(groesseSlider.value);
        if (zeitSlider != null) solarSystemManager.SetOrbitalSpeed(zeitSlider.value);
        if (inklinationSlider != null) solarSystemManager.SetInclinationMultiplier(inklinationSlider.value);
        if (exzentrizitaetSlider != null) solarSystemManager.SetEccentricityMultiplier(exzentrizitaetSlider.value);

        if (spacingModeToggle != null)
        {
            solarSystemManager.SetSpacingMode(spacingModeToggle.isOn);
        }

        AktualisiereSpacingModeVisual();
    }

    private void SetzeToggleOhneEvent(Toggle toggle, bool isOn)
    {
        if (toggle == null) return;
        toggle.SetIsOnWithoutNotify(isOn);
    }

    private void PositionierePanelNebenUser()
    {
        if (Camera.main == null) return;

        Transform kamera = Camera.main.transform;
        Vector3 forward = kamera.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude < 0.0001f)
        {
            forward = kamera.parent != null ? kamera.parent.forward : Vector3.forward;
            forward.y = 0f;
        }

        forward = forward.normalized;
        Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;

        Vector3 position = kamera.position
            + forward * abstandVorUser
            + right * seitlicherAbstand
            + Vector3.up * hoehenOffset;

        Quaternion rotation = Quaternion.LookRotation(position - new Vector3(kamera.position.x, position.y, kamera.position.z));
        transform.SetPositionAndRotation(position, rotation);
    }

    private void AktualisiereLabels()
    {
        if (solarSystemManager == null) return;

        AktualisiereSliderLabel(distanzLabel, distanzSlider);
        AktualisiereSliderLabel(groesseLabel, groesseSlider);
        AktualisiereSliderLabel(zeitLabel, zeitSlider);
        AktualisiereSliderLabel(inklinationLabel, inklinationSlider);
        AktualisiereSliderLabel(exzentrizitaetLabel, exzentrizitaetSlider);
        AktualisiereSpacingModeLabel();
    }

    private void AktualisiereSliderLabel(TMP_Text label, Slider slider)
    {
        if (label == null || slider == null) return;

        float normalisierterWert = Mathf.InverseLerp(slider.minValue, slider.maxValue, slider.value);
        label.text = $"{Mathf.RoundToInt(normalisierterWert * 100f)}%";
    }

    private void AktualisiereSpacingModeLabel()
    {
        if (spacingModeLabel == null || solarSystemManager == null) return;

        bool isCompact = solarSystemManager.darstellungsModus == SolarSystemManager.DistanceScaleMode.WurzelKompakt_XR;
        spacingModeLabel.text = isCompact ? "Compact XR" : "Linear";
    }

    private void AktualisiereSpacingModeVisual()
    {
        if (solarSystemManager == null) return;

        bool isCompact = solarSystemManager.darstellungsModus == SolarSystemManager.DistanceScaleMode.WurzelKompakt_XR;

        if (spacingModeHandle != null)
        {
            Vector2 position = spacingModeHandle.anchoredPosition;
            position.x = isCompact ? spacingHandleOnX : spacingHandleOffX;
            spacingModeHandle.anchoredPosition = position;
        }

        if (spacingModeBackground != null)
        {
            spacingModeBackground.color = isCompact ? spacingModeOnColor : spacingModeOffColor;
        }
    }
}
