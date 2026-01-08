using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool : MonoBehaviour
{
    public static ProjectilePool Instance { get; private set; }

    [Header("Pool Settings")]
    [Tooltip("The projectile prefab to pool")]
    public GameObject projectilePrefab;

    [Tooltip("Initial pool size")]
    public int poolSize = 20;

    [Tooltip("Allow pool to grow if all projectiles are in use")]
    public bool allowGrowth = true;

    private Queue<GameObject> pool = new Queue<GameObject>();
    private List<GameObject> activeProjectiles = new List<GameObject>();

    void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        InitializePool();
    }

    void InitializePool()
    {
        if (projectilePrefab == null)
        {
            Debug.LogError("ProjectilePool: No projectile prefab assigned!");
            return;
        }

        // Pre-instantiate pool objects
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(projectilePrefab);
            obj.SetActive(false);
            obj.transform.SetParent(transform);
            pool.Enqueue(obj);
        }
    }

    public GameObject GetProjectile()
    {
        GameObject projectile;

        if (pool.Count > 0)
        {
            // Get from pool
            projectile = pool.Dequeue();
        }
        else if (allowGrowth)
        {
            // Create new projectile if pool is empty and growth is allowed
            projectile = Instantiate(projectilePrefab);
            projectile.transform.SetParent(transform);
            Debug.LogWarning($"ProjectilePool: Pool exhausted, creating new projectile. Active count: {activeProjectiles.Count}");
        }
        else
        {
            Debug.LogError("ProjectilePool: Pool exhausted and growth is disabled!");
            return null;
        }

        projectile.SetActive(true);
        activeProjectiles.Add(projectile);
        return projectile;
    }

    public void ReturnProjectile(GameObject projectile)
    {
        if (projectile == null) return;

        // Deactivate and return to pool
        projectile.SetActive(false);
        activeProjectiles.Remove(projectile);
        pool.Enqueue(projectile);
    }

    public void ReturnAllProjectiles()
    {
        // Return all active projectiles to the pool
        for (int i = activeProjectiles.Count - 1; i >= 0; i--)
        {
            ReturnProjectile(activeProjectiles[i]);
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
