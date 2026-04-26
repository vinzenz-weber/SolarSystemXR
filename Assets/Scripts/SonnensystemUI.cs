using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Steuert das Slider-Panel fuer ein platziertes Sonnensystem.
// Wichtig: Das Panel bekommt seinen SolarSystemManager ueber Bind(), damit es genau
// die frisch platzierte Sonnensystem-Instanz veraendert.
public class SonnensystemUI : MonoBehaviour
{
    [Header("Referenzen")]
    [Tooltip("SolarSystemManager der platzierten Sonnensystem-Instanz")]
    public SolarSystemManager solarSystemManager;

    [Header("Slider")]
    [Tooltip("Slider fuer die Abstaende der Umlaufbahnen")]
    public Slider distanzSlider;
    [Tooltip("Slider fuer die Planetengroessen")]
    public Slider groesseSlider;
    [Tooltip("Slider fuer die Simulationsgeschwindigkeit")]
    public Slider zeitSlider;
    [Tooltip("Slider fuer die Exzentrizitaet der Umlaufbahnen")]
    public Slider exzentrizitaetSlider;

    [Header("Wert-Labels (optional)")]
    public TMP_Text distanzLabel;
    public TMP_Text groesseLabel;
    public TMP_Text zeitLabel;
    public TMP_Text exzentrizitaetLabel;

    [Header("Position")]
    [Tooltip("Wenn aktiv, stellt sich das Panel beim Anzeigen neben den User.")]
    public bool positioniereNebenUser = true;
    public float abstandVorUser = 1.2f;
    public float seitlicherAbstand = -0.45f;
    public float hoehenOffset = -0.1f;

    private const float DistanzMin = 0.002f;
    private const float DistanzMax = 0.025f;
    private const float GroesseMin = 0.0005f;
    private const float GroesseMax = 0.01f;
    private const float ZeitMin = 0f;
    private const float ZeitMax = 100f;
    private const float ExzMin = 0f;
    private const float ExzMax = 5f;

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
        if (solarSystemManager == null) return;
        solarSystemManager.distanceScale = wert;
        if (distanzLabel != null) distanzLabel.text = $"Abstaende: {wert:F3} AU/m";
    }

    public void AufGroesseGeaendert(float wert)
    {
        if (solarSystemManager == null) return;
        solarSystemManager.planetSizeScale = wert;
        if (groesseLabel != null) groesseLabel.text = $"Groessen: {wert:F4}";
    }

    public void AufZeitGeaendert(float wert)
    {
        if (solarSystemManager == null) return;
        solarSystemManager.timeScale = wert;
        if (zeitLabel != null) zeitLabel.text = $"Geschwindigkeit: {wert:F0} Tage/s";
    }

    public void AufExzentrizitaetGeaendert(float wert)
    {
        if (solarSystemManager == null) return;
        solarSystemManager.exzentrizitaetMultiplikator = wert;
        if (exzentrizitaetLabel != null) exzentrizitaetLabel.text = $"Exzentrizitaet: {wert:F1}x";
    }

    public void AufResetKlicken()
    {
        if (solarSystemManager == null) return;

        solarSystemManager.distanceScale = 0.006f;
        solarSystemManager.planetSizeScale = 0.002f;
        solarSystemManager.timeScale = 1f;
        solarSystemManager.exzentrizitaetMultiplikator = 1f;

        if (distanzSlider != null) distanzSlider.value = solarSystemManager.distanceScale;
        if (groesseSlider != null) groesseSlider.value = solarSystemManager.planetSizeScale;
        if (zeitSlider != null) zeitSlider.value = solarSystemManager.timeScale;
        if (exzentrizitaetSlider != null) exzentrizitaetSlider.value = solarSystemManager.exzentrizitaetMultiplikator;
    }

    private void InitialisierePanel()
    {
        if (solarSystemManager == null)
        {
            Debug.LogWarning("[SonnensystemUI] Kein SolarSystemManager zugewiesen.");
            return;
        }

        EntferneListener();

        InitialisiereSlider(distanzSlider, DistanzMin, DistanzMax, solarSystemManager.distanceScale);
        InitialisiereSlider(groesseSlider, GroesseMin, GroesseMax, solarSystemManager.planetSizeScale);
        InitialisiereSlider(zeitSlider, ZeitMin, ZeitMax, solarSystemManager.timeScale);
        InitialisiereSlider(exzentrizitaetSlider, ExzMin, ExzMax, solarSystemManager.exzentrizitaetMultiplikator);

        if (distanzSlider != null) distanzSlider.onValueChanged.AddListener(AufDistanzGeaendert);
        if (groesseSlider != null) groesseSlider.onValueChanged.AddListener(AufGroesseGeaendert);
        if (zeitSlider != null) zeitSlider.onValueChanged.AddListener(AufZeitGeaendert);
        if (exzentrizitaetSlider != null) exzentrizitaetSlider.onValueChanged.AddListener(AufExzentrizitaetGeaendert);

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
        if (exzentrizitaetSlider != null) exzentrizitaetSlider.onValueChanged.RemoveListener(AufExzentrizitaetGeaendert);
    }

    private void InitialisiereSlider(Slider slider, float min, float max, float startwert)
    {
        if (slider == null) return;
        slider.minValue = min;
        slider.maxValue = max;
        slider.value = startwert;
    }

    private void PositionierePanelNebenUser()
    {
        if (Camera.main == null) return;

        Transform kamera = Camera.main.transform;
        Vector3 position = kamera.position
            + kamera.forward * abstandVorUser
            + kamera.right * seitlicherAbstand
            + Vector3.up * hoehenOffset;

        Quaternion rotation = Quaternion.LookRotation(position - kamera.position);
        transform.SetPositionAndRotation(position, rotation);
    }

    private void AktualisiereLabels()
    {
        if (solarSystemManager == null) return;

        if (distanzLabel != null) distanzLabel.text = $"Abstaende: {solarSystemManager.distanceScale:F3} AU/m";
        if (groesseLabel != null) groesseLabel.text = $"Groessen: {solarSystemManager.planetSizeScale:F4}";
        if (zeitLabel != null) zeitLabel.text = $"Geschwindigkeit: {solarSystemManager.timeScale:F0} Tage/s";
        if (exzentrizitaetLabel != null) exzentrizitaetLabel.text = $"Exzentrizitaet: {solarSystemManager.exzentrizitaetMultiplikator:F1}x";
    }
}
