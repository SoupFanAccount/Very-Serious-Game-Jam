using System;
using UnityEngine;
using System.Collections;

public class TransitionSystem : MonoBehaviour
{
    public static TransitionSystem Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }
    
    public void DoTransition(Action<Door> action , Door door , float fadeIn = .3f , float fadeOut = .5f)
    {
        StartCoroutine(TransitionCoroutine(action, door, fadeIn, fadeOut));
    }

    private IEnumerator TransitionCoroutine(Action<Door> action, Door door, float fadeIn = .3f, float fadeOut = .5f)
    {
        ScreenFader.FadeIn(fadeIn);

        yield return new WaitForSecondsRealtime(fadeIn);

        action?.Invoke(door);
        ScreenFader.FadeOut(fadeOut);
    }
}
