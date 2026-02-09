using UnityEngine;

namespace LevelDesigner
{
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        [Header("Level Configuration")]
        [Tooltip("The level settings to load")]
        [SerializeField]
        private LevelSettings levelSettings;

        [Header("References")]
        [Tooltip("The path follower component (auto-found if null)")]
        [SerializeField]
        private LevelPathFollower pathFollower;

        [Tooltip("The enemy spawner component (auto-found if null)")]
        [SerializeField]
        private LevelEnemySpawner enemySpawner;

        [Header("Runtime State")]
        [SerializeField]
        private bool isInitialized;

        private GameObject terrainInstance;
        private Material previousSkybox;
        private Color previousAmbientColor;
        private bool previousFogEnabled;
        private float previousFogDensity;
        private Color previousFogColor;

        public LevelSettings CurrentSettings => levelSettings;
        public bool IsInitialized => isInitialized;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Auto-find components if not assigned
            if (pathFollower == null)
            {
                pathFollower = FindFirstObjectByType<LevelPathFollower>();
            }
            if (enemySpawner == null)
            {
                enemySpawner = FindFirstObjectByType<LevelEnemySpawner>();
            }
        }

        private void Start()
        {
            if (levelSettings != null && !isInitialized)
            {
                InitializeLevel();
            }
        }

        public void SetLevelSettings(LevelSettings settings)
        {
            levelSettings = settings;
        }

        public void InitializeLevel()
        {
            if (levelSettings == null)
            {
                Debug.LogWarning("LevelManager: No level settings assigned!");
                return;
            }

            if (!levelSettings.IsValid)
            {
                Debug.LogWarning("LevelManager: Level settings are invalid (missing or invalid path data)!");
                return;
            }

            // Store previous render settings for cleanup
            StoreRenderSettings();

            // Initialize path follower
            if (pathFollower != null)
            {
                pathFollower.SetPathData(levelSettings.pathData);
            }

            // Initialize enemy spawner
            if (enemySpawner != null)
            {
                enemySpawner.Initialize(levelSettings, pathFollower);
            }

            // Configure environment
            ConfigureEnvironment();

            isInitialized = true;
            Debug.Log($"LevelManager: Initialized level '{levelSettings.levelName}'");
        }

        private void ConfigureEnvironment()
        {
            // Skybox
            if (levelSettings.skyboxMaterial != null)
            {
                RenderSettings.skybox = levelSettings.skyboxMaterial;
            }

            // Ambient lighting
            RenderSettings.ambientLight = levelSettings.ambientLightColor;

            // Fog
            if (levelSettings.fogDensity > 0f)
            {
                RenderSettings.fog = true;
                RenderSettings.fogMode = FogMode.Exponential;
                RenderSettings.fogDensity = levelSettings.fogDensity;
                RenderSettings.fogColor = levelSettings.fogColor;
            }
            else
            {
                RenderSettings.fog = false;
            }

            // Terrain (for ground levels)
            if (levelSettings.environmentType == EnvironmentType.Ground ||
                levelSettings.environmentType == EnvironmentType.Hybrid)
            {
                SpawnTerrain();
            }
        }

        private void SpawnTerrain()
        {
            if (levelSettings.terrainPrefab == null) return;

            // Calculate terrain position based on path start
            Vector3 terrainPosition = Vector3.zero;
            if (levelSettings.pathData != null && levelSettings.pathData.IsValid)
            {
                terrainPosition = levelSettings.pathData.GetPositionAtDistance(0f);
                terrainPosition.y += levelSettings.terrainHeightOffset;
            }

            terrainInstance = Instantiate(levelSettings.terrainPrefab, terrainPosition, Quaternion.identity);
            terrainInstance.name = "Level_Terrain";
        }

        private void StoreRenderSettings()
        {
            previousSkybox = RenderSettings.skybox;
            previousAmbientColor = RenderSettings.ambientLight;
            previousFogEnabled = RenderSettings.fog;
            previousFogDensity = RenderSettings.fogDensity;
            previousFogColor = RenderSettings.fogColor;
        }

        private void RestoreRenderSettings()
        {
            RenderSettings.skybox = previousSkybox;
            RenderSettings.ambientLight = previousAmbientColor;
            RenderSettings.fog = previousFogEnabled;
            RenderSettings.fogDensity = previousFogDensity;
            RenderSettings.fogColor = previousFogColor;
        }

        public void CleanupLevel()
        {
            if (terrainInstance != null)
            {
                Destroy(terrainInstance);
                terrainInstance = null;
            }

            RestoreRenderSettings();
            isInitialized = false;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            CleanupLevel();
        }
    }
}
