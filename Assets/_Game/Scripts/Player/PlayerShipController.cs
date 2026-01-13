using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerShipMovement))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerShipController : MonoBehaviour
{
    private PlayerShipMovement movement;
    private PlayerWeaponSystem weaponSystem;
    private PlayerBoostSystem boostSystem;
    private PlayerInput playerInput;

    void Awake()
    {
        // Get required components
        movement = GetComponent<PlayerShipMovement>();
        weaponSystem = GetComponent<PlayerWeaponSystem>();
        boostSystem = GetComponent<PlayerBoostSystem>();
        playerInput = GetComponent<PlayerInput>();

        if (movement == null)
        {
            Debug.LogError("PlayerShipController requires PlayerShipMovement component!");
        }

        if (playerInput == null)
        {
            Debug.LogError("PlayerShipController requires PlayerInput component!");
        }

        // Debug: Check Y-axis invert setting
        Debug.Log($"PlayerShipController started. InvertYAxis setting: {GameSettings.InvertYAxis}");
    }

    void OnEnable()
    {
        if (playerInput != null && playerInput.actions != null)
        {
            // Subscribe to Move action
            var moveAction = playerInput.actions["Move"];
            if (moveAction != null)
            {
                moveAction.performed += OnMove;
                moveAction.canceled += OnMove;
            }

            // Subscribe to Attack action
            var attackAction = playerInput.actions["Attack"];
            if (attackAction != null)
            {
                attackAction.performed += OnAttack;
                attackAction.canceled += OnAttack;
            }

            // Subscribe to Sprint (Boost) action
            var sprintAction = playerInput.actions["Sprint"];
            if (sprintAction != null)
            {
                sprintAction.performed += OnBoost;
                sprintAction.canceled += OnBoost;
            }

            // Subscribe to Crouch (Brake) action
            var crouchAction = playerInput.actions["Crouch"];
            if (crouchAction != null)
            {
                crouchAction.performed += OnBrake;
                crouchAction.canceled += OnBrake;
            }
        }
    }

    void OnDisable()
    {
        if (playerInput != null && playerInput.actions != null)
        {
            // Unsubscribe from Move action
            var moveAction = playerInput.actions["Move"];
            if (moveAction != null)
            {
                moveAction.performed -= OnMove;
                moveAction.canceled -= OnMove;
            }

            // Unsubscribe from Attack action
            var attackAction = playerInput.actions["Attack"];
            if (attackAction != null)
            {
                attackAction.performed -= OnAttack;
                attackAction.canceled -= OnAttack;
            }

            // Unsubscribe from Sprint action
            var sprintAction = playerInput.actions["Sprint"];
            if (sprintAction != null)
            {
                sprintAction.performed -= OnBoost;
                sprintAction.canceled -= OnBoost;
            }

            // Unsubscribe from Crouch action
            var crouchAction = playerInput.actions["Crouch"];
            if (crouchAction != null)
            {
                crouchAction.performed -= OnBrake;
                crouchAction.canceled -= OnBrake;
            }
        }
    }

    void OnMove(InputAction.CallbackContext context)
    {
        if (movement != null)
        {
            Vector2 input = context.ReadValue<Vector2>();

            // Apply Y-axis inversion if setting is enabled
            if (GameSettings.InvertYAxis)
            {
                Debug.Log($"Inverting Y-axis: {input.y} -> {-input.y}");
                input.y *= -1f;
            }

            movement.SetMovementInput(input);
        }
    }

    void OnAttack(InputAction.CallbackContext context)
    {
        if (weaponSystem != null)
        {
            bool isPressed = context.ReadValueAsButton();
            weaponSystem.HandleFireInput(isPressed);
        }
    }

    void OnBoost(InputAction.CallbackContext context)
    {
        if (boostSystem == null)
            return;

        if (context.performed)
            boostSystem.StartBoost();
        else if (context.canceled)
            boostSystem.StopBoost();
    }

    void OnBrake(InputAction.CallbackContext context)
    {
        if (boostSystem == null)
            return;

        if (context.performed)
            boostSystem.StartBrake();
        else if (context.canceled)
            boostSystem.StopBrake();
    }
}
