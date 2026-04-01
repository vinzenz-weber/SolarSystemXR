using System.Collections;
using UnityEngine;

public enum SpielZustand
{
    START,
    PLACEMENT,
    EXPLORE
}

public class GameManager : MonoBehaviour
{
    public SpielZustand aktuellerZustand;

    [Header("XR References")]
    [Tooltip("OVRCameraRig → TrackingSpace → RightHandAnchor")]
    public Transform rightControllerAnchor;
    [Tooltip("OVRHand-Komponente der rechten Hand (für Hand Tracking)")]
    public OVRHand rightHand;

    [Header("Platzierungs-Einstellungen")]
    public GameObject objektPrefab;
    public float maxReichweite = 10f;
    public LayerMask placementLayer;

    private bool isPlaced = false;
    private bool wasRightPinching = false;

    void Start()
    {
        ZustandWechseln(SpielZustand.START);
        StartCoroutine(AutoStartAfterDelay());
    }

    IEnumerator AutoStartAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        ZustandWechseln(SpielZustand.PLACEMENT);
    }

    void Update()
    {
        if (aktuellerZustand != SpielZustand.PLACEMENT) return;

        if (PlaceInputDown())
            ObjektPlatzieren();

        if (isPlaced && ConfirmInputDown())
            ZustandWechseln(SpielZustand.EXPLORE);
    }

    // Index-Trigger (Controller) oder Index-Pinch (Hand) — nur beim Drücken, nicht Halten
    private bool PlaceInputDown()
    {
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch))
            return true;

        if (rightHand != null && rightHand.IsTracked)
        {
            bool isPinching = rightHand.GetFingerIsPinching(OVRHand.HandFinger.Index);
            bool justPinched = isPinching && !wasRightPinching;
            wasRightPinching = isPinching;
            return justPinched;
        }

        wasRightPinching = false;
        return false;
    }

    // A-Button (Controller) oder Mittelfinger-Pinch (Hand) zum Bestätigen
    private bool ConfirmInputDown()
    {
        if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch))
            return true;

        if (rightHand != null && rightHand.IsTracked &&
            rightHand.GetFingerIsPinching(OVRHand.HandFinger.Middle))
            return true;

        return false;
    }

    private Ray GetInputRay()
    {
        // Hand Tracking: PointerPose zeigt in Zeigerichtung der Hand
        if (rightHand != null && rightHand.IsTracked && rightHand.PointerPose != null)
            return new Ray(rightHand.PointerPose.position, rightHand.PointerPose.forward);

        // Controller: Anchor-Transform
        if (rightControllerAnchor != null)
            return new Ray(rightControllerAnchor.position, rightControllerAnchor.forward);

        // Fallback: Kamera-Mitte
        return new Ray(Camera.main.transform.position, Camera.main.transform.forward);
    }

    public void ZustandWechseln(SpielZustand neuerZustand)
    {
        aktuellerZustand = neuerZustand;
        Debug.Log("Spielzustand: " + aktuellerZustand);
    }

    private void ObjektPlatzieren()
    {
        Ray ray = GetInputRay();

        if (Physics.Raycast(ray, out RaycastHit hit, maxReichweite, placementLayer))
        {
            Instantiate(objektPrefab, hit.point, Quaternion.identity);
            isPlaced = true;
            Debug.Log("Objekt platziert bei: " + hit.point);
        }
        else
        {
            Debug.Log("Kein Treffer im Raycast.");
        }
    }
}
