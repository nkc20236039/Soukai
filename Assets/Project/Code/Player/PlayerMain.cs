using LitMotion;
using R3;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMain : MonoBehaviour, IStatus
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
    private float _groundOffset;
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
    [SerializeField]
    private int _maxHealth;

    private float _timer;
    private float _currentGravity;
    private Vector2 _input;
    private FlyState _flyState;
    private Vector3 _basePosition;
    private MotionHandle _attenuationMotionHandle;
    private PlayerInput _playerInput;
    private Rigidbody _playerRigidbody;
    private ReactiveProperty<int> _coin = new();
    private ReactiveProperty<int> _health = new();
    private ReactiveProperty<float> _speedAttenuation = new(1.0f);
    private ReactiveProperty<float> _forwardInput = new();

    public ReadOnlyReactiveProperty<int> Coin => _coin;
    public ReadOnlyReactiveProperty<int> Health => _health;
    public ReadOnlyReactiveProperty<float> ForwardInput => _forwardInput;
    public ReadOnlyReactiveProperty<float> SpeedAttenuation => _speedAttenuation;

    private void Start()
    {
        _health.Value = _maxHealth;
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
        // ���͂ɉ��������E�ړ�
        var velocity = _playerRigidbody.linearVelocity;
        velocity.x = _input.x * _speed * _speedAttenuation.Value;
        _playerRigidbody.linearVelocity = velocity;
        // �O��̓��͂�ǂݎ��
        _forwardInput.Value = _input.y;

        // �ړ��ʒu�����[������͂ݏo���Ȃ��悤�ɂ���
        var position = transform.position;
        position.x = Mathf.Clamp(position.x, _minMoveRange, _maxMoveRange);
        transform.position = position;

        // �n�ʂɐG��Ă��Ȃ��ꍇ
        if (_flyState == FlyState.Ground && !IsGround(out _))
        {
            // ������ԂɈړ�
            _flyState = FlyState.Fall;
        }

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

    public void AddCoin(int count)
    {
        _coin.Value += count;
    }

    public void Kill()
    {
        _health.Value = 0;
    }

    public void Damage()
    {
        _health.Value--;
    }

    public void SetSpeedAttenuation(float attenuation, float freezeTime, float returnTime)
    {
        // �ړ����x�̐�����0~1�ɗ}����
        _speedAttenuation.Value = Mathf.Clamp01(attenuation);

        // ���̊֐��̏㏑�����߂������ꍇ�A���[�V�����̍Đ����ł���΃L�����Z������
        if (_attenuationMotionHandle.IsPlaying())
        {
            _attenuationMotionHandle.Cancel();
        }

        // ���������ɖ߂�
        _attenuationMotionHandle = LMotion.Create(_speedAttenuation.Value, 1.0f, returnTime)
            .WithDelay(freezeTime)
            .Bind(x => _speedAttenuation.Value = x)
            .AddTo(this);
    }

    private void OnMove(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        // ���͂��擾
        _input = context.ReadValue<Vector2>();
    }

    private void OnJump(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        // ���ݒn�ʂɂ�����
        if (_flyState == FlyState.Ground && IsGround(out _))
        {
            _flyState = FlyState.Jump;
        }
    }

    private bool IsGround(out RaycastHit hitInfo)
    {
        var position = transform.position;
        var radius = transform.localScale.x * 0.5f;
        position.y += radius;

        return Physics.SphereCast(
            position,
            radius,
            Vector3.down,
            out hitInfo,
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
        if (IsGround(out _))
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
        if (IsGround(out var hitInfo))
        {
            // ���x�ƈʒu��������
            var velocity = _playerRigidbody.linearVelocity;
            velocity.y = 0.0f;
            _playerRigidbody.linearVelocity = velocity;
            var position = _playerRigidbody.position;
            position.y = hitInfo.point.y + _groundOffset;
            _playerRigidbody.position = position;
            // �n�ʔ���ɖ߂�
            _flyState = FlyState.Ground;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // �ՓˑΏۂ��M�~�b�N�ł���ꍇ
        if (other.TryGetComponent<IGimmick>(out var gimmick))
        {
            // �M�~�b�N�̏����𔭓�����
            gimmick.Execute(this);
        }
    }
}
