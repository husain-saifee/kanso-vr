using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DePhobia.Core;
using DePhobia.VR;

/// <summary>
/// Core Acrophobia exposure therapy mechanic.
/// Looking downward at the city ascends the hoverboard, rewarding the patient for confronting the height.
/// Looking away for longer than wait time causes the hoverboard to gently descend until the patient refocuses.
/// Supports both OpenXR head tracking and desktop mouse/keyboard look (Right-click drag, mouse move, Up/Down arrows).
/// Employs controlled kinematic positioning to guarantee no freefall drops, zero physics jitter, and no velocity warnings.
/// </summary>
public class vrlook : MonoBehaviour
{
    private TextMeshProUGUI maxheight;
    public GameObject Max;
    public AudioClip a1;
    public AudioClip a2;
    public AudioClip a3;
    public AudioClip b1;
    public AudioClip b2;
    public AudioClip b3;
    public AudioClip b4;
    public AudioClip b5;

    private float tlast;
    private float di;
    private float d;
    private float lastplay;
    private float lastpl;
    private float endingstate = 0;

    public Transform vrCamera;
    public float toggleAngle = 30f;
    public float speed = 2f;
    public static bool SeeingDown;
    public Rigidbody rd;
    public Text timetxt;
    private CharacterController cc;
    private float Times;
    public static int k = 0;
    public static int started = 0;
    private float TimePtleftseeingdown;
    private float TimePtStartseeingdown;
    public static float Tim;
    public static float Timer;
    public static float Timer1;
    public static int e;

    public float Timeg = 4f;
    public static float TTimeg;
    public static float TTimeg1;

    public float focusTimenotfalling = 0.6f;
    public float focusTimefalling = 0.6f;

    public const float CENTER_SPAWN_X = 298.0f;
    public const float CENTER_SPAWN_Z = 86.0f;
    private const float MIN_HOVERBOARD_Y = 32.0f;
    private const float MAX_HOVERBOARD_Y = 360.0f;

    // Desktop pitch/yaw tracking
    private float _cameraPitch = 0f;
    private float _hoverYaw = 45f;

    void Start()
    {
        var sm = TherapySettingsManager.Instance;
        if (sm != null)
        {
            Timeg = sm.acroWaitTime;
            focusTimenotfalling = sm.acroStillFocusTime;
        }

        TTimeg1 = focusTimenotfalling;
        rd = GetComponent<Rigidbody>();
        if (rd != null)
        {
            rd.useGravity = false;
            rd.isKinematic = true;
            rd.constraints = RigidbodyConstraints.FreezeAll;
        }

        cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        Times = Time.time;
        TimePtStartseeingdown = 0f;
        TimePtleftseeingdown = 0f;

        if (Max != null)
        {
            maxheight = Max.GetComponent<TextMeshProUGUI>();
            Max.SetActive(false);
        }

        if (vrCamera == null && Camera.main != null)
        {
            vrCamera = Camera.main.transform;
        }

        // Initialize position in open avenue (298, 32, 86) with clear airspace and wide clearance from building walls
        transform.position = new Vector3(CENTER_SPAWN_X, Mathf.Max(transform.position.y, MIN_HOVERBOARD_Y), CENTER_SPAWN_Z);
        transform.rotation = Quaternion.Euler(0f, 45f, 0f);
        _hoverYaw = 45f;

        ValChange.hf = (int)transform.position.y;
        ValChange.HighestHigh = transform.position.y;
        endingstate = 0;
        e = 0;
        started = 0;

    }

    void Update()
    {
        TTimeg = Timeg;
        Tim = Time.time - Times;

        HandleDesktopLook();

        bool soundEnabled = TherapySettingsManager.Instance == null || TherapySettingsManager.Instance.acroSoundEnabled;
        if (soundEnabled && rd != null)
        {
            PlayAmbientSounds();
        }

        if (endingstate == 0)
        {
            if (ValChange.hf <= 240 && ValChange.hf > 120 && d != 1)
            {
                Timeg = 2 * Timeg;
                d = 1;
            }
            if (ValChange.hf > 240 && di != 1)
            {
                Timeg = 3 * Timeg / 2;
                di = 1;
            }

            Timer = Tim - TimePtleftseeingdown;
            Timer1 = Tim - TimePtStartseeingdown;

            // Pitch check for gaze downward confrontation
            if (vrCamera != null)
            {
                float pitch = vrCamera.eulerAngles.x;
                if (pitch > 180f) pitch -= 360f;
                SeeingDown = (pitch >= toggleAngle && pitch <= 85.0f);
            }
            else
            {
                SeeingDown = false;
            }

            float currentY = transform.position.y;

            if (SeeingDown)
            {
                // Looking down at the city
                TimePtleftseeingdown = Tim;

                if (Timer1 >= focusTimenotfalling)
                {
                    started = 1;
                    k = 0;
                    // Ascend smoothly towards summit
                    currentY += speed * Time.deltaTime;
                }
            }
            else
            {
                // Looking away / resting
                TimePtStartseeingdown = Tim;

                if (Timer >= Timeg && currentY > MIN_HOVERBOARD_Y)
                {
                    k = 0;
                    // Controlled gentle descent
                    currentY -= 1.0f * Time.deltaTime;
                }
                else
                {
                    k = 1; // holding steady
                }
            }

            // Clamping bounds
            currentY = Mathf.Clamp(currentY, MIN_HOVERBOARD_Y, MAX_HOVERBOARD_Y);
            transform.position = new Vector3(CENTER_SPAWN_X, currentY, CENTER_SPAWN_Z);

            ValChange.hf = (int)currentY;
            ValChange.HighestHigh = Mathf.Max(ValChange.HighestHigh, currentY);
            PatientSessionManager.Instance.UpdatePeakMetric(ValChange.HighestHigh);

            tlast = Tim;

            if (currentY >= MAX_HOVERBOARD_Y && PatientSessionManager.Instance.GetCurrentSessionDuration() > 20f)
            {
                if (Max != null)
                {
                    Max.SetActive(true);
                    if (maxheight != null)
                    {
                        maxheight.color = ThemeTokens.AccentSage;
                        maxheight.text = "Summit Reached (360m)";
                    }
                }
                HandleEndingSequence();
            }
            else
            {
                // Ensure Max height text is strictly hidden before summit is reached
                if (endingstate == 0 && Max != null && Max.activeSelf)
                {
                    Max.SetActive(false);
                }
            }
        }
    }

    /// <summary>
    /// Enables intuitive mouse and keyboard view controls on desktop:
    /// - Right-click drag (or Left-click drag) to look around in 360 degrees
    /// - Moving mouse up/down tilts head pitch; left/right turns yaw
    /// - Up/Down Arrow keys (or W/S) tilt head up/down directly
    /// </summary>
    private void HandleDesktopLook()
    {
        // Keyboard tilt controls (Up/Down arrows or W/S)
        float keyboardPitchDelta = 0f;
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) keyboardPitchDelta += 45f * Time.deltaTime;
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) keyboardPitchDelta -= 45f * Time.deltaTime;

        float mouseX = 0f;
        float mouseY = 0f;

        // Mouse look when holding right-click, left-click drag, or cursor unlocked
        if (Input.GetMouseButton(1) || Input.GetMouseButton(0))
        {
            mouseX = Input.GetAxis("Mouse X") * 3.0f;
            mouseY = Input.GetAxis("Mouse Y") * 3.0f;
        }

        if (Mathf.Abs(mouseY) > 0.01f || Mathf.Abs(keyboardPitchDelta) > 0.01f)
        {
            _cameraPitch += (mouseY != 0f ? -mouseY : keyboardPitchDelta);
            _cameraPitch = Mathf.Clamp(_cameraPitch, -85f, 85f);

            if (vrCamera != null)
            {
                vrCamera.localEulerAngles = new Vector3(_cameraPitch, 0f, 0f);
            }
        }

        if (Mathf.Abs(mouseX) > 0.01f)
        {
            _hoverYaw += mouseX;
            transform.rotation = Quaternion.Euler(0f, _hoverYaw, 0f);
        }
    }

    private void PlayAmbientSounds()
    {
        // Gentle ambient bird song / wind cues at low volume (never sudden loud horn honks)
        if (Tim - lastplay > 8f)
        {
            int i = Random.Range(1, 4);
            AudioClip clip = i switch
            {
                1 => a1,
                2 => a2,
                _ => a3
            };
            if (clip != null && rd != null)
            {
                AudioSource.PlayClipAtPoint(clip, transform.position, 0.35f);
            }
            lastplay = Tim;
        }
    }

    private void HandleEndingSequence()
    {
        if (endingstate == 1) return;
        endingstate = 1;
        e = 1;

        if (rd != null && !rd.isKinematic)
        {
            rd.velocity = Vector3.zero;
        }

        if (Max != null)
        {
            Max.SetActive(false);
        }

        var rig = FindObjectOfType<VRPlayerRig>();
        if (rig != null)
        {
            rig.SendHapticImpulse(0.3f, 0.1f, UnityEngine.XR.XRNode.RightHand);
        }

        // Show peaceful End Ritual
        TherapySessionRitual.ShowEndRitual("High Elevation", "Summit Reached • 360m Altitude", () =>
        {
            var startVr3 = FindObjectOfType<StartVr3>();
            if (startVr3 != null)
            {
                startVr3.SaveAndReturn();
            }
            else
            {
                PatientSessionManager.Instance.EndAndSaveSession(completed: true, extraParams: "Acrophobia Summit 360m Reached");
                VRFadeManager.Instance.FadeAndLoadScene(0, 0.5f);
            }
        });
    }
}
