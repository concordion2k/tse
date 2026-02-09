using UnityEngine;

namespace LevelDesigner
{
    public enum FormationType
    {
        None,
        Line,
        V,
        Diamond,
        Circle
    }

    [System.Serializable]
    public class EnemyPlacementData
    {
        [Tooltip("The enemy prefab to spawn")]
        public GameObject enemyPrefab;

        [Tooltip("Distance along the path where this enemy spawns")]
        [Min(0f)]
        public float pathDistance;

        [Tooltip("Offset from the path center (X=lateral, Y=vertical, Z=forward)")]
        public Vector3 offsetFromPath;

        [Tooltip("Custom scale multiplier for this enemy (1 = prefab default)")]
        [Min(0.1f)]
        public float scaleMultiplier = 1f;

        [Header("Formation Settings")]
        [Tooltip("Spawn as part of a formation")]
        public bool useFormation;

        [Tooltip("Formation pattern type")]
        public FormationType formation;

        [Tooltip("Number of enemies in the formation")]
        [Range(2, 10)]
        public int formationCount = 3;

        [Tooltip("Spacing between formation members")]
        [Min(1f)]
        public float formationSpacing = 5f;

        public EnemyPlacementData()
        {
            scaleMultiplier = 1f;
            formationCount = 3;
            formationSpacing = 5f;
        }

        public EnemyPlacementData Clone()
        {
            return new EnemyPlacementData
            {
                enemyPrefab = enemyPrefab,
                pathDistance = pathDistance,
                offsetFromPath = offsetFromPath,
                scaleMultiplier = scaleMultiplier,
                useFormation = useFormation,
                formation = formation,
                formationCount = formationCount,
                formationSpacing = formationSpacing
            };
        }
    }
}
