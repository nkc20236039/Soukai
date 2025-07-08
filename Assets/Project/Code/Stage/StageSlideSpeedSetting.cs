using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/StageSlideSpeedSetting")]
public class StageSlideSpeedSetting : ScriptableObject
{
    public float DefaultSpeed;
    public float changeAmount;
    public float changeTime;
}