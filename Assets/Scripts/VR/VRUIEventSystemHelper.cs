using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace DePhobia.VR
{
    /// <summary>
    /// Automatically upgrades EventSystem to InputSystemUIInputModule when the New Input System
    /// is active, ensuring mouse clicks, touch, and UI interactions always work smoothly across all scenes.
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

        public static void EnsureEventSystemModule()
        {
            var es = FindObjectOfType<EventSystem>();
            if (es == null)
            {
                var esGo = new GameObject("EventSystem");
                es = esGo.AddComponent<EventSystem>();
            }

#if ENABLE_INPUT_SYSTEM
            var standalone = es.GetComponent<StandaloneInputModule>();
            var inputSystemModule = es.GetComponent<InputSystemUIInputModule>();

            if (inputSystemModule == null)
            {
                if (standalone != null)
                {
                    Destroy(standalone);
                }
                es.gameObject.AddComponent<InputSystemUIInputModule>();
            }
#endif
        }
    }
}
