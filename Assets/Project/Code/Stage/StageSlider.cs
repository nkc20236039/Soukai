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
        // プレイヤーの前進入力を購読し速度に適用
        _player.ForwardInput.Subscribe(input =>
        {
            // 新しい入力が来たら現在のモーションをキャンセル
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

    // ステージが前に進む前に更新する
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

        // 前のステージにピッタリ重なるように前のステージの最終座標を取得
        var createPosition = _previousStage.transform.position;
        createPosition.z += _previousStage.StageLength;

        // 新しくPrefabを生成
        var stage = Instantiate(_stageObjects[index], createPosition, Quaternion.identity);
        stage.Init(this);
        _previousStage = stage;

        // 今回生じた生成遅延を考慮したステージの長さを返す
        var stageLength = stage.StageLength;
        stageLength -= transform.position.z - createPosition.z;
        return stageLength;
    }

    public void Stop()
    {
        // 速度を0にする
        _slideSpeed = 0.0f;
    }
}