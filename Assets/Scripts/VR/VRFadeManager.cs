using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DePhobia.VR
{
    /// <summary>
    /// Provides smooth screen fades in VR during scene transitions and teleports
    /// to prevent visual jarring and simulation sickness.
    /// </summary>
    public class VRFadeManager : MonoBehaviour
    {
        private static VRFadeManager _instance;
        public static VRFadeManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("VRFadeManager");
                    _instance = go.AddComponent<VRFadeManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
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
            FadeOut(duration, () =>
            {
                SceneManager.LoadScene(sceneIndex);
                FadeIn(duration);
            });
        }

        public void FadeAndLoadScene(string sceneName, float duration = 0.5f)
        {
            FadeOut(duration, () =>
            {
                SceneManager.LoadScene(sceneName);
                FadeIn(duration);
            });
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
            Color col = Color.black;

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
