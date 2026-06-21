using UnityEngine;

[RequireComponent(typeof(Rigidbody) ,  typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
   private Rigidbody _rb;

   [SerializeField] private float moveSpeed;
   [SerializeField] private float acceleration;
   [SerializeField] private float deceleration;
   [SerializeField] private float rotationSpeed;
   
   private Vector2 _moveInputs;
   private Vector3 _moveDirection;

   private bool _isMoving;
   
   
   private void Awake()
   {
      _rb = GetComponent<Rigidbody>();
   }

   private void Update()
   {
      _moveInputs.x = InputManager.Instance.GetX();
      _moveInputs.y = InputManager.Instance.GetY();

      _moveDirection = new Vector3(_moveInputs.x, 0, _moveInputs.y).normalized;
      _isMoving = _moveDirection.sqrMagnitude > 0; 
   }

   private void FixedUpdate()
   {
      ApplyMovement();
      ApplyRotation();
   }

   private void ApplyMovement()
   {
      Vector3 target = _moveDirection * moveSpeed;
      float rate = _isMoving ? acceleration : deceleration;

      _rb.linearVelocity = Vector3.MoveTowards(_rb.linearVelocity, target, rate * Time.deltaTime);
   }
   
   private void ApplyRotation()
   {
      if (!_isMoving) return;
      
      Quaternion rotation = Quaternion.LookRotation(_moveDirection, Vector3.up);
      Quaternion currentRotation = Quaternion.RotateTowards(_rb.rotation, rotation, rotationSpeed * Time.deltaTime);
      
      _rb.MoveRotation(currentRotation);
   }
}
