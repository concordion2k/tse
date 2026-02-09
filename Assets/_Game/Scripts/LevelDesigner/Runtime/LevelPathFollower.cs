using System;
using UnityEngine;

namespace LevelDesigner
{
    public class LevelPathFollower : MonoBehaviour
    {
        [Header("Path Configuration")]
        [Tooltip("The path data to follow")]
        [SerializeField]
        private LevelPathData pathData;

        [Tooltip("Start at this distance along the path")]
        [SerializeField]
        private float startDistance = 0f;

        [Tooltip("Speed multiplier applied to path segment speeds")]
        [SerializeField]
        private float speedMultiplier = 1f;

        [Header("State")]
        [SerializeField]
        private bool isFollowing = true;

        private float currentDistance;
        private float previousDistance;
        private float fixedUpdateTimer;

        // Cached values for current frame
        private Vector3 currentPosition;
        private Vector3 currentForward;
        private Vector3 currentUp;
        private float currentSpeed;

        public event Action<float> OnDistanceChanged;
        public event Action OnPathComplete;

        public bool HasValidPath => pathData != null && pathData.IsValid;
        public float CurrentDistance => currentDistance;
        public float NormalizedProgress => HasValidPath ? currentDistance / pathData.TotalLength : 0f;
        public float TotalLength => HasValidPath ? pathData.TotalLength : 0f;
        public bool IsFollowing => isFollowing;
        public LevelPathData PathData => pathData;

        public void SetPathData(LevelPathData data)
        {
            pathData = data;
            ResetToStart();
        }

        public void ResetToStart()
        {
            currentDistance = startDistance;
            previousDistance = currentDistance;
            UpdateCachedValues();
        }

        public void StartFollowing()
        {
            isFollowing = true;
        }

        public void StopFollowing()
        {
            isFollowing = false;
        }

        public void SetDistance(float distance)
        {
            currentDistance = Mathf.Clamp(distance, 0f, HasValidPath ? pathData.TotalLength : 0f);
            previousDistance = currentDistance;
            UpdateCachedValues();
        }

        /// <summary>
        /// Interpolated position for use in Update/LateUpdate.
        /// </summary>
        public Vector3 GetPathPosition()
        {
            float interpolationFactor = Mathf.Clamp01(fixedUpdateTimer / Time.fixedDeltaTime);
            Vector3 prevPos = HasValidPath ? pathData.GetPositionAtDistance(previousDistance) : transform.position;
            return Vector3.Lerp(prevPos, currentPosition, interpolationFactor);
        }

        /// <summary>
        /// Raw cached position without interpolation. Use from FixedUpdate to avoid double-interpolation.
        /// </summary>
        public Vector3 GetCurrentPositionRaw()
        {
            return currentPosition;
        }

        public Vector3 GetPathForward()
        {
            return currentForward;
        }

        public Vector3 GetPathUp()
        {
            return currentUp;
        }

        public Quaternion GetPathRotation()
        {
            if (currentForward == Vector3.zero) return Quaternion.identity;
            return Quaternion.LookRotation(currentForward, currentUp);
        }

        public float GetCurrentSpeed()
        {
            return currentSpeed;
        }

        private void Awake()
        {
            currentDistance = startDistance;
            previousDistance = currentDistance;
        }

        private void Start()
        {
            UpdateCachedValues();
        }

        private void FixedUpdate()
        {
            if (!isFollowing || !HasValidPath) return;

            previousDistance = currentDistance;

            // Get speed for current position and advance
            currentSpeed = pathData.GetSpeedAtDistance(currentDistance) * speedMultiplier;
            currentDistance += currentSpeed * Time.fixedDeltaTime;

            // Check for path completion
            if (currentDistance >= pathData.TotalLength)
            {
                currentDistance = pathData.TotalLength;
                isFollowing = false;
                OnPathComplete?.Invoke();
            }

            UpdateCachedValues();
            OnDistanceChanged?.Invoke(currentDistance);

            fixedUpdateTimer = 0f;
        }

        private void Update()
        {
            fixedUpdateTimer += Time.deltaTime;
        }

        private void UpdateCachedValues()
        {
            if (!HasValidPath)
            {
                currentPosition = transform.position;
                currentForward = transform.forward;
                currentUp = transform.up;
                currentSpeed = 0f;
                return;
            }

            currentPosition = pathData.GetPositionAtDistance(currentDistance);
            currentForward = pathData.GetTangentAtDistance(currentDistance);
            currentUp = pathData.GetUpAtDistance(currentDistance);
            currentSpeed = pathData.GetSpeedAtDistance(currentDistance) * speedMultiplier;
        }

        private void OnDrawGizmosSelected()
        {
            if (!HasValidPath) return;

            // Draw current position on path
            Gizmos.color = Color.green;
            Vector3 pos = pathData.GetPositionAtDistance(currentDistance);
            Gizmos.DrawWireSphere(pos, 1f);

            // Draw forward direction
            Gizmos.color = Color.blue;
            Vector3 forward = pathData.GetTangentAtDistance(currentDistance);
            Gizmos.DrawRay(pos, forward * 5f);

            // Draw up direction
            Gizmos.color = Color.green;
            Vector3 up = pathData.GetUpAtDistance(currentDistance);
            Gizmos.DrawRay(pos, up * 3f);
        }
    }
}
