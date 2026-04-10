using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.SceneManagement;

public class FishingMinigame : MonoBehaviour
{
    [Header("Hazzards")]
    [SerializeField] private bool _hasDoubleRings;
    [SerializeField] private bool _hasWaves;
    [SerializeField] private bool _hasColdBreeze;
    [SerializeField] private GameObject _wavesIcon;
    [SerializeField] private GameObject _coldIcon;

    [Header("References")]
    [SerializeField] private FloatVariable _currentCatchRadius;
    [SerializeField] private Transform _indicatorRing;
    [SerializeField] private FishingRing _ringPrefab;
    [SerializeField] private PlayerInventorySO _playerInventory;
    [SerializeField] private AlmanacSO _almanacSO;
    [SerializeField] private FloatVariable _currentBaitPowerSO;
    [SerializeField] private List<BaitSlotUI> _baitSlots;
    [SerializeField] private GameObject _returnPanel;
    private Queue<FishingRing> _spawnedRings = new Queue<FishingRing>();
    private bool _playing;
    private int _currentBait = 0;
    private Fish _currentFish;
    private int _currentWave;
    private int _currentScore;
    private bool _wavesHazzardActive;
    private bool _breezeHazzardActive;

    //[Header("Tutorial / Cue Freeze")]
    private bool _freezeOnCue;   // toggle for tutorial
    private bool _waitingCueTap = false;
    private float _prevTimeScale = 1f;
    private float _cueInsideMargin = 0.4f;
    private int _cueCount = 3; // ammount of fishes to catch until cue is deactivated, player must catch the ramaingn fish alone

    [SerializeField] private bool _isTutorialScene = false;

    private void Awake()
    {
        EventManager.AddListener<bool>("SetFreezeOnCue", v => _freezeOnCue = v);
    }

    private void Start()
    {
        EventManager.AddListener<Fish>("StartFishingMinigame", StartMinigame);
        EventManager.AddListener("RingMiss", OnRingMiss);

        if(_freezeOnCue) EventManager.AddListener("FishCaught", OnCaughtFish);

        if (_playerInventory.playerBaits[0].baitNum <= 5)
            _playerInventory.playerBaits[0].baitNum = 5;

        for (int i = 0; i < _baitSlots.Count; i++)
        {
            BaitSlotUI slot = _baitSlots[i];
            slot.SetCount(_playerInventory.playerBaits[i].baitNum);
        }
    }

    private void OnDestroy()
    {
        EventManager.RemoveListener<Fish>("StartFishingMinigame", StartMinigame);
        EventManager.RemoveListener("RingMiss", OnRingMiss);
        EventManager.RemoveListener<bool>("SetFreezeOnCue", v => _freezeOnCue = v);
        EventManager.RemoveListener("FishCaught", OnCaughtFish);
    }

    private void Update()
    {
        if (!_playing)  return;
        _indicatorRing.position = _currentFish.transform.position+Vector3.back*0.1f;

        if (!_freezeOnCue) return;
        if (_waitingCueTap) return;

        if (_spawnedRings.Count > 0)
        {
            var ring = _spawnedRings.Peek(); // use Peek instead of First()
            if (IsRingInCueWindow(ring))
            {
                EventManager.TriggerEvent("Tut");
                FreezeForCue();
            }
        }
    }

    public void FishingButtonInput(InputAction.CallbackContext context)
    {
        if (!_playing) return;
        if (context.phase != InputActionPhase.Started || context.interaction is not TapInteraction) return;


        // While cue training is active, only allow taps when the cue is showing (time is frozen).
        if (_freezeOnCue && !_waitingCueTap)
        {
            return;
        }

        // If we froze time for the cue, resume ONLY when player taps
        if (_waitingCueTap)
            UnfreezeFromCue();
        //

        if (_spawnedRings.Count > 0)
        {
            var ring = _spawnedRings.First();
            if (ring.transform.lossyScale.x * 5f < _currentCatchRadius.Value)
            {
                if(_breezeHazzardActive)
                {
                    _spawnedRings.Dequeue();
                    ring.FailRing();
                    LosePoint();
                    return;
                }

                _currentScore++;
                ring.ExplodeRing();
                EventManager.TriggerEvent("ToggleCameraShake", ((float)_currentScore / _currentFish.GetFishTypeSO().fishingGoalScore) * _currentFish.GetSpeed()/2);
                if (ring.hasExploded())
                {
                    _spawnedRings.Dequeue();
                    EventManager.TriggerEvent("ScoreUpdate", _currentScore, _currentFish.GetFishTypeSO().fishingGoalScore);
                    if (_currentScore >= _currentFish.GetFishTypeSO().fishingGoalScore)
                    {
                        EndMinigame(true);
                    }
                }
                return;
            }
            _spawnedRings.Dequeue();
            ring.FailRing();
        }
        LosePoint();
    }

    public void SwitchBait(int bait)
    {
        if (_playing) return;
        if (_playerInventory.playerBaits[bait].baitNum <= 0) return;
        _currentBait = bait;
        foreach (var slot in _baitSlots)
        {
            slot.SetSelected(false);
        }
        _baitSlots[bait].SetSelected(true);
        _currentBaitPowerSO.Value = _playerInventory.playerBaits[_currentBait].baitType.baitPower;
    }

    private void OnRingMiss()
    {
        _spawnedRings.Dequeue();
        if(!_breezeHazzardActive)
            LosePoint();
    }

    private void LosePoint()
    {
        _currentScore--;
        EventManager.TriggerEvent("ScoreUpdate", _currentScore, _currentFish.GetFishTypeSO().fishingGoalScore);
        EventManager.TriggerEvent("ToggleCameraShake", ((float)_currentScore / _currentFish.GetFishTypeSO().fishingGoalScore) * 2f * _currentFish.GetSpeed());
        if (_currentScore <= -3)
        {
            EndMinigame(false);
        }
    }

    private void StartMinigame(Fish fish)
    {
        if(_playing) return;

        if (!_isTutorialScene) _baitSlots[_currentBait].SetCount(_playerInventory.playerBaits[_currentBait].baitNum - 1);
        if(!_isTutorialScene) EventManager.TriggerEvent("OnAddToPlayerBaits", _playerInventory.playerBaits[_currentBait].baitType, -1);
        EventManager.TriggerEvent("ToggleCameraShake", fish.GetSpeed());
        EventManager.TriggerEvent("TurnOffMovement");
        _currentScore = 0;
        _currentWave = 0;
        _currentFish = fish;
        _playing = true;
        _indicatorRing.localScale = Vector3.one * _currentCatchRadius.Value / 5f;
        _indicatorRing.gameObject.SetActive(true);
        StartCoroutine(GameLoop());
    }

    private void EndMinigame(bool won)
    {
        while (_spawnedRings.Count > 0)
        {
            _spawnedRings.Dequeue().FadeRing();
        }
        _indicatorRing.gameObject.SetActive(false);
        _playing = false;
        EventManager.TriggerEvent("EndFishingMinigame", won);
        EventManager.TriggerEvent("TurnOnMovement");
        if (won)
        {
            UnfreezeFromCue();
            EventManager.TriggerEvent("FishCaught");
            AlmanacFishes almanacFish = null;

            if (!_isTutorialScene)
            { 
                foreach (var fish in _almanacSO.almanacFishes)
                {
                    if (fish.fishType == _currentFish.GetFishTypeSO())
                        almanacFish = fish;
                }
                if (almanacFish != null)
                    almanacFish.hasCaught = true;
                var completed = true;
                foreach (var fish in _almanacSO.almanacFishes)
                {
                    if (!fish.hasCaught)
                        completed = false;
                }
                if (completed)
                {
                    StartCoroutine(VictorySequence());
                    return;
                }
            }
        }
        EventManager.TriggerEvent("ToggleCameraShake", 0f);
        if (_playerInventory.playerBaits[_currentBait].baitNum <= 0)
        {
            for (int i = 0; i < _playerInventory.playerBaits.Length; i++)
            {
                PlayerBait bait = _playerInventory.playerBaits[i];
                if (bait.baitNum > 0)
                {
                    SwitchBait(i);
                    return;
                }
            }
            StartCoroutine(ReturnSequence());
        }
    }

    private IEnumerator ReturnSequence()
    {
        EventManager.TriggerEvent("TurnOffControls");
        yield return new WaitForSeconds(3f);
        _returnPanel.SetActive(true);
    }

    private IEnumerator VictorySequence()
    {
        EventManager.TriggerEvent("TurnOffControls");
        AudioSystem.Instance.PlaySFX("FishingVictory");
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("EndingScene");
    }


    private IEnumerator GameLoop()
    {
        while(_playing)
        {
            if (!_wavesHazzardActive && !_breezeHazzardActive)
            {
                _wavesIcon.SetActive(false);
                _coldIcon.SetActive(false);
                if (_hasWaves && UnityEngine.Random.value <= 0.1f)
                {
                    StartCoroutine(WavesHazzardCoroutine());

                }
                else if (_hasColdBreeze && UnityEngine.Random.value <= 0.1f)
                {
                    StartCoroutine(ColdBreezeHazzardCoroutine());
                }
            }

            var ring = Instantiate(_ringPrefab, Vector2.zero, Quaternion.Euler(90f, 0f, 0f));
            ring.transform.localScale = Vector3.one * 2f;
            _spawnedRings.Enqueue(ring);
            ring.SetRing(_currentFish.transform, _currentFish.GetFishTypeSO().fishingPattern[_currentWave].speed / _playerInventory.playerBaits[_currentBait].baitType.baitPower);
            if (_hasDoubleRings)
            {
                if (UnityEngine.Random.value <= 0.1f)
                {
                    ring.SetHealth(2);
                }
            }
            if (_breezeHazzardActive)
            {
                ring.SetFrozen(true);
            }
            if(!_wavesHazzardActive)
                yield return new WaitForSeconds(_currentFish.GetFishTypeSO().fishingPattern[_currentWave].secondsDelay * 
                _playerInventory.playerBaits[_currentBait].baitType.baitPower);
            else
                yield return new WaitForSeconds((_currentFish.GetFishTypeSO().fishingPattern[_currentWave].secondsDelay *
                _playerInventory.playerBaits[_currentBait].baitType.baitPower)/2f);
            _currentWave++;
            if (_currentWave >= _currentFish.GetFishTypeSO().fishingPattern.Count)
                _currentWave = 0;
        }
    }

    private IEnumerator WavesHazzardCoroutine()
    {
        _wavesHazzardActive = true;
        _wavesIcon.SetActive(true);
        yield return new WaitForSeconds(2f);
        _wavesHazzardActive = false;
        _wavesIcon.SetActive(false);
    }

    private IEnumerator ColdBreezeHazzardCoroutine()
    {
        _breezeHazzardActive = true;
        _coldIcon.SetActive(true);
        yield return new WaitForSeconds(2f);
        _breezeHazzardActive = false;
        _coldIcon.SetActive(false);
    }

    #region Tutorial
    private bool IsRingInCueWindow(FishingRing ring)
    {
        if (ring == null) return false;

        float ringRadius = ring.transform.lossyScale.x * 5f;
        float threshold = _currentCatchRadius.Value - _cueInsideMargin;

        return ringRadius < threshold;
    }

    private void FreezeForCue()
    {
        if (_waitingCueTap) return;

        _waitingCueTap = true;
        _prevTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        // optional: show UI prompt here
        // EventManager.TriggerEvent("ShowTapCue", true);
    }

    private void UnfreezeFromCue()
    {
        if (!_waitingCueTap) return;
        EventManager.TriggerEvent("Caught3Fish");
        _waitingCueTap = false;
        Time.timeScale = _prevTimeScale <= 0 ? 1f : _prevTimeScale;

        // optional: hide UI prompt
        // EventManager.TriggerEvent("ShowTapCue", false);
    }

    private void OnCaughtFish()
    {
        _cueCount -= 1;

        if (_cueCount <= 0)
        {
            _freezeOnCue = false;
        }
    }
    #endregion
}
