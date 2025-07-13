using UnityEngine;

public class Stage : MonoBehaviour
{
    [SerializeField]
    [Header("最初からシーンに配置する場合はアタッチ")]
    private StageSlider _slider;

    [SerializeField]
    private float _stageLength;

    private float _destroyPoint;
    private IStageSlideable _stageSlider;

    public float StageLength => _stageLength;

    public void Init(IStageSlideable stageSlider)
    {
        _stageSlider = stageSlider;
    }

    private void Start()
    {
        // 最初から配置されている
        if (_slider != null)
        {
            _stageSlider = _slider;
        }

        _destroyPoint = -(_stageLength + 1);
    }

    private void Update()
    {
        var position = transform.position;
        position.z -= _stageSlider.SlideSpeed * Time.deltaTime;
        transform.position = position;

        // 削除する位置まで来た場合
        if (transform.position.z <= _destroyPoint)
        {
            // オブジェクトを削除
            Destroy(gameObject);
        }
    }
}
