using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DePhobia.Core;

namespace DePhobia.VR
{
    /// <summary>
    /// In-game floating pause menu and patient safety exit controller.
    /// Replaces the legacy dependency on Android hardware Back button (KeyCode.Escape).
    /// </summary>
    public class VRInGamePauseMenu : MonoBehaviour
    {
        [Header("References")]
        public Transform vrCamera;
        public GameObject pauseCanvas;
        public TextMeshProUGUI durationText;
        public TextMeshProUGUI statusText;

        [Header("Settings")]
        public float distanceFromPlayer = 1.8f;
        public bool isPaused { get; private set; } = false;

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
            // Toggle pause menu via Controller Menu Button, Keyboard Escape, or P key
            bool toggleRequested = Input.GetKeyDown(KeyCode.Escape) ||
                                  Input.GetKeyDown(KeyCode.P) ||
                                  Input.GetKeyDown(KeyCode.JoystickButton7); // Common VR controller menu button

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
                durationText.text = $"Session Duration: {PatientSessionManager.Instance.FormatDuration(duration)}";
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
        }

        public void Resume()
        {
            isPaused = false;
            Time.timeScale = 1f;

            if (pauseCanvas != null)
            {
                pauseCanvas.SetActive(false);
            }
        }

        public void SaveAndExitToMenu()
        {
            Time.timeScale = 1f;
            isPaused = false;

            // Save the session metrics
            PatientSessionManager.Instance.EndAndSaveSession(completed: false, extraParams: "User Exited via Pause Menu");

            // Smooth fade to black and load main menu (Index 0)
            VRFadeManager.Instance.FadeAndLoadScene(0, 0.5f);
        }

        private void PositionCanvasInFrontOfPlayer()
        {
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
            canvas.sortingOrder = 100;

            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 10;

            canvasObj.AddComponent<GraphicRaycaster>();

            var rect = canvasObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(400, 300);
            rect.localScale = Vector3.one * 0.003f; // Scale down for VR world space

            // Background panel
            var panelObj = new GameObject("Panel");
            panelObj.transform.SetParent(canvasObj.transform, false);
            var panelImg = panelObj.AddComponent<Image>();
            panelImg.color = new Color(0.1f, 0.12f, 0.16f, 0.95f);
            var panelRect = panelObj.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            // Title
            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(panelObj.transform, false);
            var titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "Session Paused";
            titleText.fontSize = 28;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = Color.white;
            var titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchoredPosition = new Vector2(0, 90);
            titleRect.sizeDelta = new Vector2(360, 40);

            // Duration label
            var durObj = new GameObject("Duration");
            durObj.transform.SetParent(panelObj.transform, false);
            durationText = durObj.AddComponent<TextMeshProUGUI>();
            durationText.text = "Session Duration: 0s";
            durationText.fontSize = 18;
            durationText.alignment = TextAlignmentOptions.Center;
            durationText.color = new Color(0.8f, 0.85f, 0.9f);
            var durRect = durObj.GetComponent<RectTransform>();
            durRect.anchoredPosition = new Vector2(0, 45);
            durRect.sizeDelta = new Vector2(360, 30);

            // Resume Button
            CreateButton(panelObj.transform, "ResumeButton", "Resume Session", new Vector2(0, -10), new Color(0.15f, 0.5f, 0.3f), Resume);

            // Save & Exit Button
            CreateButton(panelObj.transform, "ExitButton", "Save & Return to Menu", new Vector2(0, -75), new Color(0.7f, 0.2f, 0.2f), SaveAndExitToMenu);

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
            btnRect.sizeDelta = new Vector2(280, 45);

            var textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            var text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = label;
            text.fontSize = 18;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;

            var textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
        }
    }
}
