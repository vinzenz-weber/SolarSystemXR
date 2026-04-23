using UnityEngine;
using Meta.XR;

public class SceneCollisionDebug_RaycastManager : MonoBehaviour
{

    public Transform rayStartPoint;
    public EnvironmentRaycastManager envRayManager;
    public float rayLength = 5;
    public TMPro.TMP_Text debugText;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = new Ray(rayStartPoint.position, rayStartPoint.forward);


        bool HasHit = envRayManager.Raycast(ray, out var hit, rayLength);

        if (HasHit)
        {
            Vector3 hitPoint = hit.point;
            Vector3 hitNormal = hit.normal;

            debugText.transform.position = hitPoint;
            debugText.transform.rotation = Quaternion.LookRotation(-hitNormal);

            debugText.text = "Environment hit";
        }
    }
}
