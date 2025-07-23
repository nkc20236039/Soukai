using UnityEngine;

public class Heart : ItemBase
{
    [SerializeField]
    private int _healCount;
    [SerializeField]
    private AudioClip _pickupAudio;

    protected override void PickupEvent(IStatus status)
    {
        AudioPool.Instance.Play(_pickupAudio, transform.position);
        status.Heal(_healCount);
    }
}