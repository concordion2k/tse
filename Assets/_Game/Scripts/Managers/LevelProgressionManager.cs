using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using LevelDesigner;

/// <summary>
/// Persistent singleton that tracks player progression across levels.
/// Persists across scenes via DontDestroyOnLoad. Drives LevelManager
/// initialization by pushing the correct LevelSettings when each scene loads.
/// </summary>
public class LevelProgressionManager : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Singleton
    // -------------------------------------------------------------------------

    private static LevelProgressionManager instance;
    public static LevelProgressionManager Instance => instance;

    // -------------------------------------------------------------------------
    // Inspector
    // -------------------------------------------------------------------------

    [SerializeField]
    [Tooltip("The ordered sequence of levels for this game")]
    private LevelSequence levelSequence;

    // -------------------------------------------------------------------------
    // Save data
    // -------------------------------------------------------------------------

    [Serializable]
    private class LevelProgressData
    {
        public int currentLevelIndex;
        public List<int> levelStates; // 0=NotStarted, 1=InProgress, 2=Completed

        public LevelProgressData(int levelCount)
        {
            currentLevelIndex = 0;
            levelStates = new List<int>(new int[levelCount]);
        }
    }

    private LevelProgressData progressData;
    private string SavePath => Path.Combine(Application.persistentDataPath, "progression.json");

    // -------------------------------------------------------------------------
    // Runtime state
    // -------------------------------------------------------------------------

    private LevelPathFollower activePathFollower;

    // -------------------------------------------------------------------------
    // Events
    // -------------------------------------------------------------------------

    public event Action<int> OnLevelStarted;
    public event Action<int> OnLevelCompleted;

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    public int CurrentLevelIndex => progressData?.currentLevelIndex ?? 0;

    public LevelSettings CurrentLevelSettings => levelSequence.GetLevel(CurrentLevelIndex);

    public bool IsLevelCompleted(int index) => GetLevelState(index) == 2;

    public bool IsLevelAccessible(int index)
    {
        if (index == 0) return true;
        return IsLevelCompleted(index - 1);
    }

    /// <summary>Loads the current level scene via SceneTransitionManager.</summary>
    public void StartCurrentLevel()
    {
        string sceneName = levelSequence.GetSceneName(CurrentLevelIndex);
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.LoadScene(sceneName);
        else
            SceneManager.LoadScene(sceneName);
    }

    /// <summary>Marks the current level completed, fires completion events, saves, and advances to next level.</summary>
    public void CompleteCurrentLevel()
    {
        int index = CurrentLevelIndex;

        if (IsLevelCompleted(index))
            return;

        SetLevelState(index, 2);

        // Fire completion events defined in LevelSettings
        LevelSettings settings = CurrentLevelSettings;
        if (settings?.completionEvents != null)
        {
            foreach (LevelEvent evt in settings.completionEvents)
                evt?.Execute(settings);
        }

        SaveProgress();
        OnLevelCompleted?.Invoke(index);

        Debug.Log($"[LevelProgressionManager] Level {index} '{settings?.levelName}' completed.");

        AdvanceToNextLevel();
    }

    /// <summary>Advances currentLevelIndex to the next level if accessible.</summary>
    public void AdvanceToNextLevel()
    {
        int nextIndex = CurrentLevelIndex + 1;
        if (!levelSequence.IsValidIndex(nextIndex))
        {
            Debug.Log("[LevelProgressionManager] All levels completed.");
            return;
        }

        progressData.currentLevelIndex = nextIndex;
        SaveProgress();
    }

    /// <summary>Deletes save data and resets all progress.</summary>
    public void ResetProgression()
    {
        if (File.Exists(SavePath))
            File.Delete(SavePath);

        progressData = new LevelProgressData(levelSequence.LevelCount);
        Debug.Log("[LevelProgressionManager] Progression reset.");
    }

    public void SaveProgress()
    {
        string json = JsonUtility.ToJson(progressData, true);
        File.WriteAllText(SavePath, json);
    }

    public void LoadProgress()
    {
        if (File.Exists(SavePath))
        {
            string json = File.ReadAllText(SavePath);
            progressData = JsonUtility.FromJson<LevelProgressData>(json);

            // If levels were added since last save, expand the state list
            while (progressData.levelStates.Count < levelSequence.LevelCount)
                progressData.levelStates.Add(0);
        }
        else
        {
            progressData = new LevelProgressData(levelSequence.LevelCount);
        }

        Debug.Log($"[LevelProgressionManager] Progress loaded. Current level: {CurrentLevelIndex}");
    }

    // -------------------------------------------------------------------------
    // Lifecycle
    // -------------------------------------------------------------------------

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        LoadProgress();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;

        SceneManager.sceneLoaded -= OnSceneLoaded;
        UnsubscribeFromPathFollower();
    }

    // -------------------------------------------------------------------------
    // Scene integration
    // -------------------------------------------------------------------------

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        LevelManager levelManager = FindFirstObjectByType<LevelManager>();
        if (levelManager == null)
            return;

        LevelSettings settings = CurrentLevelSettings;
        if (settings == null)
        {
            Debug.LogWarning($"[LevelProgressionManager] No LevelSettings for index {CurrentLevelIndex}.");
            return;
        }

        // Push settings into the scene's LevelManager and initialize
        levelManager.SetLevelSettings(settings);
        levelManager.InitializeLevel();

        // Subscribe to path follower events
        SubscribeToPathFollower();

        // Reset trigger state for this play session
        ResetTriggers(settings);

        // Mark as InProgress if this is the first time entering
        if (GetLevelState(CurrentLevelIndex) == 0)
        {
            SetLevelState(CurrentLevelIndex, 1);
            SaveProgress();
        }

        OnLevelStarted?.Invoke(CurrentLevelIndex);
        Debug.Log($"[LevelProgressionManager] Level {CurrentLevelIndex} '{settings.levelName}' started.");
    }

    private void SubscribeToPathFollower()
    {
        UnsubscribeFromPathFollower();
        activePathFollower = FindFirstObjectByType<LevelPathFollower>();

        if (activePathFollower != null)
        {
            activePathFollower.OnDistanceChanged += HandleDistanceChanged;
            activePathFollower.OnPathComplete += HandlePathComplete;
        }
    }

    private void UnsubscribeFromPathFollower()
    {
        if (activePathFollower != null)
        {
            activePathFollower.OnDistanceChanged -= HandleDistanceChanged;
            activePathFollower.OnPathComplete -= HandlePathComplete;
            activePathFollower = null;
        }
    }

    // -------------------------------------------------------------------------
    // Trigger and path handling
    // -------------------------------------------------------------------------

    private void HandleDistanceChanged(float distance)
    {
        LevelSettings settings = CurrentLevelSettings;
        if (settings?.levelTriggers == null)
            return;

        foreach (GameplayTrigger trigger in settings.levelTriggers)
        {
            if (trigger.hasTriggered)
                continue;

            if (trigger.triggerType == TriggerType.PathDistance && distance >= trigger.pathDistance)
            {
                trigger.hasTriggered = true;
                FireTrigger(trigger, settings);
            }
        }
    }

    private void HandlePathComplete()
    {
        CompleteCurrentLevel();
    }

    private void FireTrigger(GameplayTrigger trigger, LevelSettings context)
    {
        if (trigger.events == null)
            return;

        Debug.Log($"[LevelProgressionManager] Trigger '{trigger.label}' fired at distance {trigger.pathDistance}.");

        foreach (LevelEvent evt in trigger.events)
            evt?.Execute(context);
    }

    private void ResetTriggers(LevelSettings settings)
    {
        if (settings?.levelTriggers == null)
            return;

        foreach (GameplayTrigger trigger in settings.levelTriggers)
            trigger.hasTriggered = false;
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private int GetLevelState(int index)
    {
        if (progressData?.levelStates == null || index < 0 || index >= progressData.levelStates.Count)
            return 0;
        return progressData.levelStates[index];
    }

    private void SetLevelState(int index, int state)
    {
        if (progressData?.levelStates == null || index < 0 || index >= progressData.levelStates.Count)
            return;
        progressData.levelStates[index] = state;
    }
}
