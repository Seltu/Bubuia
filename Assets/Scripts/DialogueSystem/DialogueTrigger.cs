using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] protected DialogueSO _dialogue;
    [SerializeField] private UnityEvent _onEndDialogue;
    protected virtual void Awake()
    {
        EventManager.AddListener("EndDialogue", OnEndDialogue);
    }

    protected void TriggerDialogue()
    {
        EventManager.TriggerEvent("LoadDialogue", _dialogue);
    }

    protected virtual void OnDestroy()
    {
        EventManager.RemoveListener("EndDialogue", OnEndDialogue);
    }

    protected void OnEndDialogue()
    {
        _onEndDialogue.Invoke();
    }
}
