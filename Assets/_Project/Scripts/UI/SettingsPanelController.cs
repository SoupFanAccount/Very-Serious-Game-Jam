using UnityEngine;
using UnityEngine.UI;

public class SettingsPanelController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Slider volumeSlider;

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