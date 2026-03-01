using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleScreenController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject mainPanel;
    public GameObject settingsPanel;
    public Button playButton;
    public Button settingsButton;

    [Header("Audio")]
    public AudioClip menuMusic;

    void Start()
    {
        // Setup initial state
        ShowMainPanel();

        // Start menu music
        if (AudioManager.Instance != null && menuMusic != null)
        {
            AudioManager.Instance.PlayMusic(menuMusic);
        }

        // Wire up button events
        playButton.onClick.AddListener(OnPlayClicked);
        settingsButton.onClick.AddListener(OnSettingsClicked);
    }

    void OnPlayClicked()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.StopMusic();

        if (LevelProgressionManager.Instance != null)
            LevelProgressionManager.Instance.StartCurrentLevel();
        else if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.LoadScene("level1");
        else
            SceneManager.LoadScene("level1");
    }

    void OnSettingsClicked()
    {
        ShowSettingsPanel();
    }

    public void ShowMainPanel()
    {
        mainPanel.SetActive(true);
        settingsPanel.SetActive(false);
    }

    public void ShowSettingsPanel()
    {
        mainPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }
}
