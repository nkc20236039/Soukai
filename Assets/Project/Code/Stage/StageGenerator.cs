using Cysharp.Threading.Tasks;
using R3;
using System.Threading;
using UnityEngine;

public class StageGenerator : MonoBehaviour
{
    [SerializeField]
    private StageSlider _startStage;

    [SerializeField]
    private StageSlider[] _stageObjects;

    private StageSlider _previousStage;
    private CancellationTokenSource _createStageTaskSource;
    private ReactiveProperty<float> _slideSpeed;
    public ReadOnlyReactiveProperty<float> SlideSpeed => _slideSpeed;

    private void Start()
    {
        _slideSpeed = new(20);
        _previousStage = _startStage;

        _createStageTaskSource = new();
        StageGenerateCycle();
    }

    private async void StageGenerateCycle()
    {
        // ��~�����ɂ��break�����܂Ŗ������[�v
        while (true)
        {
            var stageLength = CreateNewStage();

            // ���̐������Ԃ܂őҋ@
            var interval = stageLength / _slideSpeed.Value;
            try
            {
                await UniTask.Delay(
                    System.TimeSpan.FromSeconds(interval),
                    cancellationToken: _createStageTaskSource.Token);
            }
            catch { break; }    // �ҋ@���ɃL�����Z�����ꂽ�烋�[�v�𔲂���
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
        _previousStage = stage;

        // ���񐶂��������x�����l�������X�e�[�W�̒�����Ԃ�
        var stageLength = stage.StageLength;
        stageLength -= transform.position.z - createPosition.z;
        return stageLength;
    }

    public void Stop()
    {
        // �������~
        if (_createStageTaskSource != null)
        {
            _createStageTaskSource?.Cancel();
            _createStageTaskSource?.Dispose();
            _createStageTaskSource = null;
        }

        // ���x��0�ɂ���
        _slideSpeed.Value = 0.0f;
    }
}