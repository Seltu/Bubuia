using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject _pauseMenu;
    [SerializeField] private GameObject _inventoryScreen;
    [SerializeField] private GameObject _almanacScreen;
    [SerializeField] private BoolVariable _onMenu;

    private void Start()
    {
        _onMenu.value = false;
    }

    private void Update()
    {
        if (_onMenu.value) return;/*
        if (_pauseMenu.activeInHierarchy || _inventoryScreen.activeInHierarchy || _almanacScreen.activeInHierarchy)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Time.timeScale = 1f;
                _pauseMenu.SetActive(false);
                _inventoryScreen.SetActive(false);
                _almanacScreen.SetActive(false);
            }
            return;
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Time.timeScale = 0f;
            _pauseMenu.SetActive(true);
        }
        else if (Input.GetKeyDown(KeyCode.I))
        {
            Time.timeScale = 0f;
            _inventoryScreen.SetActive(true);
        }
        else if (Input.GetKeyDown(KeyCode.L))
        {
            Time.timeScale = 0f;
            _almanacScreen.SetActive(true);
        }*/
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
