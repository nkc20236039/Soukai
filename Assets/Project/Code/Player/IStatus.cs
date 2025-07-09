public interface IStatus
{
    public int Health { get; set; }
    public void SetSpeedAttenuation(float attenuation, float freezeTime, float returnTime);
}