using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DePhobia.Core;

namespace DePhobia.VR
{
    /// <summary>
    /// Displays high-contrast, cinematic subtitles in the lower field of view.
    /// Styled under Forest Mineral & Sage with zero blue or violet tones.
    /// Quiet and restrained: only appears when explicit dialogue or speech occurs.
    /// </summary>
    public class TherapyCaptionUI : MonoBehaviour
    {
        private static TherapyCaptionUI _instance;
        public static TherapyCaptionUI Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("TherapyCaptionUI");
                    _instance = go.AddComponent<TherapyCaptionUI>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        private Canvas _canvas;
        private GameObject _panel;
        private TextMeshProUGUI _captionText;
        private Coroutine _hideRoutine;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            CreateCaptionCanvas();
        }

        private void OnEnable()
        {
            TherapyAudioManager.OnCaptionRequested += ShowCaption;
        }

        private void OnDisable()
        {
            TherapyAudioManager.OnCaptionRequested -= ShowCaption;
        }

        private void LateUpdate()
        {
            if (_panel != null && _panel.activeSelf)
            {
                Camera cam = Camera.main;
                if (cam != null)
                {
                    Vector3 forward = cam.transform.forward; forward.y = 0;
                    if (forward.sqrMagnitude < 0.01f) forward = cam.transform.forward;
                    forward.Normalize();

                    Vector3 pos = cam.transform.position + (forward * 1.6f) - new Vector3(0, 0.38f, 0);
                    _canvas.transform.position = pos;
                    _canvas.transform.rotation = Quaternion.LookRotation(forward);
                }
            }
        }

        public void ShowCaption(string message, float duration)
        {
            if (_panel == null || _captionText == null) return;
            if (TherapySettingsManager.Instance != null && !TherapySettingsManager.Instance.subtitlesEnabled) return;

            _captionText.text = message;
            _panel.SetActive(true);

            if (_hideRoutine != null) StopCoroutine(_hideRoutine);
            _hideRoutine = StartCoroutine(HideRoutine(duration));
        }

        private IEnumerator HideRoutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (_panel != null) _panel.SetActive(false);
        }

        private void CreateCaptionCanvas()
        {
            var canvasObj = new GameObject("CaptionCanvas");
            canvasObj.transform.SetParent(transform);

            _canvas = canvasObj.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.WorldSpace;
            _canvas.sortingOrder = 400;

            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 10;

            var rect = canvasObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(500, 70);
            rect.localScale = Vector3.one * 0.0022f;

            // Background panel: Forest Mineral & Sage
            _panel = new GameObject("Panel");
            _panel.transform.SetParent(canvasObj.transform, false);
            var img = _panel.AddComponent<Image>();
            img.color = ThemeTokens.SurfacePanel;
            var panelRect = _panel.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            // Caption Text
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(_panel.transform, false);
            _captionText = textObj.AddComponent<TextMeshProUGUI>();
            _captionText.fontSize = 15;
            _captionText.alignment = TextAlignmentOptions.Center;
            _captionText.color = ThemeTokens.TextPrimary;
            var textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(16, 6);
            textRect.offsetMax = new Vector2(-16, -6);

            _panel.SetActive(false);
        }
    }
}
