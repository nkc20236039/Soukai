using LitMotion;
using R3;
using System;
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
    private float _coyoteTime;
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
    [SerializeField]
    private AudioClip _jumpAudio;
    [SerializeField]
    private AudioClip _damageAudio;


    private bool _isJumpSoundPlaying;
    private float _timer;
    private float _jumpedTime;
    private float _lastGroundTime;
    private Vector2 _input;
    private FlyState _flyState;
    private MotionHandle _attenuationMotionHandle;
    private PlayerInput _playerInput;
    private Rigidbody _playerRigidbody;
    private ReactiveProperty<int> _coin = new();
    private ReactiveProperty<int> _health = new();
    private ReactiveProperty<float> _speedAttenuation = new(1.0f);
    private ReactiveProperty<float> _forwardInput = new();
    private event Action _onDead;

    public ReadOnlyReactiveProperty<int> Coin => _coin;
    public ReadOnlyReactiveProperty<int> Health => _health;
    public ReadOnlyReactiveProperty<float> ForwardInput => _forwardInput;
    public ReadOnlyReactiveProperty<float> SpeedAttenuation => _speedAttenuation;

    public event Action OnDead
    {
        add => _onDead += value;
        remove => _onDead -= value;
    }

    private void Start()
    {
        _jumpedTime = float.MinValue;
        _lastGroundTime = float.MinValue;
        _health.Value = _maxHealth;
        _flyState = FlyState.Ground;
        _playerRigidbody = GetComponent<Rigidbody>();

        _playerInput = new();
        _playerInput.Player.Move.performed += OnMove;
        _playerInput.Player.Move.canceled += OnMove;
        _playerInput.Player.Jump.started += OnJump;
        _playerInput.Enable();
    }

    private void Update()
    {
        var isGround = IsGround(out _);

        // 入力に応じた左右移動
        var velocity = _playerRigidbody.linearVelocity;
        velocity.x = _input.x * _speed * _speedAttenuation.Value;
        _playerRigidbody.linearVelocity = velocity;
        // 前後の入力を読み取る
        _forwardInput.Value = _input.y;

        // 移動位置をレーンからはみ出さないようにする
        var position = transform.position;
        position.x = Mathf.Clamp(position.x, _minMoveRange, _maxMoveRange);
        transform.position = position;

        // ジャンプの入力許容内で着地した場合
        float jumpedElapsedTime = Time.time - _jumpedTime;
        if (_flyState == FlyState.Ground && jumpedElapsedTime < _coyoteTime)
        {
            // 状態をジャンプにする
            _flyState = FlyState.Jump;
        }

        // 地面に触れている場合
        if (isGround)
        {
            // 最後に地面にいた時間を更新
            _lastGroundTime = Time.time;
        }

        // 地面に触れていない場合
        if (_flyState == FlyState.Ground && !isGround)
        {
            // 落下状態に移動
            _flyState = FlyState.Fall;
        }

        // ジャンプが有効な場合
        switch (_flyState)
        {
            case FlyState.Ground:
                break;
            case FlyState.Jump:
                if (!_isJumpSoundPlaying)
                {
                    AudioPool.Instance.Play(_jumpAudio, transform.position);
                    _isJumpSoundPlaying = true;
                }
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

        if (_flyState != FlyState.Jump)
        {
            _isJumpSoundPlaying = false;
        }
    }

    public void AddCoin(int count)
    {
        _coin.Value += count;
    }

    public void Kill()
    {
        // 死亡判定
        _health.Value = 0;

        _onDead?.Invoke();

        // プレイヤーの入力を切る
        _playerInput.Dispose();
        _playerInput = null;
        _input = Vector2.zero;

        // プレイヤーの動きを止める
        _speedAttenuation.Value = 0.0f;
    }

    public void Damage()
    {
        // 既に体力がなくなっていたら終了
        if (_health.Value <= 0) { return; }

        AudioPool.Instance.Play(_damageAudio, transform.position);

        // 体力を減らす
        _health.Value--;

        // 体力が0になった場合、死亡処理を行う
        if (_health.Value <= 0)
        {
            Kill();
        }
    }

    public void Heal(int healCount)
    {
        var health = Mathf.Clamp(_health.Value + healCount, 0, _maxHealth);
        _health.Value = health;
    }

    public void SetSpeedAttenuation(float attenuation, float freezeTime, float returnTime)
    {
        // 移動速度の制限を0~1に抑える
        _speedAttenuation.Value = Mathf.Clamp01(attenuation);

        // この関数の上書き命令が来た場合、モーションの再生中であればキャンセルする
        if (_attenuationMotionHandle.IsPlaying())
        {
            _attenuationMotionHandle.Cancel();
        }

        // 減衰を元に戻す
        _attenuationMotionHandle = LMotion.Create(_speedAttenuation.Value, 1.0f, returnTime)
            .WithDelay(freezeTime)
            .Bind(x => _speedAttenuation.Value = x)
            .AddTo(this);
    }

    private void OnMove(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        // 入力を取得
        _input = context.ReadValue<Vector2>();
    }

    private void OnJump(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        _jumpedTime = Time.time;
        // 現在地面にいたら
        if (_flyState == FlyState.Ground && IsGround(out _))
        {
            _flyState = FlyState.Jump;
        }

        var groundElapsedTime = Time.time - _lastGroundTime;
        if (_flyState == FlyState.Fall && groundElapsedTime < _coyoteTime)
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
        // ジャンプ予約を最小値で初期化
        _jumpedTime = float.MinValue;

        // ジャンプの高さをパラメーターから取得
        _timer += Time.deltaTime;
        var lastKey = _jumpCurve[_jumpCurve.length - 1];
        var time = _timer / lastKey.time;

        // 頭がぶつからないかチェック
        var isPlateau = Physics.Raycast(
            transform.position + _headPosition,
            Vector3.up,
            _rayMaxDistance,
            _groundLayer);
        // 頭がぶつかったかアニメーションが最後まで到達した場合
        if (isPlateau || 1 < time)
        {
            // 滞空状態に移行して終了
            _flyState = FlyState.Fly;
            _timer = 0;
            return;
        }

        // 位置に挿入
        var position = transform.position;
        position.y = _jumpCurve.Evaluate(time);
        transform.position = position;
    }

    private void Fly()
    {
        // タイマー加算
        _timer += Time.deltaTime;

        // 滞空中に地面についた場合
        if (IsGround(out _))
        {
            _timer = 0;
            _flyState = FlyState.Ground;
            return;
        }

        // 滞空時間を超えた場合
        if (_flyTime <= _timer)
        {
            _timer = 0;
            _flyState = FlyState.Fall;
        }
    }

    private void Fall()
    {
        _playerRigidbody.linearVelocity += Vector3.down * _gravity * Time.deltaTime;

        // 地面についた場合
        if (IsGround(out var hitInfo))
        {
            // 速度と位置を初期化
            var velocity = _playerRigidbody.linearVelocity;
            velocity.y = 0.0f;
            _playerRigidbody.linearVelocity = velocity;
            var position = _playerRigidbody.position;
            position.y = hitInfo.point.y + _groundOffset;
            _playerRigidbody.position = position;
            // 地面判定に戻す
            _flyState = FlyState.Ground;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 衝突対象がギミックである場合
        if (other.TryGetComponent<IGimmick>(out var gimmick))
        {
            // ギミックの処理を発動する
            gimmick.Execute(this);
        }
    }
}
