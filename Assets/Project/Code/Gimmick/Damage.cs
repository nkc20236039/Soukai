using UnityEngine;

public class Damage : MonoBehaviour, IGimmick
{
    [SerializeField]
    private float _attenuation;
    [SerializeField]
    private float _freezeTime;
    [SerializeField]
    private float _returnTime;

    public void Execute(IStatus status)
    {
        // ダメージを与える
        status.Damage();
        status.SetSpeedAttenuation(_attenuation, _freezeTime, _returnTime);
    }
}