using UnityEngine;
using DePhobia.Core;

public class xnegmove : MonoBehaviour
{
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

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        tstart = Time.time;

        var sm = TherapySettingsManager.Instance;
        speed = (sm != null) ? sm.clausSpeed : 0.05f;
        timerate = (sm != null) ? sm.clausMoveAfter : 3.0f;
        vtime = (sm != null) ? sm.clausMoveDuration : 2.0f;
    }

    void Update()
    {
        time = Time.time - tstart;

        if (transform.position.x > 2.8f)
        {
            if (flag1 == 1 && (time - mstart) >= timerate)
            {
                if (rb != null) rb.velocity = new Vector3(-1, 0, 0) * speed;
                vstart = time;
                flag = 1;
                flag1 = 0;
            }

            if (flag == 1 && (time - vstart >= vtime))
            {
                if (rb != null) rb.velocity = Vector3.zero;
                flag = 0;
                mstart = time;
                flag1 = 1;
            }
            tmeas = time;
        }
        else
        {
            if (rb != null) rb.velocity = Vector3.zero;
        }
    }
}
