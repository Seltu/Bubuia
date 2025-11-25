using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BobberCollision : MonoBehaviour
{
    private Collider2D _col;
    private Rigidbody2D _rb;

    private bool _isOnWater;

    void Start()
    {
        EventManager.AddListener("RecallLine", Recall);

        _col = GetComponent<Collider2D>();
        _rb = GetComponent<Rigidbody2D>();
        _isOnWater = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
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
    