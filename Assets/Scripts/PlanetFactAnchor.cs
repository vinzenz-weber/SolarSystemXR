using TMPro;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

// Liegt auf einem Empty am Planeten und zeigt den passenden Fakt aus PlanetData.fakten.
// factNumber ist 1-basiert: 1 zeigt fakten[0], 2 zeigt fakten[1], usw.
[ExecuteAlways]
public class PlanetFactAnchor : MonoBehaviour
{
    private const string LabelPrefix = "FactLabel_";
    private const string LinePrefix = "FactLine_";

    [Header("Zuordnung")]
    [Min(1)]
    [Tooltip("1 = erster Fakt im PlanetData.fakten Array, 2 = zweiter Fakt, usw.")]
    public int factNumber = 1;

    [Tooltip("Optional. Leer lassen, wenn das Script PlanetData automatisch im Parent finden soll.")]
    public PlanetData planetDataOverride;

    [Header("Position")]
    [Tooltip("Optionaler Mittelpunkt des Planeten. Leer = wird automatisch ueber PlanetBody/Parent gesucht.")]
    public Transform planetCenter;

    [Tooltip("Optionaler Root fuer die Mesh-Suche. Leer = PlanetBody/VisualRoot wird automatisch genutzt.")]
    public Transform meshSurfaceRoot;

    [Tooltip("Wenn aktiv, nutzt das Label die echte Mesh-Normal an der naechsten Oberflaechenstelle.")]
    public bool useMeshNormal = true;

    [Tooltip("Fallback: Wenn keine Mesh-Normal gefunden wird, schwebt das Label vom Planetenzentrum aus nach aussen.")]
    public bool useDirectionFromPlanetCenter = true;

    [Tooltip("Abstand des Labels vom Empty in Normalrichtung. 0.2 = 20 cm.")]
    public float labelDistance = 0.2f;

    [Tooltip("Fallback-Offset, falls kein Planetenzentrum gefunden wird.")]
    public Vector3 labelLocalOffset = new Vector3(0f, 0.12f, 0f);

    [Header("Text")]
    public Color textColor = Color.white;
    public float fontSize = 0.19f;
    public float textWorldScale = 0.6f;
    public float maxTextWidth = 0.45f;
    public TextAlignmentOptions alignment = TextAlignmentOptions.Center;

    [Header("Linie")]
    public Color lineColor = Color.white;
    public float lineWidth = 0.003f;
    public Material lineMaterial;

    [Header("Editor Preview")]
    [Tooltip("Zeigt Text und Linie auch im Edit Mode, damit Position und Groesse feinjustiert werden koennen.")]
    public bool showPreviewInEditor = true;

    [Tooltip("Zeigt im Edit Mode einen Hinweistext, wenn PlanetData oder Fakt fehlt.")]
    public bool showMissingFactTextInEditor = true;

    public Color missingFactTextColor = new Color(1f, 0.75f, 0.2f, 1f);

    private PlanetData _planetData;
    private Transform _cameraTransform;
    private Transform _labelTransform;
    private TextMeshPro _labelText;
    private LineRenderer _lineRenderer;
    private Material _runtimeLineMaterial;
    private bool _isInitializing;
    private Vector3 _currentSurfacePoint;

    private void OnEnable()
    {
        Initialize();
    }

    private void LateUpdate()
    {
        if (ShouldShowPreview() == false)
        {
            DestroyGeneratedObjectsInEditor();
            return;
        }

        if (_labelTransform == null || _lineRenderer == null)
        {
            Initialize();
        }

        if (_labelTransform == null || _lineRenderer == null) return;

        if (_cameraTransform == null)
        {
            _cameraTransform = ResolveCameraTransform();
        }

        _labelTransform.position = GetLabelWorldPosition();
        SetWorldScale(_labelTransform, Vector3.one * textWorldScale);

        if (_cameraTransform != null)
        {
            Vector3 lookDirection = _labelTransform.position - _cameraTransform.position;
            if (lookDirection.sqrMagnitude > 0.0001f)
            {
                _labelTransform.rotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
            }
        }

        _lineRenderer.SetPosition(0, _labelTransform.position);
        _lineRenderer.SetPosition(1, _currentSurfacePoint);
    }

    private void OnDisable()
    {
        DestroyGeneratedObjectsInEditor();
    }

    private void OnDestroy()
    {
        DestroyGeneratedObjectsInEditor();

        if (_runtimeLineMaterial != null)
        {
            DestroyMaterial(_runtimeLineMaterial);
            _runtimeLineMaterial = null;
        }
    }

    private void OnValidate()
    {
        if (factNumber < 1)
        {
            factNumber = 1;
        }

        string expectedName = "FactAnchor_" + factNumber.ToString("00");
        if (gameObject.name.StartsWith("FactAnchor_"))
        {
            gameObject.name = expectedName;
        }

        if (isActiveAndEnabled)
        {
            Initialize();
        }
    }

    [ContextMenu("Fakt neu laden")]
    public void RefreshContent()
    {
        if (_labelText == null) return;

        string factText = GetFactText(out bool isMissingPreviewText);
        bool hasText = string.IsNullOrWhiteSpace(factText) == false;

        _labelText.text = hasText ? factText : "";
        _labelText.color = isMissingPreviewText ? missingFactTextColor : textColor;
        _labelText.fontSize = fontSize;
        _labelText.alignment = alignment;
        _labelText.horizontalAlignment = HorizontalAlignmentOptions.Center;
        _labelText.verticalAlignment = VerticalAlignmentOptions.Middle;

        RectTransform rectTransform = _labelText.rectTransform;
        float safeScale = Mathf.Max(textWorldScale, 0.0001f);
        rectTransform.sizeDelta = new Vector2(maxTextWidth / safeScale, 1f / safeScale);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        if (_lineRenderer != null)
        {
            _lineRenderer.enabled = hasText;
            _lineRenderer.startColor = lineColor;
            _lineRenderer.endColor = lineColor;
            _lineRenderer.widthMultiplier = lineWidth;
            ApplyLineMaterialColor(lineColor);
        }

        if (_labelTransform != null)
        {
            _labelTransform.gameObject.SetActive(hasText);
        }
    }

    [ContextMenu("Editor Preview aufraeumen")]
    public void CleanupEditorPreview()
    {
        DestroyGeneratedObjectsInEditor();
    }

    private void CreateRuntimeObjects()
    {
        DestroyGeneratedObjectsInEditor();

        GameObject labelObject = new GameObject(LabelPrefix + factNumber.ToString("00"));
        labelObject.transform.SetParent(transform, false);
        ApplyEditorPreviewHideFlags(labelObject);

        _labelTransform = labelObject.transform;
        _labelText = labelObject.AddComponent<TextMeshPro>();
        _labelText.textWrappingMode = TextWrappingModes.Normal;
        _labelText.overflowMode = TextOverflowModes.Overflow;
        _labelText.richText = true;

        GameObject lineObject = new GameObject(LinePrefix + factNumber.ToString("00"));
        lineObject.transform.SetParent(transform, false);
        ApplyEditorPreviewHideFlags(lineObject);

        _lineRenderer = lineObject.AddComponent<LineRenderer>();
        _lineRenderer.useWorldSpace = true;
        _lineRenderer.positionCount = 2;
        _lineRenderer.numCapVertices = 4;
        _lineRenderer.numCornerVertices = 4;
        _lineRenderer.material = GetLineMaterial();
    }

    private void Initialize()
    {
        if (ShouldShowPreview() == false) return;
        if (_isInitializing) return;

        _isInitializing = true;
        _planetData = ResolvePlanetData();
        planetCenter = ResolvePlanetCenter();
        meshSurfaceRoot = ResolveMeshSurfaceRoot();
        _cameraTransform = ResolveCameraTransform();

        if (_labelTransform == null || _labelText == null || _lineRenderer == null)
        {
            CreateRuntimeObjects();
        }

        RefreshContent();
        _isInitializing = false;
    }

    private bool ShouldShowPreview()
    {
        return Application.isPlaying || showPreviewInEditor;
    }

    private string GetFactText(out bool isMissingPreviewText)
    {
        isMissingPreviewText = false;

        if (_planetData == null)
        {
            return GetMissingPreviewText("Kein PlanetData gefunden", out isMissingPreviewText);
        }

        if (_planetData.fakten == null || _planetData.fakten.Length == 0)
        {
            return GetMissingPreviewText("Keine Fakten in " + _planetData.planetName, out isMissingPreviewText);
        }

        int index = factNumber - 1;
        if (index < 0 || index >= _planetData.fakten.Length)
        {
            return GetMissingPreviewText("Fakt " + factNumber + " fehlt in " + _planetData.planetName, out isMissingPreviewText);
        }

        return _planetData.fakten[index];
    }

    private string GetMissingPreviewText(string message, out bool isMissingPreviewText)
    {
        bool canShowMissingText = Application.isPlaying == false && showMissingFactTextInEditor;
        isMissingPreviewText = canShowMissingText;

        if (canShowMissingText)
        {
            return message;
        }

        Debug.LogWarning("PlanetFactAnchor: " + message + " auf " + name + ".");
        return "";
    }

    private PlanetData ResolvePlanetData()
    {
        if (planetDataOverride != null)
        {
            return planetDataOverride;
        }

        PlanetBody planetBody = GetComponentInParent<PlanetBody>(true);
        if (planetBody != null && planetBody.data != null)
        {
            return planetBody.data;
        }

        PlanetSelectable selectable = GetComponentInParent<PlanetSelectable>(true);
        if (selectable != null)
        {
            PlanetData selectableData = selectable.GetPlanetData();
            if (selectableData != null)
            {
                return selectableData;
            }
        }

        InteractablePlanetVisual visual = GetComponentInParent<InteractablePlanetVisual>(true);
        if (visual != null && visual.PlanetData != null)
        {
            return visual.PlanetData;
        }

        PlanetBody childPlanetBody = transform.root.GetComponentInChildren<PlanetBody>(true);
        if (childPlanetBody != null && childPlanetBody.data != null)
        {
            return childPlanetBody.data;
        }

        InteractablePlanetVisual childVisual = transform.root.GetComponentInChildren<InteractablePlanetVisual>(true);
        if (childVisual != null && childVisual.PlanetData != null)
        {
            return childVisual.PlanetData;
        }

        return null;
    }

    private Transform ResolvePlanetCenter()
    {
        if (planetCenter != null)
        {
            return planetCenter;
        }

        PlanetBody planetBody = GetComponentInParent<PlanetBody>(true);
        if (planetBody != null)
        {
            return planetBody.transform;
        }

        InteractablePlanetVisual visual = GetComponentInParent<InteractablePlanetVisual>(true);
        if (visual != null && visual.VisualRoot != null)
        {
            return visual.VisualRoot;
        }

        PlanetBody childPlanetBody = transform.root.GetComponentInChildren<PlanetBody>(true);
        if (childPlanetBody != null)
        {
            return childPlanetBody.transform;
        }

        return transform.parent != null ? transform.parent : transform;
    }

    private Transform ResolveCameraTransform()
    {
        if (Camera.main != null)
        {
            return Camera.main.transform;
        }

#if UNITY_EDITOR
        if (Application.isPlaying == false
            && SceneView.lastActiveSceneView != null
            && SceneView.lastActiveSceneView.camera != null)
        {
            return SceneView.lastActiveSceneView.camera.transform;
        }
#endif

        Camera anyCamera = FindFirstObjectByType<Camera>();
        return anyCamera != null ? anyCamera.transform : null;
    }

    private Vector3 GetLabelWorldPosition()
    {
        Vector3 normalDirection = GetNormalDirection(out Vector3 surfacePoint);
        _currentSurfacePoint = surfacePoint;
        return surfacePoint + normalDirection * labelDistance;
    }

    private Vector3 GetNormalDirection(out Vector3 surfacePoint)
    {
        if (useMeshNormal && TryGetMeshSurfaceNormal(out surfacePoint, out Vector3 meshNormal))
        {
            return meshNormal;
        }

        surfacePoint = transform.position;

        if (useDirectionFromPlanetCenter && planetCenter != null)
        {
            Vector3 direction = transform.position - planetCenter.position;
            if (direction.sqrMagnitude > 0.0001f)
            {
                return direction.normalized;
            }
        }

        Vector3 fallbackDirection = transform.TransformDirection(labelLocalOffset);
        if (fallbackDirection.sqrMagnitude > 0.0001f)
        {
            return fallbackDirection.normalized;
        }

        return transform.up;
    }

    private bool TryGetMeshSurfaceNormal(out Vector3 surfacePoint, out Vector3 normalDirection)
    {
        surfacePoint = transform.position;
        normalDirection = Vector3.zero;

        Transform searchRoot = meshSurfaceRoot != null ? meshSurfaceRoot : ResolveMeshSurfaceRoot();
        if (searchRoot == null) return false;

        MeshFilter[] meshFilters = searchRoot.GetComponentsInChildren<MeshFilter>(true);
        float bestDistanceSqr = float.PositiveInfinity;
        Vector3 bestPoint = transform.position;
        Vector3 bestNormal = Vector3.zero;

        foreach (MeshFilter meshFilter in meshFilters)
        {
            if (meshFilter == null || meshFilter.sharedMesh == null) continue;
            if (meshFilter.transform == transform || meshFilter.transform.IsChildOf(transform)) continue;

            Mesh mesh = meshFilter.sharedMesh;
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;
            Vector3[] normals = mesh.normals;

            if (vertices == null || triangles == null || vertices.Length == 0 || triangles.Length < 3) continue;

            Transform meshTransform = meshFilter.transform;

            for (int i = 0; i < triangles.Length; i += 3)
            {
                int index0 = triangles[i];
                int index1 = triangles[i + 1];
                int index2 = triangles[i + 2];

                if (index0 >= vertices.Length || index1 >= vertices.Length || index2 >= vertices.Length) continue;

                Vector3 worldA = meshTransform.TransformPoint(vertices[index0]);
                Vector3 worldB = meshTransform.TransformPoint(vertices[index1]);
                Vector3 worldC = meshTransform.TransformPoint(vertices[index2]);

                Vector3 closestPoint = ClosestPointOnTriangle(transform.position, worldA, worldB, worldC);
                float distanceSqr = (transform.position - closestPoint).sqrMagnitude;
                if (distanceSqr >= bestDistanceSqr) continue;

                Vector3 normal = GetTriangleNormal(meshTransform, vertices, normals, index0, index1, index2, worldA, worldB, worldC, closestPoint);
                if (normal.sqrMagnitude <= 0.0001f) continue;

                Vector3 pointToAnchor = transform.position - closestPoint;
                if (pointToAnchor.sqrMagnitude > 0.0001f && Vector3.Dot(normal, pointToAnchor) < 0f)
                {
                    normal = -normal;
                }

                bestDistanceSqr = distanceSqr;
                bestPoint = closestPoint;
                bestNormal = normal.normalized;
            }
        }

        if (bestNormal.sqrMagnitude <= 0.0001f) return false;

        surfacePoint = bestPoint;
        normalDirection = bestNormal;
        return true;
    }

    private Transform ResolveMeshSurfaceRoot()
    {
        if (meshSurfaceRoot != null)
        {
            return meshSurfaceRoot;
        }

        PlanetBody planetBody = GetComponentInParent<PlanetBody>(true);
        if (planetBody != null)
        {
            return planetBody.transform;
        }

        InteractablePlanetVisual visual = GetComponentInParent<InteractablePlanetVisual>(true);
        if (visual != null && visual.VisualRoot != null)
        {
            return visual.VisualRoot;
        }

        PlanetBody childPlanetBody = transform.root.GetComponentInChildren<PlanetBody>(true);
        if (childPlanetBody != null)
        {
            return childPlanetBody.transform;
        }

        return planetCenter != null ? planetCenter : transform.parent;
    }

    private Vector3 GetTriangleNormal(
        Transform meshTransform,
        Vector3[] vertices,
        Vector3[] normals,
        int index0,
        int index1,
        int index2,
        Vector3 worldA,
        Vector3 worldB,
        Vector3 worldC,
        Vector3 closestPoint)
    {
        if (normals != null && normals.Length == vertices.Length)
        {
            Vector3 barycentric = GetBarycentricCoordinates(closestPoint, worldA, worldB, worldC);
            Vector3 localNormal = normals[index0] * barycentric.x
                + normals[index1] * barycentric.y
                + normals[index2] * barycentric.z;

            return meshTransform.TransformDirection(localNormal).normalized;
        }

        return Vector3.Cross(worldB - worldA, worldC - worldA).normalized;
    }

    private Vector3 GetBarycentricCoordinates(Vector3 point, Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 v0 = b - a;
        Vector3 v1 = c - a;
        Vector3 v2 = point - a;

        float dot00 = Vector3.Dot(v0, v0);
        float dot01 = Vector3.Dot(v0, v1);
        float dot11 = Vector3.Dot(v1, v1);
        float dot20 = Vector3.Dot(v2, v0);
        float dot21 = Vector3.Dot(v2, v1);
        float denominator = dot00 * dot11 - dot01 * dot01;

        if (Mathf.Abs(denominator) < 0.0001f)
        {
            return new Vector3(1f, 0f, 0f);
        }

        float v = (dot11 * dot20 - dot01 * dot21) / denominator;
        float w = (dot00 * dot21 - dot01 * dot20) / denominator;
        float u = 1f - v - w;

        return new Vector3(u, v, w);
    }

    private Vector3 ClosestPointOnTriangle(Vector3 point, Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 ab = b - a;
        Vector3 ac = c - a;
        Vector3 ap = point - a;

        float d1 = Vector3.Dot(ab, ap);
        float d2 = Vector3.Dot(ac, ap);
        if (d1 <= 0f && d2 <= 0f) return a;

        Vector3 bp = point - b;
        float d3 = Vector3.Dot(ab, bp);
        float d4 = Vector3.Dot(ac, bp);
        if (d3 >= 0f && d4 <= d3) return b;

        float vc = d1 * d4 - d3 * d2;
        if (vc <= 0f && d1 >= 0f && d3 <= 0f)
        {
            float v = d1 / (d1 - d3);
            return a + ab * v;
        }

        Vector3 cp = point - c;
        float d5 = Vector3.Dot(ab, cp);
        float d6 = Vector3.Dot(ac, cp);
        if (d6 >= 0f && d5 <= d6) return c;

        float vb = d5 * d2 - d1 * d6;
        if (vb <= 0f && d2 >= 0f && d6 <= 0f)
        {
            float w = d2 / (d2 - d6);
            return a + ac * w;
        }

        float va = d3 * d6 - d5 * d4;
        if (va <= 0f && (d4 - d3) >= 0f && (d5 - d6) >= 0f)
        {
            float w = (d4 - d3) / ((d4 - d3) + (d5 - d6));
            return b + (c - b) * w;
        }

        float denominator = 1f / (va + vb + vc);
        float finalV = vb * denominator;
        float finalW = vc * denominator;
        return a + ab * finalV + ac * finalW;
    }

    private Material GetLineMaterial()
    {
        if (lineMaterial != null)
        {
            return lineMaterial;
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
        {
            shader = Shader.Find("Sprites/Default");
        }

        _runtimeLineMaterial = new Material(shader);
        if (_runtimeLineMaterial.HasProperty("_BaseColor"))
        {
            _runtimeLineMaterial.SetColor("_BaseColor", lineColor);
        }
        else if (_runtimeLineMaterial.HasProperty("_Color"))
        {
            _runtimeLineMaterial.SetColor("_Color", lineColor);
        }

        return _runtimeLineMaterial;
    }

    private void ApplyLineMaterialColor(Color color)
    {
        Material material = lineMaterial != null ? lineMaterial : _runtimeLineMaterial;
        if (material == null) return;

        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }
        else if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", color);
        }
    }

    private void SetWorldScale(Transform target, Vector3 worldScale)
    {
        Transform parent = target.parent;
        if (parent == null)
        {
            target.localScale = worldScale;
            return;
        }

        Vector3 parentScale = parent.lossyScale;
        target.localScale = new Vector3(
            SafeDivide(worldScale.x, parentScale.x),
            SafeDivide(worldScale.y, parentScale.y),
            SafeDivide(worldScale.z, parentScale.z)
        );
    }

    private float SafeDivide(float value, float divisor)
    {
        if (Mathf.Abs(divisor) < 0.0001f)
        {
            return value;
        }

        return value / divisor;
    }

    private void ApplyEditorPreviewHideFlags(GameObject target)
    {
        if (Application.isPlaying || target == null) return;

        target.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
    }

    private void DestroyGeneratedObjectsInEditor()
    {
        if (Application.isPlaying) return;

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (child.name.StartsWith(LabelPrefix) || child.name.StartsWith(LinePrefix))
            {
                DestroyImmediate(child.gameObject);
            }
        }

        _labelTransform = null;
        _labelText = null;
        _lineRenderer = null;
    }

    private void DestroyMaterial(Material material)
    {
        if (Application.isPlaying)
        {
            Destroy(material);
        }
        else
        {
            DestroyImmediate(material);
        }
    }
}
