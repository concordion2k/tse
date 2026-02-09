using System.Collections.Generic;
using UnityEngine;

namespace LevelDesigner
{
    public enum EnvironmentType
    {
        Space,
        Ground,
        Hybrid
    }

    [CreateAssetMenu(fileName = "NewLevel", menuName = "Level Designer/Level Settings")]
    public class LevelSettings : ScriptableObject
    {
        [Header("Level Info")]
        [Tooltip("Display name for this level")]
        public string levelName = "New Level";

        [Tooltip("Description of the level")]
        [TextArea(2, 4)]
        public string description;

        [Header("Path")]
        [Tooltip("The path data for this level")]
        public LevelPathData pathData;

        [Header("Environment")]
        [Tooltip("Type of environment (affects terrain and skybox)")]
        public EnvironmentType environmentType = EnvironmentType.Space;

        [Tooltip("Custom skybox material (optional - uses default if null)")]
        public Material skyboxMaterial;

        [Tooltip("Terrain prefab for ground levels (optional)")]
        public GameObject terrainPrefab;

        [Tooltip("Height offset for terrain below the path")]
        public float terrainHeightOffset = -50f;

        [Header("Lighting")]
        [Tooltip("Ambient light color for this level")]
        public Color ambientLightColor = new Color(0.2f, 0.2f, 0.3f);

        [Tooltip("Fog density (0 = no fog)")]
        [Range(0f, 0.1f)]
        public float fogDensity = 0f;

        [Tooltip("Fog color")]
        public Color fogColor = Color.gray;

        [Header("Enemy Placements")]
        [Tooltip("All enemy spawn points for this level")]
        public List<EnemyPlacementData> enemyPlacements = new List<EnemyPlacementData>();

        [Header("Audio")]
        [Tooltip("Background music track for this level")]
        public AudioClip musicTrack;

        public bool IsValid => pathData != null && pathData.IsValid;

        public void AddEnemyPlacement(EnemyPlacementData placement)
        {
            if (enemyPlacements == null)
            {
                enemyPlacements = new List<EnemyPlacementData>();
            }
            enemyPlacements.Add(placement);
            SortEnemyPlacements();
        }

        public void RemoveEnemyPlacement(int index)
        {
            if (enemyPlacements != null && index >= 0 && index < enemyPlacements.Count)
            {
                enemyPlacements.RemoveAt(index);
            }
        }

        public void SortEnemyPlacements()
        {
            if (enemyPlacements != null)
            {
                enemyPlacements.Sort((a, b) => a.pathDistance.CompareTo(b.pathDistance));
            }
        }

        public void ClearEnemyPlacements()
        {
            enemyPlacements?.Clear();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            SortEnemyPlacements();
        }
#endif
    }
}
