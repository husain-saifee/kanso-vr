using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace DePhobia.VR
{
    /// <summary>
    /// Automatically upgrades EventSystem across all scenes to InputSystemUIInputModule,
    /// removing any conflicting legacy GoogleVR or Standalone input modules so that mouse clicks,
    /// touch, and pointer interactions always work without fail.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class VRUIEventSystemHelper : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInit()
        {
            var go = new GameObject("VRUIEventSystemHelper");
            go.AddComponent<VRUIEventSystemHelper>();
            DontDestroyOnLoad(go);
        }

        private void OnEnable()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            EnsureEventSystemModule();
        }

        private void Start()
        {
            EnsureEventSystemModule();
        }

        private void Update()
        {
            // Continuously verify that no legacy input module (e.g. GvrPointerInputModule) has taken over
            var es = EventSystem.current != null ? EventSystem.current : FindObjectOfType<EventSystem>();
            if (es != null)
            {
#if ENABLE_INPUT_SYSTEM
                if (es.GetComponent<InputSystemUIInputModule>() == null)
                {
                    EnsureEventSystemModule();
                }
#endif
            }
        }

        public static void EnsureEventSystemModule()
        {
            var es = FindObjectOfType<EventSystem>();
            if (es == null)
            {
                var esGo = new GameObject("EventSystem");
                es = esGo.AddComponent<EventSystem>();
            }

            // Remove legacy GvrPointerInputModule if present
            var gvrMod = es.GetComponent<GvrPointerInputModule>();
            if (gvrMod != null)
            {
                DestroyImmediate(gvrMod);
            }

#if ENABLE_INPUT_SYSTEM
            var inputModule = es.GetComponent<InputSystemUIInputModule>();
            if (inputModule == null)
            {
                inputModule = es.gameObject.AddComponent<InputSystemUIInputModule>();
            }
            try
            {
                inputModule.AssignDefaultActions();
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("[VRUIEventSystemHelper] AssignDefaultActions note: " + ex.Message);
            }
#else
            if (es.GetComponent<StandaloneInputModule>() == null)
            {
                es.gameObject.AddComponent<StandaloneInputModule>();
            }
#endif
        }
    }
}
