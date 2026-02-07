using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR;

public class TutorialController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private QuestSO _tutorialQuest;
    [SerializeField] private TutorialStatus _tutorialStatus;
    [SerializeField] private Transform _boatTransform;
    [SerializeField] private FishingRodController _rodController;
    [SerializeField] private LayerMask _fishLayer;
    [SerializeField] private GameObject _fishSpawner;

    [Header("Sardine Id")]
    [SerializeField] private string _sardineId;

    [Header("UI")]
    [SerializeField] private TMP_Text _instructionsTxt;
    [SerializeField] private Animator _anim;
    [SerializeField] private Image _clickImg;
    [SerializeField] private Sprite _mouseIcon;
    [SerializeField] private Sprite _handIcon;
 
    private int _index = 0;   

    #region detecting movement

    [SerializeField] private float _moveAmmount; // set a minimal ammount for player to move until step is deemed complete
    private Vector3 _initialPos;
    
    #endregion

    #region detecting cast & recall
    private bool _hasCast;
    private bool _bobbleInWater;
    private bool _startTutu = false;
    #endregion


    private void Start()
    {
        EventManager.AddListener("FishCaught", OnCaughtFish);
        EventManager.AddListener("Tut", FishingAnimClue);
        EventManager.AddListener("Caught3Fish", StopFishingAnimClue);

        EventManager.TriggerEvent("SetFreezeOnCue", true);

        _initialPos = _boatTransform.position;
        ResetTutorialSteps();
        InputLock.movementLocked = true;
        InputLock.clickLocked = true;

        Invoke("TutCanStart", 2f);
    }

    private void OnDestroy()
    {
        EventManager.RemoveListener("FishCaught", OnCaughtFish);
        EventManager.RemoveListener("Tut", FishingAnimClue);
        EventManager.RemoveListener("Caught3Fish", StopFishingAnimClue);
    }

    private void Update()
    {
        if (!_startTutu) return;

        switch(_index)
        {
            case 0:
                InputLock.movementLocked = false;
                InputLock.clickLocked = true;

                _anim.SetBool("MoveTut", true);
                DetectMovement();
                break;
            case 1:
                InputLock.movementLocked = true;
                InputLock.clickLocked = false;
                _anim.SetBool("HeelTut", true);
                DetectCastAndRecall();
                break;
            case 2:
                InputLock.movementLocked = true;
                InputLock.clickLocked = false;
                _anim.SetBool("HeelTut", true);
                DetectCastAndRecall();
                break;
            case 3:
                if (!_fishSpawner.activeInHierarchy) _fishSpawner.SetActive(true);
                InputLock.movementLocked = false;
                InputLock.clickLocked = false;
                DetectAiming();
                break;
            case 4:
                DetectFishing();
                break;
            case 5:
                TutorialCompleted();
                break;
            default:
                return;
        }
    }

    private void TutCanStart()
    {
        _startTutu = true;
    }


    private void DetectMovement()
    {
        if (_index == 0 && !_tutorialQuest.objectives[0].isCompleted)
        {
            _instructionsTxt.SetText(_tutorialQuest.objectives[0].description);

            float distanceMoved = Vector3.Distance(_boatTransform.position, _initialPos);
            if (distanceMoved > _moveAmmount)
            {
                Debug.Log("Movement Step Complete!");
                _anim.SetBool("MoveTut", false);
                _tutorialQuest.objectives[0].currentAmount += 1;
                _index++;
            }
        }
    }

    /// <summary>
    /// For detecting both player casting reel & player pulling it back - BEFORE AIMING AT FISH
    /// </summary>
    private void DetectCastAndRecall()
    {
        if (_index == 1 && !_tutorialQuest.objectives[1].isCompleted)
        {
            _instructionsTxt.SetText(_tutorialQuest.objectives[1].description);

            if (!_hasCast && _rodController.IsHookInWater())
            {
                Debug.Log("Cast and Recall Step 1 Complete!");
                _hasCast = true;
                _tutorialQuest.objectives[1].currentAmount += 1;
                _hasCast = false;
                _index++;
            }
        }
        else if (_index == 2 && !_tutorialQuest.objectives[2].isCompleted)
        {
            _instructionsTxt.SetText(_tutorialQuest.objectives[2].description);

            if (!_hasCast && _rodController.IsHookInWater())
            {
                _hasCast = true;
            }

            if (_hasCast && !_rodController.IsHookInWater())
            {
                Debug.Log("Cast and Recall Step 2 Complete!");
                _tutorialQuest.objectives[2].currentAmount += 1;
                _hasCast = false;
                _index++;
                _anim.SetBool("HeelTut", false);
            }
        }
    }

    /// <summary>
    /// For teaching player to AIM/THROW close to a fish to try and catch it
    /// </summary>
    private void DetectAiming()
    {
        if (_index == 3 && !_tutorialQuest.objectives[3].isCompleted)
        {
            _instructionsTxt.SetText(_tutorialQuest.objectives[3].description);

            Collider[] nearbyFish = Physics.OverlapSphere(_rodController.GetCastTarget(), 1.5f, _fishLayer);

            if (nearbyFish.Length > 0)
            {
                Debug.Log("Aiming Step Complete!");
                _tutorialQuest.objectives[3].currentAmount += 1;
                _index++;
            }
        }
    }

    private void DetectFishing()
    {
        _instructionsTxt.SetText(_tutorialQuest.objectives[4].description);
    }

    private void OnCaughtFish()
    {
        if (_index == 4 && !_tutorialQuest.objectives[4].isCompleted)
        {
            Debug.Log("Fish Caught");
            _tutorialQuest.objectives[4].currentAmount += 1;

            if (_tutorialQuest.objectives[4].isCompleted)
            {
                _index++;
            }
        }
    }

    private void StopTimeForFishing()
    {
        Time.timeScale = 0f;
        // ui trigger for the finger touching screen
    }

    private void SetDefaultTIme()
    {
        Time.timeScale = 1f;
    }

    private void ResetTutorialSteps()
    {
        for(int i = 0; i < _tutorialQuest.objectives.Count; i++)
        {
            _tutorialQuest.objectives[i].currentAmount = 0;
        }
    }

    private void TutorialCompleted()
    {
        Debug.Log("Finished!");
        _instructionsTxt.SetText("Tutorial Completed!");
        _tutorialStatus.tutorialCompleted = true;
        PlayerPrefs.SetInt("TutorialCompleted", 1);
        Invoke("ReturnToMenu", 2f);
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

    private void ReturnToMenu()
    {
        SceneManager.LoadScene("CityScene");
    }
}
