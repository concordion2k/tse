using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [Tooltip("Movement speed in units per second")]
    public float speed = 50f;

    [Tooltip("Damage dealt on hit")]
    public float damage = 10f;

    [Tooltip("Maximum lifetime in seconds before auto-destroy")]
    public float maxLifetime = 2f;

    [Tooltip("Maximum distance from spawn position before auto-destroy")]
    public float maxDistance = 100f;

    private Rigidbody rb;
    private Vector3 startPosition;
    private float lifetimeTimer;
    private bool isActive;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Configure rigidbody for projectile physics
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    public void Initialize(Vector3 direction, float speedOverride = -1f)
    {
        startPosition = transform.position;
        lifetimeTimer = 0f;
        isActive = true;

        // Set velocity
        float finalSpeed = speedOverride > 0f ? speedOverride : speed;
        rb.linearVelocity = direction.normalized * finalSpeed;

        // Rotate to face direction of travel
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    void Update()
    {
        if (!isActive) return;

        lifetimeTimer += Time.deltaTime;

        // Check lifetime limit
        if (lifetimeTimer >= maxLifetime)
        {
            DestroyProjectile();
            return;
        }

        // Check distance limit
        float distanceTraveled = Vector3.Distance(startPosition, transform.position);
        if (distanceTraveled >= maxDistance)
        {
            DestroyProjectile();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isActive) return;

        // Check if we hit an enemy or obstacle
        if (other.CompareTag("Enemy") || other.CompareTag("Obstacle"))
        {
            // Try to apply damage if the object has a health component
            var health = other.GetComponent<PlayerShipHealth>();
            if (health != null)
            {
                health.TakeDamage(damage);
            }

            DestroyProjectile();
        }
    }

    void DestroyProjectile()
    {
        isActive = false;
        rb.linearVelocity = Vector3.zero;

        // Return to pool if ProjectilePool exists, otherwise destroy
        if (ProjectilePool.Instance != null)
        {
            ProjectilePool.Instance.ReturnProjectile(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDisable()
    {
        // Reset velocity when disabled (pooled)
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
        }
    }
}
