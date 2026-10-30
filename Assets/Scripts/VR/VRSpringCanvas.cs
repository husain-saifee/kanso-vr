using System;
using UnityEngine;

namespace DePhobia.VR
{
    /// <summary>
    /// Implements organic spring-physics arrival and dismissal for floating VR world-space canvases.
    /// Panels arrive by expanding and settling from 88% scale rather than popping into existence,
    /// and collapse smoothly when dismissed. Operates on unscaled delta time to function during pause.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class VRSpringCanvas : MonoBehaviour
    {
        [Header("Spring Dynamics")]
        public float arriveDuration = 0.26f;
        public float dismissDuration = 0.16f;
        public float startScale = 0.88f;

        private CanvasGroup _canvasGroup;
        private Vector3 _targetScale = Vector3.one;
        private Vector3 _scaleVelocity;
        private float _targetAlpha = 1.0f;
        private float _alphaVelocity;

        private bool _isTransitioning = false;
        private Action _onArrivalComplete;
        private Action _onDismissComplete;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        private void Update()
        {
            if (!_isTransitioning) return;

            // Critically damped spring scaling and alpha fade
            transform.localScale = Vector3.SmoothDamp(transform.localScale, _targetScale, ref _scaleVelocity, arriveDuration, Mathf.Infinity, Time.unscaledDeltaTime);
            _canvasGroup.alpha = Mathf.SmoothDamp(_canvasGroup.alpha, _targetAlpha, ref _alphaVelocity, arriveDuration * 0.8f, Mathf.Infinity, Time.unscaledDeltaTime);

            // Check arrival completion
            if (_targetAlpha > 0.5f && Vector3.Distance(transform.localScale, _targetScale) < 0.005f && Mathf.Abs(_canvasGroup.alpha - 1f) < 0.02f)
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

            gameObject.SetActive(true);
            transform.localScale = Vector3.one * startScale;
            _canvasGroup.alpha = 0f;

            _targetScale = Vector3.one;
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

            _targetScale = Vector3.one * startScale;
            _targetAlpha = 0f;
            _isTransitioning = true;
            _onDismissComplete = onComplete;
        }

        public static VRSpringCanvas Attach(GameObject go)
        {
            if (go == null) return null;
            var sc = go.GetComponent<VRSpringCanvas>();
            if (sc == null) sc = go.AddComponent<VRSpringCanvas>();
            return sc;
        }
    }
}
