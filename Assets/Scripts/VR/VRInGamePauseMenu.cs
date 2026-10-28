using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using TMPro;
using DePhobia.Core;

namespace DePhobia.VR
{
    /// <summary>
    /// In-game floating pause menu: Strict Minimalist Two-Button Slate.
    /// Styled under Forest Mineral & Sage with zero blue or violet tones.
    /// Focused purely on the user's primary choices:
    /// 1. "Resume Session" (Prominent Matcha Sage)
    /// 2. "Return to Menu" (Gentle Umber Clay)
    /// Recenter and seated toggles remain accessible via Right Thumbstick click or 'R' key.
    /// </summary>
    public class VRInGamePauseMenu : MonoBehaviour
    {
        [Header("References")]
        public Transform vrCamera;
        public GameObject pauseCanvas;
        public TextMeshProUGUI durationText;

        [Header("Settings")]
        public float distanceFromPlayer = 1.8f;
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
                pauseCanvas.SetActive(true);
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
                pauseCanvas.SetActive(false);
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
            rect.sizeDelta = new Vector2(440, 290);
            rect.localScale = Vector3.one * 0.0022f;

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
            var title = ThemeTokens.CreateLabel(panelObj.transform, "Title", "Session Paused", 22, ThemeTokens.TextPrimary, TextAlignmentOptions.Center, true);
            var titleRect = title.GetComponent<RectTransform>();
            titleRect.anchoredPosition = new Vector2(0, 95);
            titleRect.sizeDelta = new Vector2(400, 32);

            // Elapsed Time Subtitle
            durationText = ThemeTokens.CreateLabel(panelObj.transform, "Duration", "Elapsed Time: 0s", 14, ThemeTokens.TextSecondary, TextAlignmentOptions.Center);
            var durRect = durationText.GetComponent<RectTransform>();
            durRect.anchoredPosition = new Vector2(0, 60);
            durRect.sizeDelta = new Vector2(400, 24);

            // Button 1 (Loud & Primary): [ Resume Session ]
            CreateButton(panelObj.transform, "ResumeButton", "Resume Session", new Vector2(0, 5), ThemeTokens.AccentSage, Resume, true);

            // Button 2 (Quiet & Calm): [ Return to Menu ]
            CreateButton(panelObj.transform, "ExitButton", "Return to Menu", new Vector2(0, -60), ThemeTokens.GentleExitClay, SaveAndExitToMenu, false);

            // Quiet Recenter Hint
            var hint = ThemeTokens.CreateLabel(panelObj.transform, "Hint", "Press R or Right Stick to recenter view", 12, ThemeTokens.TextMuted, TextAlignmentOptions.Center);
            var hRect = hint.GetComponent<RectTransform>();
            hRect.anchoredPosition = new Vector2(0, -112);
            hRect.sizeDelta = new Vector2(400, 20);

            pauseCanvas = canvasObj;
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
            btnRect.sizeDelta = new Vector2(340, 48);

            var text = ThemeTokens.CreateLabel(btnObj.transform, "Text", label, 16, ThemeTokens.TextPrimary, TextAlignmentOptions.Center, bold);
            var textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
        }
    }
}
