using UnityEngine;

/// <summary>
/// Detects physical jump and crouch gestures via the device accelerometer.
/// Attach to the Player GameObject (same object with CharacterController).
/// </summary>
public class AccelerometerMovement : MonoBehaviour
{
    [Header("Jump Settings")]
    [Tooltip("Upward acceleration threshold to trigger a jump (m/s^2 above gravity).")]
    public float jumpThreshold = 1.8f;
    [Tooltip("Jump force applied to the player.")]
    public float jumpForce = 5f;
    [Tooltip("Cooldown in seconds between jumps to prevent repeated triggers.")]
    public float jumpCooldown = 0.8f;

    [Header("Crouch Settings")]
    [Tooltip("Downward acceleration threshold to trigger a crouch.")]
    public float crouchThreshold = 1.5f;
    [Tooltip("How much the camera height drops when crouching (units).")]
    public float crouchHeightOffset = 0.6f;
    [Tooltip("How fast the crouch/stand transition is.")]
    public float crouchSpeed = 8f;
    [Tooltip("How long the player stays crouched before auto-standing (seconds).")]
    public float crouchDuration = 1.0f;

    [Header("References")]
    [Tooltip("The HeightOffset transform under XRCardboardRig. Used to shift camera down when crouching.")]
    public Transform heightOffsetTransform;

    CharacterController charController;
    float verticalVelocity;
    float lastJumpTime = -10f;
    float crouchTimer;
    bool isCrouching;
    bool isJumping;
    float originalHeightOffsetY;
    float targetHeightOffsetY;

    // Smoothing buffer for accelerometer noise
    Vector3 smoothedAccel;
    const float smoothFactor = 0.5f;
    bool hasAccelerometer;

    void Start()
    {
        charController = GetComponent<CharacterController>();

        // Auto-find HeightOffset if not assigned
        if (heightOffsetTransform == null)
        {
            Transform rig = transform.Find("XRCardboardRig");
            if (rig != null)
            {
                heightOffsetTransform = rig.Find("HeightOffset");
            }
        }

        if (heightOffsetTransform != null)
        {
            originalHeightOffsetY = heightOffsetTransform.localPosition.y;
        }

        targetHeightOffsetY = originalHeightOffsetY;
        smoothedAccel = Input.acceleration;

        // Check if device has accelerometer (will be zero vector in editor)
        hasAccelerometer = SystemInfo.supportsAccelerometer;
    }

    void Update()
    {
        // Skip accelerometer logic entirely if no accelerometer (editor/desktop)
        if (!hasAccelerometer)
        {
            // Allow keyboard/controller testing: Space/Y=jump, C=crouch
            bool jumpPressed = ControllerMapping.Instance != null
                ? ControllerMapping.Instance.GetJumpDown()
                : Input.GetKeyDown(KeyCode.Space);
            if (jumpPressed && charController.isGrounded)
            {
                isJumping = true;
                verticalVelocity = jumpForce;
                lastJumpTime = Time.time;
            }
            if (Input.GetKeyDown(KeyCode.C) && !isCrouching)
            {
                isCrouching = true;
                crouchTimer = crouchDuration;
                targetHeightOffsetY = originalHeightOffsetY - crouchHeightOffset;
            }
        }
        else
        {
            // Smooth the accelerometer data to filter noise
            smoothedAccel = Vector3.Lerp(smoothedAccel, Input.acceleration, smoothFactor);

            float verticalAccel = smoothedAccel.y;

            // In Unity, when phone is in landscape held in a headset:
            // gravity reads approximately (0, -1, 0) on accelerometer.
            // Upward movement adds negative Y acceleration (stronger than gravity).
            // Downward movement adds positive Y acceleration (weaker than gravity).
            float deviationFromGravity = -verticalAccel - 1.0f;

            DetectJump(deviationFromGravity);
            DetectCrouch(deviationFromGravity);
        }

        ApplyGravityAndJump();
        UpdateCrouchState();
        UpdateCrouchHeight();
    }

    void DetectJump(float deviation)
    {
        if (deviation > jumpThreshold && Time.time - lastJumpTime > jumpCooldown && charController.isGrounded)
        {
            isJumping = true;
            verticalVelocity = jumpForce;
            lastJumpTime = Time.time;
        }
    }

    void DetectCrouch(float deviation)
    {
        // Negative deviation = fast downward movement
        if (deviation < -crouchThreshold && !isCrouching)
        {
            isCrouching = true;
            crouchTimer = crouchDuration;
            targetHeightOffsetY = originalHeightOffsetY - crouchHeightOffset;
        }
    }

    void UpdateCrouchState()
    {
        if (isCrouching)
        {
            crouchTimer -= Time.deltaTime;
            if (crouchTimer <= 0f)
            {
                isCrouching = false;
                targetHeightOffsetY = originalHeightOffsetY;
            }
        }
    }

    void ApplyGravityAndJump()
    {
        // Only apply vertical movement when jumping; CharacterMovement.SimpleMove handles normal gravity
        if (!isJumping) return;

        verticalVelocity += Physics.gravity.y * Time.deltaTime;

        Vector3 move = new Vector3(0f, verticalVelocity * Time.deltaTime, 0f);
        charController.Move(move);

        // Stop handling jump once we've landed
        if (charController.isGrounded && verticalVelocity < 0f)
        {
            isJumping = false;
            verticalVelocity = 0f;
        }
    }

    void UpdateCrouchHeight()
    {
        if (heightOffsetTransform == null) return;

        Vector3 pos = heightOffsetTransform.localPosition;
        pos.y = Mathf.Lerp(pos.y, targetHeightOffsetY, crouchSpeed * Time.deltaTime);
        heightOffsetTransform.localPosition = pos;
    }
}
