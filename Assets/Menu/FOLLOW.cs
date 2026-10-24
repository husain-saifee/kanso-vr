using UnityEngine;

/// <summary>
/// Neutralized external link handler.
/// Prevents telemetry and unauthorized external network calls in compliance
/// with clinical and patient privacy standards.
/// </summary>
public class FOLLOW : MonoBehaviour
{
    public void GITHUB()
    {
        Debug.Log("[DePhobia Privacy] External link disabled in clinical mode.");
    }

    public void INSTA()
    {
        Debug.Log("[DePhobia Privacy] External link disabled in clinical mode.");
    }

    public void Gmail()
    {
        Debug.Log("[DePhobia Privacy] External data solicitation disabled in clinical mode.");
    }
}
