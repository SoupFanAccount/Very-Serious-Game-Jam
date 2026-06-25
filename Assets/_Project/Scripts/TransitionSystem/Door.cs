using UnityEngine;
using Unity.Cinemachine;

public class Door : MonoBehaviour
{
   [SerializeField] private Transform moveToTransform;
   [SerializeField] private CinemachineCamera enableCamera;

   public Transform MoveToTransform() => moveToTransform;

   public CinemachineCamera CameraToEnable() => enableCamera;
}
