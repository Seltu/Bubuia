using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class SceneryLightsController : MonoBehaviour
{
    [SerializeField] public Light lamp;
    // This assumes the lights are only turned on during the night
    [SerializeField] int turnOnTime = 18;
    [SerializeField] int turnOffTime = 6;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        EventManager.AddListener<int, int>("TimePassed", ReactToTime);
    }

    private void OnDestroy()
    {
        EventManager.RemoveListener<int, int>("TimePassed", ReactToTime);
    }

    void ReactToTime(int Hours, int Minutes)
    {
        if ((Hours >= turnOnTime && Hours <= 24) || (Hours >= 0 && Hours <= turnOffTime))
            lamp.enabled = true;
        else
            lamp.enabled = false;
    }
}
