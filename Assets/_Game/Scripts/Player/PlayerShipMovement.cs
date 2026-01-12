using UnityEngine;

public class PlayerShipMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Constant forward movement speed in units per second")]
    public float forwardSpeed = 20f;

    [Tooltip("Maximum lateral movement speed (X/Y axes) in units per second")]
    public float maxLateralSpeed = 10f;

    [Tooltip("Time to reach maximum speed from zero (seconds)")]
    public float accelerationTime = 0.5f;

    [Tooltip("Time to come to a complete stop from maximum speed (seconds)")]
    public float decelerationTime = 0.5f;

    [Header("Movement Bounds")]
    [Tooltip("Use camera viewport to calculate bounds automatically")]
    public bool useCameraBounds = true;

    [Tooltip("Percentage of screen edge to leave as padding (0-1, e.g. 0.1 = 10% padding)")]
    [Range(0f, 0.5f)]
    public float edgePadding = 0.05f;

    [Tooltip("Manual movement boundary box half-extents (only used if useCameraBounds is false)")]
    public Vector3 manualBounds = new Vector3(10f, 6f, 0f);

    [Header("Smoothing")]
    [Tooltip("Movement smoothing factor (0 = instant, higher = smoother)")]
    public float movementSmoothing = 0.1f;

    [Header("Ship Rotation")]
    [Tooltip("Enable ship rotation based on movement direction")]
    public bool enableRotation = true;

    [Tooltip("Maximum tilt angle when moving horizontally (degrees, bank/roll)")]
    [Range(0f, 90f)]
    public float maxBankAngle = 30f;

    [Tooltip("Maximum tilt angle when moving vertically (degrees, pitch)")]
    [Range(0f, 45f)]
    public float maxPitchAngle = 15f;

    [Tooltip("How quickly the ship rotates to match movement direction")]
    public float rotationSpeed = 5f;

    private Vector2 moveInput;
    private Vector2 currentVelocity = Vector2.zero; // Current lateral velocity
    private Vector3 lateralOffset = Vector3.zero; // Offset from center path
    private Vector3 centerPath; // The center line the ship follows
    private Vector3 smoothVelocity = Vector3.zero; // For SmoothDamp
    private Vector3 calculatedBounds;
    private Camera mainCamera;

    void Start()
    {
        // Initialize center path to starting position
        centerPath = transform.position;

        // Get main camera reference
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("PlayerShipMovement: No main camera found! Using manual bounds.");
            useCameraBounds = false;
        }

        // Calculate initial bounds
        UpdateMovementBounds();
    }

    void UpdateMovementBounds()
    {
        if (useCameraBounds && mainCamera != null)
        {
            // Calculate bounds based on camera viewport
            // Get the distance from camera to the center path
            float distanceFromCamera = Vector3.Distance(mainCamera.transform.position, centerPath);

            // Calculate the world space height and width visible at this distance
            float frustumHeight = 2.0f * distanceFromCamera * Mathf.Tan(mainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float frustumWidth = frustumHeight * mainCamera.aspect;

            // Apply padding and calculate half-extents
            calculatedBounds.x = (frustumWidth * 0.5f) * (1f - edgePadding);
            calculatedBounds.y = (frustumHeight * 0.5f) * (1f - edgePadding);
            calculatedBounds.z = 0f;
        }
        else
        {
            // Use manual bounds
            calculatedBounds = manualBounds;
        }
    }

    public void SetMovementInput(Vector2 input)
    {
        moveInput = input;
    }

    // Public accessor for the center path (used by camera)
    public Vector3 GetCenterPath()
    {
        return centerPath;
    }

    void FixedUpdate()
    {
        // Move the center path forward constantly
        centerPath += transform.forward * forwardSpeed * Time.fixedDeltaTime;
    }

    void Update()
    {
        // Update bounds periodically if using camera bounds (for dynamic camera changes)
        if (useCameraBounds && Time.frameCount % 10 == 0) // Update every 10 frames for performance
        {
            UpdateMovementBounds();
        }

        // Calculate target velocity based on input
        Vector2 targetVelocity = moveInput * maxLateralSpeed;

        // Determine if we're accelerating or decelerating
        bool isAccelerating = moveInput.magnitude > 0.01f;
        float transitionTime = isAccelerating ? accelerationTime : decelerationTime;

        // Smoothly interpolate current velocity toward target velocity
        if (transitionTime > 0f)
        {
            float smoothSpeed = 1f / transitionTime;
            currentVelocity = Vector2.Lerp(currentVelocity, targetVelocity, smoothSpeed * Time.deltaTime);
        }
        else
        {
            currentVelocity = targetVelocity;
        }

        // Apply velocity to lateral offset
        lateralOffset += new Vector3(currentVelocity.x, currentVelocity.y, 0f) * Time.deltaTime;

        // Clamp lateral offset within calculated bounds
        lateralOffset.x = Mathf.Clamp(lateralOffset.x, -calculatedBounds.x, calculatedBounds.x);
        lateralOffset.y = Mathf.Clamp(lateralOffset.y, -calculatedBounds.y, calculatedBounds.y);
        lateralOffset.z = 0f; // No forward/backward offset

        // Calculate target position: center path + lateral offset
        Vector3 targetPosition = centerPath + lateralOffset;

        // Directly set position to avoid lag during speed changes
        // Lateral movement already has smoothing from velocity interpolation
        transform.position = targetPosition;

        // Update ship rotation based on movement
        if (enableRotation)
        {
            UpdateShipRotation();
        }
    }

    void UpdateShipRotation()
    {
        // Calculate normalized velocity for rotation (0-1 range)
        Vector2 normalizedVelocity = Vector2.zero;
        if (maxLateralSpeed > 0f)
        {
            normalizedVelocity = currentVelocity / maxLateralSpeed;
        }

        // Calculate target rotation angles
        // Bank (roll) based on horizontal movement (negative for correct direction)
        float targetBank = -normalizedVelocity.x * maxBankAngle;

        // Pitch based on vertical movement (negative for correct direction)
        float targetPitch = -normalizedVelocity.y * maxPitchAngle;

        // Create target rotation (euler angles: pitch, yaw, roll)
        Quaternion targetRotation = Quaternion.Euler(targetPitch, 0f, targetBank);

        // Debug logging (remove after testing)
        if (normalizedVelocity.magnitude > 0.1f)
        {
            Debug.Log($"Ship Rotation - Velocity: {currentVelocity}, Bank: {targetBank:F1}°, Pitch: {targetPitch:F1}°");
        }

        // Smoothly interpolate to target rotation
        if (rotationSpeed > 0f)
        {
            transform.localRotation = Quaternion.Slerp(
                transform.localRotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
        else
        {
            transform.localRotation = targetRotation;
        }
    }

    void OnDrawGizmos()
    {
        // Use center path if playing, otherwise use transform position
        Vector3 center = Application.isPlaying ? centerPath : transform.position;

        // Use calculated bounds if playing, otherwise use manual bounds
        Vector3 boundsToShow = Application.isPlaying ? calculatedBounds : manualBounds;

        // Visualize movement boundaries in the Scene view
        Gizmos.color = Color.yellow;
        Vector3 size = new Vector3(boundsToShow.x * 2f, boundsToShow.y * 2f, 0.1f);
        Gizmos.DrawWireCube(center, size);

        // Draw corner markers
        Gizmos.color = Color.red;
        float markerSize = 0.5f;
        Gizmos.DrawWireSphere(center + new Vector3(boundsToShow.x, boundsToShow.y, 0f), markerSize);
        Gizmos.DrawWireSphere(center + new Vector3(-boundsToShow.x, boundsToShow.y, 0f), markerSize);
        Gizmos.DrawWireSphere(center + new Vector3(boundsToShow.x, -boundsToShow.y, 0f), markerSize);
        Gizmos.DrawWireSphere(center + new Vector3(-boundsToShow.x, -boundsToShow.y, 0f), markerSize);

        // Draw center path line
        if (Application.isPlaying)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(centerPath, 0.3f);
        }
    }
}
