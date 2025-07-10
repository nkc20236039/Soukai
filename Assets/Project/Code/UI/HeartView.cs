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
            // 表示する
            if (i <= index)
            {
                Show(i);
            }
            else // 非表示にする
            {
                Hide(i);
            }
        }
    }

    private void Show(int index)
    {
        // 既に表示されている状態であれば何もせず終了
        if (_heartImage[index].gameObject.activeSelf) { return; }

        // 表示
        _heartImage[index].gameObject.SetActive(true);
    }

    private void Hide(int index)
    {
        // 既に非表示の状態であれば何もせず終了
        if (!_heartImage[index].gameObject.activeSelf) { return; }

        // 非表示
        _heartImage[index].gameObject.SetActive(false);
    }
}
