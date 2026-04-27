using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Einfaches World-Space-Panel fuer Phase 4, bis die echten Minispiele gebaut werden.
public class MinigamePlaceholderPanel : MonoBehaviour
{
    public MinigameType minigameType;
    public bool buildOnStart = true;

    private bool _isBuilt;

    private void Start()
    {
        if (buildOnStart == true)
        {
            Build(minigameType);
        }
    }

    public void Build(MinigameType type)
    {
        if (_isBuilt == true) return;

        minigameType = type;
        _isBuilt = true;

        GameObject canvasObject = new GameObject("WorldSpaceUI", typeof(RectTransform));
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10f;

        canvasObject.AddComponent<GraphicRaycaster>();

        RectTransform rootRect = canvasObject.GetComponent<RectTransform>();
        rootRect.sizeDelta = new Vector2(520f, 320f);
        rootRect.localScale = Vector3.one * 0.0025f;

        Image background = canvasObject.AddComponent<Image>();
        background.color = new Color(0.02f, 0.04f, 0.07f, 0.92f);

        CreateText(rootRect, GetHeadline(type), 0f, 95f, 38f, FontStyles.Bold);
        CreateText(rootRect, GetBodyText(type), 0f, 35f, 22f, FontStyles.Normal);

        Button resetButton = CreateButton(rootRect, "Reset", -115f, -95f);
        resetButton.onClick.AddListener(ResetMinigame);

        Button backButton = CreateButton(rootRect, "Zurueck", 115f, -95f);
        backButton.onClick.AddListener(EndMinigame);

        if (type == MinigameType.Gravity)
        {
            resetButton.interactable = false;
        }
    }

    private void ResetMinigame()
    {
        if (MinigameManager.Instance != null)
        {
            MinigameManager.Instance.ResetMinigame();
        }
    }

    private void EndMinigame()
    {
        if (MinigameManager.Instance != null)
        {
            MinigameManager.Instance.EndMinigame();
        }
    }

    private string GetHeadline(MinigameType type)
    {
        if (type == MinigameType.Reihenfolge) return "Reihenfolge";
        if (type == MinigameType.Size) return "Size";
        return "Gravity";
    }

    private string GetBodyText(MinigameType type)
    {
        if (type == MinigameType.Gravity)
        {
            return "Coming Soon";
        }

        return "Platzhalter fuer Phase 4";
    }

    private TMP_Text CreateText(RectTransform parent, string text, float x, float y, float fontSize, FontStyles style)
    {
        GameObject textObject = new GameObject("Text_" + text);
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(x, y);
        rect.sizeDelta = new Vector2(440f, 70f);

        TextMeshProUGUI label = textObject.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = fontSize;
        label.fontStyle = style;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;

        return label;
    }

    private Button CreateButton(RectTransform parent, string labelText, float x, float y)
    {
        GameObject buttonObject = new GameObject("Button_" + labelText);
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(x, y);
        rect.sizeDelta = new Vector2(170f, 58f);

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.14f, 0.39f, 0.72f, 1f);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;

        TMP_Text label = CreateText(rect, labelText, 0f, 0f, 24f, FontStyles.Bold);
        RectTransform labelRect = label.GetComponent<RectTransform>();
        labelRect.sizeDelta = rect.sizeDelta;

        return button;
    }
}
