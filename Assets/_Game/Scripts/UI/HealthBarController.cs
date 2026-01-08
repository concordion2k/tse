using UnityEngine;
using UnityEngine.UI;

public class HealthBarController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to the player's health component")]
    public PlayerShipHealth playerHealth;

    [Tooltip("UI Image component for the health bar fill")]
    public Image fillImage;

    [Tooltip("Optional background image")]
    public Image backgroundImage;

    [Header("Visual Settings")]
    [Tooltip("Color when shield is full")]
    public Color fullColor = new Color(0f, 1f, 0f, 1f); // Green

    [Tooltip("Color when shield is low")]
    public Color lowColor = new Color(1f, 0f, 0f, 1f); // Red

    [Tooltip("Shield percentage threshold for low health color (0-1)")]
    [Range(0f, 1f)]
    public float lowHealthThreshold = 0.3f;

    [Header("Animation")]
    [Tooltip("Smooth the health bar changes")]
    public bool smoothTransition = true;

    [Tooltip("Speed of health bar animation")]
    public float transitionSpeed = 5f;

    private float targetFillAmount;
    private float currentFillAmount;

    void Start()
    {
        if (playerHealth == null)
        {
            Debug.LogError("HealthBarController: No PlayerShipHealth assigned! Searching for player...");
            playerHealth = FindFirstObjectByType<PlayerShipHealth>();
        }

        if (fillImage == null)
        {
            Debug.LogError("HealthBarController: No fill image assigned!");
        }

        // Initialize
        if (playerHealth != null)
        {
            float initialHealth = playerHealth.GetShieldPercentage();
            targetFillAmount = initialHealth;
            currentFillAmount = initialHealth;

            if (fillImage != null)
            {
                fillImage.fillAmount = initialHealth;
                UpdateColor(initialHealth);
            }
        }
    }

    void OnEnable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnShieldChanged.AddListener(UpdateHealthBar);
        }
    }

    void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnShieldChanged.RemoveListener(UpdateHealthBar);
        }
    }

    void Update()
    {
        if (fillImage == null) return;

        // Smooth transition
        if (smoothTransition && Mathf.Abs(currentFillAmount - targetFillAmount) > 0.001f)
        {
            currentFillAmount = Mathf.Lerp(currentFillAmount, targetFillAmount, transitionSpeed * Time.deltaTime);
            fillImage.fillAmount = currentFillAmount;
        }
    }

    void UpdateHealthBar(float normalizedValue)
    {
        if (fillImage == null) return;

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
        if (fillImage == null) return;

        // Color gradient based on health percentage
        if (normalizedValue <= lowHealthThreshold)
        {
            // Lerp from low color to full color within the low health threshold
            float t = normalizedValue / lowHealthThreshold;
            fillImage.color = Color.Lerp(lowColor, fullColor, t);
        }
        else
        {
            fillImage.color = fullColor;
        }
    }

    public void SetPlayerHealth(PlayerShipHealth health)
    {
        // Unsubscribe from old health
        if (playerHealth != null)
        {
            playerHealth.OnShieldChanged.RemoveListener(UpdateHealthBar);
        }

        // Subscribe to new health
        playerHealth = health;
        if (playerHealth != null)
        {
            playerHealth.OnShieldChanged.AddListener(UpdateHealthBar);
            UpdateHealthBar(playerHealth.GetShieldPercentage());
        }
    }
}
