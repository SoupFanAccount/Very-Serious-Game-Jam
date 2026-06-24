using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenFaderRunner : MonoBehaviour
{
   private Canvas _canvas;
   private Image _img;

   private float _currentAlpha;

   private Coroutine _fadeCoroutine;
   
   private void Awake()
   {
      GameObject canvasObj = new GameObject("FaderCanvas");
      _canvas = canvasObj.AddComponent<Canvas>();
      canvasObj.AddComponent<CanvasScaler>();
      canvasObj.AddComponent<GraphicRaycaster>();

      _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
      
      RectTransform canvasRect = _canvas.GetComponent<RectTransform>();
      
      GameObject imageGO = new GameObject("Image");
      _img = imageGO.AddComponent<Image>();

      imageGO.transform.parent = canvasObj.transform;
      imageGO.transform.localScale = new Vector3(canvasObj.transform.localScale.x, canvasObj.transform.localScale.y, 0);

      RectTransform imgRect = imageGO.GetComponent<RectTransform>();
      imgRect.anchoredPosition = Vector2.zero;
      imgRect.sizeDelta = new Vector2(canvasRect.sizeDelta.x, canvasRect.sizeDelta.y);
      
      _img.color = new Color(0, 0, 0, 0);

      _currentAlpha = 0f;

      DontDestroyOnLoad(this);
   }
   
   public void FadeIn(float duration)
   {
      print("fade in!");
      if(_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
      
      _fadeCoroutine = StartCoroutine(FadeInCoroutine(duration));
   }
   
   private IEnumerator FadeInCoroutine(float duration)
   {
      float t = 0;
      
      Time.timeScale = 0f;
      
      float startAlpha = _currentAlpha;
      float targetAlpha = 1f;

      while (t < duration)
      {
         t += Time.unscaledDeltaTime;

         print("fade In - " + t);

         float currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, t / duration);
         _img.color = new Color(0,0,0 , currentAlpha);

         _currentAlpha = currentAlpha;
         
         yield return null;
      }

      _currentAlpha = targetAlpha;
   }

   public void FadeOut(float duration)
   {
      print("fade out");
      StartCoroutine(FadeOutCoroutine(duration));
   }

   private IEnumerator FadeOutCoroutine(float duration)
   {
      float t = 0f;

      float startAlpha = _currentAlpha;
      float targetAlpha = 0f;

      while (t < duration)
      {
         t += Time.unscaledDeltaTime;

         print("fade Out - " + t);

         float v = 1-Mathf.Pow(t / duration, 3);
         
         float currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, 1-v);
         _img.color = new Color(0, 0, 0, currentAlpha);

         _currentAlpha = currentAlpha;
         
         yield return null;
      }

      _currentAlpha = targetAlpha;
      Time.timeScale = 1f;
   }
}
