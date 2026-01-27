using UnityEngine;
using TMPro;

public class KillCounterUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Text component to display kill count")]
    public TextMeshProUGUI killCountText;

    [Header("Display Settings")]
    [Tooltip("Format string for kill count (use {0} for the number)")]
    public string displayFormat = "KILLS: {0}";

    void Start()
    {
        // Subscribe to score manager events
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnKillCountChanged.AddListener(UpdateDisplay);
            UpdateDisplay(ScoreManager.Instance.KillCount);
        }
        else
        {
            Debug.LogWarning("KillCounterUI: ScoreManager not found!");
            UpdateDisplay(0);
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnKillCountChanged.RemoveListener(UpdateDisplay);
        }
    }

    public void UpdateDisplay(int killCount)
    {
        if (killCountText != null)
        {
            killCountText.text = string.Format(displayFormat, killCount);
        }
    }
}
