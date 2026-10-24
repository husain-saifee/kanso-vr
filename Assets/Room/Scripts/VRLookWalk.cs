using UnityEngine;
using DePhobia.Core;

/// <summary>
/// Handles locomotion in Claustrophobia and Aquaphobia.
/// Supports modern thumbstick / WASD input alongside the original look-down walk mechanic.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class VRLookWalk : MonoBehaviour
{
    public Transform vrCamera;
    public float toggleAngle = 30.0f;
    public float speed = 2.2f;
    public bool moveForward;
    public bool allowGazeWalk = true;

    private CharacterController cc;

    void Start()
    {
        cc = GetComponent<CharacterController>();
        if (vrCamera == null && Camera.main != null)
        {
            vrCamera = Camera.main.transform;
        }
    }

    void Update()
    {
        Vector3 moveDir = Vector3.zero;

        // 1. Controller / Keyboard WASD movement
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) moveZ = 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) moveZ = -1f;
        if (Input.GetKey(KeyCode.A)) moveX = -1f;
        if (Input.GetKey(KeyCode.D)) moveX = 1f;

        if (vrCamera != null && (Mathf.Abs(moveX) > 0.1f || Mathf.Abs(moveZ) > 0.1f))
        {
            Vector3 forward = vrCamera.forward;
            forward.y = 0;
            forward.Normalize();

            Vector3 right = vrCamera.right;
            right.y = 0;
            right.Normalize();

            moveDir = (forward * moveZ + right * moveX).normalized;
            moveForward = true;
        }
        else if (allowGazeWalk && vrCamera != null)
        {
            // 2. Original Gaze-Walk Mechanic
            float pitch = vrCamera.eulerAngles.x;
            if (pitch > 180f) pitch -= 360f;

            if (pitch >= toggleAngle && pitch < 85.0f)
            {
                moveForward = true;
                Vector3 forward = vrCamera.forward;
                forward.y = 0;
                moveDir = forward.normalized;
            }
            else
            {
                moveForward = false;
            }
        }
        else
        {
            moveForward = false;
        }

        if (moveForward && moveDir.sqrMagnitude > 0.01f)
        {
            cc.SimpleMove(moveDir * speed);
        }
    }
}
