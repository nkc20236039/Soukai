using LitMotion;
using R3;
using UnityEngine;
using UnityEngine.UI;

public class HeartView : MonoBehaviour
{
    [SerializeField]
    private PlayerMain _player;

    [SerializeField]
    private Image[] _heartImage;

    private void Start()
    {
        _player.Health.Subscribe(ChangeHeart);
    }

    private void ChangeHeart(int count)
    {
        var index = count - 1;

        for (int i = 0; i < _heartImage.Length; i++)
        {
            // �\������
            if (i <= index)
            {
                Show(i);
            }
            else // ��\���ɂ���
            {
                Hide(i);
            }
        }
    }

    private void Show(int index)
    {
        // ���ɕ\������Ă����Ԃł���Ή��������I��
        if (_heartImage[index].gameObject.activeSelf) { return; }

        // �\��
        _heartImage[index].gameObject.SetActive(true);
    }

    private void Hide(int index)
    {
        // ���ɔ�\���̏�Ԃł���Ή��������I��
        if (!_heartImage[index].gameObject.activeSelf) { return; }

        // ��\��
        _heartImage[index].gameObject.SetActive(false);
    }
}
