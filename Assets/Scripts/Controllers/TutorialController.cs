using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

public class TutorialController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private QuestSO _tutorialQuest;
    [SerializeField] private TutorialStatus _tutorialStatus;
    [SerializeField] private Animator _uiAnim;
    [SerializeField] private Transform _boatTransform;
    [SerializeField] private FishingRodController _rodController;
    [SerializeField] private LayerMask _fishLayer;

    [Header("Sardine Id")]
    [SerializeField] private string _sardineId;

    [Header("UI")]
    [SerializeField] private TMP_Text _instructionsTxt;
 
    private int _index = 0;

    #region detecting movement

    [SerializeField] private float _moveAmmount; // set a minimal ammount for player to move until step is deemed complete
    private Vector3 _initialPos;
    
    #endregion

    #region detecting cast & recall
    private bool _hasCast;
    private bool _bobbleInWater;
    #endregion


    private void Start()
    {
        EventManager.AddListener("FishCaught", OnCaughtFish);

        EventManager.TriggerEvent("SetFreezeOnCue", true);

        _initialPos = _boatTransform.position;
        ResetTutorialSteps();
    }

    private void OnDestroy()
    {
        EventManager.RemoveListener("FishCaught", OnCaughtFish);
    }

    private void Update()
    {
        switch(_index)
        {
            case 0:
                InputLock.movementLocked = false;
                InputLock.clickLocked = true;
                DetectMovement();
                break;
            case 1:
                InputLock.movementLocked = true;
                InputLock.clickLocked = false;
                DetectCastAndRecall();
                break;
            case 2:
                InputLock.movementLocked = true;
                InputLock.clickLocked = false;
                DetectCastAndRecall();
                break;
            case 3:
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

    private void DetectMovement()
    {
        if (_index == 0 && !_tutorialQuest.objectives[0].isCompleted)
        {
            _instructionsTxt.SetText(_tutorialQuest.objectives[0].description);

            float distanceMoved = Vector3.Distance(_boatTransform.position, _initialPos);
            if (distanceMoved > _moveAmmount)
            {
                Debug.Log("Movement Step Complete!");
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
    }

    #region Animations

    private void StartMovementTutorial()
    {
        _instructionsTxt.text = "Use o Joystick para mover o barco";
        _uiAnim.SetTrigger("MoveTut");
    }

    private void StartLineTutorial()
    {
        _instructionsTxt.text = "Clique na tela para lançar o anzol";
        _uiAnim.SetTrigger("LineTut");
    }

    private void StartHeelTutorial()
    {
        //_instructionsTxt.text = "Clique na tela para puxar o anzol de volta";
        _uiAnim.SetTrigger("HeelTut");
    }
    private void StartAimTutorial()
    {
        _instructionsTxt.text = "Clique perto de um peixe para fisgá-lo";
        _uiAnim.SetTrigger("AimTut");
    }

    private void StartFishingTutorial()
    {
        _instructionsTxt.text = "Clique na tela quando o círculo branco estiver dentro do círculo!";
        _uiAnim.SetTrigger("FishingTut");
    }

    private void EndCurrentTutorial()
    {
        _uiAnim.SetTrigger("EndCurrentTut");
        _uiAnim.SetTrigger("EndTut");
    }

    public void ChangeInstructionText(string text)
    {
        _instructionsTxt.text = text;
    }

    #endregion
}
