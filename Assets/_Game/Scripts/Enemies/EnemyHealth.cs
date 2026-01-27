using UnityEngine;
using UnityEngine.Events;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("Maximum health points")]
    public float maxHealth = 10f;

    [Tooltip("Current health (set automatically from maxHealth on start)")]
    public float currentHealth;

    [Header("Events")]
    public UnityEvent OnDamaged;
    public UnityEvent OnDestroyed;

    [Header("Visual Feedback")]
    [Tooltip("Flash color when taking damage")]
    public Color damageFlashColor = Color.white;

    [Tooltip("Duration of damage flash")]
    public float flashDuration = 0.1f;

    private Renderer enemyRenderer;
    private Color originalColor;
    private bool isFlashing;

    void Awake()
    {
        currentHealth = maxHealth;
        enemyRenderer = GetComponentInChildren<Renderer>();

        if (enemyRenderer != null)
        {
            originalColor = enemyRenderer.material.color;
        }
    }

    public void TakeDamage(float damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;
        OnDamaged?.Invoke();

        // Visual feedback
        if (enemyRenderer != null && !isFlashing)
        {
            StartCoroutine(DamageFlash());
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private System.Collections.IEnumerator DamageFlash()
    {
        isFlashing = true;
        enemyRenderer.material.color = damageFlashColor;
        yield return new WaitForSeconds(flashDuration);
        enemyRenderer.material.color = originalColor;
        isFlashing = false;
    }

    private void Die()
    {
        // Notify score manager
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddKill();
        }

        OnDestroyed?.Invoke();

        // Destroy the enemy
        Destroy(gameObject);
    }
}
