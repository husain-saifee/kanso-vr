using UnityEngine;
using DePhobia.Core;

public class xposmove : MonoBehaviour
{
    public static bool sessionStarted = false;

    public float speed;
    public float timerate;
    public float vtime;

    private Rigidbody rb;
    public float time;
    private float vstart;
    private float tstart;
    private float mstart;
    private int flag;
    private int flag1 = 1;

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

        // Safety clamp: do not move beyond safe threshold (-2.85m)
        if (transform.position.x >= -2.85f)
        {
            if (rb != null && !rb.isKinematic) rb.velocity = Vector3.zero;
            return;
        }

        time = Time.time - tstart;

        if (transform.position.x < -2.85f)
        {
            if (flag1 == 1 && (time - mstart) >= timerate)
            {
                if (rb != null && !rb.isKinematic)
                {
                    rb.velocity = new Vector3(1, 0, 0) * speed;
                }
                else
                {
                    transform.position += new Vector3(1, 0, 0) * speed * Time.deltaTime;
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
        }
        else
        {
            if (rb != null && !rb.isKinematic) rb.velocity = Vector3.zero;
        }
    }
}
