using System.Collections.Generic;
using UnityEngine;

namespace LevelDesigner
{
    public class LevelEnemySpawner : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("Distance ahead of player to spawn enemies")]
        [SerializeField]
        private float spawnAheadDistance = 100f;

        [Tooltip("Distance behind player to despawn enemies")]
        [SerializeField]
        private float despawnBehindDistance = 50f;

        [Header("Debug")]
        [SerializeField]
        private bool showDebugInfo;

        private LevelSettings levelSettings;
        private LevelPathFollower pathFollower;
        private List<EnemyPlacementData> pendingSpawns = new List<EnemyPlacementData>();
        private List<SpawnedEnemy> activeEnemies = new List<SpawnedEnemy>();
        private bool isInitialized;

        private struct SpawnedEnemy
        {
            public GameObject instance;
            public float spawnDistance;
        }

        public void Initialize(LevelSettings settings, LevelPathFollower follower)
        {
            levelSettings = settings;
            pathFollower = follower;

            // Copy enemy placements to pending list
            pendingSpawns.Clear();
            if (settings.enemyPlacements != null)
            {
                foreach (var placement in settings.enemyPlacements)
                {
                    pendingSpawns.Add(placement.Clone());
                }
            }

            // Sort by distance
            pendingSpawns.Sort((a, b) => a.pathDistance.CompareTo(b.pathDistance));

            activeEnemies.Clear();
            isInitialized = true;

            if (showDebugInfo)
            {
                Debug.Log($"LevelEnemySpawner: Initialized with {pendingSpawns.Count} enemy placements");
            }
        }

        private void Update()
        {
            if (!isInitialized || pathFollower == null || !pathFollower.HasValidPath) return;

            float currentDistance = pathFollower.CurrentDistance;
            float spawnThreshold = currentDistance + spawnAheadDistance;

            // Spawn pending enemies that are within range
            while (pendingSpawns.Count > 0 && pendingSpawns[0].pathDistance <= spawnThreshold)
            {
                SpawnEnemy(pendingSpawns[0]);
                pendingSpawns.RemoveAt(0);
            }

            // Despawn enemies that are too far behind
            float despawnThreshold = currentDistance - despawnBehindDistance;
            for (int i = activeEnemies.Count - 1; i >= 0; i--)
            {
                if (activeEnemies[i].spawnDistance < despawnThreshold)
                {
                    if (activeEnemies[i].instance != null)
                    {
                        Destroy(activeEnemies[i].instance);
                    }
                    activeEnemies.RemoveAt(i);
                }
            }
        }

        private void SpawnEnemy(EnemyPlacementData placement)
        {
            if (placement.enemyPrefab == null)
            {
                Debug.LogWarning("LevelEnemySpawner: Enemy placement has no prefab assigned!");
                return;
            }

            if (placement.useFormation)
            {
                SpawnFormation(placement);
            }
            else
            {
                SpawnSingleEnemy(placement);
            }
        }

        private void SpawnSingleEnemy(EnemyPlacementData placement)
        {
            Vector3 worldPosition = CalculateWorldPosition(placement.pathDistance, placement.offsetFromPath);
            Quaternion rotation = CalculateEnemyRotation(placement.pathDistance);

            GameObject enemy = Instantiate(placement.enemyPrefab, worldPosition, rotation);
            enemy.transform.localScale *= placement.scaleMultiplier;

            activeEnemies.Add(new SpawnedEnemy
            {
                instance = enemy,
                spawnDistance = placement.pathDistance
            });

            if (showDebugInfo)
            {
                Debug.Log($"LevelEnemySpawner: Spawned {placement.enemyPrefab.name} at distance {placement.pathDistance}");
            }
        }

        private void SpawnFormation(EnemyPlacementData placement)
        {
            Vector3[] offsets = GetFormationOffsets(placement.formation, placement.formationCount, placement.formationSpacing);

            foreach (Vector3 formationOffset in offsets)
            {
                Vector3 totalOffset = placement.offsetFromPath + formationOffset;
                Vector3 worldPosition = CalculateWorldPosition(placement.pathDistance, totalOffset);
                Quaternion rotation = CalculateEnemyRotation(placement.pathDistance);

                GameObject enemy = Instantiate(placement.enemyPrefab, worldPosition, rotation);
                enemy.transform.localScale *= placement.scaleMultiplier;

                activeEnemies.Add(new SpawnedEnemy
                {
                    instance = enemy,
                    spawnDistance = placement.pathDistance
                });
            }

            if (showDebugInfo)
            {
                Debug.Log($"LevelEnemySpawner: Spawned {placement.formation} formation of {offsets.Length} enemies");
            }
        }

        private Vector3 CalculateWorldPosition(float pathDistance, Vector3 localOffset)
        {
            if (levelSettings.pathData == null) return Vector3.zero;

            Vector3 pathPosition = levelSettings.pathData.GetPositionAtDistance(pathDistance);
            Vector3 pathForward = levelSettings.pathData.GetTangentAtDistance(pathDistance);
            Vector3 pathUp = levelSettings.pathData.GetUpAtDistance(pathDistance);
            Vector3 pathRight = Vector3.Cross(pathUp, pathForward).normalized;

            // Apply local offset in path-relative space
            return pathPosition
                + pathRight * localOffset.x
                + pathUp * localOffset.y
                + pathForward * localOffset.z;
        }

        private Quaternion CalculateEnemyRotation(float pathDistance)
        {
            if (levelSettings.pathData == null) return Quaternion.identity;

            // Face opposite to path direction (toward player)
            Vector3 pathForward = levelSettings.pathData.GetTangentAtDistance(pathDistance);
            Vector3 pathUp = levelSettings.pathData.GetUpAtDistance(pathDistance);

            return Quaternion.LookRotation(-pathForward, pathUp);
        }

        private Vector3[] GetFormationOffsets(FormationType formation, int count, float spacing)
        {
            List<Vector3> offsets = new List<Vector3>();

            switch (formation)
            {
                case FormationType.Line:
                    float lineStart = -(count - 1) * spacing * 0.5f;
                    for (int i = 0; i < count; i++)
                    {
                        offsets.Add(new Vector3(lineStart + i * spacing, 0f, 0f));
                    }
                    break;

                case FormationType.V:
                    offsets.Add(Vector3.zero); // Leader
                    for (int i = 1; i < count; i++)
                    {
                        int side = (i % 2 == 1) ? 1 : -1;
                        int row = (i + 1) / 2;
                        offsets.Add(new Vector3(side * row * spacing, 0f, -row * spacing));
                    }
                    break;

                case FormationType.Diamond:
                    offsets.Add(Vector3.zero); // Center
                    if (count > 1) offsets.Add(new Vector3(spacing, 0f, 0f));
                    if (count > 2) offsets.Add(new Vector3(-spacing, 0f, 0f));
                    if (count > 3) offsets.Add(new Vector3(0f, 0f, spacing));
                    if (count > 4) offsets.Add(new Vector3(0f, 0f, -spacing));
                    // Add more in expanding pattern
                    for (int i = 5; i < count; i++)
                    {
                        float angle = (i - 5) * (360f / (count - 4)) * Mathf.Deg2Rad;
                        offsets.Add(new Vector3(Mathf.Cos(angle) * spacing * 2f, 0f, Mathf.Sin(angle) * spacing * 2f));
                    }
                    break;

                case FormationType.Circle:
                    for (int i = 0; i < count; i++)
                    {
                        float angle = i * (360f / count) * Mathf.Deg2Rad;
                        offsets.Add(new Vector3(Mathf.Cos(angle) * spacing, 0f, Mathf.Sin(angle) * spacing));
                    }
                    break;

                default:
                    offsets.Add(Vector3.zero);
                    break;
            }

            return offsets.ToArray();
        }

        public void CleanupAll()
        {
            foreach (var enemy in activeEnemies)
            {
                if (enemy.instance != null)
                {
                    Destroy(enemy.instance);
                }
            }
            activeEnemies.Clear();
            pendingSpawns.Clear();
            isInitialized = false;
        }

        private void OnDestroy()
        {
            CleanupAll();
        }
    }
}
