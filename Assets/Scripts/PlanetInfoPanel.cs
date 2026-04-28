using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Zeigt die Daten eines PlanetData-Assets in einem einzigen globalen Info-Panel.
public class PlanetInfoPanel : MonoBehaviour
{
    [Header("Texte")]
    public TMP_Text headlineText;
    public TMP_Text subHeadlineText;
    public TMP_Text descriptionText;
    public TMP_Text factsText;
    public TMP_Text diameterText;
    public TMP_Text gravityText;

    [Header("Bild")]
    public Image planetImage;

    [Header("Immersive Mode")]
    [SerializeField] private Button immersiveModeButton;
    [SerializeField] private TMP_Text immersiveModeButtonText;

    private PlanetData _currentPlanetData;
    private const string ImmersiveButtonLabel = "Immersive Mode";
    private const string LeaveImmersiveButtonLabel = "Leave Immersive Mode";

    private void Awake()
    {
        EnsureImmersiveButton();

        if (immersiveModeButton != null)
        {
            immersiveModeButton.onClick.AddListener(OpenImmersiveMode);
        }
    }

    private void OnDestroy()
    {
        if (immersiveModeButton != null)
        {
            immersiveModeButton.onClick.RemoveListener(OpenImmersiveMode);
        }
    }

    public void Bind(PlanetData data)
    {
        if (data == null)
        {
            Debug.LogWarning("PlanetInfoPanel: Kein PlanetData uebergeben.");
            return;
        }

        _currentPlanetData = data;

        SetText(headlineText, data.planetName);
        SetText(subHeadlineText, data.subHeadline);

        string description = string.IsNullOrWhiteSpace(data.beschreibung)
            ? data.shortDescription
            : data.beschreibung;
        SetText(descriptionText, description);

        SetText(diameterText, "Durchmesser: " + data.diameter.ToString("N0") + " km");
        SetText(gravityText, "Schwerkraft: " + data.schwerkraft.ToString("F2") + " m/s^2");
        SetText(factsText, BuildFactsText(data.fakten));

        if (planetImage != null)
        {
            planetImage.sprite = data.planetImage;
            planetImage.enabled = data.planetImage != null;
        }

        UpdateImmersiveButton(data);
    }

    private void SetText(TMP_Text textField, string value)
    {
        if (textField == null) return;
        textField.text = value;
    }

    private string BuildFactsText(string[] facts)
    {
        if (facts == null || facts.Length == 0)
        {
            return "";
        }

        StringBuilder builder = new StringBuilder();

        for (int i = 0; i < facts.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(facts[i])) continue;
            builder.AppendLine("- " + facts[i]);
        }

        return builder.ToString();
    }

    private void OpenImmersiveMode()
    {
        if (_currentPlanetData == null)
        {
            Debug.LogWarning("PlanetInfoPanel: Kein Planet fuer den Immersive Mode gebunden.");
            return;
        }

        if (PlanetInfoPanelManager.Instance == null)
        {
            Debug.LogWarning("PlanetInfoPanel: Kein PlanetInfoPanelManager fuer den Immersive Mode gefunden.");
            return;
        }

        PlanetInfoPanelManager.Instance.ToggleImmersiveMode(_currentPlanetData);
    }

    private void UpdateImmersiveButton(PlanetData data)
    {
        if (immersiveModeButton == null) return;

        bool isSun = data != null && data.planetName == "Sonne";
        immersiveModeButton.gameObject.SetActive(isSun == false);
        immersiveModeButton.interactable = data != null && isSun == false;

        SetImmersiveModeActive(false);
    }

    public void SetImmersiveModeActive(bool isActive)
    {
        if (immersiveModeButtonText != null)
        {
            immersiveModeButtonText.text = isActive ? LeaveImmersiveButtonLabel : ImmersiveButtonLabel;
        }
    }

    private void EnsureImmersiveButton()
    {
        if (immersiveModeButton != null) return;

        Transform existingButton = transform.Find("Button_ImmersiveMode");
        if (existingButton != null)
        {
            immersiveModeButton = existingButton.GetComponent<Button>();
            immersiveModeButtonText = existingButton.GetComponentInChildren<TMP_Text>(true);
            return;
        }

        GameObject buttonObject = new GameObject("Button_ImmersiveMode", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(transform, false);
        SetLayerRecursive(buttonObject, gameObject.layer);

        RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0f);
        rectTransform.anchorMax = new Vector2(0.5f, 0f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = new Vector2(0f, 70f);
        rectTransform.sizeDelta = new Vector2(360f, 80f);

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.09f, 0.22f, 0.42f, 0.95f);

        immersiveModeButton = buttonObject.GetComponent<Button>();

        GameObject textObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(buttonObject.transform, false);
        SetLayerRecursive(textObject, gameObject.layer);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        immersiveModeButtonText = textObject.GetComponent<TextMeshProUGUI>();
        immersiveModeButtonText.text = ImmersiveButtonLabel;
        immersiveModeButtonText.fontSize = 28f;
        immersiveModeButtonText.alignment = TextAlignmentOptions.Center;
        immersiveModeButtonText.color = Color.white;
    }

    private void SetLayerRecursive(GameObject targetObject, int layer)
    {
        if (targetObject == null) return;

        targetObject.layer = layer;

        foreach (Transform child in targetObject.transform)
        {
            SetLayerRecursive(child.gameObject, layer);
        }
    }
}
