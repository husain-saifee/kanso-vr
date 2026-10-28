using UnityEngine;
using TMPro;
using DePhobia.Core;

namespace DePhobia.VR
{
    /// <summary>
    /// In-world emergency exit beacon for patients in distress.
    /// Synchronized with the Forest Mineral & Sage visual language and Horizon Respiration Ring.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class VREmergencyExitBeacon : MonoBehaviour
    {
        public float gazeTriggerTime = 1.5f;
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

            // Also ensure the Horizon Respiration Ring is active
            VRHorizonRespirationRing.EnsureRingInScene();
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
                    statusLabel.color = ThemeTokens.GentleExitClay;
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
                    statusLabel.color = ThemeTokens.TextPrimary;
                }
                if (subLabel != null)
                {
                    subLabel.text = "Hold gaze or tap to exit safely";
                    subLabel.color = ThemeTokens.TextSecondary;
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
            TriggerEmergencyExit();
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

            // Visual background card (Forest Mineral & Sage styling)
            var cardGo = new GameObject("BeaconPlate");
            cardGo.transform.SetParent(beaconObj.transform, false);
            cardGo.transform.localScale = new Vector3(0.85f, 0.45f, 1f);

            var mf = cardGo.AddComponent<MeshFilter>();
            var quadMesh = new Mesh();
            quadMesh.name = "BeaconPlateMesh";
            quadMesh.vertices = new Vector3[]
            {
                new Vector3(-0.5f, -0.5f, 0),
                new Vector3(0.5f, -0.5f, 0),
                new Vector3(-0.5f, 0.5f, 0),
                new Vector3(0.5f, 0.5f, 0)
            };
            quadMesh.uv = new Vector2[]
            {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(1, 1)
            };
            quadMesh.triangles = new int[] { 0, 2, 1, 2, 3, 1 };
            quadMesh.RecalculateNormals();
            mf.sharedMesh = quadMesh;

            var renderer = cardGo.AddComponent<MeshRenderer>();
            var mat = new Material(Shader.Find("Standard") ?? Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Diffuse") ?? Shader.Find("Unlit/Color"));
            mat.color = ThemeTokens.SurfacePanel;
            renderer.material = mat;

            // Text Labels
            var labelObj = new GameObject("StatusLabel");
            labelObj.transform.SetParent(beaconObj.transform, false);
            labelObj.transform.localPosition = new Vector3(0, 0.08f, -0.02f);
            var statusTmp = labelObj.AddComponent<TextMeshPro>();
            statusTmp.text = "Return to Menu";
            statusTmp.fontSize = 2.4f;
            statusTmp.alignment = TextAlignmentOptions.Center;
            statusTmp.color = ThemeTokens.TextPrimary;
            beacon.statusLabel = statusTmp;

            var subObj = new GameObject("SubLabel");
            subObj.transform.SetParent(beaconObj.transform, false);
            subObj.transform.localPosition = new Vector3(0, -0.09f, -0.02f);
            var subTmp = subObj.AddComponent<TextMeshPro>();
            subTmp.text = "Hold gaze or tap to exit safely";
            subTmp.fontSize = 1.4f;
            subTmp.alignment = TextAlignmentOptions.Center;
            subTmp.color = ThemeTokens.TextSecondary;
            beacon.subLabel = subTmp;

            return beacon;
        }
    }
}
