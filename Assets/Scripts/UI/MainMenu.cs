using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // References
    [SerializeField] private GameObject _mainMenuPanel;
    [SerializeField] private GameObject _settingsPanel;
    [SerializeField] private GameObject _creditsPanel;
    [SerializeField] private GameObject _exitPopUpPanel;
    [SerializeField] private GameObject _customizationPanel;
    [SerializeField] private GameObject _fadePanel;

    private GameObject _currentScreen;

    // Variables
    [SerializeField] private string _gameSceneName;


    private void Start()
    {
        _currentScreen = _mainMenuPanel;
    }

    private void Update()
    {
        /*
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            BackButton();
        }
        */
    }

    public void PlayButtonSound()
    {

    }

    public void PlayButton()
    {
        _mainMenuPanel.SetActive(false);
        _customizationPanel.SetActive(true);
        _currentScreen = _customizationPanel;
    }

    public void EnterGame()
    {
        StartCoroutine(EnterGameDelay());
    }

    public void LoadSettingsButton()
    {
        _mainMenuPanel.SetActive(false);
        _settingsPanel.SetActive(true);

        _currentScreen = _settingsPanel;
    }

    public void LoadCreditsButton()
    {
        _settingsPanel.SetActive(false);
        _creditsPanel.SetActive(true);

        _currentScreen = _creditsPanel;
    }

    public void ExitGameButton()
    {
        _exitPopUpPanel.SetActive(true);
        _currentScreen = _exitPopUpPanel;
    }

    public void ConfirmExit()
    {
        Application.Quit();
    }

    public void BackButton()
    {
        _currentScreen.SetActive(false);

        if(_currentScreen == _customizationPanel || _currentScreen == _settingsPanel || _currentScreen == _exitPopUpPanel)
        {
            _currentScreen = _mainMenuPanel;
        }
        else if (_currentScreen == _creditsPanel)
        {
            _currentScreen = _settingsPanel;
        }

        _currentScreen.SetActive(true);
    }

    private IEnumerator EnterGameDelay()
    {
        _fadePanel.SetActive(true);

        yield return new WaitForSeconds(0.7f);

        SceneManager.LoadScene(_gameSceneName);
    }

    public void OpenURL(string url)
    {
        Application.OpenURL(url);
    }

    public void DeleteSave()
    {
        GlobalFlagsManager.DeleteSave();
        InputLock.movementLocked = false;
        InputLock.clickLocked = false;
    }
}
