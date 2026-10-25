using UnityEngine;

namespace DePhobia.VR
{
    /// <summary>
    /// Motion vignette completely removed: keeps the screen 100% bright, clear, and unhindered during all movement.
    /// </summary>
    public class VRComfortVignette : MonoBehaviour
    {
        public void SetMotionActive(bool isMoving)
        {
            // Completely disabled: no screen darkening or tunneling during movement
        }
    }
}
