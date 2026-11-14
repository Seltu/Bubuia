using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class SceneryLightsController : MonoBehaviour
{
    [SerializeField] private Light _lamp;
    [SerializeField] private MeshRenderer _renderer;
    [SerializeField] private Material _litMaterial;
    [SerializeField] private Material _unlitMaterial;

    // This assumes the lights are only turned on during the night
    [SerializeField] private int _turnOnTime = 18;
    [SerializeField] private int _turnOffTime = 6;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        EventManager.AddListener<int, int>("TimePassed", ReactToTime);
    }

    private void OnDestroy()
    {
        EventManager.RemoveListener<int, int>("TimePassed", ReactToTime);
    }

    private void ReactToTime(int Hours, int Minutes)
    {
        var materialList = _renderer.materials.ToList();
        if ((Hours >= _turnOnTime && Hours <= 24) || (Hours >= 0 && Hours <= _turnOffTime))
        {
            materialList[1] = _litMaterial;
            _lamp.enabled = true;
        }
        else
        {
            materialList[1] = _unlitMaterial;
            _lamp.enabled = false;
        }
        _renderer.SetMaterials(materialList);
    }
}
