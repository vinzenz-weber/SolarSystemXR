using UnityEngine;

// Kommt auf jeden einzelnen Planeten-Button im planetenPanel.
// Im Inspector weist du ihm zu: welcher Planet ist das, und welches Prefab soll erscheinen.
public class PlanetButton : MonoBehaviour
{
    [Tooltip("Die Daten dieses Planeten (aus Assets/Planets/ — z.B. Erde.asset)")]
    public PlanetData meinPlanet;

    [Tooltip("Das 3D-Objekt das angezeigt werden soll wenn dieser Button geklickt wird")]
    public GameObject anzeigeObjekt;

    [Tooltip("Referenz auf den GameManager in der Szene")]
    public GameManager gameManager;

    // Diese Methode verdrahtest du im Inspector unter OnClick()
    public void OnKlicken()
    {
        gameManager.PlanetAuswaehlen(meinPlanet, anzeigeObjekt);
    }
}
