using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class NavigationTutorialManager : DialogueTrigger
{
    [SerializeField] private Animator _tutorialUIAnimator;
    [SerializeField] private TextMeshProUGUI _timerText;
    [SerializeField] private TextMeshProUGUI _failText;
    [SerializeField] private InputActionReference _clickAction;
    private float _timer;
    private bool _stopped;
    private bool _canRetry;

    protected override void Awake()
    {
        base.Awake();
        EventManager.AddListener<int>("StartTutorialTimer", StartTimer);
        EventManager.AddListener("StopTutorialTimer", StopTimer);
        EventManager.AddListener("ShipBreak", OnBoatBreak);
        _onEndDialogue.AddListener(UnlockRetry);
    }

    public void StopTimer()
    {
        _stopped = true;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        EventManager.RemoveListener<int>("StartTutorialTimer", StartTimer);
        EventManager.RemoveListener("StopTutorialTimer", StopTimer);
        EventManager.RemoveListener("ShipBreak", OnBoatBreak);
        _onEndDialogue.RemoveListener(UnlockRetry);
    }

    private void StartTimer(int time)
    {
        _timer = time;
    }

    private void Update()
    {
        if (_stopped)
        {
            if (_clickAction.action.WasPressedThisFrame())
                Retry();
        }
    }

    private void FixedUpdate()
    {
        if (_stopped) return;
        if (_timer > 0)
        {
            _timer -= Time.deltaTime;
            _timerText.text = _timer.ToString("F2");
            if (_timer <= 0)
                FailTutorial("Acabou o tempo!");
        }
    }

    private void OnBoatBreak()
    {
        FailTutorial("Barco quebrou!");
    }

    private void FailTutorial(string failText)
    {
        if(_stopped) return;
        _tutorialUIAnimator.SetTrigger("Fail");
        _failText.text = failText;
        _stopped = true;
        _timerText.text = "00,00";
        _activeTrigger = this;
        EventManager.TriggerEvent("LoadDialogue", _dialogue);
    }

    public void UnlockRetry()
    {
        Time.timeScale = 0f;
        _canRetry = true;
    }

    public void Retry()
    {
        if(_canRetry)
            EventManager.TriggerEvent("ChangeScene", SceneManager.GetActiveScene().name);
    }
}
