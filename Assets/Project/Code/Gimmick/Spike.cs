using UnityEngine;

public class Spike : MonoBehaviour, IGimmick
{
    [SerializeField]
    private float _attenuation;
    [SerializeField]
    private float _freezeTime;
    [SerializeField]
    private float _returnTime;

    public void Execute(IStatus status)
    {
        // �_���[�W��^����
        status.Damage();
        status.SetSpeedAttenuation(_attenuation, _freezeTime, _returnTime);
    }
}