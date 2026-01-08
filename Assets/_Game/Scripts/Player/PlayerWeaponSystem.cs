using UnityEngine;

public class PlayerWeaponSystem : MonoBehaviour
{
    [Header("Weapon Settings")]
    [Tooltip("Fire point transform (where projectiles spawn)")]
    public Transform firePoint;

    [Tooltip("Projectile prefab to spawn (if not using pool)")]
    public GameObject projectilePrefab;

    [Tooltip("Time between shots in seconds")]
    public float fireRate = 0.15f;

    [Tooltip("Projectile speed override (-1 uses projectile's default)")]
    public float projectileSpeed = 50f;

    [Header("Audio (Optional)")]
    [Tooltip("Sound to play when firing")]
    public AudioClip fireSound;

    private bool isFiring;
    private float nextFireTime;
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (firePoint == null)
        {
            Debug.LogWarning("PlayerWeaponSystem: No fire point assigned! Searching for 'FirePoint' child...");
            Transform foundFirePoint = transform.Find("FirePoint");
            if (foundFirePoint != null)
            {
                firePoint = foundFirePoint;
            }
            else
            {
                Debug.LogError("PlayerWeaponSystem: Could not find FirePoint! Please assign manually.");
            }
        }
    }

    void Update()
    {
        if (isFiring && Time.time >= nextFireTime)
        {
            FireProjectile();
            nextFireTime = Time.time + fireRate;
        }
    }

    public void HandleFireInput(bool isPressed)
    {
        isFiring = isPressed;
    }

    void FireProjectile()
    {
        if (firePoint == null)
        {
            Debug.LogError("PlayerWeaponSystem: Cannot fire - no fire point assigned!");
            return;
        }

        GameObject projectile = null;

        // Try to get projectile from pool first
        if (ProjectilePool.Instance != null)
        {
            projectile = ProjectilePool.Instance.GetProjectile();
        }
        else if (projectilePrefab != null)
        {
            // Fallback: Instantiate if no pool available
            projectile = Instantiate(projectilePrefab);
        }
        else
        {
            Debug.LogError("PlayerWeaponSystem: No projectile pool or prefab available!");
            return;
        }

        if (projectile == null) return;

        // Position and initialize projectile
        projectile.transform.position = firePoint.position;
        projectile.transform.rotation = firePoint.rotation;

        var projectileComponent = projectile.GetComponent<Projectile>();
        if (projectileComponent != null)
        {
            projectileComponent.Initialize(firePoint.forward, projectileSpeed);
        }

        // Play fire sound
        if (audioSource != null && fireSound != null)
        {
            audioSource.PlayOneShot(fireSound);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (firePoint != null)
        {
            // Draw fire point and direction
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(firePoint.position, 0.2f);
            Gizmos.DrawRay(firePoint.position, firePoint.forward * 2f);
        }
    }
}
