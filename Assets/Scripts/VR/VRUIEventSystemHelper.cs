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

            // Clean up any legacy or dummy input modules (such as GvrPointerInputModule or StandaloneInputModule)
            var modules = es.GetComponents<BaseInputModule>();
            foreach (var mod in modules)
            {
                if (mod != null)
                {
#if ENABLE_INPUT_SYSTEM
                    if (!(mod is InputSystemUIInputModule))
                    {
                        DestroyImmediate(mod);
                    }
#else
                    if (!(mod is StandaloneInputModule))
                    {
                        DestroyImmediate(mod);
                    }
#endif
                }
            }

#if ENABLE_INPUT_SYSTEM
            if (es.GetComponent<InputSystemUIInputModule>() == null)
            {
                es.gameObject.AddComponent<InputSystemUIInputModule>();
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
