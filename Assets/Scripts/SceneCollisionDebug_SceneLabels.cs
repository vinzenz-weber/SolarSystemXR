using UnityEngine;
using Meta.XR.MRUtilityKit;

public class SceneCollisionDebug_SceneLabels : MonoBehaviour
{

    public Transform rayStartPoint;
    public float rayLength = 5;
    public MRUKAnchor.SceneLabels labelFilter;

    public OVRInput.Button button = OVRInput.Button.PrimaryShoulder;
    public TMPro.TMP_Text debugTextPrefab;

    // Mindestwert für den Winkel zur Senkrechten: 1.0 = exakt nach oben, 0.7 ≈ bis 45°
    public float minUpDot = 0.7f;

    private bool readyToPlace = false;
    private Vector3 PlacementPos;

    public GameObject PlanetPrefab;

    private TMPro.TMP_Text _debugText;


    void Start()
    {
        // Prefab einmal am Anfang instanziieren, damit wir eine echte Szenen-Instanz haben
        _debugText = Instantiate(debugTextPrefab);
    }

    void Update()
    {
        if (OVRInput.GetDown(button) && readyToPlace)
        {
            Instantiate(PlanetPrefab, PlacementPos, Quaternion.identity);
        }

        Ray ray = new Ray(rayStartPoint.position, rayStartPoint.forward);
        MRUKRoom room = MRUK.Instance.GetCurrentRoom();

        bool HasHit = room.Raycast(ray, rayLength, new LabelFilter(labelFilter), out RaycastHit hit, out MRUKAnchor anchor);

        _debugText.gameObject.SetActive(HasHit);

        if (HasHit)
        {
            _debugText.transform.SetPositionAndRotation(hit.point, Quaternion.LookRotation(-hit.normal));
            _debugText.text = "ANCHOR: " + anchor.Label.ToString();
        }
    }
}
