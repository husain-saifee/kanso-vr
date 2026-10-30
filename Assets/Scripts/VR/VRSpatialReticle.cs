using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DePhobia.Core;

namespace DePhobia.VR
{
    /// <summary>
    /// Subtle, organic spatial reticle that provides physical visual anchoring for user gaze and interaction.
    /// Employs micro-damping to absorb head tremor, depth-tests against world UI and colliders,
    /// expands softly on interactive targets, and triggers tactile hover haptics and acoustic ticks.
    /// </summary>
    public class VRSpatialReticle : MonoBehaviour
    {
        [Header("Targeting")]
        public Transform rayOrigin;
        public float defaultDistance = 2.0f;
        public float smoothTime = 0.035f;

        [Header("Reticle Visuals")]
        public float restingDiameter = 0.014f;
        public float hoverDiameter = 0.032f;

        private GameObject _reticleObj;
        private Image _ringImage;
        private Image _dotImage;
        private Canvas _reticleCanvas;

        private Vector3 _currentVelocity;
        private float _currentDiameter;
        private float _diameterVelocity;
        private bool _isHoveringInteractable = false;
        private GameObject _lastHoveredObject = null;

        private void Awake()
        {
            if (rayOrigin == null)
            {
                if (Camera.main != null) rayOrigin = Camera.main.transform;
                else rayOrigin = transform;
            }

            CreateReticleVisuals();
            _currentDiameter = restingDiameter;
        }

        private void CreateReticleVisuals()
        {
            _reticleObj = new GameObject("VRSpatialReticle_Visual");
            _reticleObj.transform.SetParent(transform, false);

            _reticleCanvas = _reticleObj.AddComponent<Canvas>();
            _reticleCanvas.renderMode = RenderMode.WorldSpace;
            _reticleCanvas.sortingOrder = 999;

            var rect = _reticleObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(100, 100);
            rect.localScale = Vector3.one * 0.0003f;

            // 1. Outer Ring (Antialiased circle texture)
            var ringGo = new GameObject("Ring");
            ringGo.transform.SetParent(_reticleObj.transform, false);
            _ringImage = ringGo.AddComponent<Image>();
            _ringImage.sprite = CreateSoftCircleSprite(64, 4);
            _ringImage.color = new Color(ThemeTokens.TextSecondary.r, ThemeTokens.TextSecondary.g, ThemeTokens.TextSecondary.b, 0.55f);
            _ringImage.raycastTarget = false;
            var ringRect = ringGo.GetComponent<RectTransform>();
            ringRect.anchorMin = Vector2.zero;
            ringRect.anchorMax = Vector2.one;
            ringRect.offsetMin = Vector2.zero;
            ringRect.offsetMax = Vector2.zero;

            // 2. Inner Center Focus Dot
            var dotGo = new GameObject("Dot");
            dotGo.transform.SetParent(_reticleObj.transform, false);
            _dotImage = dotGo.AddComponent<Image>();
            _dotImage.sprite = CreateSoftCircleSprite(32, 0);
            _dotImage.color = new Color(ThemeTokens.TextPrimary.r, ThemeTokens.TextPrimary.g, ThemeTokens.TextPrimary.b, 0.75f);
            _dotImage.raycastTarget = false;
            var dotRect = dotGo.GetComponent<RectTransform>();
            dotRect.anchorMin = new Vector2(0.4f, 0.4f);
            dotRect.anchorMax = new Vector2(0.6f, 0.6f);
            dotRect.offsetMin = Vector2.zero;
            dotRect.offsetMax = Vector2.zero;
        }

        private void Update()
        {
            if (rayOrigin == null && Camera.main != null)
            {
                rayOrigin = Camera.main.transform;
            }
            if (rayOrigin == null || _reticleObj == null) return;

            Vector3 rayPos = rayOrigin.position;
            Vector3 rayDir = rayOrigin.forward;

            // Target evaluation: GraphicRaycast on world canvases + Physics Raycast
            Vector3 targetPosition = rayPos + (rayDir * defaultDistance);
            bool hitSomething = false;
            GameObject hitObject = null;

            // 1. Check UI Canvases via EventSystem raycast
            if (EventSystem.current != null)
            {
                var pointerData = new PointerEventData(EventSystem.current)
                {
                    position = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f)
                };
                var results = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointerData, results);

                if (results.Count > 0)
                {
                    for (int i = 0; i < results.Count; i++)
                    {
                        var res = results[i];
                        if (res.gameObject != null && !res.gameObject.transform.IsChildOf(_reticleObj.transform))
                        {
                            targetPosition = res.worldPosition - (rayDir * 0.015f); // pull slightly forward to avoid z-fighting
                            hitSomething = true;
                            hitObject = res.gameObject;
                            break;
                        }
                    }
                }
            }

            // 2. Fallback Physics 3D Raycast
            if (!hitSomething && Physics.Raycast(rayPos, rayDir, out RaycastHit hit, 20f))
            {
                targetPosition = hit.point - (rayDir * 0.015f);
                hitSomething = true;
                hitObject = hit.collider.gameObject;
            }

            // Damped spring position interpolation (absorbs micro-tremor)
            _reticleObj.transform.position = Vector3.SmoothDamp(_reticleObj.transform.position, targetPosition, ref _currentVelocity, smoothTime, Mathf.Infinity, Time.unscaledDeltaTime);
            _reticleObj.transform.rotation = Quaternion.LookRotation(_reticleObj.transform.position - rayPos);

            // Determine if hovering over a clickable / interactable target
            bool isInteractive = hitObject != null && (hitObject.GetComponentInParent<Button>() != null || hitObject.GetComponentInParent<Selectable>() != null || hitObject.GetComponentInParent<VREmergencyExitBeacon>() != null);

            // Trigger hover transition and sensory acknowledgements
            if (isInteractive != _isHoveringInteractable || (isInteractive && hitObject != _lastHoveredObject))
            {
                if (isInteractive)
                {
                    // Fire tactile hover sound and micro-haptic tick
                    if (TherapyAudioManager.HasInstance && TherapyAudioManager.Instance != null)
                    {
                        TherapyAudioManager.Instance.PlayHoverTick();
                    }

                    var rig = FindObjectOfType<VRPlayerRig>();
                    if (rig != null)
                    {
                        rig.SendHapticImpulse(0.08f, 0.02f, UnityEngine.XR.XRNode.RightHand);
                    }
                }
                _isHoveringInteractable = isInteractive;
                _lastHoveredObject = hitObject;
            }

            // Animate reticle diameter & color smoothly
            float targetDiameter = _isHoveringInteractable ? hoverDiameter : restingDiameter;
            _currentDiameter = Mathf.SmoothDamp(_currentDiameter, targetDiameter, ref _diameterVelocity, 0.05f, Mathf.Infinity, Time.unscaledDeltaTime);

            float distanceToCamera = Vector3.Distance(_reticleObj.transform.position, rayPos);
            float scaleFactor = (_currentDiameter / 100f) * (distanceToCamera / 2.0f);
            _reticleObj.transform.localScale = Vector3.one * Mathf.Max(scaleFactor, 0.0001f);

            // Color response
            Color targetRingColor = _isHoveringInteractable
                ? ThemeTokens.AccentSage
                : new Color(ThemeTokens.TextSecondary.r, ThemeTokens.TextSecondary.g, ThemeTokens.TextSecondary.b, 0.45f);

            if (_ringImage != null) _ringImage.color = Color.Lerp(_ringImage.color, targetRingColor, 12f * Time.unscaledDeltaTime);
        }

        private Sprite CreateSoftCircleSprite(int size, int borderThickness)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            float center = size * 0.5f;
            float radius = center - 2f;
            float innerRadius = (borderThickness > 0) ? (radius - borderThickness) : 0f;

            Color[] colors = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(center, center));
                    float alpha = 0f;

                    if (borderThickness > 0)
                    {
                        // Ring shape
                        if (dist <= radius && dist >= innerRadius)
                        {
                            float edgeOuter = Mathf.Clamp01(radius - dist);
                            float edgeInner = Mathf.Clamp01(dist - innerRadius);
                            alpha = Mathf.Min(edgeOuter, edgeInner);
                        }
                    }
                    else
                    {
                        // Filled circle
                        alpha = Mathf.Clamp01(radius - dist);
                    }

                    colors[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            tex.SetPixels(colors);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }
    }
}
