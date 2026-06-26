using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CustomerDialogue : MonoBehaviour
{
    [SerializeField] private Image bgImg;
    [SerializeField] private TMP_Text dialogueText;

    [SerializeField] private AnimationCurve appearCurve;
    [SerializeField] private AnimationCurve disappearCurve;

    [SerializeField] private Vector3 maxScale;
    
    [Space(5f) , Header("Dialogue Sfx") , Space(5f)]
    
    [SerializeField] private AudioClip[] dialogueSounds;
    private AudioSource _audioSource;

    private Coroutine _speechCoroutine;
    
    private RectTransform _bgImgRectTransform;
    private RectTransform _textRectTransform;
    private bool _isDialogueEnable;
    
    private void Start()
    {
        _bgImgRectTransform = bgImg.GetComponent<RectTransform>();
        _textRectTransform = dialogueText.GetComponent<RectTransform>();
        _audioSource = GetComponentInParent<AudioSource>();
    }
    
    public IEnumerator ShowDialogue(string dialogue , float duration)
    {
        if (_isDialogueEnable) yield break;
        
        _isDialogueEnable = true;
        bgImg.gameObject.SetActive(true);
        
        bgImg.transform.localScale = Vector3.zero;
        yield return null;
        dialogueText.text = dialogue;
        
        _bgImgRectTransform.sizeDelta = new Vector2(dialogueText.preferredWidth + .5f, dialogueText.preferredHeight + 1.5f);
        _bgImgRectTransform.anchoredPosition = new Vector2(0, _bgImgRectTransform.sizeDelta.y / 2);
        
        dialogueText.text = "";
        _textRectTransform.anchoredPosition = new Vector2(0, .2f);
        
        yield return StartCoroutine(PopUpCoroutine(dialogue,duration));
    }

    private IEnumerator PopUpCoroutine(string dialogue, float duration)
    {
        float t = 0f;

        Vector3 startScale = Vector3.zero;
        
        while (t < duration)
        {
            t += Time.deltaTime;

            float value = appearCurve.Evaluate(t / duration);
            Vector3 scale = Vector3.LerpUnclamped(startScale, maxScale, value);

            bgImg.transform.localScale = scale;
            yield return null;
        }

        bgImg.transform.localScale = maxScale;

        yield return WriteDialogueCoroutine(dialogue);
    }

    public IEnumerator CloseDialogue(float duration)
    {
        if (_isDialogueEnable == false) yield break;
        
        float t = 0f;

        Vector3 startScale = bgImg.transform.localScale;

        while (t < duration)
        {
            t += Time.deltaTime;

            float value = disappearCurve.Evaluate(t / duration);
            Vector3 scale = Vector3.LerpUnclamped(startScale, Vector3.zero, value);

            bgImg.transform.localScale = scale;
            yield return null;
        }

        _isDialogueEnable = false;
        dialogueText.text = "";
    
        bgImg.transform.localScale = Vector3.zero;
        bgImg.gameObject.SetActive(false);
    }
    
    private IEnumerator WriteDialogueCoroutine(string dialogue)
    {
        char[] cArray = dialogue.ToCharArray();

        PlaySpeech();
        
        for (int i = 0; i < cArray.Length; i++)
        {
            dialogueText.text += cArray[i];
            yield return new WaitForSeconds(0.05f);
        }

        yield return null;
        StopSpeech();
    }

    private void PlaySpeech()
    {
        if(_speechCoroutine != null) StopCoroutine(_speechCoroutine);
        _speechCoroutine = StartCoroutine(PlaySpeechCoroutine());
    }
    
    private IEnumerator PlaySpeechCoroutine()
    {
        if (dialogueSounds.Length <= 0) yield break;
        
        while (true)
        {
            AudioClip clip = dialogueSounds[Random.Range(0, dialogueSounds.Length)];
            _audioSource.PlayOneShot(clip, 1);
            
            yield return new WaitForSeconds(clip.length + .05f);
        }
    }

    private void StopSpeech()
    {
        if(_speechCoroutine != null) StopCoroutine(_speechCoroutine);

        _speechCoroutine = null;
        _audioSource.Stop();
    }
}
