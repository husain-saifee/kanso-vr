using UnityEngine;
using UnityEngine.UI;

namespace DePhobia.VR
{
    /// <summary>
    /// Renders dynamic peripheral FOV reduction (tunneling vignette)
    /// during rapid movement or turning to reduce VR vestibular motion sickness.
    /// </summary>
    public class VRComfortVignette : MonoBehaviour
    {
        [Range(0f, 1f)]
        public float maxVignetteIntensity = 0.75f;
        public float fadeSpeed = 4f;

        private Image _vignetteImage;
        private Canvas _canvas;
        private float _targetAlpha = 0f;
        private float _currentAlpha = 0f;

        private void Awake()
        {
            SetupCanvas();
        }

        private void SetupCanvas()
        {
            var canvasObj = new GameObject("VignetteCanvas");
            canvasObj.transform.SetParent(transform, false);

            _canvas = canvasObj.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 999;

            var imageObj = new GameObject("VignetteImage");
            imageObj.transform.SetParent(canvasObj.transform, false);

            _vignetteImage = imageObj.AddComponent<Image>();
            _vignetteImage.color = new Color(0, 0, 0, 0);
            _vignetteImage.raycastTarget = false;

            var rect = _vignetteImage.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            // Generate procedural circular gradient texture for vignette
            _vignetteImage.sprite = CreateVignetteSprite();
        }

        private Sprite CreateVignetteSprite()
        {
            int size = 256;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Vector2 center = new Vector2(size / 2f, size / 2f);
            float maxDist = size / 2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center) / maxDist;
                    // Transparent center, black edges starting at radius 0.5
                    float alpha = Mathf.SmoothStep(0.4f, 1.0f, dist);
                    tex.SetPixel(x, y, new Color(0, 0, 0, alpha));
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }

        public void SetMotionActive(bool isMoving)
        {
            _targetAlpha = isMoving ? maxVignetteIntensity : 0f;
        }

        private void Update()
        {
            _currentAlpha = Mathf.MoveTowards(_currentAlpha, _targetAlpha, Time.deltaTime * fadeSpeed);
            if (_vignetteImage != null)
            {
                Color c = _vignetteImage.color;
                c.a = _currentAlpha;
                _vignetteImage.color = c;
            }
        }
    }
}
