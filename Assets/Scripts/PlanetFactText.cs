using TMPro;
using UnityEngine;

// Liegt auf einem Fact-Prefab und schreibt einen Fakt aus PlanetData.fakten in ein Textfeld.
// factIndex ist 0-basiert: 0 = erster Fakt, 1 = zweiter Fakt, usw.
[ExecuteAlways]
public class PlanetFactText : MonoBehaviour
{
    [Header("Zuordnung")]
    [Min(0)]
    [Tooltip("0 = erster Eintrag in PlanetData.fakten, 1 = zweiter Eintrag, usw.")]
    public int factIndex;

    [Tooltip("Optional. Leer lassen, wenn das passende PlanetData automatisch gefunden werden soll.")]
    public PlanetData planetDataOverride;

    [Header("Textfeld")]
    [Tooltip("Textfeld im Fact-Prefab. Leer = erstes TMP_Text in diesem Objekt oder Childs.")]
    [SerializeField] private TMP_Text _targetText;

    [Tooltip("Textfeld ausblenden, wenn kein passender Fakt gefunden wurde.")]
    [SerializeField] private bool _hideTextWhenMissing = true;

    [Tooltip("Zeigt im Editor einen Hinweistext, wenn PlanetData oder Fakt fehlt.")]
    [SerializeField] private bool _showMissingTextInEditor = true;

    private void Reset()
    {
        FindTargetText();
    }

    private void OnEnable()
    {
        RefreshFactText();
    }

    private void Start()
    {
        RefreshFactText();
    }

    private void OnValidate()
    {
        if (factIndex < 0)
        {
            factIndex = 0;
        }

        FindTargetText();
        RefreshFactText();
    }

    [ContextMenu("Fakt neu laden")]
    public void RefreshFactText()
    {
        FindTargetText();
        if (_targetText == null) return;

        PlanetData planetData = ResolvePlanetData();
        string factText = GetFactText(planetData);
        bool hasFact = string.IsNullOrWhiteSpace(factText) == false;

        _targetText.text = factText;

        if (_hideTextWhenMissing)
        {
            _targetText.gameObject.SetActive(hasFact);
        }
    }

    private void FindTargetText()
    {
        if (_targetText != null) return;

        _targetText = GetComponent<TMP_Text>();
        if (_targetText == null)
        {
            _targetText = GetComponentInChildren<TMP_Text>(true);
        }
    }

    private string GetFactText(PlanetData planetData)
    {
        if (planetData == null)
        {
            return GetMissingText("Kein PlanetData gefunden");
        }

        if (planetData.fakten == null || planetData.fakten.Length == 0)
        {
            return GetMissingText("Keine Fakten in " + planetData.planetName);
        }

        if (factIndex >= planetData.fakten.Length)
        {
            return GetMissingText("Fakt " + factIndex + " fehlt in " + planetData.planetName);
        }

        return planetData.fakten[factIndex];
    }

    private string GetMissingText(string message)
    {
        if (Application.isPlaying == false && _showMissingTextInEditor)
        {
            return message;
        }

        Debug.LogWarning("PlanetFactText: " + message + " auf " + name + ".");
        return "";
    }

    private PlanetData ResolvePlanetData()
    {
        if (planetDataOverride != null)
        {
            return planetDataOverride;
        }

        PlanetSelectable selectable = GetComponentInParent<PlanetSelectable>(true);
        if (selectable != null)
        {
            PlanetData selectableData = selectable.GetPlanetData();
            if (selectableData != null)
            {
                return selectableData;
            }
        }

        PlanetBody planetBody = GetComponentInParent<PlanetBody>(true);
        if (planetBody != null && planetBody.data != null)
        {
            return planetBody.data;
        }

        InteractablePlanetVisual visual = GetComponentInParent<InteractablePlanetVisual>(true);
        if (visual != null && visual.PlanetData != null)
        {
            return visual.PlanetData;
        }

        PlanetSelectable rootSelectable = transform.root.GetComponentInChildren<PlanetSelectable>(true);
        if (rootSelectable != null)
        {
            PlanetData rootSelectableData = rootSelectable.GetPlanetData();
            if (rootSelectableData != null)
            {
                return rootSelectableData;
            }
        }

        PlanetBody rootPlanetBody = transform.root.GetComponentInChildren<PlanetBody>(true);
        if (rootPlanetBody != null && rootPlanetBody.data != null)
        {
            return rootPlanetBody.data;
        }

        InteractablePlanetVisual rootVisual = transform.root.GetComponentInChildren<InteractablePlanetVisual>(true);
        if (rootVisual != null && rootVisual.PlanetData != null)
        {
            return rootVisual.PlanetData;
        }

        return null;
    }
}
