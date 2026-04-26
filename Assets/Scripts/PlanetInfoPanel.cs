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

    public void Bind(PlanetData data)
    {
        if (data == null)
        {
            Debug.LogWarning("PlanetInfoPanel: Kein PlanetData uebergeben.");
            return;
        }

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
}
