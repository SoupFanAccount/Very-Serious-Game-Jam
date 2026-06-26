using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Emoji : MonoBehaviour
{
    public enum EmojiType
    {
        Angry,
        Happy
    }

    [SerializeField] private Image emojiImg;

    [SerializeField] private Vector3 maxScale;
    
    [SerializeField] private AnimationCurve appearCurve;
    [SerializeField] private AnimationCurve disappearCurve;
    
    [Header("Sprites")]
    [SerializeField] private Sprite[] angryEmojiSprites;
    [SerializeField] private Sprite[] happyEmojiSprites;

    private Coroutine _currentRoutine;

    public void ShowEmoji(EmojiType type)
    {
        emojiImg.gameObject.SetActive(true);

        if (_currentRoutine != null)
            StopCoroutine(_currentRoutine);

        _currentRoutine = StartCoroutine(Animate(type));
    }

    public void Hide()
    {
        if (_currentRoutine != null)
            StopCoroutine(_currentRoutine);

        _currentRoutine = null;
        emojiImg.gameObject.SetActive(false);
    }

    private IEnumerator Animate(EmojiType type)
    {
        Sprite[] sprites = (type == EmojiType.Angry)
            ? angryEmojiSprites
            : happyEmojiSprites;

        if (sprites.Length <= 0)
            yield break;

        emojiImg.sprite = sprites[0];
        
        yield return PopEmoji(0.3f);
        
        int i = 0;

        while (i < sprites.Length)
        {
            emojiImg.sprite = sprites[i];
            i++;
            yield return new WaitForSeconds(0.05f);
        }

        yield return new WaitForSeconds(.5f);

        yield return CloseEmoji(0.3f);
    }

    private IEnumerator PopEmoji(float duration)
    {
        float t = 0f;

        Vector3 startScale = Vector3.zero;
        emojiImg.transform.localScale = Vector3.zero;
        
        while (t < duration)
        {
            t += Time.deltaTime;

            float value = disappearCurve.Evaluate(t / duration);
            Vector3 scale = Vector3.LerpUnclamped(startScale, maxScale, value);

            emojiImg.transform.localScale = scale;
            yield return null;
        }
        
        emojiImg.transform.localScale = maxScale;
    }

    private IEnumerator CloseEmoji(float duration)
    {
        float t = 0f;

        Vector3 startScale = emojiImg.transform.localScale;

        while (t < duration)
        {
            t += Time.deltaTime;

            float value = disappearCurve.Evaluate(t / duration);
            Vector3 scale = Vector3.LerpUnclamped(startScale, Vector3.zero, value);

            emojiImg.transform.localScale = scale;
            yield return null;
        }

        emojiImg.transform.localScale = Vector3.zero;
        emojiImg.gameObject.SetActive(false);
    }
}