using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcDetection : MonoBehaviour
{
    [SerializeField] protected GameObject _dialogueBalloon;
    [SerializeField] protected BoolVariable _npcInteractionAllowed;
    protected bool _isInReach = false;

    private void OnTriggerEnter(Collider other)
    {
        if (_npcInteractionAllowed.value) SetBallonsStatus(true);

        _isInReach = true;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!_npcInteractionAllowed.value) SetBallonsStatus(false);
        else _dialogueBalloon.SetActive(true);

        _isInReach = true;
    }

    private void OnTriggerExit(Collider collision)
    {
        SetBallonsStatus(false);
        _isInReach = false;
    }

    private void SetBallonsStatus(bool status)
    {
        _dialogueBalloon.SetActive(status);
    }
}
