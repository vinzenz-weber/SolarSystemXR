using TMPro;
using UnityEngine;

// Prueft laufend, welcher Planet auf welchem Orbit-Slot liegt.
// Das ist bewusst einfach gehalten: Meta SDK snappt/bewegt, dieses Script wertet nur aus.
public class ReihenfolgeChecker : MonoBehaviour
{
    [Header("Spielobjekte")]
    [Tooltip("Wenn aktiv, sammelt der Checker Slots und Planeten automatisch aus den Children ein.")]
    public bool AutoCollectChildren = true;
    public ReihenfolgeOrbitSlot[] Slots;
    public ReihenfolgePlanet[] Planets;
    public Transform[] BenchPositions;
    public TMP_Text SuccessText;

    [Header("Farben")]
    public Color NeutralColor = new Color(0.25f, 0.65f, 1f, 0.65f);
    public Color CorrectColor = new Color(0.1f, 0.9f, 0.35f, 1f);
    public Color WrongColor = new Color(1f, 0.18f, 0.12f, 1f);

    [Header("Auswertung")]
    [Tooltip("Maximaler Abstand in Metern, damit ein Planet als auf einem Slot liegend zaehlt.")]
    public float SnapCheckDistance = 0.12f;

    private bool _hasCompleted;

    private void Reset()
    {
        CollectChildren();
    }

    private void Start()
    {
        if (AutoCollectChildren)
        {
            CollectChildren();
        }

        HideSuccessText();
    }

    private void Update()
    {
        EvaluateSlots();
    }

    public void ResetGame()
    {
        _hasCompleted = false;
        HideSuccessText();

        if (Slots != null)
        {
            foreach (ReihenfolgeOrbitSlot slot in Slots)
            {
                if (slot != null)
                {
                    slot.ClearCurrentPlanet();
                }
            }
        }

        RandomizeBench();
    }

    private void EvaluateSlots()
    {
        if (Slots == null || Planets == null || Slots.Length == 0 || Planets.Length == 0) return;

        bool[] usedPlanets = new bool[Planets.Length];

        foreach (ReihenfolgeOrbitSlot slot in Slots)
        {
            if (slot != null)
            {
                if (slot.RefreshCurrentPlanetFromSnapInteractable())
                {
                    MarkPlanetAsUsed(slot.CurrentPlanet, usedPlanets);
                }
            }
        }

        float maxDistanceSqr = SnapCheckDistance * SnapCheckDistance;

        foreach (ReihenfolgeOrbitSlot slot in Slots)
        {
            if (slot == null || slot.HasPlanet) continue;

            ReihenfolgePlanet closestPlanet = null;
            int closestPlanetIndex = -1;
            float closestDistanceSqr = maxDistanceSqr;

            for (int i = 0; i < Planets.Length; i++)
            {
                ReihenfolgePlanet planet = Planets[i];
                if (planet == null || usedPlanets[i]) continue;

                float distanceSqr = (planet.transform.position - slot.transform.position).sqrMagnitude;
                if (distanceSqr <= closestDistanceSqr)
                {
                    closestPlanet = planet;
                    closestPlanetIndex = i;
                    closestDistanceSqr = distanceSqr;
                }
            }

            if (closestPlanet != null)
            {
                usedPlanets[closestPlanetIndex] = true;
                slot.SetCurrentPlanet(closestPlanet);
            }
        }

        int correctCount = 0;
        int activeSlotCount = 0;

        foreach (ReihenfolgePlanet planet in Planets)
        {
            if (planet != null)
            {
                planet.SetFeedbackColor(NeutralColor);
            }
        }

        foreach (ReihenfolgeOrbitSlot slot in Slots)
        {
            if (slot == null) continue;
            activeSlotCount++;

            if (slot.HasPlanet == false)
            {
                slot.SetFeedbackColor(NeutralColor);
                continue;
            }

            Color feedbackColor = slot.HasCorrectPlanet ? CorrectColor : WrongColor;
            slot.SetFeedbackColor(feedbackColor);
            slot.CurrentPlanet.SetFeedbackColor(feedbackColor);

            if (slot.HasCorrectPlanet)
            {
                correctCount++;
            }
        }

        if (_hasCompleted == false && activeSlotCount > 0 && correctCount == activeSlotCount)
        {
            CompleteMinigame();
        }
    }

    private void MarkPlanetAsUsed(ReihenfolgePlanet planet, bool[] usedPlanets)
    {
        if (planet == null || Planets == null || usedPlanets == null) return;

        for (int i = 0; i < Planets.Length && i < usedPlanets.Length; i++)
        {
            if (Planets[i] == planet)
            {
                usedPlanets[i] = true;
                return;
            }
        }
    }

    private void CompleteMinigame()
    {
        _hasCompleted = true;

        if (SuccessText != null)
        {
            SuccessText.gameObject.SetActive(true);
            SuccessText.text = "Geschafft!";
        }

        if (MinigameManager.Instance != null)
        {
            MinigameManager.Instance.CompleteCurrentMinigame();
        }
    }

    private void RandomizeBench()
    {
        if (Planets == null || BenchPositions == null || BenchPositions.Length == 0) return;

        int[] order = new int[Planets.Length];
        for (int i = 0; i < order.Length; i++)
        {
            order[i] = i;
        }

        for (int i = 0; i < order.Length; i++)
        {
            int randomIndex = Random.Range(i, order.Length);
            (order[i], order[randomIndex]) = (order[randomIndex], order[i]);
        }

        for (int i = 0; i < Planets.Length; i++)
        {
            ReihenfolgePlanet planet = Planets[i];
            Transform benchPosition = BenchPositions[order[i] % BenchPositions.Length];

            if (planet != null)
            {
                planet.ResetToBench(benchPosition);
            }
        }
    }

    private void HideSuccessText()
    {
        if (SuccessText != null)
        {
            SuccessText.gameObject.SetActive(false);
        }
    }

    [ContextMenu("Slots und Planeten automatisch sammeln")]
    public void CollectChildren()
    {
        Slots = GetComponentsInChildren<ReihenfolgeOrbitSlot>(true);
        Planets = GetComponentsInChildren<ReihenfolgePlanet>(true);
    }
}
