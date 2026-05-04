using UnityEngine;

public class LineBetweenPoints : MonoBehaviour
{
    public Transform PointA;
    public Transform PointB;
    public LineRenderer LineRenderer;

    [Header("Darstellung")]
    [Tooltip("Farbe der Linie im Spiel.")]
    [SerializeField] private Color _lineColor = Color.white;

    [Tooltip("Linienbreite in Metern.")]
    [SerializeField] private float _lineWidth = 0.003f;

    [Tooltip("Optionales Material. Leer = es wird automatisch ein URP-Unlit-Material erstellt.")]
    [SerializeField] private Material _lineMaterial;

    private Material _runtimeLineMaterial;

    private void OnEnable()
    {
        ApplyLineSettings();
    }

    private void LateUpdate()
    {
        UpdateLineRenderer();
    }

    private void OnValidate()
    {
        ApplyLineSettings();
    }

    private void OnDestroy()
    {
        if (_runtimeLineMaterial == null) return;

        if (Application.isPlaying)
        {
            Destroy(_runtimeLineMaterial);
        }
        else
        {
            DestroyImmediate(_runtimeLineMaterial);
        }
    }

    private void UpdateLineRenderer()
    {
        if (PointA == null || PointB == null || LineRenderer == null)
            return;

        ApplyLineSettings();
        LineRenderer.useWorldSpace = true;
        LineRenderer.positionCount = 2;
        LineRenderer.SetPosition(0, PointA.position);
        LineRenderer.SetPosition(1, PointB.position);
    }

    private void ApplyLineSettings()
    {
        if (LineRenderer == null) return;

        LineRenderer.startColor = _lineColor;
        LineRenderer.endColor = _lineColor;
        LineRenderer.widthMultiplier = _lineWidth;
        LineRenderer.numCapVertices = 4;
        LineRenderer.numCornerVertices = 4;

        Material material = _lineMaterial != null ? _lineMaterial : GetRuntimeLineMaterial();
        if (material == null) return;

        LineRenderer.sharedMaterial = material;
        ApplyMaterialColor(material, _lineColor);
    }

    private Material GetRuntimeLineMaterial()
    {
        if (_runtimeLineMaterial != null)
            return _runtimeLineMaterial;

        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
        {
            shader = Shader.Find("Sprites/Default");
        }

        if (shader == null)
            return null;

        _runtimeLineMaterial = new Material(shader);
        _runtimeLineMaterial.name = "Runtime_Line_Unlit";
        return _runtimeLineMaterial;
    }

    private void ApplyMaterialColor(Material material, Color color)
    {
        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }

        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", color);
        }
    }

    private void OnDrawGizmos()
    {
        if (PointA == null || PointB == null)
            return;

        Gizmos.color = _lineColor;
        Gizmos.DrawLine(PointA.position, PointB.position);
    }
}
