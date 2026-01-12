using UnityEngine;
using UnityEngine.UI;

public class FuelGaugeController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to the boost system component")]
    public PlayerBoostSystem boostSystem;

    [Tooltip("UI Image component for the fuel bar fill")]
    public Image fillImage;

    [Tooltip("Optional background image")]
    public Image backgroundImage;

    [Header("Visual Settings")]
    [Tooltip("Color when fuel is full")]
    public Color fullColor = new Color(0f, 1f, 1f, 1f); // Cyan

    [Tooltip("Color when fuel is low")]
    public Color lowColor = new Color(1f, 0.5f, 0f, 1f); // Orange

    [Tooltip("Fuel percentage threshold for low fuel color (0-1)")]
    [Range(0f, 1f)]
    public float lowFuelThreshold = 0.25f; // 25%

    [Header("Animation")]
    [Tooltip("Smooth the fuel bar changes")]
    public bool smoothTransition = true;

    [Tooltip("Speed of fuel bar animation")]
    public float transitionSpeed = 5f;

    private float targetFillAmount;
    private float currentFillAmount;

    void Start()
    {
        if (boostSystem == null)
        {
            Debug.LogWarning("FuelGaugeController: No PlayerBoostSystem assigned! Searching for player...");
            boostSystem = FindFirstObjectByType<PlayerBoostSystem>();
        }

        if (fillImage == null)
        {
            Debug.LogError("FuelGaugeController: No fill image assigned!");
        }

        // Initialize
        if (boostSystem != null)
        {
            float initialFuel = boostSystem.GetFuelPercentage();
            targetFillAmount = initialFuel;
            currentFillAmount = initialFuel;

            if (fillImage != null)
            {
                fillImage.fillAmount = initialFuel;
                UpdateColor(initialFuel);
            }
        }
    }

    void OnEnable()
    {
        if (boostSystem != null)
        {
            boostSystem.OnFuelChanged.AddListener(UpdateFuelBar);
        }
    }

    void OnDisable()
    {
        if (boostSystem != null)
        {
            boostSystem.OnFuelChanged.RemoveListener(UpdateFuelBar);
        }
    }

    void Update()
    {
        if (fillImage == null)
            return;

        // Smooth transition
        if (smoothTransition && Mathf.Abs(currentFillAmount - targetFillAmount) > 0.001f)
        {
            currentFillAmount = Mathf.Lerp(currentFillAmount, targetFillAmount, transitionSpeed * Time.deltaTime);
            fillImage.fillAmount = currentFillAmount;
        }
    }

    void UpdateFuelBar(float normalizedValue)
    {
        if (fillImage == null)
            return;

        targetFillAmount = normalizedValue;

        if (!smoothTransition)
        {
            fillImage.fillAmount = normalizedValue;
            currentFillAmount = normalizedValue;
        }

        UpdateColor(normalizedValue);
    }

    void UpdateColor(float normalizedValue)
    {
        if (fillImage == null)
            return;

        // Color gradient based on fuel percentage
        if (normalizedValue <= lowFuelThreshold)
        {
            // Lerp from low color to full color within the low fuel threshold
            float t = normalizedValue / lowFuelThreshold;
            fillImage.color = Color.Lerp(lowColor, fullColor, t);
        }
        else
        {
            fillImage.color = fullColor;
        }
    }

    public void SetBoostSystem(PlayerBoostSystem system)
    {
        // Unsubscribe from old boost system
        if (boostSystem != null)
        {
            boostSystem.OnFuelChanged.RemoveListener(UpdateFuelBar);
        }

        // Subscribe to new boost system
        boostSystem = system;
        if (boostSystem != null)
        {
            boostSystem.OnFuelChanged.AddListener(UpdateFuelBar);
            UpdateFuelBar(boostSystem.GetFuelPercentage());
        }
    }
}
