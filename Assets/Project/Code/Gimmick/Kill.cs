using UnityEngine;

public class Kill : MonoBehaviour, IGimmick
{
    public void Execute(IStatus status)
    {
        status.Kill();
    }
}
