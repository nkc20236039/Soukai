using Cysharp.Threading.Tasks;
using LitMotion;
using R3;
using System;
using System.Threading;
using UnityEngine;

public class StageSlider : MonoBehaviour, IStageSlideable
{
    [SerializeField]
    private Stage _startStage;
    [SerializeField]
    private Stage[] _stageObjects;
    [SerializeField]
    private PlayerMain _player;
    [SerializeField]
    private float _stopSpeed;
    [SerializeField]
    private StageSlideSpeedSetting _slideSetting;

    private float _slideSpeed = 20.0f;
    private float _previousStageLength;
    private float _timer;
    private int _coin;
    private Stage _previousStage;

    private MotionHandle _slideSpeedMotionHandle;
    private IDisposable[] _inputDisposable = new IDisposable[2];

    public float SlideSpeed => _slideSpeed;

    private void Start()
    {
        _player.OnDead += Stop;
        _player.Coin.Subscribe(x =>
        {
            _slideSpeed += _slideSetting.LevelupAcceleration * x;
            _coin = x;
        });

        // �v���C���[�̑O�i���͂��w�ǂ����x�ɓK�p
        _inputDisposable[0] = _player.ForwardInput.Subscribe(input =>
            {
                // �V�������͂������猻�݂̃��[�V�������L�����Z��
                if (_slideSpeedMotionHandle.IsPlaying())
                {
                    _slideSpeedMotionHandle.Cancel();
                }

                var speed = _slideSetting.DefaultSpeed + _slideSetting.LevelupAcceleration * _coin;

                var minSpeed = speed - _slideSetting.changeAmount;
                if (_slideSpeed < minSpeed) { return; }

                if (input == 0)
                {
                    _slideSpeedMotionHandle = LMotion.Create(
                        _slideSpeed,
                        speed,
                        _slideSetting.changeTime)
                        .Bind(x => _slideSpeed = x)
                        .AddTo(this);
                }
                else
                {
                    _slideSpeedMotionHandle = LMotion.Create(
                        _slideSpeed,
                        speed + _slideSetting.changeAmount * input,
                        _slideSetting.changeTime)
                        .Bind(x => _slideSpeed = x)
                        .AddTo(this);
                }
            })
            .AddTo(this);

        _inputDisposable[1] = _player.SpeedAttenuation.Subscribe(value =>
            {
                var speed = _slideSetting.DefaultSpeed + _slideSetting.LevelupAcceleration * _coin;

                if (_slideSpeedMotionHandle.IsPlaying())
                {
                    _slideSpeedMotionHandle.Cancel();
                }

                _slideSpeed = speed * value;
            })
            .AddTo(this);

        _previousStage = _startStage;
    }

    // �X�e�[�W���O�ɐi�ޑO�ɍX�V����
    private void LateUpdate()
    {
        _timer += Time.deltaTime;
        var interval = _previousStageLength / _slideSpeed;
        if (interval <= _timer)
        {
            _previousStageLength = CreateNewStage();
            _timer = 0;
        }
    }

    private float CreateNewStage()
    {
        var index = UnityEngine.Random.Range(0, _stageObjects.Length);

        // �O�̃X�e�[�W�Ƀs�b�^���d�Ȃ�悤�ɑO�̃X�e�[�W�̍ŏI���W���擾
        var createPosition = _previousStage.transform.position;
        createPosition.z += _previousStage.StageLength;

        // �V����Prefab�𐶐�
        var stage = Instantiate(_stageObjects[index], createPosition, Quaternion.identity);
        stage.Init(this);
        _previousStage = stage;

        // ���񐶂��������x�����l�������X�e�[�W�̒�����Ԃ�
        var stageLength = stage.StageLength;
        stageLength -= transform.position.z - createPosition.z;
        return stageLength;
    }

    public void Stop()
    {
        for (int i = 0; i < _inputDisposable.Length; i++)
        {
            _inputDisposable[i]?.Dispose();
        }

        if (_slideSpeedMotionHandle.IsPlaying())
        {
            _slideSpeedMotionHandle.Cancel();
        }

        // ���x��0�ɂ���
        LMotion.Create(_slideSpeed, 0.0f, _stopSpeed)
            .Bind(x => _slideSpeed = x)
            .AddTo(this);
    }
}