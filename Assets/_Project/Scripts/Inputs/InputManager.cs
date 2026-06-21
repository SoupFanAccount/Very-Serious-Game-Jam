using UnityEngine;

[DefaultExecutionOrder(-10)]
public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    private InputSystem_Actions _inputSystem;

    private Vector2 _moveVector;
    
    private void Awake()
    {
        Instance = this;

        _inputSystem = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        _inputSystem.Enable();
    }

    private void OnDisable()
    {
        _inputSystem.Disable();
    }

    private void Start()
    {
        _inputSystem.Player.Move.performed += ctx => _moveVector = ctx.ReadValue<Vector2>();
        _inputSystem.Player.Move.canceled += ctx => _moveVector = Vector2.zero;
    }
    
    public float GetX()
    {
        return _moveVector.x;
    }

    public float GetY()
    {
        return _moveVector.y;
    }
}
