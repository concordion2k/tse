using UnityEngine;
using UnityEngine.Events;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Score")]
    [SerializeField]
    private int killCount = 0;

    [Header("Events")]
    public UnityEvent<int> OnKillCountChanged;

    public int KillCount => killCount;

    void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void AddKill()
    {
        killCount++;
        OnKillCountChanged?.Invoke(killCount);
    }

    public void ResetScore()
    {
        killCount = 0;
        OnKillCountChanged?.Invoke(killCount);
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
