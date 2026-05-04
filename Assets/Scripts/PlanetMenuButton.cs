using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlanetMenuButton : MonoBehaviour
{
    public PlanetData planetData;
    public MainMenuController mainMenuController;

    [Header("Beschriftung auf diesem Button")]
    public TextMeshProUGUI Label;
    public TextMeshProUGUI subHeadline;

    [Header("Button-Bild")]
    public Image buttonImage;

    private void Start()
    {
        if (planetData == null)
        {
            Debug.LogWarning("PlanetMenuButton: Kein PlanetData auf " + gameObject.name + " zugewiesen.");
            return;
        }

        // Beschriftung und Bild aus den Planeten-Daten setzen.
        if (Label != null) Label.text = planetData.planetName;
        if (subHeadline != null) subHeadline.text = planetData.subHeadline;

        Button button = GetComponent<Button>();
        if (buttonImage == null)
        {
            buttonImage = FindButtonImage(button);
        }

        if (buttonImage != null && planetData.menuButtonImage != null)
        {
            buttonImage.sprite = planetData.menuButtonImage;
            buttonImage.enabled = true;
            buttonImage.preserveAspect = true;
        }

        if (button != null)
        {
            button.onClick.AddListener(OnPlanetButtonClicked);
        }
        else
        {
            Debug.LogError("Fehler: Auf dem Objekt " + gameObject.name + " fehlt die Button-Komponente!");
        }
    }

    // Wird beim Klick auf den Planeten-Button aufgerufen.
    public void OnPlanetButtonClicked()
    {
        if (mainMenuController == null)
        {
            Debug.LogWarning("PlanetMenuButton: Kein MainMenuController auf " + gameObject.name + " zugewiesen.");
            return;
        }

        mainMenuController.SelectPlanet(planetData);
    }

    private Image FindButtonImage(Button button)
    {
        Image maskedBackground = FindImageAtPath(transform, "Mask/Background");
        if (maskedBackground != null) return maskedBackground;

        Transform buttonImageRoot = FindChildWithBaseName(transform, "ButtonImage");
        maskedBackground = FindImageAtPath(buttonImageRoot, "Mask/Background");
        if (maskedBackground != null) return maskedBackground;

        Image namedImage = FindImageWithName("Image");
        if (namedImage != null) return namedImage;

        namedImage = FindImageWithName("Icon");
        if (namedImage != null) return namedImage;

        Image targetImage = button != null ? button.targetGraphic as Image : null;
        if (targetImage != null) return targetImage;

        return GetComponentInChildren<Image>(true);
    }

    private Image FindImageAtPath(Transform root, string path)
    {
        if (root == null || string.IsNullOrWhiteSpace(path)) return null;

        Transform target = root.Find(path);
        if (target == null) return null;

        return target.GetComponent<Image>();
    }

    private Transform FindChildWithBaseName(Transform root, string baseName)
    {
        if (root == null || string.IsNullOrWhiteSpace(baseName)) return null;

        Transform[] children = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < children.Length; i++)
        {
            Transform child = children[i];
            if (child != null && HasBaseName(child.name, baseName))
            {
                return child;
            }
        }

        return null;
    }

    private bool HasBaseName(string objectName, string baseName)
    {
        if (string.IsNullOrWhiteSpace(objectName) || string.IsNullOrWhiteSpace(baseName)) return false;
        if (objectName == baseName) return true;

        return objectName.StartsWith(baseName + " (");
    }

    private Image FindImageWithName(string objectName)
    {
        Image[] images = GetComponentsInChildren<Image>(true);
        for (int i = 0; i < images.Length; i++)
        {
            if (images[i] != null && images[i].name == objectName)
            {
                return images[i];
            }
        }

        return null;
    }
}
