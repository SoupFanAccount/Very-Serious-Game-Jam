using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenuController : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject hudPanel;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip menuOpenSFX;
    [SerializeField] private AudioClip menuCloseSFX;

    private InputAction _escapeAction;
    private bool _isPaused = false;

    private void Awake()
    {
        _escapeAction = new InputAction(binding: "<Keyboard>/escape");
    }

    private void OnEnable()
    {
        _escapeAction.Enable();
        _escapeAction.performed += OnEscapePressed;
    }

    private void OnDisable()
    {
        _escapeAction.performed -= OnEscapePressed;
        _escapeAction.Disable();
    }

    private void OnEscapePressed(InputAction.CallbackContext context)
    {
        if (_isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        _isPaused = true;

        if (AudioManager.Instance != null)
        {
            // Use the per-controller override if assigned, otherwise the shared menu-open sound.
            if (menuOpenSFX != null) AudioManager.Instance.PlaySFX(menuOpenSFX);
            else AudioManager.Instance.PlayMenuOpen();
        }

        if (pausePanel != null) pausePanel.SetActive(true);

        if (hudPanel != null) hudPanel.SetActive(false);

        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        _isPaused = false;

        if (AudioManager.Instance != null)
        {
            if (menuCloseSFX != null) AudioManager.Instance.PlaySFX(menuCloseSFX);
            else AudioManager.Instance.PlayMenuClose();
        }

        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        if (hudPanel != null) hudPanel.SetActive(true);

        Time.timeScale = 1f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}