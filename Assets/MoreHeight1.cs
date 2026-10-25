using UnityEngine;
using DePhobia.Core;

/// <summary>
/// Controls water elevation dynamics in the Aquaphobia simulation.
/// Uncomments and connects water rise speed, interval, and duration to TherapySettingsManager.
/// </summary>
public class MoreHeight1 : MonoBehaviour
{
    public float speed;
    public float timerate;
    public float vtime;

    public static float time;
    public static float tmeas;

    private Rigidbody rb;
    private float vstart = 0;
    private float tstart = 0;
    private float mstart = 0;
    private int flag = 0;
    private int flag1 = 1;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        tstart = Time.time;

        var sm = TherapySettingsManager.Instance;
        speed = (sm != null) ? sm.aquaWaterSpeed : 0.06f;
        timerate = (sm != null) ? sm.aquaRiseAfter : 2.0f;
        vtime = (sm != null) ? sm.aquaRiseDuration : 2.0f;
    }

    void Update()
    {
        time = Time.time - tstart;

        if (transform.position.y < 1.8f)
        {
            if (flag1 == 1 && (time - mstart) >= timerate)
            {
                if (rb != null && !rb.isKinematic)
                {
                    rb.velocity = new Vector3(0, speed, 0);
                }
                else
                {
                    transform.position += new Vector3(0, speed * Time.deltaTime, 0);
                }
                vstart = time;
                flag = 1;
                flag1 = 0;
            }

            if (flag == 1 && (time - vstart >= vtime))
            {
                if (rb != null && !rb.isKinematic) rb.velocity = Vector3.zero;
                flag = 0;
                mstart = time;
                flag1 = 1;
            }
            tmeas = time;
            PatientSessionManager.Instance.UpdatePeakMetric(transform.position.y);
        }
        else
        {
            if (rb != null && !rb.isKinematic) rb.velocity = Vector3.zero;
        }
    }
}
