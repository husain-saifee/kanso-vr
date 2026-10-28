using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DePhobia.Core;

namespace DePhobia.VR
{
    /// <summary>
    /// In-world emergency exit beacon for patients in distress.
    /// Strictly click-activated: gazing or looking around NEVER triggers auto-exit.
    /// Rendered as a clean, single-sided WorldSpace slate that never wraps or clips text.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class VREmergencyExitBeacon : MonoBehaviour
    {
        public TextMeshProUGUI statusLabel;
        public TextMeshProUGUI subLabel;
        private Image _panelImage;

        private bool _isHovered = false;
        private BoxCollider _collider;

        private void Awake()
        {
            _collider = GetComponent<BoxCollider>();
            if (_collider != null)
            {
                _collider.size = new Vector3(0.55f, 0.2f, 0.05f);
                _collider.isTrigger = false;
            }
        }

        private void Update()
        {
            if (_isHovered)
            {
                if (statusLabel != null)
                {
                    statusLabel.text = "Return to Menu";
                    statusLabel.color = ThemeTokens.GentleExitClay;
                }
                if (subLabel != null)
                {
                    subLabel.text = "Click to exit session";
                    subLabel.color = ThemeTokens.TextPrimary;
                }
                if (_panelImage != null)
                {
                    _panelImage.color = ThemeTokens.SurfaceHover;
                }
            }
            else
            {
                if (statusLabel != null)
                {
                    statusLabel.text = "Return to Menu";
                    statusLabel.color = ThemeTokens.TextPrimary;
                }
                if (subLabel != null)
                {
                    subLabel.text = "Click to exit safely";
                    subLabel.color = ThemeTokens.TextSecondary;
                }
                if (_panelImage != null)
                {
                    _panelImage.color = ThemeTokens.SurfacePanel;
                }
            }
        }

        public void OnPointerEnter()
        {
            _isHovered = true;
        }

        public void OnPointerExit()
        {
            _isHovered = false;
        }

        public void OnPointerClick()
        {
            TriggerEmergencyExit();
        }

        private void OnMouseEnter()
        {
            _isHovered = true;
        }

        private void OnMouseExit()
        {
            _isHovered = false;
        }

        private void OnMouseDown()
        {
            TriggerEmergencyExit();
        }

        public void TriggerEmergencyExit()
        {
            _isHovered = false;

            if (TherapyAudioManager.HasInstance && TherapyAudioManager.Instance != null)
            {
                TherapyAudioManager.Instance.PlayFeedbackClick();
            }

            var rig = FindObjectOfType<VRPlayerRig>();
            if (rig != null)
            {
                rig.SendHapticImpulse(0.35f, 0.12f, UnityEngine.XR.XRNode.RightHand);
            }

            if (PatientSessionManager.HasInstance && PatientSessionManager.Instance != null)
            {
                PatientSessionManager.Instance.EndAndSaveSession(completed: false, extraParams: "User Safe Exit via Beacon Click");
            }

            if (VRFadeManager.HasInstance && VRFadeManager.Instance != null)
            {
                VRFadeManager.Instance.FadeAndLoadScene(0, 0.5f);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(0);
            }
        }

        public static VREmergencyExitBeacon EnsureBeaconInScene(Transform parentTransform = null, Vector3? localOffset = null, Quaternion? localRotation = null)
        {
            var existing = FindObjectOfType<VREmergencyExitBeacon>();
            if (existing != null) return existing;

            var beaconObj = new GameObject("VREmergencyExitBeacon");
            if (parentTransform != null)
            {
                beaconObj.transform.SetParent(parentTransform, false);
                beaconObj.transform.localPosition = localOffset ?? new Vector3(-0.75f, 1.15f, 1.1f);
                beaconObj.transform.localRotation = localRotation ?? Quaternion.Euler(0, 30f, 0);
            }
            else
            {
                beaconObj.transform.position = localOffset ?? new Vector3(-1.25f, 1.35f, 2.0f);
                beaconObj.transform.rotation = localRotation ?? Quaternion.Euler(0, 25f, 0);
            }

            var beacon = beaconObj.AddComponent<VREmergencyExitBeacon>();
            beacon.BuildBeaconVisuals();
            return beacon;
        }

        private void BuildBeaconVisuals()
        {
            var canvasObj = new GameObject("BeaconCanvas");
            canvasObj.transform.SetParent(transform, false);
            canvasObj.transform.localPosition = Vector3.zero;
            canvasObj.transform.localRotation = Quaternion.identity;

            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;
            canvas.sortingOrder = 300;

            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 10;
            canvasObj.AddComponent<GraphicRaycaster>();

            var cRect = canvasObj.GetComponent<RectTransform>();
            cRect.sizeDelta = new Vector2(280, 80);
            cRect.localScale = Vector3.one * 0.0018f;

            // Panel Background
            var panelObj = new GameObject("Panel");
            panelObj.transform.SetParent(canvasObj.transform, false);
            _panelImage = panelObj.AddComponent<Image>();
            _panelImage.color = ThemeTokens.SurfacePanel;

            var btn = panelObj.AddComponent<Button>();
            btn.targetGraphic = _panelImage;
            btn.onClick.AddListener(TriggerEmergencyExit);
            ThemeTokens.StyleButtonColors(btn, ThemeTokens.SurfacePanel, ThemeTokens.SurfaceHover, ThemeTokens.GentleExitClay);

            var pRect = panelObj.GetComponent<RectTransform>();
            pRect.anchorMin = Vector2.zero; pRect.anchorMax = Vector2.one;
            pRect.offsetMin = Vector2.zero; pRect.offsetMax = Vector2.zero;

            // Top Accent Line
            var lineObj = new GameObject("AccentLine");
            lineObj.transform.SetParent(panelObj.transform, false);
            var lineImg = lineObj.AddComponent<Image>();
            lineImg.color = ThemeTokens.AccentSage;
            var lRect = lineObj.GetComponent<RectTransform>();
            lRect.anchorMin = new Vector2(0, 1); lRect.anchorMax = new Vector2(1, 1);
            lRect.pivot = new Vector2(0.5f, 1);
            lRect.anchoredPosition = Vector2.zero;
            lRect.sizeDelta = new Vector2(0, 3);

            // Title: Return to Menu
            statusLabel = ThemeTokens.CreateLabel(panelObj.transform, "StatusLabel", "Return to Menu", 17, ThemeTokens.TextPrimary, TextAlignmentOptions.Center, true);
            statusLabel.enableWordWrapping = false;
            statusLabel.overflowMode = TextOverflowModes.Ellipsis;
            var sRect = statusLabel.GetComponent<RectTransform>();
            sRect.anchoredPosition = new Vector2(0, 10);
            sRect.sizeDelta = new Vector2(260, 26);

            // Subtitle: Click to exit safely
            subLabel = ThemeTokens.CreateLabel(panelObj.transform, "SubLabel", "Click to exit safely", 11, ThemeTokens.TextSecondary, TextAlignmentOptions.Center);
            subLabel.enableWordWrapping = false;
            subLabel.overflowMode = TextOverflowModes.Ellipsis;
            var subRect = subLabel.GetComponent<RectTransform>();
            subRect.anchoredPosition = new Vector2(0, -18);
            subRect.sizeDelta = new Vector2(260, 20);

            // Backplate (so viewed from behind, it's a solid dark mineral panel with NO text)
            var backObj = new GameObject("BackPlate");
            backObj.transform.SetParent(canvasObj.transform, false);
            backObj.transform.localPosition = new Vector3(0, 0, 0.005f);
            backObj.transform.localRotation = Quaternion.Euler(0, 180f, 0);
            var backImg = backObj.AddComponent<Image>();
            backImg.color = ThemeTokens.SurfacePanel;
            var bRect = backObj.GetComponent<RectTransform>();
            bRect.anchorMin = Vector2.zero; bRect.anchorMax = Vector2.one;
            bRect.offsetMin = Vector2.zero; bRect.offsetMax = Vector2.zero;
        }
    }
}