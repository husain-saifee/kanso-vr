using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DePhobia.Core;

namespace DePhobia.VR
{
    /// <summary>
    /// Implements tactile physical micro-feedback for UI buttons in VR and desktop.
    /// Responds to hover with a subtle 2mm forward lift and acoustic/haptic tick,
    /// depresses backward on click with tactile resistance, and springs back with follow-through.
    /// Operates on unscaled time so interaction remains fluid even when the session is paused.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class VRTactileButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Tactile Settings")]
        public float hoverScale = 1.035f;
        public float pressedScale = 0.955f;
        public float hoverZOffset = -4.0f; // lifts toward player
        public float pressedZOffset = 6.0f; // depresses into panel

        [Header("Spring Dynamics")]
        public float springSmoothTime = 0.045f;

        private RectTransform _rectTransform;
        private Vector3 _initialLocalPos;
        private Vector3 _targetLocalPos;
        private Vector3 _posVelocity;

        private Vector3 _targetScale = Vector3.one;
        private Vector3 _scaleVelocity;

        private bool _isHovered = false;
        private bool _isPressed = false;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _initialLocalPos = _rectTransform.localPosition;
            _targetLocalPos = _initialLocalPos;
        }

        private void OnEnable()
        {
            _targetScale = Vector3.one;
            _targetLocalPos = _initialLocalPos;
            _isHovered = false;
            _isPressed = false;
        }

        private void Update()
        {
            // Critically damped spring interpolation on scale and local z-depth
            _rectTransform.localScale = Vector3.SmoothDamp(_rectTransform.localScale, _targetScale, ref _scaleVelocity, springSmoothTime, Mathf.Infinity, Time.unscaledDeltaTime);
            _rectTransform.localPosition = Vector3.SmoothDamp(_rectTransform.localPosition, _targetLocalPos, ref _posVelocity, springSmoothTime, Mathf.Infinity, Time.unscaledDeltaTime);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isHovered = true;
            if (!_isPressed)
            {
                _targetScale = Vector3.one * hoverScale;
                _targetLocalPos = _initialLocalPos + new Vector3(0, 0, hoverZOffset);

                // Acoustic and haptic point-of-contact hover feedback
                if (TherapyAudioManager.HasInstance && TherapyAudioManager.Instance != null)
                {
                    TherapyAudioManager.Instance.PlayHoverTick();
                }

                var rig = FindObjectOfType<VRPlayerRig>();
                if (rig != null)
                {
                    rig.SendHapticImpulse(0.08f, 0.02f, UnityEngine.XR.XRNode.RightHand);
                }
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHovered = false;
            _isPressed = false;
            _targetScale = Vector3.one;
            _targetLocalPos = _initialLocalPos;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _isPressed = true;
            _targetScale = Vector3.one * pressedScale;
            _targetLocalPos = _initialLocalPos + new Vector3(0, 0, pressedZOffset);

            // Tactile bottoming-out feedback
            if (TherapyAudioManager.HasInstance && TherapyAudioManager.Instance != null)
            {
                TherapyAudioManager.Instance.PlayFeedbackClick();
            }

            var rig = FindObjectOfType<VRPlayerRig>();
            if (rig != null)
            {
                rig.SendHapticImpulse(0.24f, 0.06f, UnityEngine.XR.XRNode.RightHand);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isPressed = false;
            if (_isHovered)
            {
                _targetScale = Vector3.one * hoverScale;
                _targetLocalPos = _initialLocalPos + new Vector3(0, 0, hoverZOffset);
            }
            else
            {
                _targetScale = Vector3.one;
                _targetLocalPos = _initialLocalPos;
            }
        }
    }
}
