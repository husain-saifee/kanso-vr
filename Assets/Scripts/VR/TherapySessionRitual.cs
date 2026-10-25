using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DePhobia.Core;

namespace DePhobia.VR
{
    /// <summary>
    /// Implements gentle Start and End rituals for exposure therapy sessions.
    /// Fosters containment, emotional safety, predictable transitions, and deliberate breathing.
    /// Supports direct mouse clicks (ScreenSpaceOverlay on Desktop), keyboard shortcuts (Space/Enter to begin, Esc to exit),
    /// and VR controller point-and-click.
    /// </summary>
    public class TherapySessionRitual : MonoBehaviour
    {
        private Action _onStartCompleted;
        private Action _onEndCompleted;
        private GameObject _ritualCanvas;
        private TextMeshProUGUI _breathingText;
        private Image _breathingRing;
        private bool _isRitualActive = false;
        private bool _isEndRitual = false;

        private RectTransform _beginBtnRect;
        private RectTransform _exitBtnRect;
        private RectTransform _endReturnBtnRect;
        private RectTransform _endStayBtnRect;

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

            CreateStartCanvas(phobiaName);
            StartCoroutine(BreathingCycleRoutine());
        }

        private void InitializeEndRitual(string phobiaName, string summaryMetrics, Action onReturnToMenu)
        {
            _onEndCompleted = onReturnToMenu;
            _isRitualActive = true;
            _isEndRitual = true;
            CreateEndCanvas(phobiaName, summaryMetrics, onReturnToMenu);
        }

        private void Update()
        {
            if (!_isRitualActive) return;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Direct mouse hit-testing fallback: guarantees mouse click always works regardless of input module
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 mousePos = Input.mousePosition;
                var canvas = _ritualCanvas != null ? _ritualCanvas.GetComponent<Canvas>() : null;
                Camera uiCam = (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceCamera) ? canvas.worldCamera : null;

                if (!_isEndRitual)
                {
                    if (_beginBtnRect != null && RectTransformUtility.RectangleContainsScreenPoint(_beginBtnRect, mousePos, uiCam))
                    {
                        OnBeginClicked();
                        return;
                    }
                    if (_exitBtnRect != null && RectTransformUtility.RectangleContainsScreenPoint(_exitBtnRect, mousePos, uiCam))
                    {
                        OnSafeExitClicked();
                        return;
                    }
                }
                else
                {
                    if (_endReturnBtnRect != null && RectTransformUtility.RectangleContainsScreenPoint(_endReturnBtnRect, mousePos, uiCam))
                    {
                        OnEndReturnClicked();
                        return;
                    }
                    if (_endStayBtnRect != null && RectTransformUtility.RectangleContainsScreenPoint(_endStayBtnRect, mousePos, uiCam))
                    {
                        OnEndStayClicked();
                        return;
                    }
                }
            }

            // Global Keyboard / Controller shortcuts for immediate accessibility
            if (!_isEndRitual)
            {
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    OnBeginClicked();
                }
                else if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Backspace))
                {
                    OnSafeExitClicked();
                }
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Escape))
                {
                    OnEndReturnClicked();
                }
            }
        }

        private IEnumerator BreathingCycleRoutine()
        {
            float timer = 0f;
            const float cycleDuration = 4.5f;

            while (_isRitualActive)
            {
                timer += Time.deltaTime;
                float t = (timer % cycleDuration) / cycleDuration;
                // Cosine breathing wave [0..1..0]
                float wave = 0.5f - 0.5f * Mathf.Cos(t * Mathf.PI * 2f);

                if (_breathingRing != null)
                {
                    float scale = Mathf.Lerp(0.85f, 1.25f, wave);
                    _breathingRing.transform.localScale = Vector3.one * scale;
                }

                if (_breathingText != null)
                {
                    _breathingText.text = (wave > 0.5f) ? "Gently breathe in..." : "Slowly breathe out...";
                }

                yield return null;
            }
        }

        private void OnBeginClicked()
        {
            if (!_isRitualActive) return;
            _isRitualActive = false;
            StopAllCoroutines();

            TherapyAudioManager.Instance.PlayFeedbackClick();
            var rig = FindObjectOfType<VRPlayerRig>();
            if (rig != null)
            {
                rig.SendHapticImpulse(0.2f, 0.06f, UnityEngine.XR.XRNode.RightHand);
            }

            if (_ritualCanvas != null)
            {
                Destroy(_ritualCanvas);
            }

            _onStartCompleted?.Invoke();
            Destroy(gameObject, 0.5f);
        }

        private void OnSafeExitClicked()
        {
            if (!_isRitualActive) return;
            _isRitualActive = false;
            StopAllCoroutines();

            TherapyAudioManager.Instance.PlayFeedbackClick();
            PatientSessionManager.Instance.EndAndSaveSession(completed: false, extraParams: "User Exited During Start Ritual");
            VRFadeManager.Instance.FadeAndLoadScene(0, 0.4f);
            Destroy(gameObject, 0.5f);
        }

        private void OnEndReturnClicked()
        {
            if (!_isRitualActive) return;
            _isRitualActive = false;

            TherapyAudioManager.Instance.PlayFeedbackClick();
            _onEndCompleted?.Invoke();
            VRFadeManager.Instance.FadeAndLoadScene(0, 0.5f);
            Destroy(gameObject, 0.5f);
        }

        private void OnEndStayClicked()
        {
            _isRitualActive = false;
            TherapyAudioManager.Instance.PlayFeedbackClick();
            if (_ritualCanvas != null) Destroy(_ritualCanvas);
            Destroy(gameObject, 0.5f);
        }

        private void CreateStartCanvas(string phobiaName)
        {
            _ritualCanvas = new GameObject("StartRitualCanvas");
            _ritualCanvas.transform.SetParent(transform);

            var canvas = _ritualCanvas.AddComponent<Canvas>();
            if (UnityEngine.XR.XRSettings.isDeviceActive)
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                Camera cam = Camera.main != null ? Camera.main : FindObjectOfType<Camera>();
                canvas.worldCamera = cam;
                canvas.planeDistance = 1.2f;
            }
            else
            {
                // In desktop mode, ScreenSpaceOverlay guarantees mouse coordinates align perfectly with 0 camera artifacts
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }
            canvas.sortingOrder = 999;

            var scaler = _ritualCanvas.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            _ritualCanvas.AddComponent<GraphicRaycaster>();

            // Background Surface: #242338 (95% opacity)
            var panelObj = new GameObject("Panel");
            panelObj.transform.SetParent(_ritualCanvas.transform, false);
            var panelImg = panelObj.AddComponent<Image>();
            panelImg.color = new Color(0.141f, 0.137f, 0.220f, 0.95f);
            panelImg.raycastTarget = false; // Do not intercept clicks destined for buttons
            var panelRect = panelObj.GetComponent<RectTransform>();
            panelRect.sizeDelta = new Vector2(560, 420);

            // Title: #F4F3F8
            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(panelObj.transform, false);
            var titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = $"Welcome to {phobiaName}";
            titleText.fontSize = 26;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = new Color(0.957f, 0.953f, 0.973f, 1f);
            titleText.raycastTarget = false;
            var titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchoredPosition = new Vector2(0, 150);
            titleRect.sizeDelta = new Vector2(500, 40);

            // Instructions: #B6B3CE
            var instObj = new GameObject("Instructions");
            instObj.transform.SetParent(panelObj.transform, false);
            var instText = instObj.AddComponent<TextMeshProUGUI>();
            instText.text = "Settle into a comfortable position.\nYou are in complete control at all times.";
            instText.fontSize = 15;
            instText.alignment = TextAlignmentOptions.Center;
            instText.color = new Color(0.714f, 0.702f, 0.808f, 1f);
            instText.raycastTarget = false;
            var instRect = instObj.GetComponent<RectTransform>();
            instRect.anchoredPosition = new Vector2(0, 105);
            instRect.sizeDelta = new Vector2(500, 40);

            // Breathing Ring Visualizer: #9B98E6 (Lavender)
            var ringObj = new GameObject("BreathingRing");
            ringObj.transform.SetParent(panelObj.transform, false);
            _breathingRing = ringObj.AddComponent<Image>();
            _breathingRing.color = new Color(0.608f, 0.596f, 0.902f, 0.45f);
            _breathingRing.raycastTarget = false;
            var ringRect = ringObj.GetComponent<RectTransform>();
            ringRect.anchoredPosition = new Vector2(0, 25);
            ringRect.sizeDelta = new Vector2(75, 75);

            // Breathing prompt: #E8B89E (Dawn Amber)
            var breathTextObj = new GameObject("BreathingText");
            breathTextObj.transform.SetParent(panelObj.transform, false);
            _breathingText = breathTextObj.AddComponent<TextMeshProUGUI>();
            _breathingText.text = "Gently breathe in...";
            _breathingText.fontSize = 16;
            _breathingText.alignment = TextAlignmentOptions.Center;
            _breathingText.color = new Color(0.910f, 0.722f, 0.620f, 1f);
            _breathingText.raycastTarget = false;
            var breathRect = breathTextObj.GetComponent<RectTransform>();
            breathRect.anchoredPosition = new Vector2(0, -40);
            breathRect.sizeDelta = new Vector2(480, 30);

            // Begin Button: #7E9F94 (Serene)
            _beginBtnRect = CreateButton(panelObj.transform, "BeginButton", "Begin Session  [ Click / Space ]", new Vector2(0, -105), new Color(0.494f, 0.624f, 0.580f, 1f), OnBeginClicked);

            // Safe Exit Button: #D48D8D (Gentle exit)
            _exitBtnRect = CreateButton(panelObj.transform, "ExitButton", "Return to Menu  [ Click / Esc ]", new Vector2(0, -165), new Color(0.831f, 0.553f, 0.553f, 1f), OnSafeExitClicked);
        }

        private void CreateEndCanvas(string phobiaName, string summaryMetrics, Action onReturnToMenu)
        {
            _ritualCanvas = new GameObject("EndRitualCanvas");
            _ritualCanvas.transform.SetParent(transform);

            var canvas = _ritualCanvas.AddComponent<Canvas>();
            if (UnityEngine.XR.XRSettings.isDeviceActive)
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                Camera cam = Camera.main != null ? Camera.main : FindObjectOfType<Camera>();
                canvas.worldCamera = cam;
                canvas.planeDistance = 1.2f;
            }
            else
            {
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }
            canvas.sortingOrder = 999;

            var scaler = _ritualCanvas.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            _ritualCanvas.AddComponent<GraphicRaycaster>();

            // Background Surface: #242338 (95% opacity)
            var panelObj = new GameObject("Panel");
            panelObj.transform.SetParent(_ritualCanvas.transform, false);
            var panelImg = panelObj.AddComponent<Image>();
            panelImg.color = new Color(0.141f, 0.137f, 0.220f, 0.95f);
            panelImg.raycastTarget = false;
            var panelRect = panelObj.GetComponent<RectTransform>();
            panelRect.sizeDelta = new Vector2(560, 420);

            // Title: #F4F3F8
            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(panelObj.transform, false);
            var titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "Session Completed Gently";
            titleText.fontSize = 26;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = new Color(0.957f, 0.953f, 0.973f, 1f);
            titleText.raycastTarget = false;
            var titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchoredPosition = new Vector2(0, 140);
            titleRect.sizeDelta = new Vector2(500, 40);

            var instObj = new GameObject("Instructions");
            instObj.transform.SetParent(panelObj.transform, false);
            var instText = instObj.AddComponent<TextMeshProUGUI>();
            instText.text = "Take a peaceful moment to breathe.\nWhen you are ready, you may return safely.";
            instText.fontSize = 15;
            instText.alignment = TextAlignmentOptions.Center;
            instText.color = new Color(0.714f, 0.702f, 0.808f, 1f);
            instText.raycastTarget = false;
            var instRect = instObj.GetComponent<RectTransform>();
            instRect.anchoredPosition = new Vector2(0, 85);
            instRect.sizeDelta = new Vector2(500, 40);

            var metricsObj = new GameObject("Metrics");
            metricsObj.transform.SetParent(panelObj.transform, false);
            var metricsText = metricsObj.AddComponent<TextMeshProUGUI>();
            metricsText.text = summaryMetrics;
            metricsText.fontSize = 16;
            metricsText.alignment = TextAlignmentOptions.Center;
            metricsText.color = new Color(0.910f, 0.722f, 0.620f, 1f);
            metricsText.raycastTarget = false;
            var metricsRect = metricsObj.GetComponent<RectTransform>();
            metricsRect.anchoredPosition = new Vector2(0, 25);
            metricsRect.sizeDelta = new Vector2(480, 35);

            _endReturnBtnRect = CreateButton(panelObj.transform, "ReturnButton", "Save & Return to Menu  [ Click / Esc ]", new Vector2(0, -65), new Color(0.494f, 0.624f, 0.580f, 1f), OnEndReturnClicked);

            _endStayBtnRect = CreateButton(panelObj.transform, "StayButton", "Remain Here & Breathe", new Vector2(0, -135), new Color(0.227f, 0.220f, 0.329f, 0.95f), OnEndStayClicked);
        }

        private RectTransform CreateButton(Transform parent, string name, string label, Vector2 pos, Color btnColor, UnityEngine.Events.UnityAction action)
        {
            var btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);

            var img = btnObj.AddComponent<Image>();
            img.color = btnColor;
            img.raycastTarget = true;

            var btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(action);

            var btnRect = btnObj.GetComponent<RectTransform>();
            btnRect.anchoredPosition = pos;
            btnRect.sizeDelta = new Vector2(400, 48);

            var textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            var text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = label;
            text.fontSize = 16;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.color = new Color(0.957f, 0.953f, 0.973f, 1f);
            text.raycastTarget = false; // Allow clicks to pass directly to Button Image

            var textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero; textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero; textRect.offsetMax = Vector2.zero;

            return btnRect;
        }
    }
}
