using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIButtonAudio : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [Header("Audio Clips")]
    [SerializeField] private AudioClip hoverSFX;
    [SerializeField] private AudioClip clickSFX;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (AudioManager.Instance == null) return;
        // Fall back to the shared default so a button only needs the component, not its own clips.
        AudioClip clip = hoverSFX != null ? hoverSFX : AudioManager.Instance.UiHover;
        AudioManager.Instance.PlaySFX(clip);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (AudioManager.Instance == null) return;
        AudioClip clip = clickSFX != null ? clickSFX : AudioManager.Instance.UiClick;
        AudioManager.Instance.PlaySFX(clip);
    }
}