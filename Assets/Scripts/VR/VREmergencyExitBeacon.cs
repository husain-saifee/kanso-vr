using UnityEngine;
using TMPro;
using DePhobia.Core;

namespace DePhobia.VR
{
    /// <summary>
    /// In-world physical emergency exit beacon for patients in distress.
    /// Can be triggered via ray pointer click, gaze hold, or proximity.
    /// </summary>
    public class VREmergencyExitBeacon : MonoBehaviour
    {
        [Tooltip("Gaze dwell time required to trigger exit if gaze-selected")]
        public float gazeTriggerTime = 2.0f;
        public TextMeshPro statusLabel;

        private float _currentGazeTime = 0f;
        private bool _isGazedAt = false;

        private void Update()
        {
            if (_isGazedAt)
            {
                _currentGazeTime += Time.deltaTime;
                if (statusLabel != null)
                {
                    float remaining = Mathf.Max(0, gazeTriggerTime - _currentGazeTime);
                    statusLabel.text = $"EXITING IN {remaining:F1}s";
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
                    statusLabel.text = "EMERGENCY EXIT";
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
            Debug.Log("[VREmergencyExitBeacon] Patient triggered in-world emergency exit.");
            PatientSessionManager.Instance.EndAndSaveSession(completed: false, extraParams: "Emergency Exit Triggered");
            VRFadeManager.Instance.FadeAndLoadScene(0, 0.4f);
        }

        private void OnMouseDown()
        {
            // Desktop mouse click support
            TriggerEmergencyExit();
        }
    }
}
