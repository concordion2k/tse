using UnityEngine;

namespace LevelDesigner
{
    [System.Serializable]
    public struct PathSegmentSettings
    {
        [Tooltip("Movement speed in units per second for this segment")]
        [Min(1f)]
        public float speed;

        [Tooltip("Time in seconds to ease into this segment's speed from the previous")]
        [Min(0f)]
        public float easeInDuration;

        public static PathSegmentSettings Default => new PathSegmentSettings
        {
            speed = 20f,
            easeInDuration = 0.5f
        };
    }
}
