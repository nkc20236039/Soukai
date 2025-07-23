using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

public class GameOver : MonoBehaviour
{
    [SerializeField]
    private PlayerMain _player;
    [SerializeField]
    private GameObject _gameOverText;
    [SerializeField]
    private GameObject _replayButton;
    [SerializeField]
    private float _viewSpeed;
    [SerializeField]
    private float _buttonSpeed;
    [SerializeField]
    private AudioClip _gameOverAudio;

    private void Start()
    {
        _player.OnDead += DeadEvent;
    }

    private void DeadEvent()
    {
        AudioPool.Instance.Play(_gameOverAudio, transform.position);
        _gameOverText.SetActive(true);
        _replayButton.transform.localScale = Vector3.zero;
        _replayButton.SetActive(true);

        LMotion.Create(_gameOverText.transform.localPosition, Vector3.zero, _viewSpeed)
            .WithEase(Ease.OutBounce)
            .BindToLocalPosition(_gameOverText.transform)
            .AddTo(this);

        LMotion.Create(Vector3.zero, Vector3.one, _buttonSpeed)
            .WithEase(Ease.OutQuart)
            .WithDelay(_viewSpeed)
            .BindToLocalScale(_replayButton.transform)
            .AddTo(this);
    }
}
