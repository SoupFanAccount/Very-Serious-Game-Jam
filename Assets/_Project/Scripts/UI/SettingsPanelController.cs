using UnityEngine;
using UnityEngine.UI;

public class SettingsPanelController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Slider volumeSlider;

    // Last volume at which a tick sound played, so dragging the slider ratchets
    // instead of firing a sound every frame.
    private float _lastTickVolume = -1f;

    private void Start()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
        ApplySavedVolumeOnLoad();
    }

    private void ApplySavedVolumeOnLoad()
    {
        float savedVolume = PlayerPrefs.GetFloat("MasterVolumeLevel", 1f);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMasterVolume(savedVolume);
        }
    }

    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }

        if (volumeSlider != null)
        {
            volumeSlider.value = PlayerPrefs.GetFloat("MasterVolumeLevel", 1f);
        }
    }

    public void SetVolume(float sliderValue)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMasterVolume(sliderValue);

            // Ratchet a tick every ~5% of travel so dragging feels responsive without spamming.
            if (_lastTickVolume < 0f || Mathf.Abs(sliderValue - _lastTickVolume) >= 0.05f)
            {
                AudioManager.Instance.PlayVolumeTick();
                _lastTickVolume = sliderValue;
            }

            PlayerPrefs.SetFloat("MasterVolumeLevel", sliderValue);
            PlayerPrefs.Save();
        }
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }
}