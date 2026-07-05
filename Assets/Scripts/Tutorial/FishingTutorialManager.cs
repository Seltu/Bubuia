using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class FishingTutorialManager : DialogueTrigger
{
    [SerializeField] private Animator _tutorialUIAnimator;
    [SerializeField] private Animator _fishingClueAnimator;
    [SerializeField] private TextMeshProUGUI _timerText;
    [SerializeField] private TextMeshProUGUI _failText;
    [SerializeField] private InputActionReference _clickAction;
    [SerializeField] private PointingArrow _poitingArrow;
    [SerializeField] private DialogueSO _victoryDialogue;
    [SerializeField] private QuestSO _fishingTutorialQuest;
    private float _timer;
    private bool _stopped;
    private bool _canRetry;

    protected override void Awake()
    {
        base.Awake();
        EventManager.AddListener<int>("StartTutorialTimer", StartTimer);
        EventManager.AddListener("StopTutorialTimer", StopTimer);
        EventManager.AddListener("QuestsUpdated", CheckTutorialProgress);
        EventManager.AddListener("ShipBreak", OnBoatBreak);
        EventManager.AddListener("Tut", FishingAnimClue);
        EventManager.AddListener("Caught3Fish", StopFishingAnimClue);
        GlobalFlagsManager.SetFlag(_fishingTutorialQuest.questFlag, 1);
    }

    private void CheckTutorialProgress()
    {
        if (!(_fishingTutorialQuest.CurrentProgress >= 4) || _stopped) return;
        _stopped = true;
        _dialogue = _victoryDialogue;
        TriggerDialogue();
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
        EventManager.RemoveListener("QuestsUpdated", CheckTutorialProgress);
        EventManager.RemoveListener("ShipBreak", OnBoatBreak);
        EventManager.RemoveListener("Tut", FishingAnimClue);
        EventManager.RemoveListener("Caught3Fish", StopFishingAnimClue);
    }

    private void StartTimer(int time)
    {
        _timer = time;
        EventManager.TriggerEvent("SetFreezeOnCue", true);
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
            TimeSpan timerValue = TimeSpan.FromSeconds(_timer);
            _timerText.text = timerValue.ToString(@"mm\:ss");
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
        _onEndDialogue.AddListener(() => { _tutorialUIAnimator.SetTrigger("Fail"); });
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

    private void FishingAnimClue()
    {
        _fishingClueAnimator.SetTrigger("AimTut");
        _fishingClueAnimator.SetBool("AimTutBool", true);
    }

    private void StopFishingAnimClue()
    {
        _fishingClueAnimator.ResetTrigger("AimTut");
        _fishingClueAnimator.SetBool("AimTutBool", false);
    }

    private void StartPointingArrow()
    {
        _poitingArrow.gameObject.SetActive(true);
        _poitingArrow.SetLookAtTreasure();
    }
}
