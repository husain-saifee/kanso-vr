using UnityEngine;
using UnityEngine.XR;
using DePhobia.Core;

namespace DePhobia.VR
{
    /// <summary>
    /// Unified modern OpenXR Player Rig for DePhobia.
    /// Provides dual-input locomotion (VR controllers + desktop WASD fallback),
    /// snap/smooth turning, comfort vignette, seated mode recalibration,
    /// haptic feedback, and gaze pitch tracking.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class VRPlayerRig : MonoBehaviour
    {
        [Header("Camera & Tracking")]
        public Transform headCamera;
        public float playerEyeHeight = 1.65f;

        [Header("Locomotion")]
        public bool enableLocomotion = true;
        public bool enableGravity = true;
        public float moveSpeed = 2.0f;
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
        private bool _recenterPressedLastFrame = false;

        private float _desktopPitch = 0f;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();

            if (headCamera == null)
            {
                var cam = GetComponentInChildren<Camera>();
                if (cam != null) headCamera = cam.transform;
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

        private void OnEnable()
        {
            if (TherapySettingsManager.Instance != null)
            {
                TherapySettingsManager.Instance.OnSettingsChanged += LoadComfortPreferences;
            }
        }

        private void OnDisable()
        {
            if (TherapySettingsManager.HasInstance && TherapySettingsManager.Instance != null)
            {
                TherapySettingsManager.Instance.OnSettingsChanged -= LoadComfortPreferences;
            }
        }

        public void LoadComfortPreferences()
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
                ApplySeatedHeightOffset();
            }
        }

        private void Update()
        {
            CheckGlobalShortcuts();

            if (pauseMenu != null && pauseMenu.isPaused)
                return;

            HandleTurning();
            if (enableLocomotion)
            {
                HandleMovement();
            }
            if (enableGravity)
            {
                HandleGravity();
            }
        }

        private void CheckGlobalShortcuts()
        {
            // Check controller recenter shortcut (e.g. thumbstick click or R key)
            bool recenterRequested = Input.GetKeyDown(KeyCode.R);
            var rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            if (rightHand.isValid && rightHand.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out bool thumbClick))
            {
                if (thumbClick && !_recenterPressedLastFrame)
                {
                    recenterRequested = true;
                }
                _recenterPressedLastFrame = thumbClick;
            }

            if (recenterRequested)
            {
                RecenterView();
                SendHapticImpulse(0.3f, 0.1f, XRNode.RightHand);
            }
        }

        private void HandleTurning()
        {
            // Input from right VR thumbstick or keyboard Q/E / Horizontal arrows
            float turnInput = 0f;

            // Desktop mouse horizontal turn
            if (Input.GetMouseButton(1) || Input.GetMouseButton(0))
            {
                float mouseX = Input.GetAxis("Mouse X");
                float mouseY = Input.GetAxis("Mouse Y");
                if (Mathf.Abs(mouseX) > 0.01f) transform.Rotate(0, mouseX * 3.0f, 0);
                if (headCamera != null && Mathf.Abs(mouseY) > 0.01f)
                {
                    _desktopPitch -= mouseY * 3.0f;
                    _desktopPitch = Mathf.Clamp(_desktopPitch, -85f, 85f);
                    headCamera.localEulerAngles = new Vector3(_desktopPitch, 0f, 0f);
                }
            }

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
                    SendHapticImpulse(0.15f, 0.04f, XRNode.RightHand);
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
            if (isMoving && _characterController != null && _characterController.enabled)
            {
                _characterController.Move(moveDirection * (moveSpeed * Time.deltaTime));
            }


        }

        private void HandleGravity()
        {
            if (_characterController == null || !_characterController.enabled) return;

            if (_characterController.isGrounded && _velocity.y < 0)
            {
                _velocity.y = -2f;
            }
            else
            {
                _velocity.y += gravity * Time.deltaTime;
                _velocity.y = Mathf.Max(_velocity.y, -12f);
            }
            _characterController.Move(_velocity * Time.deltaTime);

            // Floor safety clamp
            if (transform.position.y < 0.2f) { _velocity.y = 0f; Vector3 sp = transform.position; sp.y = 1.0f; transform.position = sp; }
        }

        /// <summary>
        /// Sends gentle, informational haptic impulse to the specified VR controller.
        /// </summary>
        public void SendHapticImpulse(float amplitude = 0.2f, float duration = 0.06f, XRNode node = XRNode.RightHand)
        {
            var device = InputDevices.GetDeviceAtXRNode(node);
            if (device.isValid && device.TryGetHapticCapabilities(out HapticCapabilities caps) && caps.supportsImpulse)
            {
                device.SendHapticImpulse(0u, Mathf.Clamp01(amplitude), duration);
            }
        }

        /// <summary>
        /// Recalibrates head position and yaw to face forward comfortably at eye level.
        /// Fully supports seated and standing transitions.
        /// </summary>
        public void RecenterView()
        {
            if (headCamera != null)
            {
                // Reset yaw rotation
                Vector3 headEuler = headCamera.eulerAngles;
                transform.rotation = Quaternion.Euler(0, headEuler.y, 0) * transform.rotation;

                // Adjust seated height
                ApplySeatedHeightOffset();
            }

            // Also recenter active UI canvases
            var canvasHelpers = FindObjectsOfType<VRWorldSpaceCanvasHelper>();
            foreach (var helper in canvasHelpers)
            {
                helper.RecenterToPlayer();
            }

            Debug.Log("[VRPlayerRig] View and height successfully recentered.");
        }

        private void ApplySeatedHeightOffset()
        {
            var sm = TherapySettingsManager.Instance;
            if (sm != null && headCamera != null && _characterController != null)
            {
                float targetEye = sm.seatedMode ? (playerEyeHeight - sm.seatedHeightOffset) : playerEyeHeight;
                float currentEye = headCamera.localPosition.y;
                float deltaY = targetEye - currentEye;

                if (Mathf.Abs(deltaY) > 0.05f)
                {
                    _characterController.stepOffset = 0.3f;
                    transform.position += new Vector3(0, deltaY, 0);
                }
            }
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
            RecenterView();
        }
    }
}
