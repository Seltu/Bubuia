using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] protected DialogueSO _dialogue;
    [SerializeField] private UnityEvent _onEndDialogue;

    private static DialogueTrigger _activeTrigger;

    protected virtual void Awake()
    {
        EventManager.AddListener("EndDialogue", OnEndDialogue);
    }

    protected void TriggerDialogue()
    {
        _activeTrigger = this;
        EventManager.TriggerEvent("LoadDialogue", _dialogue);
        EventManager.TriggerEvent("CameraFocusOnTarget", true, transform);
    }

    protected virtual void OnDestroy()
    {
        EventManager.RemoveListener("EndDialogue", OnEndDialogue);
        if (_activeTrigger == this)
            _activeTrigger = null;
    }

    protected void OnEndDialogue()
    {
        if (_activeTrigger != this)
            return;

        EventManager.TriggerEvent("CameraFocusOnTarget", false, transform);
        _onEndDialogue.Invoke();
        _activeTrigger = null;
    }
}
