using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class MainMenuController : MonoBehaviour
{
    [Header("Scene Configuration")]
    [SerializeField] private string gameplaySceneName = "Main";

    [Header("UI Panels")]
    [SerializeField] private GameObject settingsPanel;

    private void Start()
    {
        // Audio: start menu music (Dongas)
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMenuMusic();
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}