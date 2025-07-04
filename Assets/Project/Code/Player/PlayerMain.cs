using UnityEngine;

public class PlayerMain : MonoBehaviour
{
    [SerializeField]
    private float _speed;
    [SerializeField]
    private float _minMoveRange;
    [SerializeField]
    private float _maxMoveRange;

    private PlayerInput _playerInput;
    private Rigidbody _playerRigidbody;
    private Vector3 _basePosition;

    private void Start()
    {
        _basePosition = transform.position;
        _playerRigidbody = GetComponent<Rigidbody>();

        _playerInput = new();
        _playerInput.Player.Move.performed += OnMove;
        _playerInput.Player.Move.canceled += OnMove;
        _playerInput.Enable();
    }

    private void Update()
    {
        var position = transform.position;
        position.x = Mathf.Clamp(position.x, _minMoveRange, _maxMoveRange);
        transform.position = position;
    }

    private void OnMove(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        var input = context.ReadValue<Vector2>();
        var velocity = _playerRigidbody.linearVelocity;
        velocity.x = input.x * _speed * Time.deltaTime;
        _playerRigidbody.linearVelocity = velocity;
    }
}
