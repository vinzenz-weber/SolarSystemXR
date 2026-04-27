using TMPro;
using UnityEngine;

// Optionale Art-Direction-Anker fuer das Reihenfolge-Minispiel.
// Wenn dieses Script im Prefab liegt, nutzt der Builder diese Transforms statt harter Zahlen.
public class ReihenfolgeMinigameLayout : MonoBehaviour
{
    [Header("Generierter Inhalt")]
    [Tooltip("Parent fuer Planeten, Orbits und Snap-Slots. Leer = Builder-GameObject.")]
    public Transform GeneratedContentParent;

    [Tooltip("Mittelpunkt der Orbit-Ringe. Leer = Builder nutzt OrbitCenterLocal.")]
    public Transform OrbitCenter;

    [Tooltip("Startpositionen der Planeten. Wenn 8 gesetzt sind, kann die Bench komplett per Editor gestaltet werden.")]
    public Transform[] BenchPositions;

    [Header("UI")]
    [Tooltip("Optionales Anweisungs-Panel im Prefab. Der Builder laesst es nur stehen; Layout passiert im Editor.")]
    public GameObject InstructionPanel;

    [Tooltip("Optionaler Geschafft-Text. Leer = Builder erzeugt einfachen 3D-Text.")]
    public TMP_Text SuccessText;
}
