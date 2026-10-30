using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DePhobia.Core;

namespace DePhobia.VR
{
    /// <summary>
    /// In-world emergency exit beacon for patients in distress.
    /// Strictly click-activated: gazing or looking around NEVER triggers auto-exit.
    /// Rendered as a bold, high-contrast, crystal-clear WorldSpace slate with zero occlusions.
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
                _collider.size = new Vector3(0.65f, 0.22f, 0.05f);
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
                    subLabel.color = Color.white;
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
                    statusLabel.color = Color.white;
                }
                if (subLabel != null)
                {
                    subLabel.text = "Click to exit safely";
                    subLabel.color = new Color(0.78f, 0.85f, 0.75f, 1f);
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
            canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1 | AdditionalCanvasShaderChannels.Normal | AdditionalCanvasShaderChannels.Tangent;

            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 10;
            canvasObj.AddComponent<GraphicRaycaster>();

            var cRect = canvasObj.GetComponent<RectTransform>();
            cRect.sizeDelta = new Vector2(320, 96);
            cRect.localScale = Vector3.one * 0.002f;

            // Outer crisp border frame
            var borderObj = new GameObject("BorderFrame");
            borderObj.transform.SetParent(canvasObj.transform, false);
            var borderImg = borderObj.AddComponent<Image>();
            borderImg.color = ThemeTokens.AccentSage;
            var brRect = borderObj.GetComponent<RectTransform>();
            brRect.anchorMin = Vector2.zero;
            brRect.anchorMax = Vector2.one;
            brRect.offsetMin = Vector2.zero;
            brRect.offsetMax = Vector2.zero;

            // Inner dark panel
            var panelObj = new GameObject("Panel");
            panelObj.transform.SetParent(canvasObj.transform, false);
            _panelImage = panelObj.AddComponent<Image>();
            _panelImage.color = ThemeTokens.SurfacePanel;

            var btn = panelObj.AddComponent<Button>();
            btn.targetGraphic = _panelImage;
            btn.onClick.AddListener(TriggerEmergencyExit);
            ThemeTokens.StyleButtonColors(btn, ThemeTokens.SurfacePanel, ThemeTokens.SurfaceHover, ThemeTokens.GentleExitClay);

            var pRect = panelObj.GetComponent<RectTransform>();
            pRect.anchorMin = Vector2.zero;
            pRect.anchorMax = Vector2.one;
            pRect.offsetMin = new Vector2(3, 3);
            pRect.offsetMax = new Vector2(-3, -3);

            // Title: Pure bright white (#FFFFFF), bold, 22pt, crystal clear
            statusLabel = ThemeTokens.CreateLabel(panelObj.transform, "StatusLabel", "Return to Menu", 22, Color.white, TextAlignmentOptions.Center, true);
            statusLabel.enableWordWrapping = false;
            statusLabel.overflowMode = TextOverflowModes.Ellipsis;
            var sRect = statusLabel.GetComponent<RectTransform>();
            sRect.anchoredPosition = new Vector2(0, 12);
            sRect.sizeDelta = new Vector2(300, 32);

            // Subtitle: Soft clear matcha sage, 13pt
            subLabel = ThemeTokens.CreateLabel(panelObj.transform, "SubLabel", "Click to exit safely", 13, new Color(0.78f, 0.85f, 0.75f, 1f), TextAlignmentOptions.Center);
            subLabel.enableWordWrapping = false;
            subLabel.overflowMode = TextOverflowModes.Ellipsis;
            var subRect = subLabel.GetComponent<RectTransform>();
            subRect.anchoredPosition = new Vector2(0, -18);
            subRect.sizeDelta = new Vector2(300, 24);
        }
    }
}
