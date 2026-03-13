using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class GameplayMenuUI : MonoBehaviour
{
    [SerializeField] private BoolVariable _onMenu;
    [SerializeField] private Animator _menuAnimator;
    [SerializeField] private UnityEvent _onPause;
    [SerializeField] private InputActionReference _pauseAction;

    private void Start()
    {
        _onMenu.value = false;
        _pauseAction.action.performed += Pause;
    }

    private void Update()
    {
        if (_onMenu.value) return;
    }

    public void Pause(InputAction.CallbackContext ctx)
    {
        _onPause.Invoke();
        Time.timeScale = 0f;
    }

    public void Unpause()
    {
        Time.timeScale = 1f;
    }
}
