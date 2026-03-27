using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class StartDialogueTrigger : DialogueTrigger
{
    [SerializeField] float _timeDelay;
    private void Start()
    {
        StartCoroutine(WaitThenTrigger(_timeDelay));
    }

    private IEnumerator WaitThenTrigger(float delay)
    {
        yield return new WaitForSeconds(delay);
        TriggerDialogue();
    }
}
