using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using TMPro;
using DePhobia.Core;

namespace DePhobia.VR
{
    /// <summary>
    /// In-game floating pause menu: Calibrated Compact Minimalist Two-Button Slate.
    /// Styled under Forest Mineral & Sage with zero blue or violet tones.
    /// Arrives and dismisses organically via spring physics (VRSpringCanvas),
    /// sized comfortably for VR (360x230 @ 0.0018 scale at 1.4m),
    /// and provides tactile 3D button depression with physical haptic and acoustic confirmation.
    /// </summary>
    public class VRInGamePauseMenu : MonoBehaviour
    {
        [Header("References")]
        public Transform vrCamera;
        public GameObject pauseCanvas;
        public TextMeshProUGUI durationText;

        [Header("Settings")]
        public float distanceFromPlayer = 1.4f;
        public bool isPaused { get; private set; } = false;

        private bool _menuButtonPressedLastFrame = false;
        private bool _secondaryButtonPressedLastFrame = false;

        private void Start()
        {
            if (vrCamera == null && Camera.main != null)
            {
                vrCamera = Camera.main.transform;
            }

            if (pauseCanvas == null)
            {
                CreatePauseCanvas();
            }
            else
            {
                var sc = VRSpringCanvas.Attach(pauseCanvas);
                sc.SetBaseScale(pauseCanvas.transform.localScale);
                pauseCanvas.SetActive(false);
            }
        }

        private void Update()
        {
            // Universal OpenXR & desktop inputs
            bool toggleRequested = Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P);

            // Left Hand Menu Button or Secondary Button (Y / B)
            var leftHand = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
            if (leftHand.isValid)
            {
                if (leftHand.TryGetFeatureValue(CommonUsages.menuButton, out bool menuPressed))
                {
                    if (menuPressed && !_menuButtonPressedLastFrame) toggleRequested = true;
                    _menuButtonPressedLastFrame = menuPressed;
                }
                if (leftHand.TryGetFeatureValue(CommonUsages.secondaryButton, out bool yPressed))
                {
                    if (yPressed && !_secondaryButtonPressedLastFrame) toggleRequested = true;
                    _secondaryButtonPressedLastFrame = yPressed;
                }
            }

            // Right Hand Secondary Button (B)
            var rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            if (rightHand.isValid && rightHand.TryGetFeatureValue(CommonUsages.secondaryButton, out bool bPressed))
            {
                if (bPressed && !_secondaryButtonPressedLastFrame) toggleRequested = true;
                _secondaryButtonPressedLastFrame = bPressed;
            }

            if (toggleRequested)
            {
                if (isPaused)
                    Resume();
                else
                    Pause();
            }

            if (isPaused && durationText != null)
            {
                float duration = PatientSessionManager.Instance.GetCurrentSessionDuration();
                durationText.text = $"Elapsed Time: {PatientSessionManager.Instance.FormatDuration(duration)}";
            }
        }

        public void Pause()
        {
            isPaused = true;
            Time.timeScale = 0f;

            if (pauseCanvas != null)
            {
                PositionCanvasInFrontOfPlayer();
                var spring = VRSpringCanvas.Attach(pauseCanvas);
                spring.Arrive();
            }

            var rig = FindObjectOfType<VRPlayerRig>();
            if (rig != null)
            {
                rig.SendHapticImpulse(0.25f, 0.08f, XRNode.RightHand);
            }
        }

        public void Resume()
        {
            isPaused = false;
            Time.timeScale = 1f;

            if (pauseCanvas != null)
            {
                var spring = pauseCanvas.GetComponent<VRSpringCanvas>();
                if (spring != null)
                {
                    spring.Dismiss();
                }
                else
                {
                    pauseCanvas.SetActive(false);
                }
            }

            var rig = FindObjectOfType<VRPlayerRig>();
            if (rig != null)
            {
                rig.SendHapticImpulse(0.2f, 0.05f, XRNode.RightHand);
            }
        }

        public void SaveAndExitToMenu()
        {
            Time.timeScale = 1f;
            isPaused = false;

            var rig = FindObjectOfType<VRPlayerRig>();
            if (rig != null)
            {
                rig.SendHapticImpulse(0.35f, 0.12f, XRNode.RightHand);
            }

            PatientSessionManager.Instance.EndAndSaveSession(completed: false, extraParams: "User Returned to Menu via Pause");
            VRFadeManager.Instance.FadeAndLoadScene(0, 0.5f);
        }

        private void PositionCanvasInFrontOfPlayer()
        {
            if (vrCamera == null && Camera.main != null)
            {
                vrCamera = Camera.main.transform;
            }
            if (vrCamera == null) return;

            Vector3 forward = vrCamera.forward;
            forward.y = 0;
            if (forward.sqrMagnitude < 0.01f) forward = Vector3.forward;
            forward.Normalize();

            Vector3 targetPos = vrCamera.position + (forward * distanceFromPlayer);
            targetPos.y = vrCamera.position.y; // Centered at eye level

            pauseCanvas.transform.position = targetPos;
            pauseCanvas.transform.rotation = Quaternion.LookRotation(forward);
        }

        private void CreatePauseCanvas()
        {
            var canvasObj = new GameObject("VRPauseCanvas");
            canvasObj.transform.SetParent(transform);

            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = vrCamera != null ? vrCamera.GetComponent<Camera>() : Camera.main;
            canvas.sortingOrder = 500;

            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 10;
            canvasObj.AddComponent<GraphicRaycaster>();

            var rect = canvasObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(360, 230);
            rect.localScale = Vector3.one * 0.0018f;

            // Background Surface: Forest Mineral & Sage #1E221E (96% opacity)
            var panelObj = new GameObject("Panel");
            panelObj.transform.SetParent(canvasObj.transform, false);
            var panelImg = panelObj.AddComponent<Image>();
            panelImg.color = ThemeTokens.SurfacePanel;
            var panelRect = panelObj.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            // Title: #F3F5F2
            var title = ThemeTokens.CreateLabel(panelObj.transform, "Title", "Session Paused", 18, ThemeTokens.TextPrimary, TextAlignmentOptions.Center, true);
            var titleRect = title.GetComponent<RectTransform>();
            titleRect.anchoredPosition = new Vector2(0, 75);
            titleRect.sizeDelta = new Vector2(340, 26);

            // Elapsed Time Subtitle
            durationText = ThemeTokens.CreateLabel(panelObj.transform, "Duration", "Elapsed Time: 0s", 13, ThemeTokens.TextSecondary, TextAlignmentOptions.Center);
            var durRect = durationText.GetComponent<RectTransform>();
            durRect.anchoredPosition = new Vector2(0, 48);
            durRect.sizeDelta = new Vector2(340, 20);

            // Button 1 (Loud & Primary): [ Resume Session ]
            CreateButton(panelObj.transform, "ResumeButton", "Resume Session", new Vector2(0, 0), ThemeTokens.AccentSage, Resume, true);

            // Button 2 (Quiet & Calm): [ Return to Menu ]
            CreateButton(panelObj.transform, "ExitButton", "Return to Menu", new Vector2(0, -52), ThemeTokens.GentleExitClay, SaveAndExitToMenu, false);

            // Quiet Recenter Hint
            var hint = ThemeTokens.CreateLabel(panelObj.transform, "Hint", "Press R or Right Stick to recenter view", 11, ThemeTokens.TextMuted, TextAlignmentOptions.Center);
            var hRect = hint.GetComponent<RectTransform>();
            hRect.anchoredPosition = new Vector2(0, -92);
            hRect.sizeDelta = new Vector2(340, 18);

            pauseCanvas = canvasObj;
            var sc = VRSpringCanvas.Attach(pauseCanvas);
            sc.SetBaseScale(rect.localScale);
            pauseCanvas.SetActive(false);
        }

        private void CreateButton(Transform parent, string name, string label, Vector2 pos, Color btnColor, UnityEngine.Events.UnityAction action, bool bold)
        {
            var btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);

            var img = btnObj.AddComponent<Image>();
            img.color = btnColor;

            var btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(action);

            var btnRect = btnObj.GetComponent<RectTransform>();
            btnRect.anchoredPosition = pos;
            btnRect.sizeDelta = new Vector2(280, 42);

            // Endow with 3D tactile depression and haptic feedback
            ThemeTokens.StyleButtonColors(btn, btnColor, btnColor * 1.12f, btnColor * 0.88f);

            var text = ThemeTokens.CreateLabel(btnObj.transform, "Text", label, 15, ThemeTokens.TextPrimary, TextAlignmentOptions.Center, bold);
            var textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
        }
    }
}
