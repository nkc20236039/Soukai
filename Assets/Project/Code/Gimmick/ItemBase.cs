using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using System.Threading;
using UnityEngine;

public abstract class ItemBase : MonoBehaviour, IGimmick
{
    [SerializeField]
    private float _defaultRotationSpeed;
    [SerializeField]
    private float _pickUppedRotationSpeed;
    [SerializeField]
    private Vector3 _riseAltitude;
    [SerializeField]
    private float _pickUppedAnimationSpeed;

    private float _rotationSpeed;
    private Collider _coinColider;
    private CancellationTokenSource _cancelToken;

    private void Start()
    {
        _rotationSpeed = _defaultRotationSpeed;
        _coinColider = GetComponent<Collider>();
    }

    private void Update()
    {
        var rotation = transform.rotation;
        var rotate = rotation.eulerAngles.y + _rotationSpeed * Time.deltaTime;
        transform.rotation = Quaternion.Euler(0, rotate, 0);
    }

    public async void Execute(IStatus status)
    {
        PickupEvent(status);

        // 当たり判定をなくす
        _coinColider.enabled = false;

        // 回転を適用
        _rotationSpeed = _pickUppedRotationSpeed;

        try
        {
            await LMotion.Create(transform.position, transform.position + _riseAltitude, _pickUppedAnimationSpeed)
                .WithEase(Ease.OutCubic)
                .BindToPosition(transform)
                .AddTo(this)
                .ToUniTask();
            // サイズを変更
            await LMotion.Create(Vector3.one, Vector3.zero, _pickUppedAnimationSpeed)
                .WithEase(Ease.OutCubic)
                .BindToLocalScale(transform)
                .AddTo(this)
                .ToUniTask();
        }
        catch { return; }

        Destroy(gameObject);
    }

    protected abstract void PickupEvent(IStatus status);
}