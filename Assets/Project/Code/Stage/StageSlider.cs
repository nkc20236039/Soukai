using UnityEngine;

public class StageSlider : MonoBehaviour
{
    [SerializeField]
    private StageSlideSpeedSetting _slideSpeedSetting;
    [SerializeField]
    private float _stageLength;

    private float _slideSpeed;

    public float StageLength => _stageLength;

    private void Start()
    {
        _slideSpeed = _slideSpeedSetting.DefaultSpeed;
    }

    private void Update()
    {
        var position = transform.position;
        position.z -= _slideSpeed * Time.deltaTime;
        transform.position = position;
    }
}
