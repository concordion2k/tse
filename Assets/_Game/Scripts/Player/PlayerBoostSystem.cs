using UnityEngine;
using UnityEngine.Events;

public class PlayerBoostSystem : MonoBehaviour
{
    [Header("Fuel Settings")]
    [Tooltip("Maximum fuel capacity")]
    public float maxFuel = 100f;

    [Tooltip("Fuel consumed per second while boosting")]
    public float boostFuelCost = 20f;

    [Tooltip("Fuel consumed per second while braking")]
    public float brakeFuelCost = 15f;

    [Tooltip("Fuel regenerated per second when idle")]
    public float fuelRegenRate = 10f;

    [Tooltip("Delay in seconds before fuel starts regenerating after use")]
    public float fuelRegenDelay = 0.5f;

    [Header("Speed Modifiers")]
    [Tooltip("Normal forward flight speed")]
    public float normalSpeed = 20f;

    [Tooltip("Forward speed when boosting")]
    public float boostSpeed = 35f;

    [Tooltip("Forward speed when braking")]
    public float brakeSpeed = 10f;

    [Header("References")]
    [Tooltip("Reference to the ship movement component")]
    public PlayerShipMovement shipMovement;

    [Tooltip("Particle system for engine visual effects")]
    public ParticleSystem engineParticles;

    [Header("Events")]
    [Tooltip("Fired when fuel changes (passes normalized 0-1 value)")]
    public UnityEvent<float> OnFuelChanged;

    [Tooltip("Fired when fuel is depleted")]
    public UnityEvent OnFuelDepleted;

    [Tooltip("Fired when fuel is fully restored")]
    public UnityEvent OnFuelRestored;

    [Header("Current State (Read-Only)")]
    [SerializeField]
    private float currentFuel;

    private bool isBoosting;
    private bool isBraking;
    private float timeSinceLastUse;

    void Start()
    {
        // Initialize fuel to maximum
        currentFuel = maxFuel;
        timeSinceLastUse = fuelRegenDelay; // Start ready to regen

        // Auto-find ship movement if not assigned
        if (shipMovement == null)
        {
            shipMovement = GetComponent<PlayerShipMovement>();
            if (shipMovement == null)
            {
                Debug.LogError("PlayerBoostSystem: No PlayerShipMovement component found!");
            }
        }

        // Notify listeners of initial state
        OnFuelChanged?.Invoke(GetFuelPercentage());
    }

    void Update()
    {
        // Handle fuel consumption
        if (isBoosting && currentFuel > 0)
        {
            ConsumeFuel(boostFuelCost * Time.deltaTime);
            if (currentFuel <= 0)
            {
                StopBoost();
            }
        }
        else if (isBraking && currentFuel > 0)
        {
            ConsumeFuel(brakeFuelCost * Time.deltaTime);
            if (currentFuel <= 0)
            {
                StopBrake();
            }
        }

        // Handle fuel regeneration
        if (!isBoosting && !isBraking)
        {
            timeSinceLastUse += Time.deltaTime;

            if (timeSinceLastUse >= fuelRegenDelay && currentFuel < maxFuel)
            {
                RegenerateFuel(fuelRegenRate * Time.deltaTime);
            }
        }
    }

    public void StartBoost()
    {
        if (currentFuel <= 0 || isBoosting)
            return;

        isBoosting = true;
        isBraking = false; // Cancel brake if active
        timeSinceLastUse = 0f;

        if (shipMovement != null)
        {
            shipMovement.forwardSpeed = boostSpeed;
        }

        UpdateParticles();
    }

    public void StopBoost()
    {
        if (!isBoosting)
            return;

        isBoosting = false;

        if (shipMovement != null)
        {
            shipMovement.forwardSpeed = normalSpeed;
        }

        UpdateParticles();
    }

    public void StartBrake()
    {
        if (currentFuel <= 0 || isBraking)
            return;

        isBraking = true;
        isBoosting = false; // Cancel boost if active
        timeSinceLastUse = 0f;

        if (shipMovement != null)
        {
            shipMovement.forwardSpeed = brakeSpeed;
        }

        UpdateParticles();
    }

    public void StopBrake()
    {
        if (!isBraking)
            return;

        isBraking = false;

        if (shipMovement != null)
        {
            shipMovement.forwardSpeed = normalSpeed;
        }

        UpdateParticles();
    }

    void UpdateParticles()
    {
        if (engineParticles == null)
            return;

        var main = engineParticles.main;
        var emission = engineParticles.emission;

        if (isBoosting)
        {
            // Enlarge and intensify for boost
            main.startSize = 1.5f;
            emission.rateOverTime = 100f;
        }
        else if (isBraking)
        {
            // Dim and shrink for brake
            main.startSize = 0.3f;
            emission.rateOverTime = 20f;
        }
        else
        {
            // Normal shimmer
            main.startSize = 0.8f;
            emission.rateOverTime = 50f;
        }
    }

    void ConsumeFuel(float amount)
    {
        currentFuel = Mathf.Max(0f, currentFuel - amount);
        OnFuelChanged?.Invoke(GetFuelPercentage());

        if (currentFuel <= 0f)
        {
            OnFuelDepleted?.Invoke();
        }
    }

    void RegenerateFuel(float amount)
    {
        float previousFuel = currentFuel;
        currentFuel = Mathf.Min(maxFuel, currentFuel + amount);
        OnFuelChanged?.Invoke(GetFuelPercentage());

        if (previousFuel < maxFuel && currentFuel >= maxFuel)
        {
            OnFuelRestored?.Invoke();
        }
    }

    public float GetFuelPercentage()
    {
        return currentFuel / maxFuel;
    }

    public float GetCurrentFuel()
    {
        return currentFuel;
    }

    public bool IsBoosting()
    {
        return isBoosting;
    }

    public bool IsBraking()
    {
        return isBraking;
    }
}
