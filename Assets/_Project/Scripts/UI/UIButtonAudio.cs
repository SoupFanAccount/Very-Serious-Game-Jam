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
        if (hoverSFX != null)
        {
            AudioManager.Instance.PlaySFX(hoverSFX);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (clickSFX != null)
        {
            AudioManager.Instance.PlaySFX(clickSFX);
        }
    }
}