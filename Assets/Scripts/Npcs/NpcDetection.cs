using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcDetection : MonoBehaviour
{
    [SerializeField] protected GameObject _dialogueBalloon;
    protected bool _isInReach = false;

    private void OnTriggerEnter(Collider other)
    {
        _dialogueBalloon.SetActive(true);
        _isInReach = true;
    }

    private void OnTriggerExit(Collider collision)
    {
        _dialogueBalloon.SetActive(false);
        _isInReach = false;
    }
}
