using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    private bool _selected;
    protected virtual void Start()
    {
        EventManager.AddListener<GameObject>("InteractorSelect", CheckSelect);
        EventManager.AddListener<GameObject>("CheckInteract", CheckInteract);
    }

    protected virtual void OnDestroy()
    {
        EventManager.RemoveListener<GameObject>("InteractorSelect", CheckSelect);
        EventManager.RemoveListener<GameObject>("CheckInteract", CheckInteract);
    }
    private void CheckSelect(GameObject obj)
    {
        if (!gameObject.Equals(obj))
        {
            if(_selected)
                OnDeselect();
            _selected = false;
            return;
        }
        if (!_selected)
            OnSelect();
        _selected = true;
    }

    private void CheckInteract(GameObject obj)
    {
        if (!gameObject.Equals(obj)) return;
        Interact();
    }

    protected virtual void OnSelect()
    {
        return;
    }

    protected virtual void OnDeselect()
    {
        return;
    }

    protected virtual void Interact()
    {
        Debug.Log("Interacted with " + name);
    }
}
