using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Zeigt die Daten eines PlanetData-Assets in einem einzigen globalen Info-Panel.
public class PlanetInfoPanel : MonoBehaviour
{
    private struct DetailCard
    {
        public Transform root;
        public TMP_Text categoryText;
        public TMP_Text categoryDescriptionText;
        public TMP_Text valueText;
        public TMP_Text infoText;
        public TMP_Text sliderLabelText;
        public Slider slider;
    }

    [Header("Texte")]
    public TMP_Text headlineText;
    public TMP_Text subHeadlineText;
    public TMP_Text descriptionText;
    public TMP_Text factsText;
    public TMP_Text diameterText;
    public TMP_Text gravityText;

    [Header("Bild")]
    public Image planetImage;

    [Header("Neues PlanetDetail Root")]
    [SerializeField] private bool autoBindPlanetDetailRoot = true;

    [Header("Immersive Mode")]
    [SerializeField] private Toggle immersiveModeToggle;
    [SerializeField] private Button immersiveModeButton;
    [SerializeField] private TMP_Text immersiveModeButtonText;

    private PlanetData _currentPlanetData;
    private DetailCard[] _detailCards;
    private DetailCard _relativeSizeCard;
    private bool _hasBoundPlanetDetailRoot;
    private bool _usesPlanetDetailRootLayout;
    private bool _hasBoundImmersiveControl;
    private const string ImmersiveButtonLabel = "Immersive Mode";
    private const string LeaveImmersiveButtonLabel = "Leave Immersive Mode";
    private const string LoremIpsum = "Lorem ipsum dolor sit amet.";
    private const float EarthDiameterKm = 12742f;
    private const float JupiterDiameterKm = 142984f;
    private const float AstronomicalUnitKm = 149597870.7f;

    private void Awake()
    {
        BindImmersiveControlIfNeeded();
    }

    private void OnDestroy()
    {
        if (immersiveModeButton != null)
        {
            immersiveModeButton.onClick.RemoveListener(OpenImmersiveMode);
        }

        if (immersiveModeToggle != null)
        {
            immersiveModeToggle.onValueChanged.RemoveListener(SetImmersiveModeFromToggle);
        }
    }

    public void Bind(PlanetData data)
    {
        AutoBindPlanetDetailRoot();
        BindImmersiveControlIfNeeded();

        if (data == null)
        {
            Debug.LogWarning("PlanetInfoPanel: Kein PlanetData uebergeben.");
            return;
        }

        _currentPlanetData = data;

        SetText(headlineText, data.planetName);
        string subHeadline = _usesPlanetDetailRootLayout
            ? FirstFilled(data.beschreibung, data.shortDescription, data.subHeadline)
            : data.subHeadline;
        SetText(subHeadlineText, subHeadline);

        string description = string.IsNullOrWhiteSpace(data.beschreibung)
            ? data.shortDescription
            : data.beschreibung;
        SetText(descriptionText, description);

        SetText(diameterText, "Durchmesser: " + data.diameter.ToString("N0") + " km");
        SetText(gravityText, "Schwerkraft: " + data.schwerkraft.ToString("F2") + " m/s^2");
        SetText(factsText, BuildFactsText(data.fakten));
        BindDetailCards(data);

        if (planetImage != null)
        {
            planetImage.sprite = data.planetImage;
            planetImage.enabled = data.planetImage != null;
        }

        UpdateImmersiveButton(data);
    }

    private void AutoBindPlanetDetailRoot()
    {
        if (autoBindPlanetDetailRoot == false || _hasBoundPlanetDetailRoot) return;

        Transform root = transform;
        Transform contentText = FindDeepChild(root, "ContentText");
        if (contentText != null)
        {
            _usesPlanetDetailRootLayout = true;
            headlineText = FindText(FindDirectChild(contentText, "Label"), headlineText);
            subHeadlineText = FindText(FindDirectChild(contentText, "Subheadline"), subHeadlineText);
        }

        Transform contentContainer = FindDeepChild(root, "ContentContainer");
        Transform upperContainer = FindDirectChild(contentContainer, "UpperContainer");
        Transform middleContainer = FindDirectChild(contentContainer, "MiddleContainer");
        Transform lowerContainer = FindDirectChild(contentContainer, "LowerContainer");

        _detailCards = new[]
        {
            CreateDetailCard(FindDirectChild(upperContainer, "ContentRotSpeed")),
            CreateDetailCard(FindDirectChild(middleContainer, "ContentRotSpeed")),
            CreateDetailCard(FindDirectChild(middleContainer, "ContentRotSpeed (1)")),
            CreateDetailCard(FindDirectChild(middleContainer, "ContentRotSpeed (2)")),
            CreateDetailCard(FindDirectChild(lowerContainer, "ContentRotSpeed"))
        };

        _relativeSizeCard = CreateDetailCard(FindDirectChild(lowerContainer, "ContentRelSize"));
        _hasBoundPlanetDetailRoot = true;
    }

    private DetailCard CreateDetailCard(Transform cardRoot)
    {
        DetailCard card = new DetailCard();
        if (cardRoot == null) return card;

        card.root = cardRoot;
        card.categoryText = FindText(FindDirectChild(cardRoot, "Category"), null);
        card.categoryDescriptionText = FindText(FindDirectChild(cardRoot, "Category Description"), null);
        card.valueText = FindText(FindDirectChild(cardRoot, "Value"), null);
        card.infoText = FindText(FindDirectChild(cardRoot, "Info"), null);
        card.sliderLabelText = FindText(FindDirectChild(cardRoot, "Slider Label"), null);
        card.slider = cardRoot.GetComponentInChildren<Slider>(true);
        return card;
    }

    private void BindDetailCards(PlanetData data)
    {
        if (_detailCards == null || _detailCards.Length < 5) return;

        SetCard(_detailCards[0], "Planet Size", "Diameter from side to side", FormatDiameter(data), data.planetSizeInfoText);
        SetCard(_detailCards[1], "Orbital Distance", "Space between planet and the Sun", FormatOrbitalDistance(data), data.orbitalDistanceInfoText);
        SetCard(_detailCards[2], "Orbital Speed", "How fast the planet travels around the Sun", FormatOrbitalSpeed(data), data.orbitalSpeedInfoText);
        SetCard(_detailCards[3], "Axial Tilt", "How strongly the planet leans on its axis", FormatAxialTilt(data), data.axialTiltInfoText);
        SetCard(_detailCards[4], "Eccentricity", "How oval the orbit is", FormatEccentricity(data), data.eccentricityInfoText);

        SetCard(_relativeSizeCard, "Planet Scale", "Size compared with Earth", "", data.planetScaleInfoText);
        SetText(_relativeSizeCard.sliderLabelText, FormatRelativeSize(data));

        if (_relativeSizeCard.slider != null)
        {
            float largestPlanetRatio = JupiterDiameterKm / EarthDiameterKm;
            float normalizedValue = data.diameter > 0f ? Mathf.Clamp01((data.diameter / EarthDiameterKm) / largestPlanetRatio) : 0f;
            _relativeSizeCard.slider.SetValueWithoutNotify(normalizedValue);
        }
    }

    private void SetCard(DetailCard card, string category, string categoryDescription, string value, string infoText)
    {
        if (IsImmersiveModeCard(card))
        {
            return;
        }

        SetText(card.categoryText, category);
        SetText(card.categoryDescriptionText, categoryDescription);
        SetText(card.valueText, value);
        SetText(card.infoText, GetDetailInfoText(infoText));
    }

    private bool IsImmersiveModeCard(DetailCard card)
    {
        if (card.root == null) return false;

        if (card.categoryText != null && ContainsImmersiveModeText(card.categoryText.text))
        {
            return true;
        }

        Button button = card.root.GetComponent<Button>();
        if (button != null && ContainsImmersiveModeText(GetCombinedText(card.root)))
        {
            return true;
        }

        Toggle toggle = card.root.GetComponent<Toggle>();
        return toggle != null && ContainsImmersiveModeText(GetCombinedText(card.root));
    }

    private void SetText(TMP_Text textField, string value)
    {
        if (textField == null) return;
        textField.text = value;
    }

    private string FirstFilled(params string[] values)
    {
        if (values == null) return "";

        for (int i = 0; i < values.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(values[i]) == false)
            {
                return values[i];
            }
        }

        return "";
    }

    private string GetDetailInfoText(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? LoremIpsum : value;
    }

    private string FormatDiameter(PlanetData data)
    {
        return data.diameter > 0f ? data.diameter.ToString("N0") + " km" : "-";
    }

    private string FormatOrbitalDistance(PlanetData data)
    {
        return data.semiMajorAxis > 0f ? data.semiMajorAxis.ToString("F2") + " AU" : "0 AU";
    }

    private string FormatOrbitalSpeed(PlanetData data)
    {
        if (data.semiMajorAxis <= 0f || data.orbitalPeriod <= 0f)
        {
            return "-";
        }

        float orbitCircumferenceKm = 2f * Mathf.PI * data.semiMajorAxis * AstronomicalUnitKm;
        float orbitalPeriodSeconds = data.orbitalPeriod * 86400f;
        float speedKmPerSecond = orbitCircumferenceKm / orbitalPeriodSeconds;
        return speedKmPerSecond.ToString("F1") + " km/s";
    }

    private string FormatAxialTilt(PlanetData data)
    {
        return data.axialTilt.ToString("F1") + "°";
    }

    private string FormatEccentricity(PlanetData data)
    {
        return data.eccentricity.ToString("F3");
    }

    private string FormatRelativeSize(PlanetData data)
    {
        if (data.diameter <= 0f) return "-";

        float earthRatio = data.diameter / EarthDiameterKm;
        if (earthRatio >= 10f)
        {
            return earthRatio.ToString("F1") + "x Earth";
        }

        return (earthRatio * 100f).ToString("F0") + "% of Earth";
    }

    private Transform FindDirectChild(Transform parent, string childName)
    {
        if (parent == null || string.IsNullOrEmpty(childName)) return null;

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.name == childName) return child;
        }

        return null;
    }

    private Transform FindDeepChild(Transform parent, string childName)
    {
        if (parent == null || string.IsNullOrEmpty(childName)) return null;

        if (parent.name == childName) return parent;

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform found = FindDeepChild(parent.GetChild(i), childName);
            if (found != null) return found;
        }

        return null;
    }

    private TMP_Text FindText(Transform root, TMP_Text fallback)
    {
        if (root == null) return fallback;
        TMP_Text text = root.GetComponent<TMP_Text>();
        if (text != null) return text;
        return root.GetComponentInChildren<TMP_Text>(true) ?? fallback;
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

    // Kann direkt im Toggle unter On Value Changed (bool) eingetragen werden.
    public void SetImmersiveModeFromToggle(bool isActive)
    {
        if (_currentPlanetData == null)
        {
            Debug.LogWarning("PlanetInfoPanel: Kein Planet fuer den Immersive Mode gebunden.");
            SetImmersiveModeActive(false);
            return;
        }

        if (PlanetInfoPanelManager.Instance == null)
        {
            Debug.LogWarning("PlanetInfoPanel: Kein PlanetInfoPanelManager fuer den Immersive Mode gefunden.");
            SetImmersiveModeActive(false);
            return;
        }

        PlanetInfoPanelManager.Instance.SetImmersiveModeActive(isActive, _currentPlanetData);
    }

    private void UpdateImmersiveButton(PlanetData data)
    {
        if (immersiveModeButton == null && immersiveModeToggle == null) return;

        bool isSun = data != null && data.planetName == "Sonne";
        bool isInteractable = data != null && isSun == false;

        if (immersiveModeButton != null)
        {
            immersiveModeButton.gameObject.SetActive(isSun == false);
            immersiveModeButton.interactable = isInteractable;
        }

        if (immersiveModeToggle != null)
        {
            immersiveModeToggle.gameObject.SetActive(isSun == false);
            immersiveModeToggle.interactable = isInteractable;
        }

        SetImmersiveModeActive(false);
    }

    public void SetImmersiveModeActive(bool isActive)
    {
        if (immersiveModeToggle != null)
        {
            immersiveModeToggle.SetIsOnWithoutNotify(isActive);
        }

        if (immersiveModeButtonText != null)
        {
            immersiveModeButtonText.text = isActive ? LeaveImmersiveButtonLabel : ImmersiveButtonLabel;
        }
    }

    private void EnsureImmersiveControl()
    {
        if (immersiveModeToggle != null || immersiveModeButton != null) return;

        Transform existingToggle = FindDeepChild(transform, "Toggle_ImmersiveMode");
        if (existingToggle == null) existingToggle = FindDeepChild(transform, "ImmersiveModeToggle");
        if (existingToggle == null) existingToggle = FindDeepChild(transform, "ToggleImmersiveMode");
        if (existingToggle == null) existingToggle = FindDeepChild(transform, "Immersive Mode");
        if (existingToggle != null)
        {
            immersiveModeToggle = existingToggle.GetComponent<Toggle>();
            if (immersiveModeToggle == null) immersiveModeToggle = existingToggle.GetComponentInChildren<Toggle>(true);
            immersiveModeButtonText = existingToggle.GetComponentInChildren<TMP_Text>(true);
            if (immersiveModeToggle != null) return;
        }

        immersiveModeToggle = FindImmersiveModeToggle();
        if (immersiveModeToggle != null)
        {
            immersiveModeButtonText = immersiveModeToggle.GetComponentInChildren<TMP_Text>(true);
            return;
        }

        Transform existingButton = FindDeepChild(transform, "Button_ImmersiveMode");
        if (existingButton != null)
        {
            immersiveModeButton = existingButton.GetComponent<Button>();
            immersiveModeButtonText = existingButton.GetComponentInChildren<TMP_Text>(true);
            return;
        }

        immersiveModeButton = FindImmersiveModeButton();
        if (immersiveModeButton != null)
        {
            immersiveModeButtonText = immersiveModeButton.GetComponentInChildren<TMP_Text>(true);
            return;
        }

        if (FindDeepChild(transform, "PlanetDetailCanvas") != null)
        {
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

    private Toggle FindImmersiveModeToggle()
    {
        Toggle[] toggles = GetComponentsInChildren<Toggle>(true);
        for (int i = 0; i < toggles.Length; i++)
        {
            Toggle toggle = toggles[i];
            if (toggle == null) continue;

            string toggleName = toggle.name.ToLowerInvariant();
            if (toggleName.Contains("immersive"))
            {
                return toggle;
            }

            TMP_Text label = toggle.GetComponentInChildren<TMP_Text>(true);
            if (label != null && label.text.ToLowerInvariant().Contains("immersive"))
            {
                return toggle;
            }

            Transform current = toggle.transform.parent;
            int parentDepth = 0;
            while (current != null && current != transform && parentDepth < 4)
            {
                string parentName = current.name.ToLowerInvariant();
                if (parentName.Contains("immersive"))
                {
                    return toggle;
                }

                TMP_Text[] parentTexts = current.GetComponentsInChildren<TMP_Text>(true);
                for (int textIndex = 0; textIndex < parentTexts.Length; textIndex++)
                {
                    TMP_Text parentText = parentTexts[textIndex];
                    if (parentText != null && parentText.text.ToLowerInvariant().Contains("immersive"))
                    {
                        return toggle;
                    }
                }

                current = current.parent;
                parentDepth++;
            }
        }

        return null;
    }

    private Button FindImmersiveModeButton()
    {
        Button[] buttons = GetComponentsInChildren<Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = buttons[i];
            if (button == null) continue;

            string buttonName = button.name.ToLowerInvariant();
            if (buttonName.Contains("immersive"))
            {
                return button;
            }

            if (ContainsImmersiveModeText(GetCombinedText(button.transform)))
            {
                return button;
            }
        }

        return null;
    }

    private string GetCombinedText(Transform root)
    {
        if (root == null) return "";

        TMP_Text[] texts = root.GetComponentsInChildren<TMP_Text>(true);
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < texts.Length; i++)
        {
            if (texts[i] == null) continue;
            builder.Append(' ');
            builder.Append(texts[i].text);
        }

        return builder.ToString();
    }

    private bool ContainsImmersiveModeText(string value)
    {
        return string.IsNullOrWhiteSpace(value) == false
            && value.ToLowerInvariant().Contains("immersive");
    }

    private void BindImmersiveControlIfNeeded()
    {
        EnsureImmersiveControl();

        if (_hasBoundImmersiveControl) return;

        if (immersiveModeToggle != null)
        {
            immersiveModeToggle.onValueChanged.RemoveListener(SetImmersiveModeFromToggle);
            immersiveModeToggle.onValueChanged.AddListener(SetImmersiveModeFromToggle);
            _hasBoundImmersiveControl = true;
            Debug.Log("PlanetInfoPanel: Immersive-Mode-Toggle verbunden: " + immersiveModeToggle.name);
            return;
        }

        if (immersiveModeButton != null)
        {
            immersiveModeButton.onClick.RemoveListener(OpenImmersiveMode);
            immersiveModeButton.onClick.AddListener(OpenImmersiveMode);
            _hasBoundImmersiveControl = true;
            Debug.Log("PlanetInfoPanel: Immersive-Mode-Button verbunden: " + immersiveModeButton.name);
            return;
        }

        Debug.LogWarning("PlanetInfoPanel: Kein Immersive-Mode-Toggle oder Button im PlanetDetailRoot gefunden.");
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
