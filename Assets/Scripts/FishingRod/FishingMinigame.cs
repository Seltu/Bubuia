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
    [SerializeField] private InputActionReference _fishingAction;
    [SerializeField] private Transform _indicatorRing;
    [SerializeField] private FishingRing _ringPrefab;
    [SerializeField] private Transform _durabilityIconsParent;
    [SerializeField] private DurabilityUI _durabilityIconPrefab;
    [SerializeField] private PlayerInventorySO _playerInventory;
    [SerializeField] private AlmanacSO _almanacSO;
    [SerializeField] private GameObject _returnPanel;

    [Header("Settings")]
    [SerializeField] private bool _isTutorialScene = false;

    private Queue<FishingRing> _spawnedRings = new Queue<FishingRing>();
    private Queue<DurabilityUI> _durabilityIcons = new Queue<DurabilityUI>();
    private bool _playing;
    private Fish _currentFish;
    private int _currentWave;
    private int _currentScore;
    private int _currentDurability;
    private bool _wavesHazzardActive;
    private bool _breezeHazzardActive;

    //[Header("Tutorial / Cue Freeze")]
    private bool _freezeOnCue;   // toggle for tutorial
    private bool _waitingCueTap = false;
    private float _prevTimeScale = 1f;
    private float _cueInsideMargin = 0.4f;
    private int _cueCount = 2; // ammount of fishes to catch until cue is deactivated, player must catch the ramaingn fish alone
    private List<InventoryItem> PlayerBaits => _playerInventory.items.Where(x => x.itemData is BaitTypeSO).ToList();

    private void Awake()
    {
        EventManager.AddListener<bool>("SetFreezeOnCue", v => _freezeOnCue = v);
        _fishingAction.action.performed += FishingButtonInput;
    }

    private void Start()
    {
        EventManager.AddListener<Fish>("StartFishingMinigame", StartMinigame);
        EventManager.AddListener("RingMiss", OnRingMiss);

        if (PlayerBaits[0].amount <= 5)
            PlayerBaits[0].amount = 5;

        if (_playerInventory.GetEquippedItem(EquipSlot.Bait).amount <= 0)
        {
            foreach (InventoryItem bait in PlayerBaits)
            {
                if (bait.amount > 0)
                {
                    SwitchBait(bait);
                    return;
                }
            }
        }
    }

    private void OnDestroy()
    {
        EventManager.RemoveListener<Fish>("StartFishingMinigame", StartMinigame);
        EventManager.RemoveListener("RingMiss", OnRingMiss);
        EventManager.RemoveListener<bool>("SetFreezeOnCue", v => _freezeOnCue = v);
        _fishingAction.action.performed -= FishingButtonInput;
    }

    private void Update()
    {
        if (!_playing)  return;
        _indicatorRing.position = _currentFish.transform.position;
        _indicatorRing.position = new Vector3(_indicatorRing.position.x, 0, _indicatorRing.position.z);

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
        //if (context.phase != InputActionPhase.Started || context.interaction is not TapInteraction) return;


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
            if (ring.transform.lossyScale.x * 4f < _playerInventory.CurrentFishingRod.catchRadius)
            {
                if(_breezeHazzardActive)
                {
                    _spawnedRings.Dequeue();
                    ring.FailRing();
                    LosePoint();
                    return;
                }

                _currentScore += _playerInventory.CurrentMoulinet.pullForce;
                ring.ExplodeRing();
                EventManager.TriggerEvent("ToggleCameraShake", ((float)_currentScore / _currentFish.GetFishTypeSO().fishingGoalScore) * _currentFish.GetSpeed()/2);
                if (ring.hasExploded())
                {
                    _spawnedRings.Dequeue();
                    EventManager.TriggerEvent("DistanceUpdate", _currentDurability + _currentScore, _currentFish.GetFishTypeSO().fishingGoalScore + _playerInventory.CurrentFishingLine.durability);
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

    public void SwitchBait(InventoryItem newBait)
    {
        if (_playing) return;
        if (newBait.amount <= 0) return;
        _playerInventory.EquipItem(newBait);
    }

    private void OnRingMiss()
    {
        _spawnedRings.Dequeue();
        if(!_breezeHazzardActive)
            LosePoint();
    }

    private void LosePoint()
    {
        _currentDurability--;
        var durabilityIcon = _durabilityIcons.Dequeue();
        durabilityIcon.PlayDestruction();
        EventManager.TriggerEvent("DistanceUpdate", _currentDurability + _currentScore, _currentFish.GetFishTypeSO().fishingGoalScore + _currentFish.GetFishTypeSO().fishingGoalScore + _playerInventory.CurrentFishingLine.durability);
        EventManager.TriggerEvent("ToggleCameraShake", ((float)_currentScore / _currentFish.GetFishTypeSO().fishingGoalScore) * 2f * _currentFish.GetSpeed());
        if (_currentDurability <= 0)
        {
            EndMinigame(false);
        }
    }

    private void StartMinigame(Fish fish)
    {
        if(_playing) return;


        if (!_isTutorialScene)
        {
            _playerInventory.AddItem(_playerInventory.CurrentBait, -1);
        }
        EventManager.TriggerEvent("ToggleCameraShake", fish.GetSpeed());
        EventManager.TriggerEvent("FocusOnHook", true);
        EventManager.TriggerEvent("TurnOffMovement");
        _currentScore = 0;
        _currentDurability = _playerInventory.CurrentFishingLine.durability;
        for (var i = 0; i < _playerInventory.CurrentFishingLine.durability; i++)
        {
            _durabilityIcons.Enqueue(Instantiate(_durabilityIconPrefab, _durabilityIconsParent));
        }
        _currentWave = 0;
        _currentFish = fish;
        _playing = true;
        _indicatorRing.gameObject.SetActive(true);
        StartCoroutine(GameLoop());
    }

    private void EndMinigame(bool won)
    {
        while (_spawnedRings.Count > 0)
        {
            _spawnedRings.Dequeue().FadeRing();
        }
        foreach (Transform child in _durabilityIconsParent)
        {
            Destroy(child.gameObject);
        }
        _durabilityIcons.Clear();
        _indicatorRing.gameObject.SetActive(false);
        _playing = false;
        EventManager.TriggerEvent("EndFishingMinigame", won);
        EventManager.TriggerEvent("FocusOnHook", false);
        EventManager.TriggerEvent("TurnOnMovement");
        if (won)
        {
            if (_freezeOnCue)
            {
                UnfreezeFromCue();
                _cueCount -= 1;
                if (_cueCount <= 0)
                {
                    _freezeOnCue = false;
                }
            }
            EventManager.TriggerEvent("FishCaught");
            _playerInventory.AddItem(_currentFish.GetFishTypeSO(), 1);
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
        if (_playerInventory.GetEquippedItem(EquipSlot.Bait).amount <= 0)
        {
            foreach (InventoryItem bait in PlayerBaits)
            {
                if (bait.amount > 0)
                {
                    SwitchBait(bait);
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
            ring.SetRing(_currentFish.transform, _currentFish.GetFishTypeSO().fishingPattern[_currentWave].speed / _playerInventory.CurrentBait.baitPower);
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
                _playerInventory.CurrentBait.baitPower);
            else
                yield return new WaitForSeconds((_currentFish.GetFishTypeSO().fishingPattern[_currentWave].secondsDelay *
                _playerInventory.CurrentBait.baitPower)/2f);
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
        float threshold = _playerInventory.CurrentFishingRod.catchRadius - _cueInsideMargin;

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
    #endregion
}
