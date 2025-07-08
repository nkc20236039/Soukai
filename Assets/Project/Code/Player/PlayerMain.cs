using R3;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
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

    private ReactiveProperty<float> _forwardInput = new();
    public ReadOnlyReactiveProperty<float> ForwardInput => _forwardInput;

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
        // �ړ��ʒu�����[������͂ݏo���Ȃ��悤�ɂ���
        var position = transform.position;
        position.x = Mathf.Clamp(position.x, _minMoveRange, _maxMoveRange);
        transform.position = position;
    }

    private void OnMove(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        // ���͂��擾
        var input = context.ReadValue<Vector2>();
        // ���͂ɉ��������E�ړ�
        var velocity = _playerRigidbody.linearVelocity;
        velocity.x = input.x * _speed * Time.deltaTime;
        _playerRigidbody.linearVelocity = velocity;
        // �O��̓��͂�ǂݎ��
        _forwardInput.Value = input.y;
    }
}
