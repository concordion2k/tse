using UnityEngine;
using UnityEngine.UI;

public class SettingsMenuController : MonoBehaviour
{
    [Header("UI References")]
    public Toggle invertYToggle;
    public Button backButton;

    private TitleScreenController titleScreen;

    void Start()
    {
        titleScreen = FindFirstObjectByType<TitleScreenController>();

        // Load current setting
        if (invertYToggle != null)
        {
            invertYToggle.isOn = GameSettings.InvertYAxis;
            invertYToggle.onValueChanged.AddListener(OnInvertYChanged);
        }

        // Wire up back button
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackClicked);
        }
    }

    void OnInvertYChanged(bool value)
    {
        GameSettings.InvertYAxis = value;
        Debug.Log($"Y-Axis Invert set to: {value}");
    }

    void OnBackClicked()
    {
        if (titleScreen != null)
        {
            titleScreen.ShowMainPanel();
        }
    }

    void OnDestroy()
    {
        if (invertYToggle != null)
        {
            invertYToggle.onValueChanged.RemoveListener(OnInvertYChanged);
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveListener(OnBackClicked);
        }
    }
}
