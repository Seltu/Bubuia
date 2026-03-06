using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayMenuUI : MonoBehaviour
{
    [SerializeField] private BoolVariable _onMenu;
    [SerializeField] private Animator _menuAnimator;

    private void Start()
    {
        _onMenu.value = false;
    }

    private void Update()
    {
        if (_onMenu.value) return;
    }

    public void Pause()
    {
        Time.timeScale = 0f;
    }

    public void Unpause()
    {
        Time.timeScale = 1f;
    }
}
