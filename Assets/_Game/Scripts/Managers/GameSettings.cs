using UnityEngine;

public class GameSettings : MonoBehaviour
{
    private static GameSettings instance;
    public static GameSettings Instance => instance;

    private const string INVERT_Y_KEY = "InvertYAxis";
    private bool invertYAxis;

    public static bool InvertYAxis
    {
        get => Instance != null ? Instance.invertYAxis : false;
        set
        {
            if (Instance != null)
            {
                Instance.invertYAxis = value;
                PlayerPrefs.SetInt(INVERT_Y_KEY, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        LoadSettings();
    }

    void LoadSettings()
    {
        invertYAxis = PlayerPrefs.GetInt(INVERT_Y_KEY, 0) == 1;
        Debug.Log($"GameSettings loaded: InvertYAxis = {invertYAxis}");
    }
}
