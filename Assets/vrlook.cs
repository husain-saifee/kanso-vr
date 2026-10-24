using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DePhobia.Core;

/// <summary>
/// Core Acrophobia exposure therapy mechanic.
/// Looking downward at the city ascends the hoverboard, rewarding the patient for confronting the height.
/// Looking away for longer than wait time causes the hoverboard to descend until the patient refocuses.
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

    private const float MIN_HOVERBOARD_Y = 27.04f;

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
        cc = GetComponent<CharacterController>();
        Times = Time.time;

        if (Max != null)
        {
            maxheight = Max.GetComponent<TextMeshProUGUI>();
            Max.SetActive(false);
        }

        if (vrCamera == null && Camera.main != null)
        {
            vrCamera = Camera.main.transform;
        }
    }

    void Update()
    {
        TTimeg = Timeg;
        Tim = Time.time - Times;

        bool soundEnabled = TherapySettingsManager.Instance == null || TherapySettingsManager.Instance.acroSoundEnabled;
        if (soundEnabled && rd != null)
        {
            PlayAmbientSounds();
        }

        if (ValChange.HighestHigh <= 360 && endingstate == 0)
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

            if (rd != null && rd.transform.position.y <= MIN_HOVERBOARD_Y)
            {
                rd.isKinematic = true;
                rd.transform.position = new Vector3(rd.transform.position.x, MIN_HOVERBOARD_Y, rd.transform.position.z);
                k = 1;
            }

            // Robust 6DoF pitch check
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

            if (SeeingDown)
            {
                if (rd != null && rd.isKinematic)
                {
                    Timer1 = Tim - TimePtStartseeingdown;
                    if (Timer1 >= focusTimenotfalling)
                    {
                        rd.isKinematic = false;
                        k = 0;
                        rd.velocity = new Vector3(0, speed, 0);
                        TimePtleftseeingdown = Tim;
                    }
                }
                else if (rd != null && !rd.isKinematic)
                {
                    if (Tim - TimePtStartseeingdown >= focusTimefalling)
                    {
                        started = 1;
                        rd.isKinematic = false;
                        k = 0;
                        rd.velocity = new Vector3(0, speed, 0);
                        TimePtleftseeingdown = Tim;
                    }
                }
            }
            else
            {
                // Looking away from the abyss
                if (rd != null)
                {
                    if (Timer >= Timeg && !(rd.transform.position.y <= MIN_HOVERBOARD_Y))
                    {
                        rd.isKinematic = false;
                        k = 0;
                    }
                    else
                    {
                        rd.isKinematic = true;
                        k = 1;
                    }
                }
                TimePtStartseeingdown = Tim;
            }

            tlast = Tim;
            PatientSessionManager.Instance.UpdatePeakMetric(ValChange.HighestHigh);
        }
        else
        {
            // Ceiling Reached (> 360m) - Ending Sequence
            HandleEndingSequence();
        }
    }

    private void PlayAmbientSounds()
    {
        if (Tim - lastplay > 5f)
        {
            int i = Random.Range(1, 4);
            AudioClip clip = i switch
            {
                1 => a1,
                2 => a2,
                _ => a3
            };
            if (clip != null)
                AudioSource.PlayClipAtPoint(clip, rd.transform.position, 1f);
            lastplay = Tim;
        }

        if (Tim - lastpl > 7f)
        {
            int j = Random.Range(2, 100);
            Vector3 soundPos = new Vector3(rd.transform.position.x, 15f, rd.transform.position.z);
            AudioClip bClip = null;

            if (j % 11 == 0) bClip = b1;
            else if (j % 3 == 0) bClip = b2;
            else if (j % 2 == 0) bClip = b3;
            else if (j % 6 == 0) bClip = b5;
            else bClip = b4;

            if (bClip != null)
                AudioSource.PlayClipAtPoint(bClip, soundPos, 1.4f);
            lastpl = Tim;
        }
    }

    private void HandleEndingSequence()
    {
        endingstate = 1;
        float elapsedSinceCeiling = Tim - tlast;

        if (Max != null)
        {
            if (elapsedSinceCeiling <= 8f)
            {
                Max.SetActive(true);
                if (maxheight != null)
                {
                    maxheight.color = ((int)elapsedSinceCeiling % 2 == 0) ? new Color32(184, 29, 19, 255) : Color.white;
                    maxheight.text = "MAXIMUM ALTITUDE REACHED!";
                }
            }
            else if (elapsedSinceCeiling > 8f && elapsedSinceCeiling < 30f)
            {
                Max.SetActive(false);
            }
            else if (elapsedSinceCeiling >= 30f && elapsedSinceCeiling <= 59f)
            {
                Max.SetActive(true);
                if (maxheight != null)
                {
                    int remaining = Mathf.Max(0, 60 - (int)elapsedSinceCeiling);
                    maxheight.text = $"Session Concluding in {remaining}s";
                }
            }
        }

        if (elapsedSinceCeiling >= 60f)
        {
            if (rd != null) rd.isKinematic = true;
            if (Max != null)
            {
                Max.SetActive(true);
                if (maxheight != null) maxheight.text = "SESSION COMPLETE!";
            }
            e = 1;
        }
    }
}
