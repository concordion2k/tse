using UnityEngine;

public class TargetingReticle : MonoBehaviour
{
    [Header("Targeting")]
    [Tooltip("Reference to the weapon system's fire point")]
    public Transform firePoint;

    [Tooltip("Distance ahead of the fire point to place the outer reticle")]
    [Range(20f, 100f)]
    public float outerReticleDistance = 40f;

    [Tooltip("Distance ahead of the fire point to place the inner reticle")]
    [Range(10f, 80f)]
    public float innerReticleDistance = 20f;

    [Header("Billboard")]
    [Tooltip("Camera to face (auto-finds main camera if null)")]
    public Camera targetCamera;

    [Header("Sprites")]
    [Tooltip("Outer reticle sprite (circle)")]
    public SpriteRenderer outerReticle;

    [Tooltip("Inner reticle sprite (square)")]
    public SpriteRenderer innerReticle;

    [Header("Animation")]
    [Tooltip("Rotation speed for inner reticle (degrees per second)")]
    public float innerRotationSpeed = 30f;

    [Tooltip("Smoothing for position updates (0 = instant)")]
    [Range(0f, 0.5f)]
    public float positionSmoothing = 0.1f;

    private Vector3 smoothedOuterPosition;
    private Vector3 outerPositionVelocity;
    private Vector3 smoothedInnerPosition;
    private Vector3 innerPositionVelocity;

    void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (firePoint == null)
        {
            var weaponSystem = FindFirstObjectByType<PlayerWeaponSystem>();
            if (weaponSystem != null && weaponSystem.firePoint != null)
            {
                firePoint = weaponSystem.firePoint;
            }
            else
            {
                Debug.LogWarning("TargetingReticle: No fire point found! Please assign manually.");
            }
        }

        if (firePoint != null)
        {
            smoothedOuterPosition = CalculateTargetPosition(outerReticleDistance);
            smoothedInnerPosition = CalculateTargetPosition(innerReticleDistance);
        }
    }

    void LateUpdate()
    {
        if (firePoint == null || targetCamera == null)
        {
            return;
        }

        UpdatePosition();
        UpdateBillboard();
        UpdateAnimation();
    }

    Vector3 CalculateTargetPosition(float distance)
    {
        return firePoint.position + firePoint.forward * distance;
    }

    void UpdatePosition()
    {
        // Update outer reticle position
        if (outerReticle != null)
        {
            Vector3 outerTargetPos = CalculateTargetPosition(outerReticleDistance);
            if (positionSmoothing > 0f)
            {
                smoothedOuterPosition = Vector3.SmoothDamp(
                    smoothedOuterPosition,
                    outerTargetPos,
                    ref outerPositionVelocity,
                    positionSmoothing
                );
                outerReticle.transform.position = smoothedOuterPosition;
            }
            else
            {
                outerReticle.transform.position = outerTargetPos;
            }
        }

        // Update inner reticle position
        if (innerReticle != null)
        {
            Vector3 innerTargetPos = CalculateTargetPosition(innerReticleDistance);
            if (positionSmoothing > 0f)
            {
                smoothedInnerPosition = Vector3.SmoothDamp(
                    smoothedInnerPosition,
                    innerTargetPos,
                    ref innerPositionVelocity,
                    positionSmoothing
                );
                innerReticle.transform.position = smoothedInnerPosition;
            }
            else
            {
                innerReticle.transform.position = innerTargetPos;
            }
        }
    }

    void UpdateBillboard()
    {
        // Billboard outer reticle
        if (outerReticle != null)
        {
            Vector3 dirToCamera = targetCamera.transform.position - outerReticle.transform.position;
            if (dirToCamera != Vector3.zero)
            {
                outerReticle.transform.rotation = Quaternion.LookRotation(-dirToCamera);
            }
        }

        // Billboard inner reticle
        if (innerReticle != null)
        {
            Vector3 dirToCamera = targetCamera.transform.position - innerReticle.transform.position;
            if (dirToCamera != Vector3.zero)
            {
                innerReticle.transform.rotation = Quaternion.LookRotation(-dirToCamera);
            }
        }
    }

    void UpdateAnimation()
    {
        if (innerReticle != null && innerRotationSpeed != 0f)
        {
            // Rotate around local forward axis (the axis facing camera after billboard)
            innerReticle.transform.Rotate(Vector3.forward, innerRotationSpeed * Time.deltaTime, Space.Self);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (firePoint != null)
        {
            // Draw line to outer reticle
            Gizmos.color = Color.green;
            Vector3 outerPos = CalculateTargetPosition(outerReticleDistance);
            Gizmos.DrawLine(firePoint.position, outerPos);
            Gizmos.DrawWireSphere(outerPos, 0.5f);

            // Draw inner reticle position
            Gizmos.color = Color.yellow;
            Vector3 innerPos = CalculateTargetPosition(innerReticleDistance);
            Gizmos.DrawWireSphere(innerPos, 0.3f);
        }
    }
}
