using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PatienceClock : MonoBehaviour
{
   [SerializeField] private GameObject clock;
   [SerializeField] private Image clockImg;

   [SerializeField] private AnimationCurve appearCurve;
   [SerializeField] private AnimationCurve disappearCurve;
   
   [SerializeField] private Vector3 maxScale;
   
   // BG BLINK COLORS
   private Color _defaultColor;
   private Color _blinkColor;

   public bool _isClockEnable;

   private void Start()
   {
      _defaultColor = clockImg.color;
      _blinkColor = Color.red;
      
      clock.gameObject.SetActive(false);
      clock.transform.localScale = Vector3.zero;
   }
   
   public IEnumerator ShowClock(float duration)
   {
      if (_isClockEnable) yield break;
      
      _isClockEnable = true;
      clock.gameObject.SetActive(true);
      StartCoroutine(BlinkBG());
      StartCoroutine(EnableClockCoroutine(duration));
   }

   public IEnumerator HideClock(float duration)
   {
      if (_isClockEnable == false) yield break;
      StartCoroutine(DisableClock(duration));
   }
   
   public void UpdateClockFillAmount(float value)
   {
      if (_isClockEnable == false) return;
      clockImg.fillAmount = value;
   }

   private IEnumerator BlinkBG()
   {
      bool blink = false;
      
      while (true)
      {
         clockImg.color = blink? _blinkColor: _defaultColor;
         blink = !blink;
         
         yield return new WaitForSeconds(Random.Range(0.1f , .3f));
      }
   }
   
   private IEnumerator EnableClockCoroutine(float duration)
   {
      float t = 0f;

      Vector3 startScale = Vector3.zero;
      
      while (t < duration)
      {
         t += Time.deltaTime;

         float value = appearCurve.Evaluate(t / duration);
         Vector3 scale = Vector3.LerpUnclamped(startScale, maxScale, value);
         
         clock.transform.localScale = scale;
         
         yield return null;
      }

      clock.transform.localScale = maxScale;
   }

   private IEnumerator DisableClock(float duration)
   {
      float t = 0f;

      Vector3 startScale = transform.localScale;
      Vector3 endScale = Vector3.zero;
      
      while (t < duration)
      {
         t += Time.deltaTime;

         float value = disappearCurve.Evaluate(t / duration);
         Vector3 scale = Vector3.LerpUnclamped(startScale, endScale, value);

         transform.localScale = scale;
         
         yield return null;
      }

      _isClockEnable = false;
      
      transform.localScale = Vector3.zero;
      clock.gameObject.SetActive(false);
   }
}
