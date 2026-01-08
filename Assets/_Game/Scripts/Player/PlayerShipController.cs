using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerShipMovement))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerShipController : MonoBehaviour
{
    private PlayerShipMovement movement;
    private PlayerWeaponSystem weaponSystem;
    private PlayerInput playerInput;

    void Awake()
    {
        // Get required components
        movement = GetComponent<PlayerShipMovement>();
        weaponSystem = GetComponent<PlayerWeaponSystem>();
        playerInput = GetComponent<PlayerInput>();

        if (movement == null)
        {
            Debug.LogError("PlayerShipController requires PlayerShipMovement component!");
        }

        if (playerInput == null)
        {
            Debug.LogError("PlayerShipController requires PlayerInput component!");
        }
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
        }
    }

    void OnMove(InputAction.CallbackContext context)
    {
        if (movement != null)
        {
            Vector2 input = context.ReadValue<Vector2>();
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
}
