using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DePhobia.Core;

namespace DePhobia.VR
{
    /// <summary>
    /// Horizon Respiration Ring & Safe Escape Affordance.
    /// Anchored at the lower peripheral boundary of the player's field of view.
    /// Dual purpose:
    /// 1. Provides a calming 4.5s respiration guide (gentle expansion/contraction).
    /// 2. Looking directly downward at the ring for 1.5s smoothly triggers safe exit to menu.
    /// Also supports direct mouse click or controller pointer tap.
    /// Completely non-intrusive: zero text clutter, zero jarring popups.
    /// </summary>
    public class VRHorizonRespirationRing : MonoBehaviour
    {
        private static VRHorizonRespirationRing _instance;
        public static VRHorizonRespirationRing Instance => _instance;

        [Header("Settings")]
        public float distanceFromPlayer = 1.5f;
        public float verticalDrop = 0.42f;
        public float gazeHoldDuration = 1.5f;

        private Canvas _canvas;
        private Image _ringOuter;
        private Image _ringCore;
        private RectTransform _ringRect;

        private float _breatheTimer = 0f;
        private float _gazeDwellTimer = 0f;
        private bool _isGazedAt = false;
        private bool _isExiting = false;

        public static VRHorizonRespirationRing EnsureRingInScene()
        {
            var existing = FindObjectOfType<VRHorizonRespirationRing>();
            if (existing != null) return existing;

            var go = new GameObject("VRHorizonRespirationRing");
            return go.AddComponent<VRHorizonRespirationRing>();
        }

        private void Awake()
        {
            _instance = this;
            BuildHorizonCanvas();
        }

        private void LateUpdate()
        {
            if (_isExiting) return;

            // Keep positioned at the lower edge of vision
            Camera cam = Camera.main;
            if (cam != null && _canvas != null)
            {
                Vector3 forward = cam.transform.forward; forward.y = 0;
                if (forward.sqrMagnitude < 0.01f) forward = cam.transform.forward;
                forward.Normalize();

                Vector3 targetPos = cam.transform.position + (forward * distanceFromPlayer) - (Vector3.up * verticalDrop);
                _canvas.transform.position = targetPos;
                _canvas.transform.rotation = Quaternion.LookRotation(_canvas.transform.position - cam.transform.position);

                // Check head pitch looking downward at the ring
                float pitch = cam.transform.eulerAngles.x;
                if (pitch > 180f) pitch -= 360f;
                bool lookingAtRing = (pitch >= 22f && pitch <= 70f);

                if (lookingAtRing || _isGazedAt)
                {
                    HandleGazeDwell();
                }
                else
                {
                    HandleNormalRespiration();
                }
            }
        }

        private void HandleNormalRespiration()
        {
            _gazeDwellTimer = 0f;
            _breatheTimer += Time.deltaTime;

            const float cycleDuration = 4.5f;
            float t = (_breatheTimer % cycleDuration) / cycleDuration;
            // Cosine breathing wave [0..1..0]
            float wave = 0.5f - 0.5f * Mathf.Cos(t * Mathf.PI * 2f);

            float scale = Mathf.Lerp(0.88f, 1.15f, wave);
            if (_ringRect != null)
            {
                _ringRect.localScale = Vector3.one * scale;
            }

            if (_ringOuter != null)
            {
                // Soft matcha sage breathing glow
                float alpha = Mathf.Lerp(0.20f, 0.42f, wave);
                _ringOuter.color = new Color(ThemeTokens.AccentSage.r, ThemeTokens.AccentSage.g, ThemeTokens.AccentSage.b, alpha);
            }

            if (_ringCore != null)
            {
                _ringCore.color = new Color(ThemeTokens.SurfaceBorder.r, ThemeTokens.SurfaceBorder.g, ThemeTokens.SurfaceBorder.b, 0.25f);
            }
        }

        private void HandleGazeDwell()
        {
            _gazeDwellTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(_gazeDwellTimer / gazeHoldDuration);

            // Ring steadily illuminates towards gentle Umber Clay return color
            Color activeColor = Color.Lerp(ThemeTokens.AccentSage, ThemeTokens.GentleExitClay, progress);
            float alpha = Mathf.Lerp(0.45f, 0.95f, progress);

            if (_ringOuter != null)
            {
                _ringOuter.color = new Color(activeColor.r, activeColor.g, activeColor.b, alpha);
            }
            if (_ringRect != null)
            {
                float scale = Mathf.Lerp(1.0f, 1.30f, progress);
                _ringRect.localScale = Vector3.one * scale;
            }

            // Haptic progression
            if (progress > 0.3f && progress < 0.9f)
            {
                var rig = FindObjectOfType<VRPlayerRig>();
                if (rig != null && Time.frameCount % 20 == 0)
                {
                    rig.SendHapticImpulse(0.12f, 0.04f, UnityEngine.XR.XRNode.RightHand);
                }
            }

            if (_gazeDwellTimer >= gazeHoldDuration)
            {
                TriggerSafeExit();
            }
        }

        public void OnPointerEnter()
        {
            _isGazedAt = true;
        }

        public void OnPointerExit()
        {
            _isGazedAt = false;
        }

        public void TriggerSafeExit()
        {
            if (_isExiting) return;
            _isExiting = true;

            var rig = FindObjectOfType<VRPlayerRig>();
            if (rig != null)
            {
                rig.SendHapticImpulse(0.35f, 0.12f, UnityEngine.XR.XRNode.RightHand);
            }

            TherapyAudioManager.Instance.PlayFeedbackClick();
            PatientSessionManager.Instance.EndAndSaveSession(completed: false, extraParams: "User Safe Exit via Horizon Ring");

            if (VRFadeManager.Instance != null)
            {
                VRFadeManager.Instance.FadeAndLoadScene(0, 0.5f);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(0);
            }
        }

        private void BuildHorizonCanvas()
        {
            var canvasObj = new GameObject("HorizonRingCanvas");
            canvasObj.transform.SetParent(transform);

            _canvas = canvasObj.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.WorldSpace;
            _canvas.sortingOrder = 300;

            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 10;
            canvasObj.AddComponent<GraphicRaycaster>();

            var cRect = canvasObj.GetComponent<RectTransform>();
            cRect.sizeDelta = new Vector2(160, 160);
            cRect.localScale = Vector3.one * 0.0022f;

            // Outer ring button
            var ringGo = new GameObject("RingOuter");
            ringGo.transform.SetParent(canvasObj.transform, false);

            _ringRect = ringGo.AddComponent<RectTransform>();
            _ringRect.sizeDelta = new Vector2(70, 70);

            _ringOuter = ringGo.AddComponent<Image>();
            _ringOuter.color = new Color(ThemeTokens.AccentSage.r, ThemeTokens.AccentSage.g, ThemeTokens.AccentSage.b, 0.35f);
            _ringOuter.raycastTarget = true;

            var btn = ringGo.AddComponent<Button>();
            btn.targetGraphic = _ringOuter;
            btn.onClick.AddListener(TriggerSafeExit);

            // Core dot
            var coreGo = new GameObject("RingCore");
            coreGo.transform.SetParent(ringGo.transform, false);
            var coreRect = coreGo.AddComponent<RectTransform>();
            coreRect.sizeDelta = new Vector2(24, 24);
            _ringCore = coreGo.AddComponent<Image>();
            _ringCore.color = new Color(ThemeTokens.SurfaceBorder.r, ThemeTokens.SurfaceBorder.g, ThemeTokens.SurfaceBorder.b, 0.30f);
            _ringCore.raycastTarget = false;
        }
    }
}
