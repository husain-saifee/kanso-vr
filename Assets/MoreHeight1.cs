using UnityEngine;
using DePhobia.Core;

/// <summary>
/// Controls water elevation dynamics in the Aquaphobia simulation.
/// Connects water rise speed, interval, and duration to TherapySettingsManager,
/// and provides smooth sinusoidal tide elevation, fluid surface tension oscillation,
/// and physical submersion acoustics via TherapyAudioManager.
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

    private float _baseWaterY = 0f;
    private const float MAX_WATER_Y = 1.75f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        tstart = Time.time;
        _baseWaterY = transform.position.y;

        var sm = TherapySettingsManager.Instance;
        speed = (sm != null) ? sm.aquaWaterSpeed : 0.06f;
        timerate = (sm != null) ? sm.aquaRiseAfter : 2.0f;
        vtime = (sm != null) ? sm.aquaRiseDuration : 2.0f;
    }

    void Update()
    {
        time = Time.time - tstart;

        // Continually update submersion acoustics in TherapyAudioManager
        float relativeSubmersion = Mathf.InverseLerp(0.0f, 1.70f, _baseWaterY);
        if (TherapyAudioManager.HasInstance && TherapyAudioManager.Instance != null)
        {
            TherapyAudioManager.Instance.SetWaterSubmersion(relativeSubmersion);
        }

        if (_baseWaterY < MAX_WATER_Y)
        {
            if (flag1 == 1 && (time - mstart) >= timerate)
            {
                vstart = time;
                flag = 1;
                flag1 = 0;

                // Play gentle shore/water lap sound when tide elevation begins surging
                if (TherapyAudioManager.HasInstance && TherapyAudioManager.Instance != null)
                {
                    TherapyAudioManager.Instance.PlayWaterLap(0.40f);
                }
            }

            if (flag == 1)
            {
                float elapsedInInterval = time - vstart;
                if (elapsedInInterval <= vtime)
                {
                    // Smooth sinusoidal bell-curve velocity (eases in, peaks, eases out)
                    float intervalProgress = Mathf.Clamp01(elapsedInInterval / vtime);
                    float smoothVelFactor = Mathf.Sin(intervalProgress * Mathf.PI) * 1.4f + 0.2f;
                    _baseWaterY += speed * smoothVelFactor * Time.deltaTime;
                    _baseWaterY = Mathf.Min(_baseWaterY, MAX_WATER_Y);
                }
                else
                {
                    flag = 0;
                    mstart = time;
                    flag1 = 1;
                }
            }

            tmeas = time;
            PatientSessionManager.Instance.UpdatePeakMetric(_baseWaterY);
        }

        // Calibrated fluid surface tension:
        // Gentle micro-tide swell (approx 8mm) gives the ocean physical breathing presence
        float surfaceTide = Mathf.Sin(Time.time * 1.6f) * 0.007f + Mathf.Cos(Time.time * 2.4f) * 0.004f;
        transform.position = new Vector3(transform.position.x, _baseWaterY + surfaceTide, transform.position.z);
    }
}
