using UnityEngine;

// Kleine Bruecke fuer Buttons in Minigame-Prefabs.
// So muessen Prefabs keine Szene-Referenz auf den MinigameManager speichern.
public class MinigameUIActions : MonoBehaviour
{
    public void CompleteAndReturnToMenu()
    {
        if (MinigameManager.Instance == null)
        {
            Debug.LogWarning("MinigameUIActions: Kein MinigameManager in der Szene gefunden.");
            return;
        }

        MinigameManager.Instance.CompleteCurrentMinigame();
        MinigameManager.Instance.EndMinigame();
    }

    public void ExitWithoutCompleting()
    {
        if (MinigameManager.Instance == null)
        {
            Debug.LogWarning("MinigameUIActions: Kein MinigameManager in der Szene gefunden.");
            return;
        }

        MinigameManager.Instance.EndMinigame();
    }
}
