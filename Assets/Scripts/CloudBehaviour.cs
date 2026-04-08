using UnityEngine;

public class CloudBehaviour : MonoBehaviour
{
    [Range(0f, 3)]
    [SerializeField] private float sizePercentage = 0f;
    [SerializeField]    private float cloudSpeed = 10f;

    void Start()
    {
        ApplyScale();
    }

    void Update()
    {
        transform.Rotate(0f, cloudSpeed * Time.deltaTime, 0f);
    }

    void ApplyScale()
    {
        float scaleFactor = 1f + sizePercentage / 100f;
        transform.localScale = Vector3.one * scaleFactor;
    }

    void OnValidate()
    {
        ApplyScale();
    }
}
