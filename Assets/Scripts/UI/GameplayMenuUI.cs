using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class GameplayMenuUI : MonoBehaviour
{
    [SerializeField] private BoolVariable _onMenu;
    [SerializeField] private BoolVariable _canPause;
    [SerializeField] private Animator _menuAnimator;
    [SerializeField] private UnityEvent _onPause;
    [SerializeField] private UnityEvent _onUnpause;
    [SerializeField] private InputActionReference _pauseAction;

    private void Start()
    {
        _onMenu.value = false;
        _canPause.value = true;
        _pauseAction.action.performed += Pause;
    }

    public void Pause(InputAction.CallbackContext ctx)
    {
        if (!_canPause.value) return;
        if(ctx.started) return;
        if (!(Time.timeScale != 0))
        {
            Unpause();
            return;
        }
        _onPause.Invoke();
        Time.timeScale = 0f;
    }

    public void Unpause()
    {
        _onUnpause.Invoke();
        Time.timeScale = 1f;
    }
}
