using UnityEngine;

public class GameplaySceneController : MonoBehaviour
{
    void Start()
    {
        // Start gameplay music from AudioManager
        if (AudioManager.Instance != null && AudioManager.Instance.gameplayMusic != null)
        {
            AudioManager.Instance.PlayMusic(AudioManager.Instance.gameplayMusic);
        }
    }
}
