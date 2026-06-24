using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Scene Configuration")]
    [SerializeField] private string gameplaySceneName = "Main";

    [Header("UI Panels")]
    [SerializeField] private GameObject settingsPanel;

    [Header("Audio Settings")]
    [SerializeField] private AudioMixer mainMixer;

    private void Start()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }

    public void SetVolume(float sliderValue)
    {
        if (sliderValue <= -40f)
        {
            mainMixer.SetFloat("MasterVolume", -80f);
        }
        else
        {
            mainMixer.SetFloat("MasterVolume", sliderValue);
        }
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}