using System;
using UnityEngine;

namespace DePhobia.VR
{
    /// <summary>
    /// Implements organic spring-physics arrival and dismissal for floating VR world-space canvases.
    /// Preserves the canvas's calibrated world-space scale (e.g. 0.0018f or 0.0020f), scaling between 88% and 100%
    /// of base scale rather than hardcoding Vector3.one. Operates on unscaled time during pause.
    /// Locks base scale once configured to prevent scale decay across multiple pauses.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class VRSpringCanvas : MonoBehaviour
    {
        [Header("Spring Dynamics")]
        public float arriveDuration = 0.24f;
        public float dismissDuration = 0.16f;
        public float startScaleFraction = 0.88f;

        private CanvasGroup _canvasGroup;
        private Vector3 _baseScale = new Vector3(0.0020f, 0.0020f, 0.0020f);
        private Vector3 _targetScale;
        private Vector3 _scaleVelocity;
        private float _targetAlpha = 1.0f;
        private float _alphaVelocity;

        private bool _baseScaleLocked = false;
        private bool _isTransitioning = false;
        private Action _onArrivalComplete;
        private Action _onDismissComplete;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();

            CaptureBaseScale();
        }

        private void CaptureBaseScale()
        {
            if (_baseScaleLocked) return;

            // If localScale is already configured to a non-unit world scale (e.g. 0.0018f), record and lock it
            if (transform.localScale.sqrMagnitude > 0.00000001f && transform.localScale != Vector3.one)
            {
                _baseScale = transform.localScale;
                _baseScaleLocked = true;
            }
            _targetScale = _baseScale;
        }

        public void SetBaseScale(Vector3 scale)
        {
            _baseScale = scale;
            _targetScale = scale;
            _baseScaleLocked = true;
        }

        private void Update()
        {
            if (!_isTransitioning) return;

            // Critically damped spring scaling and alpha fade
            transform.localScale = Vector3.SmoothDamp(transform.localScale, _targetScale, ref _scaleVelocity, arriveDuration, Mathf.Infinity, Time.unscaledDeltaTime);
            _canvasGroup.alpha = Mathf.SmoothDamp(_canvasGroup.alpha, _targetAlpha, ref _alphaVelocity, arriveDuration * 0.75f, Mathf.Infinity, Time.unscaledDeltaTime);

            float scaleTolerance = Mathf.Max(_baseScale.x * 0.01f, 0.00001f);

            // Check arrival completion
            if (_targetAlpha > 0.5f && Vector3.Distance(transform.localScale, _targetScale) < scaleTolerance && Mathf.Abs(_canvasGroup.alpha - 1f) < 0.02f)
            {
                transform.localScale = _targetScale;
                _canvasGroup.alpha = 1.0f;
                _isTransitioning = false;
                _onArrivalComplete?.Invoke();
                _onArrivalComplete = null;
            }
            // Check dismissal completion
            else if (_targetAlpha < 0.5f && _canvasGroup.alpha < 0.03f)
            {
                _canvasGroup.alpha = 0f;
                _isTransitioning = false;
                gameObject.SetActive(false);
                _onDismissComplete?.Invoke();
                _onDismissComplete = null;
            }
        }

        /// <summary>
        /// Smoothly brings the canvas into view using spring physics.
        /// </summary>
        public void Arrive(Action onComplete = null)
        {
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();

            CaptureBaseScale();

            gameObject.SetActive(true);
            transform.localScale = _baseScale * startScaleFraction;
            _canvasGroup.alpha = 0f;

            _targetScale = _baseScale;
            _targetAlpha = 1.0f;
            _scaleVelocity = Vector3.zero;
            _alphaVelocity = 0f;
            _isTransitioning = true;
            _onArrivalComplete = onComplete;
        }

        /// <summary>
        /// Smoothly collapses the canvas out of view and deactivates it.
        /// </summary>
        public void Dismiss(Action onComplete = null)
        {
            if (!gameObject.activeSelf)
            {
                onComplete?.Invoke();
                return;
            }

            CaptureBaseScale();

            _targetScale = _baseScale * startScaleFraction;
            _targetAlpha = 0f;
            _isTransitioning = true;
            _onDismissComplete = onComplete;
        }

        public static VRSpringCanvas Attach(GameObject go)
        {
            if (go == null) return null;
            var sc = go.GetComponent<VRSpringCanvas>();
            if (sc == null)
            {
                sc = go.AddComponent<VRSpringCanvas>();
            }
            sc.CaptureBaseScale();
            return sc;
        }
    }
}
