using UnityEngine;

public class Stage : MonoBehaviour
{
    [SerializeField]
    [Header("�ŏ�����V�[���ɔz�u����ꍇ�̓A�^�b�`")]
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
        // �ŏ�����z�u����Ă���
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

        // �폜����ʒu�܂ŗ����ꍇ
        if (transform.position.z <= _destroyPoint)
        {
            // �I�u�W�F�N�g���폜
            Destroy(gameObject);
        }
    }
}
