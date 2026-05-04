using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class AlwaysFaceCamera : MonoBehaviour
{
    [Header("Ziel")]
    [Tooltip("Optional: Wenn leer, wird automatisch die Hauptkamera benutzt.")]
    [SerializeField] private Transform _targetCamera;

    [Header("Ausrichtung")]
    [Tooltip("Aktivieren, wenn das Objekt nur um die Y-Achse rotieren soll.")]
    [SerializeField] private bool _lockYAxis;

    [Tooltip("Aktivieren, wenn die Rueckseite statt der Vorderseite zur Kamera zeigt.")]
    [SerializeField] private bool _flipDirection;

    [Tooltip("Im Editor zur Scene-View-Kamera ausrichten, solange das Spiel nicht laeuft.")]
    [SerializeField] private bool _useSceneViewCameraInEditor = true;

    private void LateUpdate()
    {
        Transform cameraTransform = GetCameraTransform();
        if (cameraTransform == null) return;

        Vector3 directionToCamera = cameraTransform.position - transform.position;

        if (_lockYAxis)
        {
            directionToCamera.y = 0f;
        }

        if (directionToCamera.sqrMagnitude < 0.0001f) return;

        if (_flipDirection)
        {
            directionToCamera *= -1f;
        }

        transform.rotation = Quaternion.LookRotation(directionToCamera, Vector3.up);
    }

    private Transform GetCameraTransform()
    {
        if (_targetCamera != null)
        {
            return _targetCamera;
        }

#if UNITY_EDITOR
        if (Application.isPlaying == false && _useSceneViewCameraInEditor)
        {
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null && sceneView.camera != null)
            {
                return sceneView.camera.transform;
            }
        }
#endif

        Camera mainCamera = Camera.main;
        return mainCamera != null ? mainCamera.transform : null;
    }
}
