using UnityEngine;
using UnityEngine.UI;

namespace DePhobia.VR
{
    /// <summary>
    /// Automatically converts UI canvases to comfortable 3D World Space in VR,
    /// preventing eye strain and locked-to-head motion sickness.
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public class VRWorldSpaceCanvasHelper : MonoBehaviour
    {
        [Tooltip("Distance in front of player eye position (meters)")]
        public float spawnDistance = 2.4f;
        [Tooltip("Height of canvas relative to player origin (meters)")]
        public float eyeHeight = 1.5f;
        [Tooltip("Uniform scale factor for world space canvas")]
        public float worldScale = 0.0025f;

        private Canvas _canvas;

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
            SetupWorldSpace();
        }

        public void SetupWorldSpace()
        {
            if (_canvas == null) return;

            _canvas.renderMode = RenderMode.WorldSpace;

            if (_canvas.worldCamera == null && Camera.main != null)
            {
                _canvas.worldCamera = Camera.main;
            }

            transform.localScale = Vector3.one * worldScale;

            // Position comfortably 2.4m in front of player at eye level
            Vector3 targetPos = new Vector3(0, eyeHeight, spawnDistance);
            transform.position = targetPos;
            transform.rotation = Quaternion.identity;

            // Ensure GraphicRaycaster exists for pointer interaction
            if (GetComponent<GraphicRaycaster>() == null)
            {
                gameObject.AddComponent<GraphicRaycaster>();
            }
        }
    }
}
