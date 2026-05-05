using UnityEngine;

// Schaltet optionale Visuals nur sichtbar, wenn ein platzierter Planet in der Welt steht.
// Wichtig: Dieses Script selbst sollte auf einem Parent liegen, der aktiv bleibt.
public class PlacementStateVisibility : MonoBehaviour
{
    [Tooltip("Objekte, die nur beim platzierten Planeten im WORLD-State sichtbar sein sollen, z.B. Saturn-Ring-Partikel.")]
    [SerializeField] private GameObject[] objectsToToggle;

    [Tooltip("Partikel beim Ausblenden stoppen und beim Einblenden wieder starten.")]
    [SerializeField] private bool controlParticlePlayback = true;

    [Tooltip("Aktiv = die zugewiesenen Objekte behalten beim Einblenden ihre Zielgroesse in Weltkoordinaten, auch wenn der Planet-Root skaliert ist.")]
    [SerializeField] private bool keepWorldScaleWhenVisible = true;

    [Tooltip("Gewuenschte Welt-Skalierung fuer die zugewiesenen Objekte. Fuer normale Partikel meist (1, 1, 1).")]
    [SerializeField] private Vector3 visibleWorldScale = Vector3.one;

    [Tooltip("Aktiv = sichtbar nur bei einzeln platzierten Planeten, nicht im Sonnensystem-Prefab oder in Minigames.")]
    [SerializeField] private bool onlyForPlacedSinglePlanet = true;

    private void OnEnable()
    {
        GameManager.StateChanged += HandleStateChanged;
        Refresh();
    }

    private void OnDisable()
    {
        GameManager.StateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged(GameState newState)
    {
        SetVisible(CanShowInState(newState));
    }

    private void Refresh()
    {
        GameState currentState = GameManager.Instance != null
            ? GameManager.Instance.CurrentState
            : GameState.MAIN_MENU;

        SetVisible(CanShowInState(currentState));
    }

    private bool CanShowInState(GameState state)
    {
        if (state != GameState.WORLD) return false;
        if (onlyForPlacedSinglePlanet == false) return true;

        return GetComponentInParent<PlanetSelectable>(true) != null;
    }

    private void SetVisible(bool isVisible)
    {
        for (int i = 0; i < objectsToToggle.Length; i++)
        {
            GameObject targetObject = objectsToToggle[i];
            if (targetObject == null) continue;

            if (isVisible)
            {
                targetObject.SetActive(true);
                ApplyWorldScale(targetObject);

                if (controlParticlePlayback)
                {
                    SetParticlePlayback(targetObject, true);
                }

                continue;
            }

            if (controlParticlePlayback)
            {
                SetParticlePlayback(targetObject, false);
            }

            targetObject.SetActive(false);
        }
    }

    private void ApplyWorldScale(GameObject targetObject)
    {
        if (keepWorldScaleWhenVisible == false) return;

        Transform parent = targetObject.transform.parent;
        if (parent == null)
        {
            targetObject.transform.localScale = visibleWorldScale;
            return;
        }

        Vector3 parentWorldScale = parent.lossyScale;
        targetObject.transform.localScale = new Vector3(
            DivideSafe(visibleWorldScale.x, parentWorldScale.x),
            DivideSafe(visibleWorldScale.y, parentWorldScale.y),
            DivideSafe(visibleWorldScale.z, parentWorldScale.z));
    }

    private float DivideSafe(float value, float divisor)
    {
        if (Mathf.Abs(divisor) <= 0.0001f)
        {
            return value;
        }

        return value / divisor;
    }

    private void SetParticlePlayback(GameObject targetObject, bool isPlaying)
    {
        ParticleSystem[] particleSystems = targetObject.GetComponentsInChildren<ParticleSystem>(true);

        for (int i = 0; i < particleSystems.Length; i++)
        {
            ParticleSystem particleSystem = particleSystems[i];
            if (particleSystem == null) continue;

            if (isPlaying)
            {
                particleSystem.Play(true);
            }
            else
            {
                particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
    }
}
