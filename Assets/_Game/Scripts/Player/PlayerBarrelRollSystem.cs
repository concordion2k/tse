using UnityEngine;
using UnityEngine.Events;

public class PlayerBarrelRollSystem : MonoBehaviour
{
    public enum RollDirection { None, Left, Right }

    [Header("Roll Settings")]
    [Tooltip("Total angle to roll (degrees)")]
    public float rollAngle = 90f;

    [Tooltip("Duration of the roll animation (seconds)")]
    public float rollDuration = 0.5f;

    [Tooltip("Animation curve for roll smoothing (0=start, 1=end)")]
    public AnimationCurve rollCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Speed Boost")]
    [Tooltip("Lateral speed multiplier in roll direction while holding")]
    [Range(1f, 2f)]
    public float lateralSpeedBoost = 1.3f;

    [Header("Events")]
    public UnityEvent<RollDirection> OnRollStarted;
    public UnityEvent OnRollCompleted;

    // State
    private RollDirection currentRollDirection = RollDirection.None;
    private bool isRolling;
    private bool isReturning; // True while animating back to 0
    private bool isHoldingRoll; // True while button is held after roll completes
    private float rollTimer;
    private float returnTimer;
    private float returnStartAngle; // Angle we're returning from
    private float currentRollAngle;
    private bool isHoldingLeft;
    private bool isHoldingRight;

    // Properties
    public bool IsRolling => isRolling;
    public RollDirection CurrentDirection => currentRollDirection;

    /// <summary>
    /// Returns the current barrel roll angle offset to add to ship bank.
    /// </summary>
    public float GetRollAngleOffset()
    {
        return currentRollAngle;
    }

    /// <summary>
    /// Returns which direction gets the speed boost (-1 = left, 1 = right, 0 = none)
    /// </summary>
    public float GetBoostedDirection()
    {
        if (isHoldingLeft) return -1f;
        if (isHoldingRight) return 1f;
        return 0f;
    }

    public void StartLeftRoll(bool isPressed)
    {
        isHoldingLeft = isPressed;

        if (isPressed && !isRolling && !isHoldingRoll)
        {
            StartRoll(RollDirection.Left);
        }
        else if (!isPressed && isHoldingRoll && currentRollDirection == RollDirection.Left)
        {
            // Button released, reset rotation
            ReleaseRoll();
        }
    }

    public void StartRightRoll(bool isPressed)
    {
        isHoldingRight = isPressed;

        if (isPressed && !isRolling && !isHoldingRoll)
        {
            StartRoll(RollDirection.Right);
        }
        else if (!isPressed && isHoldingRoll && currentRollDirection == RollDirection.Right)
        {
            // Button released, reset rotation
            ReleaseRoll();
        }
    }

    private void StartRoll(RollDirection direction)
    {
        isRolling = true;
        currentRollDirection = direction;
        rollTimer = 0f;
        currentRollAngle = 0f;

        OnRollStarted?.Invoke(direction);
    }

    void Update()
    {
        if (!isRolling) return;

        rollTimer += Time.deltaTime;
        float normalizedTime = Mathf.Clamp01(rollTimer / rollDuration);
        float curveValue = rollCurve.Evaluate(normalizedTime);

        // Calculate target cumulative angle at this point
        float targetAngle = curveValue * rollAngle;

        // Direction multiplier: Left = positive (CCW), Right = negative (CW)
        float directionMultiplier = currentRollDirection == RollDirection.Left ? 1f : -1f;

        currentRollAngle = targetAngle * directionMultiplier;

        // Check if roll completed
        if (normalizedTime >= 1f)
        {
            CompleteRoll();
        }
    }

    private void CompleteRoll()
    {
        isRolling = false;

        // Check if button is still held - if so, maintain the rotation
        bool buttonStillHeld = (currentRollDirection == RollDirection.Left && isHoldingLeft) ||
                               (currentRollDirection == RollDirection.Right && isHoldingRight);

        if (buttonStillHeld)
        {
            // Keep the rotation at full angle while button is held
            isHoldingRoll = true;
            float directionMultiplier = currentRollDirection == RollDirection.Left ? 1f : -1f;
            currentRollAngle = rollAngle * directionMultiplier;
        }
        else
        {
            // Button was released during roll, reset everything
            currentRollDirection = RollDirection.None;
            currentRollAngle = 0f;
            rollTimer = 0f;
        }

        OnRollCompleted?.Invoke();
    }

    private void ReleaseRoll()
    {
        isHoldingRoll = false;
        currentRollDirection = RollDirection.None;
        currentRollAngle = 0f;
        rollTimer = 0f;
    }
}
