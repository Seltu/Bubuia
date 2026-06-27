using System.Collections;
using UnityEngine;

public class EnterDialogueTrigger : DialogueTrigger
{
    [SerializeField] float _timeDelay;
    private bool _entered;
    private void OnTriggerEnter(Collider other)
    {
        if (_entered) return;
        if (other.CompareTag("Player"))
        {
            _entered = true;
            StartCoroutine(WaitThenTrigger(_timeDelay));
        }
    }

    private IEnumerator WaitThenTrigger(float delay)
    {
        yield return new WaitForSeconds(delay);
        TriggerDialogue();
    }
}
