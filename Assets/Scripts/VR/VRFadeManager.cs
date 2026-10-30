using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DePhobia.Core;

namespace DePhobia.VR
{
    /// <summary>
    /// Provides cinema fades with environmental audio pre-roll in VR.
    /// Eliminates visual cuts and simulation sickness, and ensures the ears perceive
    /// destination room acoustics (350ms pre-roll) before the eyes see the world,
    /// maintaining continuous 432Hz harmonic bed audio throughout.
    /// </summary>
    public class VRFadeManager : MonoBehaviour
    {
        private static VRFadeManager _instance;
        private static bool _isApplicationQuitting = false;
        public static bool HasInstance => _instance != null;

        public static VRFadeManager Instance
        {
            get
            {
                if (_isApplicationQuitting) return null;
                if (_instance == null)
                {
                    _instance = FindObjectOfType<VRFadeManager>();
                    if (_instance == null && !_isApplicationQuitting)
                    {
                        var go = new GameObject("VRFadeManager");
                        _instance = go.AddComponent<VRFadeManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        private void OnApplicationQuit()
        {
            _isApplicationQuitting = true;
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _isApplicationQuitting = true;
            }
        }

        private Canvas _fadeCanvas;
        private Image _fadeImage;
        private Coroutine _currentFadeRoutine;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            CreateFadeCanvas();
        }

        private void CreateFadeCanvas()
        {
            var canvasGo = new GameObject("VRFadeCanvas");
            canvasGo.transform.SetParent(transform);

            _fadeCanvas = canvasGo.AddComponent<Canvas>();
            _fadeCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            if (Camera.main != null)
            {
                _fadeCanvas.worldCamera = Camera.main;
            }
            _fadeCanvas.planeDistance = 0.25f;
            _fadeCanvas.sortingOrder = 9999;

            var imageGo = new GameObject("FadeOverlay");
            imageGo.transform.SetParent(canvasGo.transform, false);

            _fadeImage = imageGo.AddComponent<Image>();
            _fadeImage.color = new Color(0, 0, 0, 0);
            _fadeImage.raycastTarget = false;

            var rect = _fadeImage.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        public void FadeIn(float duration = 0.6f, Action onComplete = null)
        {
            StartFade(1f, 0f, duration, onComplete);
        }

        public void FadeOut(float duration = 0.6f, Action onComplete = null)
        {
            StartFade(0f, 1f, duration, onComplete);
        }

        public void FadeAndLoadScene(int sceneIndex, float duration = 0.5f)
        {
            if (_currentFadeRoutine != null) StopCoroutine(_currentFadeRoutine);
            _currentFadeRoutine = StartCoroutine(TransitionWithAudioPreRollRoutine(sceneIndex, duration));
        }

        public void FadeAndLoadScene(string sceneName, float duration = 0.5f)
        {
            if (_currentFadeRoutine != null) StopCoroutine(_currentFadeRoutine);
            _currentFadeRoutine = StartCoroutine(TransitionWithAudioPreRollRoutine(sceneName, duration));
        }

        private IEnumerator TransitionWithAudioPreRollRoutine(int sceneIndex, float duration)
        {
            yield return FadeRoutine(0f, 1f, duration, null);

            SceneManager.LoadScene(sceneIndex);
            yield return null; // Wait one frame for scene activation

            if (Camera.main != null && _fadeCanvas != null)
            {
                _fadeCanvas.worldCamera = Camera.main;
            }

            // Environmental Audio Pre-Roll: Destination room acoustics fade in before sight
            string scenario = GetScenarioNameForScene(sceneIndex);
            if (TherapyAudioManager.HasInstance && TherapyAudioManager.Instance != null)
            {
                TherapyAudioManager.Instance.PlayScenarioAmbience(scenario);
            }

            // 350ms acoustic pre-roll hold in blackout
            float preRollTimer = 0f;
            while (preRollTimer < 0.35f)
            {
                preRollTimer += Time.unscaledDeltaTime;
                yield return null;
            }

            // Visual reveal
            yield return FadeRoutine(1f, 0f, duration, null);
        }

        private IEnumerator TransitionWithAudioPreRollRoutine(string sceneName, float duration)
        {
            yield return FadeRoutine(0f, 1f, duration, null);

            SceneManager.LoadScene(sceneName);
            yield return null; // Wait one frame for scene activation

            if (Camera.main != null && _fadeCanvas != null)
            {
                _fadeCanvas.worldCamera = Camera.main;
            }

            string scenario = GetScenarioNameForScene(sceneName);
            if (TherapyAudioManager.HasInstance && TherapyAudioManager.Instance != null)
            {
                TherapyAudioManager.Instance.PlayScenarioAmbience(scenario);
            }

            float preRollTimer = 0f;
            while (preRollTimer < 0.35f)
            {
                preRollTimer += Time.unscaledDeltaTime;
                yield return null;
            }

            yield return FadeRoutine(1f, 0f, duration, null);
        }

        private string GetScenarioNameForScene(int buildIndex)
        {
            switch (buildIndex)
            {
                case 1: return "Claustrophobia";
                case 2: return "Aquaphobia";
                case 3: return "Acrophobia";
                default: return "Menu";
            }
        }

        private string GetScenarioNameForScene(string sceneName)
        {
            string s = sceneName.ToLowerInvariant();
            if (s.Contains("room") || s.Contains("revelation") || s.Contains("claus")) return "Claustrophobia";
            if (s.Contains("island") || s.Contains("beach") || s.Contains("aqua") || s.Contains("water")) return "Aquaphobia";
            if (s.Contains("geopipe") || s.Contains("demo") || s.Contains("acro") || s.Contains("elevation")) return "Acrophobia";
            return "Menu";
        }

        private void StartFade(float fromAlpha, float toAlpha, float duration, Action onComplete)
        {
            if (_fadeCanvas != null && (_fadeCanvas.worldCamera == null || !_fadeCanvas.worldCamera.gameObject.activeInHierarchy) && Camera.main != null)
            {
                _fadeCanvas.worldCamera = Camera.main;
            }

            if (_currentFadeRoutine != null)
                StopCoroutine(_currentFadeRoutine);

            _currentFadeRoutine = StartCoroutine(FadeRoutine(fromAlpha, toAlpha, duration, onComplete));
        }

        private IEnumerator FadeRoutine(float fromAlpha, float toAlpha, float duration, Action onComplete)
        {
            float elapsed = 0f;
            Color col = ThemeTokens.CanvasBg;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                col.a = Mathf.Lerp(fromAlpha, toAlpha, t);
                if (_fadeImage != null) _fadeImage.color = col;
                yield return null;
            }

            col.a = toAlpha;
            if (_fadeImage != null) _fadeImage.color = col;
            onComplete?.Invoke();
        }
    }
}
