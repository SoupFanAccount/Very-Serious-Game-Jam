using UnityEngine;

public class Door : MonoBehaviour
{
   [SerializeField] private Transform moveToTransform;
   [SerializeField] private Camera enableCamera;

   public Transform MoveToTransform() => moveToTransform;

   public Camera CameraToEnable() => enableCamera;
}
