// Copyright (c) Meta Platforms, Inc. and affiliates.

using Meta.XR.Samples;
using MRMotifs.SharedAssets;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MRMotifs.PassthroughTransitioning
{
    [MetaCodeSample("MRMotifs-PassthroughTransitioning")]
    public class PassthroughDissolver : MonoBehaviour
    {
        [Tooltip("The range of the passthrough dissolver sphere.")]
        [SerializeField]
        private float distance = 20f;

        [Tooltip("The inverted alpha value at which the contextual boundary should be enabled/disabled.")]
        [SerializeField]
        private float boundaryThreshold = 0.25f;

        [Tooltip("Geschwindigkeit, mit der zwischen VR und Passthrough gewechselt wird.")]
        [SerializeField]
        private float dissolveSpeed = 1.5f;

        private Camera m_mainCamera;
        private Material m_material;
        private MeshRenderer m_meshRenderer;
        private MenuPanel m_menuPanel;
        private Slider m_alphaSlider;
        private OVRPassthroughLayer m_oVRPassthroughLayer;
        private float m_currentDissolveLevel;
        private const float DISSOLVE_TOLERANCE = 0.001f;

        private static readonly int s_dissolutionLevel = Shader.PropertyToID("_Level");

        public bool IsPassthroughActive => m_currentDissolveLevel > boundaryThreshold;

        private void Awake()
        {
            m_mainCamera = Camera.main;
            if (m_mainCamera != null)
            {
                m_mainCamera.clearFlags = CameraClearFlags.Skybox;
            }

            // This is a property that determines whether premultiplied alpha blending is used for the eye field of view
            // layer, which can be adjusted to enhance the blending with underlays and potentially improve visual quality.
            OVRManager.eyeFovPremultipliedAlphaModeEnabled = false;

            m_meshRenderer = GetComponent<MeshRenderer>();
            m_material = m_meshRenderer.material;
            SetDissolveLevel(0);
            m_meshRenderer.enabled = true;
            m_oVRPassthroughLayer = FindAnyObjectByType<OVRPassthroughLayer>();

            SetSphereSize(distance);

            m_menuPanel = FindAnyObjectByType<MenuPanel>();

            if (m_menuPanel != null)
            {
                m_alphaSlider = m_menuPanel.PassthroughFaderSlider;
                if (m_alphaSlider != null)
                {
                    m_alphaSlider.onValueChanged.AddListener(HandleSliderChange);
                }
            }

#if UNITY_ANDROID
            CheckIfPassthroughIsRecommended();
#endif
        }

        private void OnDestroy()
        {
            if (m_alphaSlider != null)
            {
                m_alphaSlider.onValueChanged.RemoveListener(HandleSliderChange);
            }
        }

        private void SetSphereSize(float size)
        {
            transform.localScale = new Vector3(size, size, size);
        }

        private void CheckIfPassthroughIsRecommended()
        {
            SetPassthroughActiveImmediate(OVRManager.IsPassthroughRecommended());

            if (m_alphaSlider != null)
            {
                m_alphaSlider.value = OVRManager.IsPassthroughRecommended() ? 1 : 0;
            }
        }

        private void HandleSliderChange(float value)
        {
            StopAllCoroutines();
            SetDissolveLevel(value);
        }

        public void TogglePassthrough()
        {
            SetPassthroughActive(IsPassthroughActive == false);
        }

        public void SetPassthroughActive(bool isActive)
        {
            StopAllCoroutines();

            if (isActive && m_oVRPassthroughLayer != null)
            {
                m_oVRPassthroughLayer.enabled = true;
            }

            StartCoroutine(DissolveToTarget(isActive ? 1f : 0f));
        }

        public void SetPassthroughActiveImmediate(bool isActive)
        {
            StopAllCoroutines();

            if (m_oVRPassthroughLayer != null)
            {
                m_oVRPassthroughLayer.enabled = isActive;
            }

            SetDissolveLevel(isActive ? 1f : 0f);
        }

        private IEnumerator DissolveToTarget(float targetValue)
        {
            while (Mathf.Abs(m_currentDissolveLevel - targetValue) > DISSOLVE_TOLERANCE)
            {
                var newValue = Mathf.MoveTowards(m_currentDissolveLevel, targetValue, dissolveSpeed * Time.deltaTime);
                SetDissolveLevel(newValue);
                yield return null;
            }

            SetDissolveLevel(targetValue);

            if (Mathf.Approximately(targetValue, 0f) && m_oVRPassthroughLayer != null)
            {
                m_oVRPassthroughLayer.enabled = false;
            }
        }

        private void SetDissolveLevel(float value)
        {
            m_currentDissolveLevel = value;
            m_material.SetFloat(s_dissolutionLevel, value);

            if (OVRManager.instance != null)
            {
                OVRManager.instance.shouldBoundaryVisibilityBeSuppressed = value > boundaryThreshold;
            }
        }
    }
}
