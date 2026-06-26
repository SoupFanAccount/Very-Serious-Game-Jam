using UnityEngine;

public class CanvasBillboard : MonoBehaviour
{
   private Canvas _canvas;
   private Camera _mainCam;
   
   private void Start()
   {
      _mainCam = Camera.main;
      _canvas = GetComponent<Canvas>();
   }

   private void LateUpdate()
   {
      Vector3 direction = (_canvas.transform.position - _mainCam.transform.position).normalized;
      
      Quaternion rotation = Quaternion.LookRotation(direction);
      transform.rotation = rotation;
   }
}
