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
        // 停止処理によりbreakされるまで無限ループ
        while (true)
        {
            var stageLength = CreateNewStage();

            // 次の生成時間まで待機
            var interval = stageLength / _slideSpeed.Value;
            try
            {
                await UniTask.Delay(
                    System.TimeSpan.FromSeconds(interval),
                    cancellationToken: _createStageTaskSource.Token);
            }
            catch { break; }    // 待機中にキャンセルされたらループを抜ける
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
        _previousStage = stage;

        // 今回生じた生成遅延を考慮したステージの長さを返す
        var stageLength = stage.StageLength;
        stageLength -= transform.position.z - createPosition.z;
        return stageLength;
    }

    public void Stop()
    {
        // 生成を停止
        if (_createStageTaskSource != null)
        {
            _createStageTaskSource?.Cancel();
            _createStageTaskSource?.Dispose();
            _createStageTaskSource = null;
        }

        // 速度を0にする
        _slideSpeed.Value = 0.0f;
    }
}