using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using TMPro;
using DePhobia.Core;

namespace DePhobia.VR
{
    /// <summary>
    /// In-game floating pause menu and patient safety controller.
    /// Styled under Direction C (Soft & Abstract Horizon) with non-shaming,
    /// supportive copy and accessible comfort quick-toggles.
    /// </summary>
    public class VRInGamePauseMenu : MonoBehaviour
    {
        [Header("References")]
        public Transform vrCamera;
        public GameObject pauseCanvas;
        public TextMeshProUGUI durationText;
        public TextMeshProUGUI subtitleText;

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
            // Check universal OpenXR inputs
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

        public void Recenter()
        {
            var rig = FindObjectOfType<VRPlayerRig>();
            if (rig != null)
            {
                rig.RecenterView();
                PositionCanvasInFrontOfPlayer();
                rig.SendHapticImpulse(0.3f, 0.1f, XRNode.RightHand);
            }
        }

        public void ToggleSeatedMode()
        {
            var sm = TherapySettingsManager.Instance;
            if (sm != null)
            {
                sm.SetSeatedMode(!sm.seatedMode);
                Recenter();
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
            targetPos.y = vrCamera.position.y; // Eye level

            pauseCanvas.transform.position = targetPos;
            pauseCanvas.transform.rotation = Quaternion.LookRotation(forward);
        }

        private void CreatePauseCanvas()
        {
            var canvasObj = new GameObject("VRPauseCanvas");
            canvasObj.transform.SetParent(transform);

            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 500;

            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 10;

            canvasObj.AddComponent<GraphicRaycaster>();

            var rect = canvasObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(460, 360);
            rect.localScale = Vector3.one * 0.0022f; // Calibrated for VR eye-level sharpness

            // Direction C Background Surface: #242338 (95% opacity)
            var panelObj = new GameObject("Panel");
            panelObj.transform.SetParent(canvasObj.transform, false);
            var panelImg = panelObj.AddComponent<Image>();
            panelImg.color = new Color(0.141f, 0.137f, 0.220f, 0.95f);
            var panelRect = panelObj.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            // Title: #F4F3F8
            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(panelObj.transform, false);
            var titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "Session Paused";
            titleText.fontSize = 24;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = new Color(0.957f, 0.953f, 0.973f, 1f);
            var titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchoredPosition = new Vector2(0, 125);
            titleRect.sizeDelta = new Vector2(400, 35);

            // Subtitle: #B6B3CE
            var subObj = new GameObject("Subtitle");
            subObj.transform.SetParent(panelObj.transform, false);
            subtitleText = subObj.AddComponent<TextMeshProUGUI>();
            subtitleText.text = "You are in control. Take all the time you need.";
            subtitleText.fontSize = 14;
            subtitleText.alignment = TextAlignmentOptions.Center;
            subtitleText.color = new Color(0.714f, 0.702f, 0.808f, 1f);
            var subRect = subObj.GetComponent<RectTransform>();
            subRect.anchoredPosition = new Vector2(0, 90);
            subRect.sizeDelta = new Vector2(400, 25);

            // Duration: #E8B89E (Dawn Amber gentle metric)
            var durObj = new GameObject("Duration");
            durObj.transform.SetParent(panelObj.transform, false);
            durationText = durObj.AddComponent<TextMeshProUGUI>();
            durationText.text = "Elapsed Time: 0s";
            durationText.fontSize = 15;
            durationText.alignment = TextAlignmentOptions.Center;
            durationText.color = new Color(0.910f, 0.722f, 0.620f, 1f);
            var durRect = durObj.GetComponent<RectTransform>();
            durRect.anchoredPosition = new Vector2(0, 55);
            durRect.sizeDelta = new Vector2(400, 25);

            // Button 1: Resume Session (StateSerene #7E9F94)
            CreateButton(panelObj.transform, "ResumeButton", "Resume Session", new Vector2(0, 10), new Color(0.494f, 0.624f, 0.580f, 1f), Resume);

            // Button 2: Recenter View (SurfaceBorder #3A3854)
            CreateButton(panelObj.transform, "RecenterButton", "Recenter View", new Vector2(0, -45), new Color(0.227f, 0.220f, 0.329f, 0.95f), Recenter);

            // Button 3: Toggle Seated/Standing Mode (SurfaceBorder #3A3854)
            CreateButton(panelObj.transform, "SeatedButton", "Toggle Seated / Standing", new Vector2(0, -100), new Color(0.227f, 0.220f, 0.329f, 0.95f), ToggleSeatedMode);

            // Button 4: Save & Return to Menu (StateGentleExit #D48D8D, soft rose, never harsh red)
            CreateButton(panelObj.transform, "ExitButton", "Save & Return to Menu", new Vector2(0, -155), new Color(0.831f, 0.553f, 0.553f, 1f), SaveAndExitToMenu);

            pauseCanvas = canvasObj;
            pauseCanvas.SetActive(false);
        }

        private void CreateButton(Transform parent, string name, string label, Vector2 pos, Color btnColor, UnityEngine.Events.UnityAction action)
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
            btnRect.sizeDelta = new Vector2(340, 42);

            var textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            var text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = label;
            text.fontSize = 15;
            text.alignment = TextAlignmentOptions.Center;
            text.color = new Color(0.957f, 0.953f, 0.973f, 1f);

            var textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
        }
    }
}
