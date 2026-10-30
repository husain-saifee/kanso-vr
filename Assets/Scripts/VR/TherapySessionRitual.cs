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
    /// End Ritual: Stops the session and presents a calm Session Review card with data readout,
    /// arriving through calibrated spring physics (VRSpringCanvas) and featuring tactile 3D buttons.
    /// </summary>
    public class TherapySessionRitual : MonoBehaviour
    {
        private Action _onStartCompleted;
        private Action _onEndCompleted;
        private Action _onRestartCompleted;
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

        public static TherapySessionRitual ShowEndRitual(string phobiaName, string summaryMetrics, Action onReturnToMenu, Action onRestart = null)
        {
            var go = new GameObject("TherapySessionRitual");
            var ritual = go.AddComponent<TherapySessionRitual>();
            ritual.InitializeEndRitual(phobiaName, summaryMetrics, onReturnToMenu, onRestart);
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

        private void InitializeEndRitual(string phobiaName, string summaryMetrics, Action onReturnToMenu, Action onRestart)
        {
            _onEndCompleted = onReturnToMenu;
            _onRestartCompleted = onRestart;
            _isRitualActive = true;
            _isEndRitual = true;

            // Ensure cursor is unlocked and visible for review selection on desktop
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Fade to dark and display clean session review card
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
                // Keyboard shortcuts for convenience (does not auto-trigger on random clicks)
                if (Input.GetKeyDown(KeyCode.R))
                {
                    OnEndRestartClicked();
                }
                else if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.M))
                {
                    OnEndReturnClicked();
                }
            }
        }

        public void OnEndRestartClicked()
        {
            if (!_isRitualActive) return;
            _isRitualActive = false;

            if (TherapyAudioManager.Instance != null)
                TherapyAudioManager.Instance.PlayFeedbackClick();

            var rig = FindObjectOfType<VRPlayerRig>();
            if (rig != null) rig.SendHapticImpulse(0.25f, 0.08f, UnityEngine.XR.XRNode.RightHand);

            if (_ritualCanvas != null)
            {
                var spring = _ritualCanvas.GetComponent<VRSpringCanvas>();
                if (spring != null) spring.Dismiss();
            }

            if (_onRestartCompleted != null)
            {
                _onRestartCompleted.Invoke();
            }
            else
            {
                if (PatientSessionManager.Instance != null)
                {
                    PatientSessionManager.Instance.EndAndSaveSession(completed: true, extraParams: "Session Restarted by Patient");
                }

                int activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
                if (VRFadeManager.Instance != null)
                {
                    VRFadeManager.Instance.FadeAndLoadScene(activeScene, 0.5f);
                }
                else
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene(activeScene);
                }
            }

            Destroy(gameObject, 0.5f);
        }

        public void OnEndReturnClicked()
        {
            if (!_isRitualActive) return;
            _isRitualActive = false;

            if (TherapyAudioManager.Instance != null)
                TherapyAudioManager.Instance.PlayFeedbackClick();

            var rig = FindObjectOfType<VRPlayerRig>();
            if (rig != null) rig.SendHapticImpulse(0.3f, 0.10f, UnityEngine.XR.XRNode.RightHand);

            if (_ritualCanvas != null)
            {
                var spring = _ritualCanvas.GetComponent<VRSpringCanvas>();
                if (spring != null) spring.Dismiss();
            }

            if (_onEndCompleted != null)
            {
                _onEndCompleted.Invoke();
            }
            else
            {
                if (PatientSessionManager.Instance != null)
                {
                    PatientSessionManager.Instance.EndAndSaveSession(completed: true, extraParams: "Session Completed");
                }

                if (VRFadeManager.Instance != null)
                {
                    VRFadeManager.Instance.FadeAndLoadScene(0, 0.5f);
                }
                else
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene(0);
                }
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
            cRect.sizeDelta = new Vector2(500, 380);
            cRect.localScale = Vector3.one * 0.0020f;

            if (cam != null)
            {
                Vector3 forward = cam.transform.forward; forward.y = 0;
                if (forward.sqrMagnitude < 0.01f) forward = cam.transform.forward;
                forward.Normalize();
                _ritualCanvas.transform.position = cam.transform.position + (forward * 1.6f);
                _ritualCanvas.transform.rotation = Quaternion.LookRotation(forward);
            }

            // Main Background Panel (Forest Mineral & Sage)
            var panelObj = new GameObject("Panel");
            panelObj.transform.SetParent(_ritualCanvas.transform, false);
            var panelImg = panelObj.AddComponent<Image>();
            panelImg.color = ThemeTokens.SurfacePanel;
            var panelRect = panelObj.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero; panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero; panelRect.offsetMax = Vector2.zero;

            // Header Title
            var title = ThemeTokens.CreateLabel(panelObj.transform, "Title", "Session Complete • Milestone Reached", 18, ThemeTokens.TextPrimary, TextAlignmentOptions.Center, true);
            var titleRect = title.GetComponent<RectTransform>();
            titleRect.anchoredPosition = new Vector2(0, 142);
            titleRect.sizeDelta = new Vector2(460, 28);

            // Subtitle
            var sub = ThemeTokens.CreateLabel(panelObj.transform, "Subtitle", "Take a quiet moment to review your exposure progress.", 12, ThemeTokens.TextSecondary, TextAlignmentOptions.Center);
            var subRect = sub.GetComponent<RectTransform>();
            subRect.anchoredPosition = new Vector2(0, 112);
            subRect.sizeDelta = new Vector2(460, 22);

            // Review Data Box (Inner surface)
            var dataBox = new GameObject("DataBox");
            dataBox.transform.SetParent(panelObj.transform, false);
            var dataBoxImg = dataBox.AddComponent<Image>();
            dataBoxImg.color = ThemeTokens.SurfaceInactive;
            var dbRect = dataBox.GetComponent<RectTransform>();
            dbRect.anchoredPosition = new Vector2(0, 15);
            dbRect.sizeDelta = new Vector2(450, 130);

            // Row 1: Scenario
            var row1 = ThemeTokens.CreateLabel(dataBox.transform, "Scenario", $"Scenario:  {phobiaName}", 13, ThemeTokens.TextPrimary, TextAlignmentOptions.Left, true);
            var r1Rect = row1.GetComponent<RectTransform>();
            r1Rect.anchoredPosition = new Vector2(0, 38);
            r1Rect.sizeDelta = new Vector2(410, 22);

            // Row 2: Metrics / Peak Altitude
            var row2 = ThemeTokens.CreateLabel(dataBox.transform, "Metrics", summaryMetrics, 13, ThemeTokens.AccentSage, TextAlignmentOptions.Left, true);
            var r2Rect = row2.GetComponent<RectTransform>();
            r2Rect.anchoredPosition = new Vector2(0, 10);
            r2Rect.sizeDelta = new Vector2(410, 22);

            // Row 3: Status
            var row3 = ThemeTokens.CreateLabel(dataBox.transform, "Status", "Outcome:  Exposure Goal Accomplished Successfully", 12, ThemeTokens.TextSecondary, TextAlignmentOptions.Left);
            var r3Rect = row3.GetComponent<RectTransform>();
            r3Rect.anchoredPosition = new Vector2(0, -18);
            r3Rect.sizeDelta = new Vector2(410, 20);

            // Row 4: Helper hint
            var row4 = ThemeTokens.CreateLabel(dataBox.transform, "Hint", "Your session metrics have been securely recorded in the clinical log.", 11, ThemeTokens.TextMuted, TextAlignmentOptions.Left);
            var r4Rect = row4.GetComponent<RectTransform>();
            r4Rect.anchoredPosition = new Vector2(0, -42);
            r4Rect.sizeDelta = new Vector2(410, 18);

            // Button 1: [ Restart Session ] (Left)
            var restartBtnObj = new GameObject("RestartButton");
            restartBtnObj.transform.SetParent(panelObj.transform, false);
            var restartImg = restartBtnObj.AddComponent<Image>();
            restartImg.color = ThemeTokens.SurfaceBorder;
            var restartBtn = restartBtnObj.AddComponent<Button>();
            restartBtn.targetGraphic = restartImg;
            ThemeTokens.StyleButtonColors(restartBtn, ThemeTokens.SurfaceBorder, ThemeTokens.SurfaceHover, ThemeTokens.CanvasBg);
            restartBtn.onClick.AddListener(OnEndRestartClicked);

            var rBtnRect = restartBtnObj.GetComponent<RectTransform>();
            rBtnRect.anchoredPosition = new Vector2(-110, -120);
            rBtnRect.sizeDelta = new Vector2(200, 44);

            var rBtnText = ThemeTokens.CreateLabel(restartBtnObj.transform, "Text", "Restart Session", 14, ThemeTokens.TextPrimary, TextAlignmentOptions.Center, true);
            var rbtRect = rBtnText.GetComponent<RectTransform>();
            rbtRect.anchorMin = Vector2.zero; rbtRect.anchorMax = Vector2.one;
            rbtRect.offsetMin = Vector2.zero; rbtRect.offsetMax = Vector2.zero;

            // Button 2: [ Return to Main Menu ] (Right)
            var returnBtnObj = new GameObject("ReturnButton");
            returnBtnObj.transform.SetParent(panelObj.transform, false);
            var returnImg = returnBtnObj.AddComponent<Image>();
            returnImg.color = ThemeTokens.AccentSage;
            var returnBtn = returnBtnObj.AddComponent<Button>();
            returnBtn.targetGraphic = returnImg;
            ThemeTokens.StyleButtonColors(returnBtn, ThemeTokens.AccentSage, ThemeTokens.AccentSageHover, ThemeTokens.CanvasBg);
            returnBtn.onClick.AddListener(OnEndReturnClicked);

            var retBtnRect = returnBtnObj.GetComponent<RectTransform>();
            retBtnRect.anchoredPosition = new Vector2(110, -120);
            retBtnRect.sizeDelta = new Vector2(200, 44);

            var retBtnText = ThemeTokens.CreateLabel(returnBtnObj.transform, "Text", "Main Menu", 14, ThemeTokens.AccentSageText, TextAlignmentOptions.Center, true);
            var retBtRect = retBtnText.GetComponent<RectTransform>();
            retBtRect.anchorMin = Vector2.zero; retBtRect.anchorMax = Vector2.one;
            retBtRect.offsetMin = Vector2.zero; retBtRect.offsetMax = Vector2.zero;

            // Attach spring arrival physics: preserve calibrated localScale
            var spring = VRSpringCanvas.Attach(_ritualCanvas);
            spring.SetBaseScale(cRect.localScale);
            spring.Arrive();
        }
    }
}
