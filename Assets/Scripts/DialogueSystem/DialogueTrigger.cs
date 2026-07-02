using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] protected DialogueSO _dialogue;
    [SerializeField] protected UnityEvent _onEndDialogue;
    [SerializeField] protected UnityEvent _onStartDialogue;
    protected bool _focusable = false;

    protected static DialogueTrigger _activeTrigger;

    protected virtual void Awake()
    {
        EventManager.AddListener("EndDialogue", OnEndDialogue);
    }

    protected void TriggerDialogue()
    {
        if (_activeTrigger != null) return;
        _activeTrigger = this;
        _onStartDialogue.Invoke();
        EventManager.TriggerEvent("LoadDialogue", _dialogue);
        if(_focusable)
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
