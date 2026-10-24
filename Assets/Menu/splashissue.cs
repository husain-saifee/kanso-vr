using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Handles smooth transition once Unity splash screen finishes.
/// </summary>
public class splashissue : MonoBehaviour
{
    public GameObject a;
    public GameObject b;

    void Start()
    {
        if (b != null) b.SetActive(false);
        if (a != null) a.SetActive(true);
    }
}
