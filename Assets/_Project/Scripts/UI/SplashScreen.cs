using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// Fades the logo in, plays the splash sound, holds, fades out, then loads MainMenu.
public class SplashScreen : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Image logo;            // full-screen or centered logo Image
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip splashSound;

    [Header("Timing (seconds)")]
    [SerializeField] private float fadeInDuration = 0.6f;
    [SerializeField] private float holdDuration = 1.6f;
    [SerializeField] private float fadeOutDuration = 0.6f;

    [Header("Next scene")]
    [SerializeField] private string nextScene = "MainMenu";

    private void Start()
    {
        StartCoroutine(RunSplash());
    }

    private IEnumerator RunSplash()
    {
        if (splashSound != null && audioSource != null)
            audioSource.PlayOneShot(splashSound);

        yield return Fade(0f, 1f, fadeInDuration);
        yield return new WaitForSeconds(holdDuration);
        yield return Fade(1f, 0f, fadeOutDuration);

        SceneManager.LoadScene(nextScene);
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        if (logo == null) yield break;
        float t = 0f;
        SetAlpha(from);
        while (t < duration)
        {
            t += Time.deltaTime;
            SetAlpha(Mathf.Lerp(from, to, t / duration));
            yield return null;
        }
        SetAlpha(to);
    }

    private void SetAlpha(float a)
    {
        Color c = logo.color;
        c.a = a;
        logo.color = c;
    }
}