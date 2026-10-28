using UnityEngine;
using UnityEngine.UI;

namespace DePhobia.VR
{
    /// <summary>
    /// Automatically converts UI canvases to comfortable 3D World Space in VR,
    /// preventing eye strain and locked-to-head motion sickness.
    /// Calibrated for 1.8m eye-level sharpness in OpenXR and desktop.
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public class VRWorldSpaceCanvasHelper : MonoBehaviour
    {
        [Tooltip("Distance in front of player eye position (meters)")]
        public float spawnDistance = 1.8f;
        [Tooltip("Height of canvas relative to player origin (meters)")]
        public float eyeHeight = 1.5f;
        [Tooltip("Uniform scale factor for world space canvas")]
        public float worldScale = 0.0022f;

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

            Camera cam = Camera.main;
            if (cam != null)
            {
                _canvas.worldCamera = cam;
            }

            transform.localScale = Vector3.one * worldScale;

            RecenterToPlayer();

            // Ensure GraphicRaycaster exists for pointer interaction
            if (GetComponent<GraphicRaycaster>() == null)
            {
                gameObject.AddComponent<GraphicRaycaster>();
            }
        }

        public void RecenterToPlayer()
        {
            Camera cam = Camera.main != null ? Camera.main : (_canvas != null ? _canvas.worldCamera : null);
            if (cam != null)
            {
                Vector3 forward = cam.transform.forward;
                forward.y = 0;
                if (forward.sqrMagnitude < 0.01f) forward = cam.transform.forward;
                forward.Normalize();

                Vector3 targetPos = cam.transform.position + (forward * spawnDistance);
                targetPos.y = cam.transform.position.y; // Eye level

                transform.position = targetPos;
                transform.rotation = Quaternion.LookRotation(forward);
            }
            else
            {
                transform.position = new Vector3(0, eyeHeight, spawnDistance);
                transform.rotation = Quaternion.identity;
            }
        }
    }
}
