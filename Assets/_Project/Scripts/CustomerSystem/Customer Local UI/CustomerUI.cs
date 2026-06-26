using System;
using System.Collections;
using UnityEngine;

public class CustomerUI : MonoBehaviour
{
    [SerializeField] private PatienceClock patienceClock;
    [SerializeField] private CustomerDialogue dialogue;
    [SerializeField] private Emoji emoji;
    
    private Coroutine _currentSequence;

    public void UpdateClock(float value)
    {
        patienceClock.UpdateClockFillAmount(value);
    }

    public void ShowPatienceClock(float popDuration)
    {
        PlaySequence(PatienceClockSequence(popDuration));
    }

    public void HidePatienceClock(float duration)
    {
        PlaySequence(HideClockSequence(duration));
    }

    public void ShowDialogue(string text, float popDuration, float stayTime, float closeDuration, Action onComplete = null)
    {
        PlaySequence(DialogueSequence(text, popDuration, stayTime, closeDuration, onComplete));
    }

    public void PlayAngrySequence(string text, float duration, Action onComplete = null)
    {
        PlaySequence(AngrySequence(text, duration, onComplete));
    }

    public void ShowEmoji(Emoji.EmojiType emojiType)
    {
        emoji.ShowEmoji(emojiType);
    }
    
    private void PlaySequence(IEnumerator sequence)
    {
        if (_currentSequence != null)
            StopCoroutine(_currentSequence);

        _currentSequence = StartCoroutine(sequence);
    }

    private IEnumerator PatienceClockSequence(float duration)
    {
        yield return dialogue.CloseDialogue(duration * 0.5f);
        yield return patienceClock.ShowClock(duration);
    }

    private IEnumerator HideClockSequence(float duration)
    {
        yield return patienceClock.HideClock(duration);
    }

    private IEnumerator DialogueSequence(string text, float popDuration, float stayTime, float closeDuration, Action onComplete)
    {
        yield return patienceClock.HideClock(0.2f);

        yield return dialogue.ShowDialogue(text, popDuration);

        yield return new WaitForSeconds(stayTime);

        yield return dialogue.CloseDialogue(closeDuration);

        onComplete?.Invoke();
    }

    private IEnumerator AngrySequence(string text, float duration, Action onComplete)
    {
        yield return patienceClock.HideClock(0.2f);

        yield return dialogue.ShowDialogue(text, duration);

        yield return new WaitForSeconds(.5f);

        yield return dialogue.CloseDialogue(0.5f);

        onComplete?.Invoke();
    }
}