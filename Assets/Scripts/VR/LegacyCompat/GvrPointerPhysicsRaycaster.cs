using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Lightweight compatibility adapter for legacy scenes referencing GvrPointerPhysicsRaycaster.
/// Inherits from standard Unity PhysicsRaycaster so physics raycasts and clicks work seamlessly
/// without requiring the obsolete GoogleVR SDK.
/// </summary>
public class GvrPointerPhysicsRaycaster : PhysicsRaycaster
{
    // Inherits standard Unity 3D physics raycasting
}
