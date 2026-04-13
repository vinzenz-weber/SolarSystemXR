using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Steuert das Parameter-Panel im SONNENSYSTEM-Zustand.
// Verbindet UI-Slider mit dem SolarSystemManager — Änderungen sind live sichtbar.
//
// SETUP im Unity-Editor:
// 1. Neuen World-Space Canvas als "SonnensystemPanel" erstellen
// 2. 4 Slider hinzufügen und in die entsprechenden Felder ziehen
// 3. Optional: TMP_Text-Labels für die aktuellen Werte
// 4. Dieses Script auf das Panel-GameObject legen
public class SonnensystemUI : MonoBehaviour
{
    [Header("Referenzen")]
    [Tooltip("SolarSystemManager aus der Szene")]
    public SolarSystemManager solarSystemManager;

    [Header("Slider")]
    [Tooltip("Slider für die Abstände der Umlaufbahnen")]
    public Slider distanzSlider;
    [Tooltip("Slider für die Planetengrößen")]
    public Slider groesseSlider;
    [Tooltip("Slider für die Simulationsgeschwindigkeit")]
    public Slider zeitSlider;
    [Tooltip("Slider für die Exzentrizität der Umlaufbahnen")]
    public Slider exzentrizitaetSlider;

    [Header("Wert-Labels (optional)")]
    [Tooltip("Zeigt den aktuellen Wert des Distanz-Sliders an")]
    public TMP_Text distanzLabel;
    [Tooltip("Zeigt den aktuellen Wert des Größen-Sliders an")]
    public TMP_Text groesseLabel;
    [Tooltip("Zeigt den aktuellen Wert des Zeit-Sliders an")]
    public TMP_Text zeitLabel;
    [Tooltip("Zeigt den aktuellen Wert des Exzentrizitäts-Sliders an")]
    public TMP_Text exzentrizitaetLabel;

    // ─── Slider-Wertebereiche ──────────────────────────────────────────────
    // Distanz: 0.002 = sehr eng, 0.02 = weit auseinander
    private const float DistanzMin   = 0.002f;
    private const float DistanzMax   = 0.025f;

    // Größe: 0.0005 = sehr klein, 0.01 = übertrieben groß
    private const float GroesseMin   = 0.0005f;
    private const float GroesseMax   = 0.01f;

    // Zeit: 0 = Stopp, 100 = sehr schnell (Tage/Sekunde)
    private const float ZeitMin      = 0f;
    private const float ZeitMax      = 100f;

    // Exzentrizität: 0 = alle Kreise, 5 = extreme Ellipsen
    private const float ExzMin       = 0f;
    private const float ExzMax       = 5f;

    // ══════════════════════════════════════════════════════════════════════
    // UNITY LIFECYCLE
    // ══════════════════════════════════════════════════════════════════════

    void OnEnable()
    {
        // SolarSystemManager suchen, falls nicht zugewiesen
        if (solarSystemManager == null)
            solarSystemManager = FindObjectOfType<SolarSystemManager>();

        if (solarSystemManager == null)
        {
            Debug.LogWarning("[SonnensystemUI] Kein SolarSystemManager gefunden!");
            return;
        }

        // Slider konfigurieren und auf aktuelle Manager-Werte setzen
        InitialisiereSlider(distanzSlider, DistanzMin, DistanzMax, solarSystemManager.distanceScale);
        InitialisiereSlider(groesseSlider, GroesseMin, GroesseMax, solarSystemManager.planetSizeScale);
        InitialisiereSlider(zeitSlider, ZeitMin, ZeitMax, solarSystemManager.timeScale);
        InitialisiereSlider(exzentrizitaetSlider, ExzMin, ExzMax, solarSystemManager.exzentrizitaetMultiplikator);

        // Callbacks registrieren
        if (distanzSlider        != null) distanzSlider.onValueChanged.AddListener(AufDistanzGeaendert);
        if (groesseSlider        != null) groesseSlider.onValueChanged.AddListener(AufGroesseGeaendert);
        if (zeitSlider           != null) zeitSlider.onValueChanged.AddListener(AufZeitGeaendert);
        if (exzentrizitaetSlider != null) exzentrizitaetSlider.onValueChanged.AddListener(AufExzentrizitaetGeaendert);

        // Panel vor dem Spieler positionieren
        PanelPositionieren();

        // Labels initial befüllen
        AktualisiereLabels();
    }

    void OnDisable()
    {
        // Callbacks wieder abmelden (verhindert Fehler beim Zerstören)
        if (distanzSlider        != null) distanzSlider.onValueChanged.RemoveListener(AufDistanzGeaendert);
        if (groesseSlider        != null) groesseSlider.onValueChanged.RemoveListener(AufGroesseGeaendert);
        if (zeitSlider           != null) zeitSlider.onValueChanged.RemoveListener(AufZeitGeaendert);
        if (exzentrizitaetSlider != null) exzentrizitaetSlider.onValueChanged.RemoveListener(AufExzentrizitaetGeaendert);
    }

    // ══════════════════════════════════════════════════════════════════════
    // SLIDER-CALLBACKS  (werden per Code oder Inspector-OnValueChanged verbunden)
    // ══════════════════════════════════════════════════════════════════════

    public void AufDistanzGeaendert(float wert)
    {
        if (solarSystemManager == null) return;
        solarSystemManager.distanceScale = wert;
        if (distanzLabel != null)
            distanzLabel.text = $"Abstände: {wert:F3} AU/m";
    }

    public void AufGroesseGeaendert(float wert)
    {
        if (solarSystemManager == null) return;
        solarSystemManager.planetSizeScale = wert;
        if (groesseLabel != null)
            groesseLabel.text = $"Größen: {wert:F4}";
    }

    public void AufZeitGeaendert(float wert)
    {
        if (solarSystemManager == null) return;
        solarSystemManager.timeScale = wert;
        if (zeitLabel != null)
            zeitLabel.text = $"Geschwindigkeit: {wert:F0} Tage/s";
    }

    public void AufExzentrizitaetGeaendert(float wert)
    {
        if (solarSystemManager == null) return;
        solarSystemManager.exzentrizitaetMultiplikator = wert;
        if (exzentrizitaetLabel != null)
            exzentrizitaetLabel.text = $"Exzentrizität: {wert:F1}x";
    }

    // Reset-Button: alle Werte auf realistische Defaults zurücksetzen
    public void AufResetKlicken()
    {
        if (solarSystemManager == null) return;

        solarSystemManager.distanceScale              = 0.006f;
        solarSystemManager.planetSizeScale            = 0.002f;
        solarSystemManager.timeScale                  = 1f;
        solarSystemManager.exzentrizitaetMultiplikator = 1f;

        // Slider-Werte synchronisieren
        if (distanzSlider        != null) distanzSlider.value        = 0.006f;
        if (groesseSlider        != null) groesseSlider.value        = 0.002f;
        if (zeitSlider           != null) zeitSlider.value           = 1f;
        if (exzentrizitaetSlider != null) exzentrizitaetSlider.value = 1f;
    }

    // ══════════════════════════════════════════════════════════════════════
    // HILFSMETHODEN
    // ══════════════════════════════════════════════════════════════════════

    // Setzt Min/Max/Startwert eines Sliders
    private void InitialisiereSlider(Slider slider, float min, float max, float startwert)
    {
        if (slider == null) return;
        slider.minValue = min;
        slider.maxValue = max;
        slider.value    = startwert;
    }

    // Panel 1.2m vor und etwas unterhalb der Kamera positionieren
    private void PanelPositionieren()
    {
        if (Camera.main == null) return;
        Transform kamera = Camera.main.transform;
        Vector3 position = kamera.position + kamera.forward * 1.2f + Vector3.down * 0.1f;
        Quaternion rotation = Quaternion.LookRotation(position - kamera.position);
        transform.SetPositionAndRotation(position, rotation);
    }

    // Labels initial mit aktuellen Werten befüllen
    private void AktualisiereLabels()
    {
        if (solarSystemManager == null) return;
        if (distanzLabel        != null) distanzLabel.text        = $"Abstände: {solarSystemManager.distanceScale:F3} AU/m";
        if (groesseLabel        != null) groesseLabel.text        = $"Größen: {solarSystemManager.planetSizeScale:F4}";
        if (zeitLabel           != null) zeitLabel.text           = $"Geschwindigkeit: {solarSystemManager.timeScale:F0} Tage/s";
        if (exzentrizitaetLabel != null) exzentrizitaetLabel.text = $"Exzentrizität: {solarSystemManager.exzentrizitaetMultiplikator:F1}x";
    }
}
