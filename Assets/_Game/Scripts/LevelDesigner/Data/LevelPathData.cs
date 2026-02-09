using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

namespace LevelDesigner
{
    [CreateAssetMenu(fileName = "NewPath", menuName = "Level Designer/Path Data")]
    public class LevelPathData : ScriptableObject
    {
        [SerializeField]
        private Spline spline = new Spline();

        [SerializeField]
        private List<PathSegmentSettings> segmentSettings = new List<PathSegmentSettings>();

        [Tooltip("Default speed used when no segment settings exist")]
        [SerializeField]
        private float defaultSpeed = 20f;

        private float cachedLength = -1f;

        public Spline Spline => spline;

        public float TotalLength
        {
            get
            {
                if (cachedLength < 0f)
                {
                    cachedLength = spline.GetLength();
                }
                return cachedLength;
            }
        }

        public int KnotCount => spline.Count;

        public bool IsValid => spline != null && spline.Count >= 2;

        public void InvalidateCache()
        {
            cachedLength = -1f;
        }

        public Vector3 GetPositionAtDistance(float distance)
        {
            if (!IsValid) return Vector3.zero;

            float t = DistanceToNormalizedT(distance);
            return spline.EvaluatePosition(t);
        }

        public Vector3 GetTangentAtDistance(float distance)
        {
            if (!IsValid) return Vector3.forward;

            float t = DistanceToNormalizedT(distance);
            return ((Vector3)spline.EvaluateTangent(t)).normalized;
        }

        public Vector3 GetUpAtDistance(float distance)
        {
            if (!IsValid) return Vector3.up;

            float t = DistanceToNormalizedT(distance);
            return spline.EvaluateUpVector(t);
        }

        public Quaternion GetRotationAtDistance(float distance)
        {
            Vector3 forward = GetTangentAtDistance(distance);
            Vector3 up = GetUpAtDistance(distance);

            if (forward == Vector3.zero) return Quaternion.identity;
            return Quaternion.LookRotation(forward, up);
        }

        public float GetSpeedAtDistance(float distance)
        {
            if (segmentSettings == null || segmentSettings.Count == 0)
            {
                return defaultSpeed;
            }

            int segmentIndex = GetSegmentIndexAtDistance(distance);
            segmentIndex = Mathf.Clamp(segmentIndex, 0, segmentSettings.Count - 1);

            PathSegmentSettings currentSettings = segmentSettings[segmentIndex];
            float segmentStart = GetSegmentStartDistance(segmentIndex);
            float distanceIntoSegment = distance - segmentStart;

            // Handle easing from previous segment's speed
            if (currentSettings.easeInDuration > 0f && distanceIntoSegment < GetDistanceForEasing(segmentIndex))
            {
                float previousSpeed = segmentIndex > 0 ? segmentSettings[segmentIndex - 1].speed : currentSettings.speed;
                float easeProgress = distanceIntoSegment / GetDistanceForEasing(segmentIndex);
                easeProgress = Mathf.SmoothStep(0f, 1f, easeProgress);
                return Mathf.Lerp(previousSpeed, currentSettings.speed, easeProgress);
            }

            return currentSettings.speed;
        }

        public int GetSegmentIndexAtDistance(float distance)
        {
            if (!IsValid || spline.Count < 2) return 0;

            float t = DistanceToNormalizedT(distance);
            int segmentCount = spline.Count - 1;
            int segmentIndex = Mathf.FloorToInt(t * segmentCount);
            return Mathf.Clamp(segmentIndex, 0, segmentCount - 1);
        }

        public float GetSegmentStartDistance(int segmentIndex)
        {
            if (!IsValid || segmentIndex <= 0) return 0f;

            int segmentCount = spline.Count - 1;
            float normalizedStart = (float)segmentIndex / segmentCount;
            return normalizedStart * TotalLength;
        }

        public void AddKnot(Vector3 position)
        {
            BezierKnot knot = new BezierKnot(position);

            // Set reasonable tangents for smooth curves
            if (spline.Count > 0)
            {
                Vector3 lastPos = spline[spline.Count - 1].Position;
                Vector3 direction = (position - lastPos).normalized;
                float tangentLength = Vector3.Distance(position, lastPos) * 0.3f;
                knot.TangentIn = -direction * tangentLength;
                knot.TangentOut = direction * tangentLength;
            }

            spline.Add(knot);
            segmentSettings.Add(PathSegmentSettings.Default);
            InvalidateCache();
        }

        public void InsertKnot(int index, Vector3 position)
        {
            BezierKnot knot = new BezierKnot(position);
            spline.Insert(index, knot);

            if (index < segmentSettings.Count)
            {
                segmentSettings.Insert(index, PathSegmentSettings.Default);
            }
            else
            {
                segmentSettings.Add(PathSegmentSettings.Default);
            }

            InvalidateCache();
        }

        public void RemoveKnot(int index)
        {
            if (index >= 0 && index < spline.Count)
            {
                spline.RemoveAt(index);
                InvalidateCache();
            }

            if (index >= 0 && index < segmentSettings.Count)
            {
                segmentSettings.RemoveAt(index);
            }
        }

        public void SetKnotPosition(int index, Vector3 position)
        {
            if (index >= 0 && index < spline.Count)
            {
                BezierKnot knot = spline[index];
                knot.Position = position;
                spline[index] = knot;
                InvalidateCache();
            }
        }

        public Vector3 GetKnotPosition(int index)
        {
            if (index >= 0 && index < spline.Count)
            {
                return spline[index].Position;
            }
            return Vector3.zero;
        }

        public void SetSegmentSettings(int index, PathSegmentSettings settings)
        {
            if (index >= 0 && index < segmentSettings.Count)
            {
                segmentSettings[index] = settings;
            }
        }

        public PathSegmentSettings GetSegmentSettings(int index)
        {
            if (index >= 0 && index < segmentSettings.Count)
            {
                return segmentSettings[index];
            }
            return PathSegmentSettings.Default;
        }

        public void ClearPath()
        {
            spline.Clear();
            segmentSettings.Clear();
            InvalidateCache();
        }

        private float DistanceToNormalizedT(float distance)
        {
            float length = TotalLength;
            if (length <= 0f) return 0f;
            return Mathf.Clamp01(distance / length);
        }

        private float GetDistanceForEasing(int segmentIndex)
        {
            if (segmentIndex < 0 || segmentIndex >= segmentSettings.Count)
            {
                return 0f;
            }

            PathSegmentSettings settings = segmentSettings[segmentIndex];
            // Convert ease duration to distance using current speed
            return settings.easeInDuration * settings.speed;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            InvalidateCache();
        }
#endif
    }
}
