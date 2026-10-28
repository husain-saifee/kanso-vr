using UnityEngine;
using TMPro;
using DePhobia.Core;

namespace DePhobia.VR
{
    /// <summary>
    /// In-world emergency exit beacon for patients in distress.
    /// Strictly click-activated: gazing or looking around NEVER triggers auto-exit.
    /// Only explicit mouse clicks, controller pointer clicks, or tap actions trigger safe return to menu.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class VREmergencyExitBeacon : MonoBehaviour
    {
        public TextMeshPro statusLabel;
        public TextMeshPro subLabel;

        private bool _isHovered = false;
        private BoxCollider _collider;

        private void Awake()
        {
            _collider = GetComponent<BoxCollider>();
            if (_collider != null)
            {
                _collider.size = new Vector3(0.8f, 0.6f, 0.2f);
                _collider.isTrigger = true;
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

        private void OnMouseDown()
        {
            TriggerEmergencyExit();
        }

        public void TriggerEmergencyExit()
        {
            _isHovered = false;

            var rig = FindObjectOfType<VRPlayerRig>();
            if (rig != null)
            {
                rig.SendHapticImpulse(0.35f, 0.12f, UnityEngine.XR.XRNode.RightHand);
            }

            PatientSessionManager.Instance.EndAndSaveSession(completed: false, extraParams: "User Safe Exit via Beacon Click");
            VRFadeManager.Instance.FadeAndLoadScene(0, 0.5f);
        }

        public static VREmergencyExitBeacon EnsureBeaconInScene(Transform parentTransform = null, Vector3? localOffset = null)
        {
            var existing = FindObjectOfType<VREmergencyExitBeacon>();
            if (existing != null) return existing;

            var beaconObj = new GameObject("VREmergencyExitBeacon");
            if (parentTransform != null)
            {
                beaconObj.transform.SetParent(parentTransform, false);
                beaconObj.transform.localPosition = localOffset ?? new Vector3(-1.4f, 1.3f, 1.8f);
                beaconObj.transform.localRotation = Quaternion.Euler(0, 25f, 0);
            }
            else
            {
                beaconObj.transform.position = localOffset ?? new Vector3(-1.2f, 1.4f, 2.0f);
                beaconObj.transform.rotation = Quaternion.identity;
            }

            var beacon = beaconObj.AddComponent<VREmergencyExitBeacon>();
            beacon.BuildBeaconVisuals();
            return beacon;
        }

        private void BuildBeaconVisuals()
        {
            var bgObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bgObj.name = "BeaconBackground";
            bgObj.transform.SetParent(transform, false);
            bgObj.transform.localPosition = Vector3.zero;
            bgObj.transform.localScale = new Vector3(0.72f, 0.44f, 0.04f);

            var bgCol = bgObj.GetComponent<Collider>();
            if (bgCol != null) Destroy(bgCol);

            var bgRenderer = bgObj.GetComponent<MeshRenderer>();
            if (bgRenderer != null)
            {
                var bgMat = new Material(Shader.Find("Sprites/Default"));
                bgMat.color = ThemeTokens.SurfacePanel;
                bgRenderer.material = bgMat;
            }

            var labelObj = new GameObject("StatusLabel");
            labelObj.transform.SetParent(transform, false);
            labelObj.transform.localPosition = new Vector3(0, 0.08f, -0.025f);
            statusLabel = labelObj.AddComponent<TextMeshPro>();
            statusLabel.text = "Return to Menu";
            statusLabel.fontSize = 2.4f;
            statusLabel.alignment = TextAlignmentOptions.Center;
            statusLabel.color = ThemeTokens.TextPrimary;
            var labelRect = labelObj.GetComponent<RectTransform>();
            if (labelRect != null) labelRect.sizeDelta = new Vector2(0.68f, 0.2f);

            var subObj = new GameObject("SubLabel");
            subObj.transform.SetParent(transform, false);
            subObj.transform.localPosition = new Vector3(0, -0.08f, -0.025f);
            subLabel = subObj.AddComponent<TextMeshPro>();
            subLabel.text = "Click to exit safely";
            subLabel.fontSize = 1.3f;
            subLabel.alignment = TextAlignmentOptions.Center;
            subLabel.color = ThemeTokens.TextSecondary;
            var subRect = subObj.GetComponent<RectTransform>();
            if (subRect != null) subRect.sizeDelta = new Vector2(0.68f, 0.16f);
        }
    }
}
