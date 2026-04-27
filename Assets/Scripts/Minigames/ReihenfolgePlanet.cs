using Oculus.Interaction;
using UnityEngine;

// Haelt die Daten fuer einen einzelnen Planeten im Reihenfolge-Minispiel.
// Die eigentliche Grab- und Snap-Logik kommt vom Meta Interaction SDK.
public class ReihenfolgePlanet : MonoBehaviour
{
    [Header("Daten")]
    public PlanetData PlanetData;
    public int OrbitIndex;

    [Header("Meta SDK")]
    public Rigidbody Rigidbody;
    public Grabbable Grabbable;
    public SnapInteractor SnapInteractor;

    [Header("Feedback")]
    public Renderer FeedbackRenderer;

    private Color _neutralColor = Color.white;
    private Material _feedbackMaterial;

    public void Initialize(PlanetData planetData, int orbitIndex, Color neutralColor)
    {
        PlanetData = planetData;
        OrbitIndex = orbitIndex;
        _neutralColor = neutralColor;

        CacheFeedbackMaterial();
        SetFeedbackColor(_neutralColor);
    }

    public void ResetToBench(Transform benchPosition)
    {
        if (benchPosition == null) return;

        transform.SetPositionAndRotation(benchPosition.position, benchPosition.rotation);

        if (Rigidbody != null)
        {
            Rigidbody.linearVelocity = Vector3.zero;
            Rigidbody.angularVelocity = Vector3.zero;
        }

        SetFeedbackColor(_neutralColor);
    }

    public void SetFeedbackColor(Color color)
    {
        CacheFeedbackMaterial();
        if (_feedbackMaterial == null) return;

        if (_feedbackMaterial.HasProperty("_BaseColor"))
        {
            _feedbackMaterial.SetColor("_BaseColor", color);
            return;
        }

        if (_feedbackMaterial.HasProperty("_Color"))
        {
            _feedbackMaterial.SetColor("_Color", color);
        }
    }

    private void CacheFeedbackMaterial()
    {
        if (_feedbackMaterial != null) return;

        if (FeedbackRenderer == null)
        {
            FeedbackRenderer = GetComponentInChildren<Renderer>();
        }

        if (FeedbackRenderer != null)
        {
            _feedbackMaterial = FeedbackRenderer.material;
        }
    }
}
