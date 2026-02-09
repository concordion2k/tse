using UnityEngine;
using LevelDesigner;

public class CameraFollowController : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("The transform to follow (PlayerShip)")]
    public Transform target;

    [Header("Path Following")]
    [Tooltip("Reference to level path follower for rotation alignment")]
    public LevelPathFollower pathFollower;

    [Header("Camera Positioning")]
    [Tooltip("Offset from target in local space (relative to target's rotation)")]
    public Vector3 offset = new Vector3(0f, 3f, -8f);

    [Tooltip("How far ahead of the target to look")]
    public float lookAhead = 8f;

    [Header("Smoothing")]
    [Tooltip("How quickly the camera follows the target position (higher = faster)")]
    public float followSpeed = 10f;

    [Tooltip("How quickly the camera rotates to look at the target (higher = faster)")]
    public float rotationSpeed = 5f;

    private Vector3 currentVelocity = Vector3.zero;
    private PlayerShipMovement shipMovement;
    private bool hasSnappedToTarget;

    void LateUpdate()
    {
        if (target == null)
        {
            Debug.LogWarning("CameraFollowController: No target assigned!");
            return;
        }

        // Get the ship movement component if we don't have it yet
        if (shipMovement == null)
        {
            shipMovement = target.GetComponent<PlayerShipMovement>();
            if (shipMovement == null)
            {
                Debug.LogWarning("CameraFollowController: Target doesn't have PlayerShipMovement component! Falling back to transform follow.");
            }
        }

        // Snap to target on first frame to prevent initial jerk
        if (!hasSnappedToTarget)
        {
            hasSnappedToTarget = true;
            SnapToTarget();
        }

        UpdateCameraPosition();
        UpdateCameraRotation();
    }

    void SnapToTarget()
    {
        Vector3 followPosition = (shipMovement != null) ? shipMovement.GetCenterPath() : target.position;
        transform.position = followPosition + offset;

        Vector3 forward = (pathFollower != null && pathFollower.HasValidPath)
            ? pathFollower.GetPathForward()
            : target.forward;
        Vector3 lookAtPoint = followPosition + forward * lookAhead;
        transform.rotation = Quaternion.LookRotation(lookAtPoint - transform.position);
    }

    void UpdateCameraPosition()
    {
        // Use center path if available, otherwise use target's position
        Vector3 followPosition;
        if (shipMovement != null)
        {
            // Follow the center path, not the ship's actual position
            // This allows the ship to move within the camera view
            followPosition = shipMovement.GetCenterPath();
        }
        else
        {
            // Fallback to following the transform directly
            followPosition = target.position;
        }

        // Calculate desired position: follow position + offset
        // Use world space offset, not affected by ship rotation
        Vector3 desiredPosition = followPosition + offset;

        // Smooth follow using Lerp
        if (followSpeed > 0f)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                desiredPosition,
                followSpeed * Time.deltaTime
            );
        }
        else
        {
            transform.position = desiredPosition;
        }
    }

    void UpdateCameraRotation()
    {
        // Use center path for look-ahead calculation
        Vector3 lookAtBase;
        if (shipMovement != null)
        {
            lookAtBase = shipMovement.GetCenterPath();
        }
        else
        {
            lookAtBase = target.position;
        }

        // Use path forward if available, otherwise target forward
        Vector3 forward = (pathFollower != null && pathFollower.HasValidPath)
            ? pathFollower.GetPathForward()
            : target.forward;

        // Calculate look-at point: slightly ahead of the center path
        Vector3 lookAtPoint = lookAtBase + forward * lookAhead;

        // Calculate desired rotation
        Quaternion desiredRotation = Quaternion.LookRotation(lookAtPoint - transform.position);

        // Smooth rotation using Slerp
        if (rotationSpeed > 0f)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                desiredRotation,
                rotationSpeed * Time.deltaTime
            );
        }
        else
        {
            transform.rotation = desiredRotation;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (target == null) return;

        // Get the follow position (center path or transform)
        Vector3 followPosition = target.position;
        if (Application.isPlaying && shipMovement != null)
        {
            followPosition = shipMovement.GetCenterPath();
        }

        // Draw camera offset position (world space, not rotated with ship)
        Gizmos.color = Color.cyan;
        Vector3 targetPos = followPosition + offset;
        Gizmos.DrawWireSphere(targetPos, 0.5f);
        Gizmos.DrawLine(followPosition, targetPos);

        // Draw look-at point
        Gizmos.color = Color.green;
        Vector3 lookAtPoint = followPosition + Vector3.forward * lookAhead;
        Gizmos.DrawWireSphere(lookAtPoint, 0.3f);
        Gizmos.DrawLine(transform.position, lookAtPoint);
    }
}
