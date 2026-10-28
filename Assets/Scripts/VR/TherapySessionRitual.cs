using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DePhobia.Core;

namespace DePhobia.VR
{
    /// <summary>
    /// Manages the Ambient Horizon Arrival (Start Ritual) and Clean Milestone Closure (End Ritual).
    /// Styled under Forest Mineral & Sage with zero blue or violet tones.
    /// Start Ritual: Non-blocking; provides 3-breath ambient pacing and starts on gaze forward or tap.
    /// End Ritual: Smooth fade to a clean milestone closure card with duration and metrics.
    /// </summary>
    public class TherapySessionRitual : MonoBehaviour
    {
        private Action _onStartCompleted;
        private Action _onEndCompleted;
        private GameObject _ritualCanvas;
        private bool _isRitualActive = false;
        private bool _isEndRitual = false;

        public static TherapySessionRitual ShowStartRitual(string phobiaName, Action onReadyToBegin)
        {
            var go = new GameObject("TherapySessionRitual");
            var ritual = go.AddComponent<TherapySessionRitual>();
            ritual.InitializeStartRitual(phobiaName, onReadyToBegin);
            return ritual;
        }

        public static TherapySessionRitual ShowEndRitual(string phobiaName, string summaryMetrics, Action onReturnToMenu)
        {
            var go = new GameObject("TherapySessionRitual");
            var ritual = go.AddComponent<TherapySessionRitual>();
            ritual.InitializeEndRitual(phobiaName, summaryMetrics, onReturnToMenu);
            return ritual;
        }

        private void InitializeStartRitual(string phobiaName, Action onReadyToBegin)
        {
            _onStartCompleted = onReadyToBegin;
            _isRitualActive = true;
            _isEndRitual = false;


            // Settle sequence: 3 breaths (approx 5-6s) or immediate press of Space/Enter/Click
            StartCoroutine(StartRitualPacingRoutine());
        }

        private void InitializeEndRitual(string phobiaName, string summaryMetrics, Action onReturnToMenu)
        {
            _onEndCompleted = onReturnToMenu;
            _isRitualActive = true;
            _isEndRitual = true;

            // Immediately fade to black, then show clean milestone card
            if (VRFadeManager.Instance != null)
            {
                VRFadeManager.Instance.FadeOut(0.5f, () =>
                {
                    CreateEndSummaryCanvas(phobiaName, summaryMetrics);
                    VRFadeManager.Instance.FadeIn(0.4f);
                });
            }
            else
            {
                CreateEndSummaryCanvas(phobiaName, summaryMetrics);
            }
        }

        private IEnumerator StartRitualPacingRoutine()
        {
            float timer = 0f;
            const float settleWindow = 5.0f;

            while (timer < settleWindow && _isRitualActive)
            {
                timer += Time.deltaTime;

                // Any user input starts immediately
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0))
                {
                    break;
                }

                yield return null;
            }

            CompleteStartRitual();
        }

        private void CompleteStartRitual()
        {
            if (!_isRitualActive) return;
            _isRitualActive = false;

            var rig = FindObjectOfType<VRPlayerRig>();
            if (rig != null)
            {
                rig.SendHapticImpulse(0.2f, 0.06f, UnityEngine.XR.XRNode.RightHand);
            }

            _onStartCompleted?.Invoke();
            Destroy(gameObject, 0.5f);
        }

        private void Update()
        {
            if (!_isRitualActive) return;

            if (_isEndRitual)
            {
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(0))
                {
                    OnEndReturnClicked();
                }
            }
        }

        private void OnEndReturnClicked()
        {
            if (!_isRitualActive) return;
            _isRitualActive = false;

            TherapyAudioManager.Instance.PlayFeedbackClick();
            _onEndCompleted?.Invoke();

            if (VRFadeManager.Instance != null)
            {
                VRFadeManager.Instance.FadeAndLoadScene(0, 0.5f);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(0);
            }

            Destroy(gameObject, 0.5f);
        }

        private void CreateEndSummaryCanvas(string phobiaName, string summaryMetrics)
        {
            _ritualCanvas = new GameObject("EndSummaryCanvas");
            _ritualCanvas.transform.SetParent(transform);

            var canvas = _ritualCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            Camera cam = Camera.main;
            canvas.worldCamera = cam;
            canvas.sortingOrder = 999;

            var scaler = _ritualCanvas.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 10;
            _ritualCanvas.AddComponent<GraphicRaycaster>();

            var cRect = _ritualCanvas.GetComponent<RectTransform>();
            cRect.sizeDelta = new Vector2(500, 340);
            cRect.localScale = Vector3.one * 0.0022f;

            if (cam != null)
            {
                Vector3 forward = cam.transform.forward; forward.y = 0;
                if (forward.sqrMagnitude < 0.01f) forward = Vector3.forward;
                forward.Normalize();
                _ritualCanvas.transform.position = cam.transform.position + (forward * 1.8f);
                _ritualCanvas.transform.rotation = Quaternion.LookRotation(forward);
            }

            // Background Surface: Forest Mineral & Sage
            var panelObj = new GameObject("Panel");
            panelObj.transform.SetParent(_ritualCanvas.transform, false);
            var panelImg = panelObj.AddComponent<Image>();
            panelImg.color = ThemeTokens.SurfacePanel;
            var panelRect = panelObj.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero; panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero; panelRect.offsetMax = Vector2.zero;

            // Title: #F3F5F2
            var title = ThemeTokens.CreateLabel(panelObj.transform, "Title", "Session Completed", 22, ThemeTokens.TextPrimary, TextAlignmentOptions.Center, true);
            var titleRect = title.GetComponent<RectTransform>();
            titleRect.anchoredPosition = new Vector2(0, 110);
            titleRect.sizeDelta = new Vector2(440, 32);

            // Subtitle: #9AA299
            var sub = ThemeTokens.CreateLabel(panelObj.transform, "Subtitle", "Take a quiet moment to breathe. When you are ready, return safely.", 14, ThemeTokens.TextSecondary, TextAlignmentOptions.Center);
            var subRect = sub.GetComponent<RectTransform>();
            subRect.anchoredPosition = new Vector2(0, 65);
            subRect.sizeDelta = new Vector2(440, 36);

            // Summary Metrics readout
            var metrics = ThemeTokens.CreateLabel(panelObj.transform, "Metrics", summaryMetrics, 15, ThemeTokens.AccentSage, TextAlignmentOptions.Center, true);
            var mRect = metrics.GetComponent<RectTransform>();
            mRect.anchoredPosition = new Vector2(0, 10);
            mRect.sizeDelta = new Vector2(440, 32);

            // Button: [ Return to Sanctuary ]
            var btnObj = new GameObject("ReturnButton");
            btnObj.transform.SetParent(panelObj.transform, false);
            var btnImg = btnObj.AddComponent<Image>();
            btnImg.color = ThemeTokens.AccentSage;
            var btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = btnImg;
            btn.onClick.AddListener(OnEndReturnClicked);

            var btnRect = btnObj.GetComponent<RectTransform>();
            btnRect.anchoredPosition = new Vector2(0, -65);
            btnRect.sizeDelta = new Vector2(360, 48);

            var btnText = ThemeTokens.CreateLabel(btnObj.transform, "Text", "Return to Sanctuary", 16, ThemeTokens.TextPrimary, TextAlignmentOptions.Center, true);
            var btRect = btnText.GetComponent<RectTransform>();
            btRect.anchorMin = Vector2.zero; btRect.anchorMax = Vector2.one;
            btRect.offsetMin = Vector2.zero; btRect.offsetMax = Vector2.zero;
        }
    }
}
