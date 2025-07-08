using R3;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMain : MonoBehaviour
{
    enum FlyState
    {
        Ground,
        Jump,
        Fly,
        Fall,
    }

    [SerializeField]
    private float _speed;
    [SerializeField]
    private float _minMoveRange;
    [SerializeField]
    private float _maxMoveRange;
    [SerializeField]
    private AnimationCurve _jumpCurve;
    [SerializeField]
    private float _flyTime;
    [SerializeField]
    private float _gravity;
    [SerializeField]
    private Vector3 _headPosition;
    [SerializeField]
    private float _rayMaxDistance;
    [SerializeField]
    private LayerMask _groundLayer;

    private FlyState _flyState;
    private float _timer;
    private PlayerInput _playerInput;
    private Rigidbody _playerRigidbody;
    private Vector3 _basePosition;

    private ReactiveProperty<float> _forwardInput = new();
    public ReadOnlyReactiveProperty<float> ForwardInput => _forwardInput;

    private void Start()
    {
        _flyState = FlyState.Ground;
        _basePosition = transform.position;
        _playerRigidbody = GetComponent<Rigidbody>();

        _playerInput = new();
        _playerInput.Player.Move.performed += OnMove;
        _playerInput.Player.Move.canceled += OnMove;
        _playerInput.Player.Jump.started += OnJump;
        _playerInput.Enable();
    }

    private void Update()
    {
        // �ړ��ʒu�����[������͂ݏo���Ȃ��悤�ɂ���
        var position = transform.position;
        position.x = Mathf.Clamp(position.x, _minMoveRange, _maxMoveRange);
        transform.position = position;

        // �W�����v���L���ȏꍇ
        switch (_flyState)
        {
            case FlyState.Ground:
                break;
            case FlyState.Jump:
                Jumping();
                break;
            case FlyState.Fly:
                Fly();
                break;
            case FlyState.Fall:
                Fall();
                break;
            default:
                break;
        }
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

    private void OnJump(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        UnityEngine.Debug.Log($"{IsGround()}");
        // ���ݒn�ʂɂ�����
        if (_flyState == FlyState.Ground && IsGround())
        {
            _flyState = FlyState.Jump;
        }
    }

    private bool IsGround()
    {
        var position = transform.position;
        position.y += _rayMaxDistance;

        return Physics.SphereCast(
            position,
            transform.localScale.y * 0.5f,
            Vector3.down,
            out var _,
            _rayMaxDistance,
            _groundLayer);
    }

    private void Jumping()
    {
        // �W�����v�̍������p�����[�^�[����擾
        _timer += Time.deltaTime;
        var lastKey = _jumpCurve[_jumpCurve.length - 1];
        var time = _timer / lastKey.time;

        // �����Ԃ���Ȃ����`�F�b�N
        var isPlateau = Physics.Raycast(
            transform.position + _headPosition,
            Vector3.up,
            _rayMaxDistance,
            _groundLayer);
        // �����Ԃ��������A�j���[�V�������Ō�܂œ��B�����ꍇ
        if (isPlateau || 1 < time)
        {
            // �؋��ԂɈڍs���ďI��
            _flyState = FlyState.Fly;
            _timer = 0;
            return;
        }

        // �ʒu�ɑ}��
        var position = transform.position;
        position.y = _jumpCurve.Evaluate(time);
        transform.position = position;
    }

    private void Fly()
    {
        // �^�C�}�[���Z
        _timer += Time.deltaTime;

        // �؋󒆂ɒn�ʂɂ����ꍇ
        if (IsGround())
        {
            _timer = 0;
            _flyState = FlyState.Ground;
            return;
        }

        // �؋󎞊Ԃ𒴂����ꍇ
        if (_flyTime <= _timer)
        {
            _timer = 0;
            _flyState = FlyState.Fall;
        }
    }

    private void Fall()
    {
        _playerRigidbody.linearVelocity += Vector3.down * _gravity * Time.deltaTime;

        // �n�ʂɂ����ꍇ
        if (IsGround())
        {
            _flyState = FlyState.Ground;
        }
    }
}
