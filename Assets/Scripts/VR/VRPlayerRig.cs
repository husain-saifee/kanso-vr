using UnityEngine;
using DePhobia.Core;

namespace DePhobia.VR
{
    /// <summary>
    /// Unified modern OpenXR Player Rig for DePhobia.
    /// Provides dual-input locomotion (VR controllers + desktop WASD fallback),
    /// snap/smooth turning, comfort vignette, and gaze pitch tracking.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class VRPlayerRig : MonoBehaviour
    {
        [Header("Camera & Tracking")]
        public Transform headCamera;
        public float playerEyeHeight = 1.65f;

        [Header("Locomotion")]
        public float moveSpeed = 2.2f;
        public float gravity = -9.81f;
        public bool allowGazeWalkFallback = false;
        public float gazeWalkMinPitch = 30f;
        public float gazeWalkMaxPitch = 85f;

        [Header("Turning")]
        public bool useSnapTurn = true;
        public float snapTurnAngle = 45f;
        public float smoothTurnSpeed = 60f;

        [Header("Comfort & Safety")]
        public VRComfortVignette comfortVignette;
        public VRInGamePauseMenu pauseMenu;

        private CharacterController _characterController;
        private Vector3 _velocity;
        private bool _snapTurnCooldown = false;
        private float _snapCooldownTimer = 0f;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();

            if (headCamera == null)
            {
                var cam = GetComponentInChildren<Camera>();
                if (cam != null) headCamera = cam.transform;
            }

            if (comfortVignette == null)
            {
                comfortVignette = GetComponentInChildren<VRComfortVignette>();
                if (comfortVignette == null)
                {
                    var vigObj = new GameObject("VRComfortVignette");
                    vigObj.transform.SetParent(transform, false);
                    comfortVignette = vigObj.AddComponent<VRComfortVignette>();
                }
            }

            if (pauseMenu == null)
            {
                pauseMenu = GetComponentInChildren<VRInGamePauseMenu>();
                if (pauseMenu == null)
                {
                    pauseMenu = gameObject.AddComponent<VRInGamePauseMenu>();
                    pauseMenu.vrCamera = headCamera;
                }
            }

            LoadComfortPreferences();
        }

        private void LoadComfortPreferences()
        {
            var sm = TherapySettingsManager.Instance;
            if (sm != null)
            {
                useSnapTurn = sm.snapTurn;
                snapTurnAngle = sm.snapTurnAngle;
                if (comfortVignette != null)
                {
                    comfortVignette.enabled = sm.comfortVignetteEnabled;
                }
            }
        }

        private void Update()
        {
            if (pauseMenu != null && pauseMenu.isPaused)
                return;

            HandleTurning();
            HandleMovement();
            HandleGravity();
        }

        private void HandleTurning()
        {
            // Input from right VR thumbstick or keyboard Q/E / Horizontal arrows
            float turnInput = 0f;

            if (Input.GetKey(KeyCode.E)) turnInput = 1f;
            else if (Input.GetKey(KeyCode.Q)) turnInput = -1f;
            else turnInput = Input.GetAxis("Horizontal"); // Right stick or A/D

            if (useSnapTurn)
            {
                if (_snapTurnCooldown)
                {
                    _snapCooldownTimer -= Time.deltaTime;
                    if (_snapCooldownTimer <= 0f && Mathf.Abs(turnInput) < 0.2f)
                    {
                        _snapTurnCooldown = false;
                    }
                }
                else if (Mathf.Abs(turnInput) > 0.5f)
                {
                    float angle = Mathf.Sign(turnInput) * snapTurnAngle;
                    transform.Rotate(0, angle, 0);
                    _snapTurnCooldown = true;
                    _snapCooldownTimer = 0.35f; // prevent rapid continuous snapping
                }
            }
            else
            {
                // Smooth turn
                if (Mathf.Abs(turnInput) > 0.1f)
                {
                    transform.Rotate(0, turnInput * smoothTurnSpeed * Time.deltaTime, 0);
                }
            }
        }

        private void HandleMovement()
        {
            Vector3 moveDirection = Vector3.zero;

            // 1. Desktop Keyboard WASD / Left VR Thumbstick
            float moveX = 0f;
            float moveZ = 0f;

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) moveZ += 1f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) moveZ -= 1f;
            if (Input.GetKey(KeyCode.D)) moveX += 1f;
            if (Input.GetKey(KeyCode.A)) moveX -= 1f;

            // Controller vertical axis
            float vertAxis = Input.GetAxis("Vertical");
            if (Mathf.Abs(vertAxis) > 0.15f) moveZ += vertAxis;

            // Direction relative to head orientation
            if (headCamera != null)
            {
                Vector3 forward = headCamera.forward;
                forward.y = 0;
                forward.Normalize();

                Vector3 right = headCamera.right;
                right.y = 0;
                right.Normalize();

                moveDirection = (forward * moveZ + right * moveX).normalized;
            }

            // 2. Gaze-look Walking Fallback (Original Cardboard Mechanic)
            if (allowGazeWalkFallback && moveDirection.sqrMagnitude < 0.01f && IsLookingDown(gazeWalkMinPitch, gazeWalkMaxPitch))
            {
                if (headCamera != null)
                {
                    Vector3 forward = headCamera.forward;
                    forward.y = 0;
                    moveDirection = forward.normalized;
                }
            }

            // Execute movement via CharacterController
            bool isMoving = moveDirection.sqrMagnitude > 0.01f;
            if (isMoving)
            {
                _characterController.Move(moveDirection * (moveSpeed * Time.deltaTime));
            }

            // Trigger Comfort Vignette when moving
            if (comfortVignette != null)
            {
                comfortVignette.SetMotionActive(isMoving);
            }
        }

        private void HandleGravity()
        {
            if (_characterController.isGrounded && _velocity.y < 0)
            {
                _velocity.y = -2f;
            }
            else
            {
                _velocity.y += gravity * Time.deltaTime;
            }
            _characterController.Move(_velocity * Time.deltaTime);
        }

        /// <summary>
        /// Returns the head pitch angle in degrees (positive = tilted downward).
        /// </summary>
        public float GetHeadPitch()
        {
            if (headCamera == null) return 0f;
            float xRot = headCamera.eulerAngles.x;
            if (xRot > 180f) xRot -= 360f; // Range: -180 to +180
            return xRot;
        }

        /// <summary>
        /// Checks whether the user is currently looking downward within the specified angle range.
        /// Essential for Acrophobia exposure therapy.
        /// </summary>
        public bool IsLookingDown(float minAngle = 30f, float maxAngle = 75f)
        {
            float pitch = GetHeadPitch();
            return pitch >= minAngle && pitch <= maxAngle;
        }

        /// <summary>
        /// Calibrates head height for seated or standing mode.
        /// </summary>
        public void RecenterHeight()
        {
            if (headCamera != null && _characterController != null)
            {
                float deltaY = playerEyeHeight - headCamera.localPosition.y;
                transform.position += new Vector3(0, deltaY, 0);
            }
        }
    }
}
