using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DePhobia.Core;

namespace DePhobia.VR
{
    /// <summary>
    /// In-world physical emergency exit beacon for patients in distress.
    /// Can be triggered via ray pointer click, gaze dwell (2.0s), or proximity.
    /// Styled under Direction C (Soft & Abstract Horizon) with non-shaming copy.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class VREmergencyExitBeacon : MonoBehaviour
    {
        [Tooltip("Gaze dwell time required to trigger exit if gaze-selected")]
        public float gazeTriggerTime = 2.0f;
        public TextMeshPro statusLabel;
        public TextMeshPro subLabel;

        private float _currentGazeTime = 0f;
        private bool _isGazedAt = false;
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
            if (_isGazedAt)
            {
                _currentGazeTime += Time.deltaTime;
                float remaining = Mathf.Max(0, gazeTriggerTime - _currentGazeTime);

                if (statusLabel != null)
                {
                    statusLabel.text = "Returning to Menu...";
                    statusLabel.color = new Color(0.831f, 0.553f, 0.553f, 1f); // StateGentleExit #D48D8D
                }
                if (subLabel != null)
                {
                    subLabel.text = $"Holding gaze... ({remaining:F1}s)";
                }

                if (_currentGazeTime >= gazeTriggerTime)
                {
                    TriggerEmergencyExit();
                }
            }
            else
            {
                _currentGazeTime = 0f;
                if (statusLabel != null)
                {
                    statusLabel.text = "Return to Menu";
                    statusLabel.color = new Color(0.957f, 0.953f, 0.973f, 1f); // TextPrimary #F4F3F8
                }
                if (subLabel != null)
                {
                    subLabel.text = "Hold gaze or tap to exit safely";
                    subLabel.color = new Color(0.714f, 0.702f, 0.808f, 1f); // TextSecondary #B6B3CE
                }
            }
        }

        public void OnPointerEnter()
        {
            _isGazedAt = true;
        }

        public void OnPointerExit()
        {
            _isGazedAt = false;
        }

        public void TriggerEmergencyExit()
        {
            _isGazedAt = false;
            Debug.Log("[VREmergencyExitBeacon] Patient requested calm return to menu.");

            var rig = FindObjectOfType<VRPlayerRig>();
            if (rig != null)
            {
                rig.SendHapticImpulse(0.35f, 0.12f, UnityEngine.XR.XRNode.RightHand);
            }

            PatientSessionManager.Instance.EndAndSaveSession(completed: false, extraParams: "User Safe Exit via Beacon");
            VRFadeManager.Instance.FadeAndLoadScene(0, 0.5f);
        }

        private void OnMouseDown()
        {
            // Desktop mouse / pointer click support
            TriggerEmergencyExit();
        }

        /// <summary>
        /// Guarantees that a calm emergency exit beacon exists in the scene.
        /// Spawns one automatically in front of the user if none is found.
        /// </summary>
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
                Camera cam = Camera.main;
                if (cam != null)
                {
                    Vector3 forward = cam.transform.forward; forward.y = 0; forward.Normalize();
                    Vector3 right = cam.transform.right; right.y = 0; right.Normalize();
                    beaconObj.transform.position = cam.transform.position + (forward * 1.8f) - (right * 1.2f) + Vector3.up * 0.1f;
                    beaconObj.transform.rotation = Quaternion.LookRotation(cam.transform.position - beaconObj.transform.position);
                }
                else
                {
                    beaconObj.transform.position = new Vector3(-1.2f, 1.4f, 1.8f);
                    beaconObj.transform.rotation = Quaternion.Euler(0, 20f, 0);
                }
            }

            var beacon = beaconObj.AddComponent<VREmergencyExitBeacon>();

            // Visual background card (Direction C styling)
            var cardGo = GameObject.CreatePrimitive(PrimitiveType.Quad);
            cardGo.name = "BeaconPlate";
            cardGo.transform.SetParent(beaconObj.transform, false);
            cardGo.transform.localScale = new Vector3(0.85f, 0.45f, 1f);

            var quadCollider = cardGo.GetComponent<Collider>();
            if (quadCollider != null) Destroy(quadCollider);

            var renderer = cardGo.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                var mat = new Material(Shader.Find("Standard") ?? Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Diffuse"));
                mat.color = new Color(0.141f, 0.137f, 0.220f, 0.95f); // SurfacePanel #242338
                renderer.material = mat;
            }

            // Text Labels
            var labelObj = new GameObject("StatusLabel");
            labelObj.transform.SetParent(beaconObj.transform, false);
            labelObj.transform.localPosition = new Vector3(0, 0.08f, -0.02f);
            var statusTmp = labelObj.AddComponent<TextMeshPro>();
            statusTmp.text = "Return to Menu";
            statusTmp.fontSize = 2.4f;
            statusTmp.alignment = TextAlignmentOptions.Center;
            statusTmp.color = new Color(0.957f, 0.953f, 0.973f, 1f);
            beacon.statusLabel = statusTmp;

            var subObj = new GameObject("SubLabel");
            subObj.transform.SetParent(beaconObj.transform, false);
            subObj.transform.localPosition = new Vector3(0, -0.09f, -0.02f);
            var subTmp = subObj.AddComponent<TextMeshPro>();
            subTmp.text = "Hold gaze or tap to exit safely";
            subTmp.fontSize = 1.4f;
            subTmp.alignment = TextAlignmentOptions.Center;
            subTmp.color = new Color(0.714f, 0.702f, 0.808f, 1f);
            beacon.subLabel = subTmp;

            return beacon;
        }
    }
}
