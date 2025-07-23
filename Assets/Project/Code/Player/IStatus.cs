public interface IStatus
{
    public void AddCoin(int count);
    public void Damage();
    public void Heal(int healCount);
    public void Kill();
    public void SetSpeedAttenuation(float attenuation, float freezeTime, float returnTime);
}