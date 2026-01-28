using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Projectile prefab to spawn. Defaults to LaserCapsule.")]
    public GameObject projectilePrefab;

    [Tooltip("Optional player reference; auto-found if left empty.")]
    public Transform player;

    [Header("Firing")]
    [Tooltip("Shots per second.")]
    public float fireRate = 0.8f;

    [Tooltip("Projectile speed override (-1 uses projectile default).")]
    public float projectileSpeed = 60f;

    [Tooltip("Maximum aim error angle in degrees.")]
    public float accuracyAngle = 6f;

    [Tooltip("Fire immediately on spawn.")]
    public bool fireOnStart = true;

    private float nextFireTime;

    void Start()
    {
        if (player == null)
        {
            var playerHealth = FindFirstObjectByType<PlayerShipHealth>();
            if (playerHealth != null)
            {
                player = playerHealth.transform;
            }
        }

        if (fireOnStart)
        {
            nextFireTime = Time.time;
        }
        else
        {
            nextFireTime = Time.time + 1f / Mathf.Max(0.01f, fireRate);
        }
    }

    void Update()
    {
        if (player == null) return;
        if (Time.time < nextFireTime) return;

        FireAtPlayer();
        nextFireTime = Time.time + 1f / Mathf.Max(0.01f, fireRate);
    }

    void FireAtPlayer()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("EnemyShooter: No projectile prefab assigned.");
            return;
        }

        GameObject projectile = null;

        if (ProjectilePool.Instance != null)
        {
            projectile = ProjectilePool.Instance.GetProjectile();
        }
        else
        {
            projectile = Instantiate(projectilePrefab);
        }

        if (projectile == null) return;

        Vector3 origin = transform.position;
        Vector3 direction = (player.position - origin).normalized;
        direction = ApplyInaccuracy(direction);

        projectile.transform.position = origin;
        projectile.transform.rotation = Quaternion.LookRotation(direction);

        var projectileComponent = projectile.GetComponent<Projectile>();
        if (projectileComponent != null)
        {
            projectileComponent.Initialize(direction, projectileSpeed, Projectile.ProjectileTarget.Player);
        }
    }

    Vector3 ApplyInaccuracy(Vector3 direction)
    {
        if (accuracyAngle <= 0f) return direction;

        // Randomize aim within a cone around the intended direction.
        Vector2 offset = Random.insideUnitCircle * Mathf.Tan(accuracyAngle * Mathf.Deg2Rad);
        Vector3 right = Vector3.Cross(direction, Vector3.up);
        if (right.sqrMagnitude < 0.001f)
        {
            right = Vector3.Cross(direction, Vector3.right);
        }
        right.Normalize();
        Vector3 up = Vector3.Cross(right, direction).normalized;

        Vector3 inaccurate = (direction + right * offset.x + up * offset.y).normalized;
        return inaccurate;
    }
}
