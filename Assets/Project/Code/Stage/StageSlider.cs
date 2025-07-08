using Cysharp.Threading.Tasks;
using LitMotion;
using R3;
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
    private StageSlideSpeedSetting _slideSetting;

    private float _slideSpeed = 20.0f;
    private float _previousStageLength;
    private float _timer;
    private Stage _previousStage;

    private MotionHandle _slideSpeedMotionHandle;

    public float SlideSpeed => _slideSpeed;

    private void Start()
    {
        // �v���C���[�̑O�i���͂��w�ǂ����x�ɓK�p
        _player.ForwardInput.Subscribe(input =>
        {
            // �V�������͂������猻�݂̃��[�V�������L�����Z��
            if (_slideSpeedMotionHandle.IsPlaying())
            {
                _slideSpeedMotionHandle.Cancel();
            }

            if (input == 0)
            {
                _slideSpeedMotionHandle = LMotion.Create(
                    _slideSpeed,
                    _slideSetting.DefaultSpeed,
                    _slideSetting.changeTime)
                    .Bind(x => _slideSpeed = x)
                    .AddTo(this);
            }
            else
            {
                _slideSpeedMotionHandle = LMotion.Create(
                    _slideSpeed,
                    _slideSetting.DefaultSpeed + _slideSetting.changeAmount * input,
                    _slideSetting.changeTime)
                    .Bind(x => _slideSpeed = x)
                    .AddTo(this);
            }
        });

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
        var index = Random.Range(0, _stageObjects.Length);

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
        // ���x��0�ɂ���
        _slideSpeed = 0.0f;
    }
}