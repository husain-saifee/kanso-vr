using UnityEngine;

/// <summary>
/// Maintains wall alignment offset relative to moving room boundary in Claustrophobia.
/// </summary>
public class wallstable : MonoBehaviour
{
    public GameObject a;

    void Update()
    {
        if (a != null)
        {
            transform.position = new Vector3(a.transform.position.x - 0.2f, a.transform.position.y, a.transform.position.z);
        }
    }
}
