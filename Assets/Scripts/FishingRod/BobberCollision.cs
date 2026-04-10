using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BobberCollision : MonoBehaviour
{
    private Collider _col;
    private Rigidbody _rb;

    private bool _isOnWater;

    void Start()
    {
        EventManager.AddListener("RecallLine", Recall);

        _col = GetComponent<Collider>();
        _rb = GetComponent<Rigidbody>();
        _isOnWater = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(!_isOnWater && collision.gameObject.transform.tag == "Water")
        {
            _isOnWater = true;
            EventManager.TriggerEvent("BobberHitWater");
            EventManager.TriggerEvent("CallTutorial", "Tutorial_BobbleMovement");
            EventManager.TriggerEvent("CallTutorial", "Tutorial_HookMovement");
            gameObject.GetComponent<AudioCaller>().CallSFX("WaterSplash");
            gameObject.GetComponent<AudioCaller>().StopLoopingSfx("FishReelingLoop");
        }
    }

    private void Recall()
    {
        _isOnWater = false;
    }
}
    