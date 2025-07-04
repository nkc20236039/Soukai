using R3;
using UnityEngine;

public class StageGenerator : MonoBehaviour
{
    [SerializeField]
    private StageSlider[] _stagePrefabs;

    private ReactiveProperty<float> _slideSpeed;
    public ReadOnlyReactiveProperty<float> SlideSpeed => _slideSpeed;

    private void Update()
    {

    }
    
    private void CreateNewStage()
    {

    }
}