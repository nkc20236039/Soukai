using LitMotion;
using R3;
using TMPro;
using UnityEngine;

public class CoinView : MonoBehaviour
{
    [SerializeField]
    private PlayerMain _player;
    [SerializeField]
    private TMP_Text _coinText;
    [SerializeField]
    private int _maxCoinCount = 999;
    [SerializeField]
    private float _coinCountUpSpeed;

    private int _viewCoin;
    private MotionHandle _coinMotionHandle;

    private void Start()
    {
        _player.Coin.Subscribe(View);
    }

    private void View(int coin)
    {
        if (_coinMotionHandle.IsPlaying())
        {
            _coinMotionHandle.Cancel();
        }

        _coinMotionHandle = LMotion.Create(_viewCoin, coin, _coinCountUpSpeed)
            .Bind(x =>
            {
                _viewCoin = Mathf.Min(x, _maxCoinCount);
                _coinText.text = _viewCoin.ToString("000");
            });
    }
}