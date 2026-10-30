using UnityEngine;
using DePhobia.Core;

/// <summary>
/// Controls negative-direction wall contraction in Claustrophobia.
/// Implements structural resistance and smooth deceleration as walls near the safe boundary (+2.85m),
/// eliminating jarring mechanical stops and providing organic acoustic proximity feedback.
/// </summary>
public class xnegmove : MonoBehaviour
{
    public static bool sessionStarted = false;

    public float speed;
    public float timerate;
    public float vtime;

    private Rigidbody rb;
    public static float time;
    public static float tmeas;

    private float vstart = 0;
    private float tstart = 0;
    private float mstart = 0;
    private int flag = 0;
    private int flag1 = 1;

    private const float SAFE_CLAMP_X = 2.85f;
    private const float INITIAL_SPAWN_X = 6.0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null && !rb.isKinematic)
        {
            rb.velocity = Vector3.zero;
        }
    }

    void Start()
    {
        tstart = Time.time;

        var sm = TherapySettingsManager.Instance;
        speed = (sm != null) ? sm.clausSpeed : 0.05f;
        timerate = (sm != null) ? sm.clausMoveAfter : 3.0f;
        vtime = (sm != null) ? sm.clausMoveDuration : 2.0f;
    }

    void Update()
    {
        if (!sessionStarted)
        {
            if (rb != null && !rb.isKinematic) rb.velocity = Vector3.zero;
            tstart = Time.time;
            return;
        }

        // Continually update room acoustic intimacy based on wall distance
        float normProximity = Mathf.InverseLerp(INITIAL_SPAWN_X, SAFE_CLAMP_X, transform.position.x);
        if (TherapyAudioManager.HasInstance && TherapyAudioManager.Instance != null)
        {
            TherapyAudioManager.Instance.SetRoomProximity(normProximity);
        }

        // Safety clamp: do not move beyond safe threshold (+2.85m)
        if (transform.position.x <= SAFE_CLAMP_X)
        {
            if (rb != null && !rb.isKinematic) rb.velocity = Vector3.zero;
            Vector3 clampedPos = transform.position;
            clampedPos.x = SAFE_CLAMP_X;
            transform.position = clampedPos;
            return;
        }

        time = Time.time - tstart;

        if (transform.position.x > SAFE_CLAMP_X)
        {
            // Deceleration easing as wall approaches safe threshold (last 0.8 meters)
            float distToClamp = transform.position.x - SAFE_CLAMP_X;
            float decelFactor = Mathf.Clamp(distToClamp / 0.8f, 0.20f, 1.0f);
            float effectiveSpeed = speed * decelFactor;

            if (flag1 == 1 && (time - mstart) >= timerate)
            {
                if (rb != null && !rb.isKinematic)
                {
                    rb.velocity = new Vector3(-1, 0, 0) * effectiveSpeed;
                }
                else
                {
                    transform.position += new Vector3(-1, 0, 0) * effectiveSpeed * Time.deltaTime;
                }
                vstart = time;
                flag = 1;
                flag1 = 0;

                // Play subtle structural tension creak as movement begins
                if (TherapyAudioManager.HasInstance && TherapyAudioManager.Instance != null)
                {
                    TherapyAudioManager.Instance.PlayWallCreak(0.35f * decelFactor);
                }
            }

            if (flag == 1 && (time - vstart >= vtime))
            {
                if (rb != null && !rb.isKinematic) rb.velocity = Vector3.zero;
                flag = 0;
                mstart = time;
                flag1 = 1;
            }
            tmeas = time;
        }
        else
        {
            if (rb != null && !rb.isKinematic) rb.velocity = Vector3.zero;
        }
    }
}
