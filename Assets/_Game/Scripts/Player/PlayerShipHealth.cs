using UnityEngine;
using UnityEngine.Events;

public class PlayerShipHealth : MonoBehaviour
{
    [Header("Shield Settings")]
    [Tooltip("Maximum shield value")]
    public float maxShield = 100f;

    [Tooltip("Shield regeneration rate (points per second)")]
    public float shieldRegenRate = 10f;

    [Tooltip("Delay before shield starts regenerating after taking damage (seconds)")]
    public float shieldRegenDelay = 3f;

    [Tooltip("Minimum shield value required to start regeneration")]
    public float minShieldForRegen = 0f;

    [Header("Current State")]
    [SerializeField]
    [Tooltip("Current shield value (read-only, for debugging)")]
    private float currentShield;

    [Header("Events")]
    [Tooltip("Called when shield value changes (passes normalized value 0-1)")]
    public UnityEvent<float> OnShieldChanged;

    [Tooltip("Called when shield is depleted (reaches 0)")]
    public UnityEvent OnShieldDepleted;

    [Tooltip("Called when shield is fully restored")]
    public UnityEvent OnShieldRestored;

    private float timeSinceLastDamage;
    private bool isShieldDepleted;

    void Start()
    {
        // Initialize shield to maximum
        currentShield = maxShield;
        timeSinceLastDamage = shieldRegenDelay; // Start ready to regen
        isShieldDepleted = false;

        // Notify listeners of initial state
        OnShieldChanged?.Invoke(GetShieldPercentage());
    }

    void Update()
    {
        // Increment timer
        timeSinceLastDamage += Time.deltaTime;

        // Check if we should regenerate shield
        if (timeSinceLastDamage >= shieldRegenDelay &&
            currentShield < maxShield &&
            currentShield >= minShieldForRegen)
        {
            RegenerateShield();
        }
    }

    public void TakeDamage(float damage)
    {
        if (damage <= 0f) return;

        // Reduce shield
        currentShield = Mathf.Max(0f, currentShield - damage);

        // Reset regeneration timer
        timeSinceLastDamage = 0f;

        // Notify listeners
        OnShieldChanged?.Invoke(GetShieldPercentage());

        // Check if shield depleted
        if (currentShield <= 0f && !isShieldDepleted)
        {
            isShieldDepleted = true;
            OnShieldDepleted?.Invoke();
        }

        Debug.Log($"PlayerShip took {damage} damage. Shield: {currentShield}/{maxShield}");
    }

    public void RestoreShield(float amount)
    {
        if (amount <= 0f) return;

        float previousShield = currentShield;
        currentShield = Mathf.Min(maxShield, currentShield + amount);

        // Notify listeners
        OnShieldChanged?.Invoke(GetShieldPercentage());

        // Check if shield was restored
        if (previousShield < maxShield && currentShield >= maxShield)
        {
            OnShieldRestored?.Invoke();
        }

        if (isShieldDepleted && currentShield > 0f)
        {
            isShieldDepleted = false;
        }
    }

    void RegenerateShield()
    {
        float previousShield = currentShield;
        currentShield = Mathf.Min(maxShield, currentShield + shieldRegenRate * Time.deltaTime);

        // Notify listeners
        OnShieldChanged?.Invoke(GetShieldPercentage());

        // Check if shield was fully restored
        if (previousShield < maxShield && currentShield >= maxShield)
        {
            OnShieldRestored?.Invoke();
        }

        if (isShieldDepleted && currentShield > 0f)
        {
            isShieldDepleted = false;
        }
    }

    public float GetShieldPercentage()
    {
        return currentShield / maxShield;
    }

    public float GetCurrentShield()
    {
        return currentShield;
    }

    public bool IsShieldDepleted()
    {
        return isShieldDepleted;
    }

    public void SetShield(float value)
    {
        currentShield = Mathf.Clamp(value, 0f, maxShield);
        OnShieldChanged?.Invoke(GetShieldPercentage());

        isShieldDepleted = currentShield <= 0f;
    }
}
