using UnityEngine;

[RequireComponent(typeof(Rigidbody) ,  typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
   private Rigidbody _rb;
   private Animator _animator;
   
   [SerializeField] private float moveSpeed;
   [SerializeField] private float decelerationTime;
   [SerializeField] private float accelerationTime;
   [SerializeField] private float rotationSpeed;

   [SerializeField] private AnimationCurve decCurve;
   [SerializeField] private AnimationCurve accCurve;
   private float _smoothMotion;
   
   private Vector2 _moveInputs;
   private Vector3 _moveDirection;
   private Vector3 _startVelocity;
   private Vector2 _lastMoveInputs;
   
   private bool _isMoving;
   private bool _canTransition;
   
   [Space(10f), Header("Body Tilt Settings"), Space(5f)] 
   
   [SerializeField] private bool canBodyTilt;
   [SerializeField] private Transform body;
   [SerializeField] private float maxTiltAngle;
  
   private float _currentTilt;

   [Space(10f), Header("Move Particle"), Space(5f)]
   
   [SerializeField] private ParticleSystem moveParticle;
   [SerializeField] private float minParticle;
   [SerializeField] private float maxParticle;
   
   private ParticleSystem.EmissionModule _emission;
   
   
   private void Awake()
   {
      _rb = GetComponent<Rigidbody>();
      _animator = GetComponentInChildren<Animator>();
      _emission = moveParticle.emission;

      if (_animator == null)
      {
         Debug.LogWarning("PlayerController: no Animator found in children. Movement works but animations are skipped.", this);
      }
      else if (_animator.runtimeAnimatorController == null)
      {
         // Calling SetBool/SetFloat on an Animator with no controller spams
         // "Animator is not playing an AnimatorController" every frame. Treat it
         // as "no usable animator" so the existing guards skip animation calls.
         Debug.LogWarning($"PlayerController: Animator on '{_animator.name}' has no AnimatorController assigned. Movement works but animations are skipped.", this);
         _animator = null;
      }

      _canTransition = true;
   }

   private void Update()
   {
      _moveInputs.x = InputManager.Instance.GetX();
      _moveInputs.y = InputManager.Instance.GetY();


      // Added by Donags. To make movement relative to camera yaw so it matches the flipped top-down view
      Vector3 rawDir = new Vector3(_moveInputs.x, 0, _moveInputs.y);
      _moveDirection = (Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0) * rawDir).normalized;
      _isMoving = _moveDirection.sqrMagnitude > 0;

      if (_lastMoveInputs != _moveInputs)
      {
         _startVelocity = _rb.linearVelocity;
         _lastMoveInputs = _moveInputs;
      }

      HandleBodyTilt();
   }

   private void FixedUpdate()
   {
      ApplyMovement();
      ApplyRotation();
   }

   private void ApplyMovement()
   {
      if (_isMoving) _smoothMotion = Mathf.MoveTowards(_smoothMotion, 1f, Time.fixedDeltaTime/accelerationTime);
      else _smoothMotion = Mathf.MoveTowards(_smoothMotion, 0 , Time.fixedDeltaTime / decelerationTime);
      
      Vector3 target = _moveDirection * moveSpeed;

      float curveRate = _isMoving? accCurve.Evaluate(_smoothMotion) : decCurve.Evaluate(1-_smoothMotion);

      Vector3 targetVelocity = Vector3.Lerp(_startVelocity, target,  curveRate);
      _rb.linearVelocity = targetVelocity;

      bool moving = _rb.linearVelocity.sqrMagnitude > 0;

      if (_animator)
      {
         _animator.SetBool("Move" , moving);
         _animator.SetBool("Idle" , !moving);
      }

      if (!moving) return;

      float speed01 = _rb.linearVelocity.magnitude / moveSpeed;
      float stepsDistance = Mathf.Lerp(minParticle, maxParticle, speed01);

      if (_animator) _animator.SetFloat("SpeedUp", speed01 * 2);

      _emission.rateOverDistance = new ParticleSystem.MinMaxCurve(stepsDistance);
   }
   
   private void ApplyRotation()
   {
      if (_moveDirection.sqrMagnitude < .01f) return;

      Quaternion rotation = Quaternion.LookRotation(_moveDirection, Vector3.up);
      Quaternion currentRotation = Quaternion.RotateTowards(_rb.rotation, rotation, rotationSpeed * Time.deltaTime);
      
      _rb.MoveRotation(currentRotation);
   }

   private void HandleBodyTilt()
   {
      if (!canBodyTilt) return;
      
      float speed01 = _rb.linearVelocity.magnitude / moveSpeed;
      float targetTilt = maxTiltAngle * speed01;
      
      _currentTilt = targetTilt;
      body.localRotation = Quaternion.Euler(_currentTilt, 0, 0);
   }

   private void OnTransition(Door door)
   {
      // JUST SO WE DON'T MOVE WHEN WE REACH IN OTHER ROOM
      _smoothMotion = 0f;
      _rb.linearVelocity = Vector3.zero;
      
      // _currentTilt = 0;
      // body.localRotation = Quaternion.Euler(0,0,0);
      
      transform.position = door.MoveToTransform().position;
      moveParticle.gameObject.SetActive(true);
   }
   
   private void OnTriggerEnter(Collider other)
   {
      if (other.TryGetComponent(out Door door) && _canTransition)
      {
         _canTransition = false;
         moveParticle.gameObject.SetActive(false);
         TransitionSystem.Instance.DoTransition(OnTransition, door, .5f ,1f);
      }
   }

   private void OnTriggerExit(Collider other)
   {
      if (other.TryGetComponent(out Door door)) _canTransition = true;
   }
}
