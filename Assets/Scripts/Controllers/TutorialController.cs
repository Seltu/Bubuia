using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TutorialController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private QuestSO _tutorialQuest;
    [SerializeField] private TutorialStatus _tutorialStatus;
    [SerializeField] private Transform _boatTransform;
    [SerializeField] private Transform _boatModelTransform;
    [SerializeField] private FishingRodController _rodController;
    [SerializeField] private PlayerShipController _shipController;
    [SerializeField] private LayerMask _treasureLayer;
    [SerializeField] private LayerMask _fishLayer;
    [SerializeField] private GameObject _fishSpawner;
    [SerializeField] private GameObject _treasureSpawner;

    [Header("Pop Up UI")]
    [SerializeField] private CanvasGroup _tutCanvasGroup;
    [SerializeField] private float _fadeTime = 0.5f;
    [SerializeField] private TMP_Text _popUpText;

    [Header("UI In-Game")]
    [SerializeField] private CanvasGroup _fishingCanvas;
    [SerializeField] private CanvasGroup _sideInstructionsCG;
    [SerializeField] private TMP_Text _instructionsTxt;
    [SerializeField] private Animator _anim;

    private int _index = 0;
    private bool _startTutu = false;
    private bool _waitingForPopup = true;

    [Header("Movement Detection")]
    [SerializeField] private GameObject _movementCircle;
    [SerializeField] private GameObject _wasdIcons;
    [SerializeField] private GameObject _spaceIcon;
    [SerializeField] private float _moveAmmount = 3f;
    private Vector3 _initialPos;
    private bool _wasBoostLockedLastFrame = false;

    [Header("Cast & Recall Detection")]
    [SerializeField] private GameObject _spots;

    [Header("Gem Detection")]
    [SerializeField] private PointingArrow _poitingArrow;

    private bool _hasCast;

    [Header("Step Transition")]
    [SerializeField] private float _stepTransitionDelay = 0.35f;
    [TextArea(3, 10)]
    [SerializeField] private string[] _stepTexts;
    private bool _isAdvancingStep = false;

    private bool _firstGemCollected = false;

    #region Step constants
    private const int OBJ_MOVE = 0;
    private const int OBJ_BOOST = 1;
    private const int OBJ_CAST_RECALL = 2;
    private const int OBJ_AIM = 3;
    private const int OBJ_FISH = 4;
    private const int OBJ_GEM = 5;
    private const int OBJ_TREASURE_NEAR = 6;
    private const int OBJ_TREASURE_CATCH = 7;
    #endregion


    private void Start()
    {
        EventManager.AddListener("FishCaught", OnCaughtFish);
        EventManager.AddListener("TreasureCaught", DetectCaughtTreasure);
        EventManager.AddListener("Tut", FishingAnimClue);
        EventManager.AddListener("HookChest", FishingAnimClue);
        EventManager.AddListener("Caught3Fish", StopFishingAnimClue);
        EventManager.AddListener("TreasureFail", StopFishingAnimClue);
        EventManager.AddListener("OnUpdateMoneyUI", PlayMoneyAnim);

        EventManager.TriggerEvent("SetFreezeOnCue", true);

        _initialPos = _boatTransform.position;
        ResetTutorialSteps();

        InputLock.movementLocked = true;
        InputLock.clickLocked = true;

        _tutCanvasGroup.alpha = 0f;
        _tutCanvasGroup.interactable = false;
        _tutCanvasGroup.blocksRaycasts = false;

        Invoke(nameof(ShowNextStepPopup), 1f);
    }

    private void OnDestroy()
    {
        EventManager.RemoveListener("FishCaught", OnCaughtFish);
        EventManager.RemoveListener("TreasureCaught", DetectCaughtTreasure);
        EventManager.RemoveListener("Tut", FishingAnimClue);
        EventManager.RemoveListener("Caught3Fish", StopFishingAnimClue);
        EventManager.RemoveListener("HookChest", FishingAnimClue);
        EventManager.RemoveListener("TreasureFail", StopFishingAnimClue);
        EventManager.RemoveListener("OnUpdateMoneyUI", PlayMoneyAnim);
    }

    private void Update()
    {
        if (!_startTutu || _waitingForPopup) return;

        switch (_index)
        {
            case 0: AdvanceIndex(); break;
            case 1: DetectMovement(); break;
            case 2: DetectBoost(); break;
            case 3: DetectCastAndRecall(); break;
            case 4: DetectAiming(); break;
            case 5: DetectFishing(); break;
            case 6: DetectGemCollection(); break;
            case 7: DetectTresureNearby(); break;
            case 8: DetectingTreasuing(); break;
            case 9: TutorialCompleted(); break;
        }
    }

    public void TutCanStart()
    {
        _waitingForPopup = false;
        _startTutu = true;
        FadeOutPopUp();
        UpdateInputLocks();
    }

    private void ShowNextStepPopup()
    {
        _waitingForPopup = true;
        SetupPopupText(_index);
        FadeInPopUp();
    }

    private void SetupPopupText(int index)
    {
        _popUpText.text = _stepTexts[index];

        switch (index)
        {
            case OBJ_FISH:
                if (!_fishSpawner.activeInHierarchy)
                {
                    _fishSpawner.SetActive(true);
                }
                break;

            case OBJ_GEM:
                if (!_treasureSpawner.activeInHierarchy)
                {
                    _fishSpawner.SetActive(false);
                    _treasureSpawner.SetActive(true);
                }
                break;
        }
    }

    private void UpdateInputLocks()
    {
        // movement locked only on cast / recall steps
        InputLock.movementLocked = (_index == 0 || _index == 3);

        // click locked during movement, gem collection, and treasure-nearby search
        InputLock.clickLocked = (_index == 0 || _index == 2 || _index == 6 || _index == 7);
    }

    private void AdvanceIndex()
    {
        if (_isAdvancingStep) return;
        StartCoroutine(AdvanceIndexRoutine());
    }

    private IEnumerator AdvanceIndexRoutine()
    {
        _isAdvancingStep = true;

        _waitingForPopup = true;
        InputLock.movementLocked = true;
        InputLock.clickLocked = true;

        if (_stepTransitionDelay > 0f)
            yield return new WaitForSecondsRealtime(_stepTransitionDelay);

        _index++;
        ShowNextStepPopup();

        _isAdvancingStep = false;
    }

    #region Pop Up Animation
    public void FadeInPopUp()
    {
        Time.timeScale = 0f;

        _tutCanvasGroup.DOKill();
        _fishingCanvas.DOKill();
        _sideInstructionsCG.DOKill();

        _tutCanvasGroup.interactable = true;
        _tutCanvasGroup.blocksRaycasts = true;

        _tutCanvasGroup.alpha = 0f;
        _tutCanvasGroup.DOFade(1f, _fadeTime).SetUpdate(true);

        _fishingCanvas.DOFade(0f, _fadeTime).SetUpdate(true);
        _sideInstructionsCG.DOFade(0f, _fadeTime).SetUpdate(true);
    }

    public void FadeOutPopUp()
    {
        _tutCanvasGroup.DOKill();
        _fishingCanvas.DOKill();
        _sideInstructionsCG.DOKill();

        _tutCanvasGroup.interactable = false;
        _tutCanvasGroup.DOFade(0f, _fadeTime).SetUpdate(true).OnComplete(() =>
        {
            _tutCanvasGroup.blocksRaycasts = false;
            Time.timeScale = 1f;
        });

        _sideInstructionsCG.DOFade(1f, _fadeTime).SetUpdate(true);
        _fishingCanvas.DOFade(1f, _fadeTime).SetUpdate(true);

        if (_index == 5 && _firstGemCollected)
        {
            AdvanceIndex();
        }
    }
    #endregion

    #region Detections
    private void DetectMovement()
    {
        if (!_movementCircle.activeInHierarchy) _movementCircle.SetActive(true);
        if (!_wasdIcons.activeInHierarchy) _wasdIcons.SetActive(true);
        _instructionsTxt.SetText(_tutorialQuest.objectives[OBJ_MOVE].description);
        float distanceMoved = Vector3.Distance(_boatTransform.position, _initialPos);
        _anim.SetBool("MoveTut", true);

        if (distanceMoved > _moveAmmount)
        {
            _anim.SetBool("MoveTut", false);
            _tutorialQuest.objectives[0].currentAmount = 1;
            _wasdIcons.SetActive(false);
            _movementCircle.SetActive(false);
            AdvanceIndex();
        }
    }

    private void DetectBoost()
    {
        QuestObjective boostObjective = _tutorialQuest.objectives[OBJ_BOOST];
        _instructionsTxt.SetText(boostObjective.description + "\n" + boostObjective.currentAmount + "/" + boostObjective.requiredAmount);
        if(!_spaceIcon.activeInHierarchy) _spaceIcon.SetActive(true);

        bool isBoostLocked = _shipController.IsBoostLocked();

        // Count only once when boost changes from available -> depleted/locked
        if (!_wasBoostLockedLastFrame && isBoostLocked)
        {
            boostObjective.currentAmount++;

            if (boostObjective.isCompleted)
            {
                _spaceIcon.SetActive(false);
                AdvanceIndex();
            }
        }

        _wasBoostLockedLastFrame = isBoostLocked;
    }

    private void DetectCastAndRecall()
    {
        QuestObjective castRecallObjective = _tutorialQuest.objectives[OBJ_CAST_RECALL];
        _instructionsTxt.SetText(castRecallObjective.description + "\n" + castRecallObjective.currentAmount + "/" + castRecallObjective.requiredAmount);
        _anim.SetBool("HeelTut", true);
        if (!_spots.activeInHierarchy) _spots.SetActive(true);

        // First half: player has cast the hook
        if (!_hasCast && _rodController.IsHookInWater())
        {
            _hasCast = true;
        }

        // Second half: player had cast before, and now fully recalled
        if (_hasCast && !_rodController.IsHookInWater())
        {
            _hasCast = false;
            //castRecallObjective.currentAmount++;

            if (castRecallObjective.isCompleted)
            {
                _spots.SetActive(false);
                _anim.SetBool("HeelTut", false);
                AdvanceIndex();
            }
        }
    }

    private void DetectAiming()
    {
        _instructionsTxt.SetText(_tutorialQuest.objectives[3].description);
        var pos = _rodController.GetHookPosition();
        Collider[] nearbyFish = Physics.OverlapSphere(pos, 1.5f, _fishLayer, QueryTriggerInteraction.Collide);

        if (nearbyFish.Length > 0 && _rodController.IsHookInWater())
        {
            _tutorialQuest.objectives[3].currentAmount = 1;
            AdvanceIndex();
        }
    }

    private void DetectFishing()
    {
        QuestObjective fishingObj = _tutorialQuest.objectives[OBJ_FISH];
        _instructionsTxt.SetText(fishingObj.description + "\n" + fishingObj.currentAmount + "/" + fishingObj.requiredAmount);
    }

    private void OnCaughtFish()
    {
        if (_index == 5)
        {
            _tutorialQuest.objectives[OBJ_FISH].currentAmount += 1;
            if (_tutorialQuest.objectives[OBJ_FISH].isCompleted)
            {
                AdvanceIndex();
            }
        }
    }

    private void DetectGemCollection()
    {
        QuestObjective gemObjective = _tutorialQuest.objectives[OBJ_GEM];
        _instructionsTxt.SetText(gemObjective.description);

        if (!_poitingArrow.gameObject.activeInHierarchy) _poitingArrow.gameObject.SetActive(true);
    }

    private void PlayMoneyAnim()
    {
        // Only handle the first gem event
        if (_firstGemCollected) return;

        _firstGemCollected = true;

        InputLock.movementLocked = true;
        InputLock.clickLocked = true;
        
        StartCoroutine(GemAnimDelay());
    }

    private IEnumerator GemAnimDelay()
    {
        yield return new WaitForSecondsRealtime(_stepTransitionDelay);

        _poitingArrow.SetLookAtTreasure();

        _tutorialQuest.objectives[OBJ_GEM].currentAmount = 1;

        AdvanceIndex();
    }

    private void DetectTresureNearby()
    {
        _instructionsTxt.SetText(_tutorialQuest.objectives[OBJ_TREASURE_NEAR].description);
        Collider[] nearbyTreasure = Physics.OverlapSphere(_boatModelTransform.position, 20f, _treasureLayer, QueryTriggerInteraction.Collide);

        if (nearbyTreasure.Length > 0)
        {
            _tutorialQuest.objectives[6].currentAmount = 1;
            AdvanceIndex();
        }
    }

    private void DetectingTreasuing()
    {
        _instructionsTxt.SetText(_tutorialQuest.objectives[7].description);
    }

    private void DetectCaughtTreasure()
    {
        Debug.Log("Caught Treasure");

        if (_index == 8)
        {
            _tutorialQuest.objectives[OBJ_TREASURE_CATCH].currentAmount += 1;
            StopFishingAnimClue();

            if (_tutorialQuest.objectives[OBJ_TREASURE_CATCH].isCompleted)
            {
                Invoke("AdvanceIndex", _stepTransitionDelay);
            }
        }
    }
    #endregion

    private void ResetTutorialSteps()
    {
        foreach (var obj in _tutorialQuest.objectives)
            obj.currentAmount = 0;

        _firstGemCollected = false;
        _firstGemCollected = false;
        _wasBoostLockedLastFrame = false;
    }

    private void TutorialCompleted()
    {
        _instructionsTxt.SetText("Tutorial Completado!");
        _tutorialStatus.tutorialCompleted = true;
        PlayerPrefs.SetInt("TutorialCompleted", 1);
    }

    public void ReturnToMenuButton()
    {
        if (_index == 9)
        {
            Time.timeScale = 1f;
            ReturnToMenu();
        }
    }

    private void FishingAnimClue()
    {
        _anim.SetTrigger("AimTut");
        _anim.SetBool("AimTutBool", true);
    }

    private void StopFishingAnimClue()
    {
        _anim.ResetTrigger("AimTut");
        _anim.SetBool("AimTutBool", false);
    }

    private void ReturnToMenu() => SceneManager.LoadScene("CityScene");
}