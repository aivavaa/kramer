using UnityEngine;

public class Jump : MonoBehaviour
{
    Rigidbody rigidbody;

    [Tooltip("Jump takeoff speed in m/s, applied directly to vertical velocity (not a force). Consistent height every time, independent of mass/drag/framerate.")]
    public float jumpSpeed = 8f;

    public event System.Action Jumped;

    [SerializeField, Tooltip("Prevents jumping when the transform is in mid-air.")]
    GroundCheck groundCheck;

    // Set on the frame the button is pressed, consumed on the next FixedUpdate.
    // This is a small input buffer: without it, a press can land between FixedUpdate
    // calls and get missed, which feels bad for a fast-paced controller.
    bool jumpRequested;

    void Reset()
    {
        // Try to get groundCheck.
        groundCheck = GetComponentInChildren<GroundCheck>();
    }

    void Awake()
    {
        // Get rigidbody.
        rigidbody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Capture input every frame; consumption happens in FixedUpdate.
        if (Input.GetButtonDown("Jump"))
        {
            jumpRequested = true;
        }
    }

    void FixedUpdate()
    {
        if (!jumpRequested)
        {
            return;
        }

        // Only clear the request once we've actually had a chance to act on it while grounded,
        // or immediately if we can't jump at all right now.
        if (!groundCheck || groundCheck.isGrounded)
        {
            Vector3 velocity = rigidbody.linearVelocity;
            velocity.y = jumpSpeed; // direct assignment, not AddForce: consistent height every jump
            rigidbody.linearVelocity = velocity;

            Jumped?.Invoke();
            jumpRequested = false;
        }
    }
}