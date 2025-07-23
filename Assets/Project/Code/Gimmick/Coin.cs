using UnityEngine;

public class Coin : ItemBase
{
    [SerializeField]
    private int _count;
    [SerializeField]
    private AudioClip _pickupAudio;
    [SerializeField]
    private float _pitch;

    protected override void PickupEvent(IStatus status)
    {
        status.AddCoin(_count);
        AudioPool.Instance.Play(_pickupAudio, transform.position, _pitch);
    }
}