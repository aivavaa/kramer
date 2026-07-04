using System.Collections.Generic;
using UnityEngine;

public class FirstPersonMovement : MonoBehaviour
{
    public float speed = 5;

    [Header("Running")]
    public bool canRun = true;
    public bool IsRunning { get; private set; }
    public float runSpeed = 9;
    [Tooltip("Sprinting is driven entirely by this state, not by input. Set this from whatever system tracks the Manic state.")]
    public bool isManic = false;

    [Header("Acceleration / Friction")]
    [Tooltip("How quickly velocity ramps up toward target speed while grounded.")]
    public float groundAcceleration = 60f;
    [Tooltip("How quickly velocity bleeds off when there's no input (or opposing input) while grounded.")]
    public float groundFriction = 45f;
    [Tooltip("How quickly velocity ramps up toward target speed while airborne. Lower than ground accel so you can't fully redirect mid-air.")]
    public float airAcceleration = 25f;
    [Tooltip("Speed cap applied to air acceleration, independent of ground speed/run speed.")]
    public float airMaxSpeed = 7f;

    [Header("Ground Check")]
    [SerializeField, Tooltip("Shared ground-check component. Assign the same one used by Jump so movement and jumping always agree on grounded state.")]
    GroundCheck groundCheck;
    public bool IsGrounded => groundCheck && groundCheck.isGrounded;

    Rigidbody rigidbody;
    /// <summary> Functions to override movement speed. Will use the last added override. </summary>
    public List<System.Func<float>> speedOverrides = new List<System.Func<float>>();

    void Reset()
    {
        // Try to get groundCheck, same convention as Jump.cs.
        groundCheck = GetComponentInChildren<GroundCheck>();
    }

    void Awake()
    {
        // Get the rigidbody on this.
        rigidbody = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // Update IsRunning from the Manic state (not input).
        IsRunning = canRun && isManic;

        // Get targetMovingSpeed.
        float targetMovingSpeed = IsRunning ? runSpeed : speed;
        if (speedOverrides.Count > 0)
        {
            targetMovingSpeed = speedOverrides[speedOverrides.Count - 1]();
        }

        // Build the wish direction (camera/body relative) from input.
        Vector2 inputAxis = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        Vector3 wishDir = transform.rotation * new Vector3(inputAxis.x, 0f, inputAxis.y);
        if (wishDir.sqrMagnitude > 1f)
        {
            wishDir.Normalize();
        }

        // Split current velocity into horizontal (XZ) and vertical (Y) components.
        Vector3 currentVelocity = rigidbody.linearVelocity;
        Vector3 horizontalVelocity = new Vector3(currentVelocity.x, 0f, currentVelocity.z);

        if (IsGrounded)
        {
            horizontalVelocity = ApplyFriction(horizontalVelocity, groundFriction, Time.fixedDeltaTime);
            horizontalVelocity = Accelerate(horizontalVelocity, wishDir, targetMovingSpeed, groundAcceleration, Time.fixedDeltaTime);
        }
        else
        {
            // No friction in the air, keeps momentum; acceleration is weaker and capped independently.
            horizontalVelocity = Accelerate(horizontalVelocity, wishDir, airMaxSpeed, airAcceleration, Time.fixedDeltaTime);
        }

        // Apply movement, preserving whatever vertical velocity physics/gravity has already produced.
        rigidbody.linearVelocity = new Vector3(horizontalVelocity.x, currentVelocity.y, horizontalVelocity.z);
    }

    // Only slows the velocity down; never used to add speed.
    static Vector3 ApplyFriction(Vector3 velocity, float friction, float deltaTime)
    {
        float speed = velocity.magnitude;
        if (speed <= 0f) return velocity;

        float drop = speed * friction * deltaTime;
        float newSpeed = Mathf.Max(speed - drop, 0f);
        return velocity * (newSpeed / speed);
    }

    // Quake-style acceleration: pushes velocity toward wishDir up to wishSpeed,
    // scaled by how aligned current velocity already is with wishDir.
    // This is what gives strafing/turning a snappy feel instead of a sluggish ramp.
    static Vector3 Accelerate(Vector3 currentVelocity, Vector3 wishDir, float wishSpeed, float acceleration, float deltaTime)
    {
        if (wishDir.sqrMagnitude < 0.0001f)
        {
            return currentVelocity;
        }

        float currentSpeedInWishDir = Vector3.Dot(currentVelocity, wishDir);
        float addSpeed = wishSpeed - currentSpeedInWishDir;
        if (addSpeed <= 0f) return currentVelocity;

        float accelSpeed = Mathf.Min(acceleration * deltaTime * wishSpeed, addSpeed);
        return currentVelocity + wishDir * accelSpeed;
    }
}