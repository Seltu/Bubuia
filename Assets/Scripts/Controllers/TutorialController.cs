using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
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
    [SerializeField] private LayerMask _treasureLayer;
    [SerializeField] private LayerMask _fishLayer;
    [SerializeField] private GameObject _fishSpawner;
    [SerializeField] private GameObject _treasureSpawner;

    [Header("Pop Up UI")]
    [SerializeField] private CanvasGroup _tutCanvasGroup;
    [SerializeField] private float _fadeTime = 0.5f;
    [SerializeField] private TMP_Text _upperText;
    [SerializeField] private TMP_Text _lowerText;

    [Header("UI In-Game")]
    [SerializeField] private TMP_Text _instructionsTxt;
    [SerializeField] private Animator _anim;

    private int _index = 0;
    private bool _startTutu = false;
    private bool _waitingForPopup = true;

    #region detecting movement
    [SerializeField] private float _moveAmmount = 3f;
    private Vector3 _initialPos;
    #endregion

    private bool _hasCast;

    [Header("Step Transition")]
    [SerializeField] private float _stepTransitionDelay = 0.35f;
    private bool _isAdvancingStep = false;

    private bool _firstGemCollected = false;

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
            case 0: DetectMovement(); break;
            case 1: DetectCastAndRecall(); break;
            case 2: DetectCastAndRecall(); break;
            case 3: DetectAiming(); break;
            case 4: DetectFishing(); break;
            case 5: DetectGemCollection(); break;
            case 6: DetectTresureNearby(); break;
            case 7: DetectingTreasuing(); break;
            case 8: TutorialCompleted(); break;
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
        switch (index)
        {
            case 0:
                _upperText.text = "Bem vindo ao Bubuia! Nesta fase voçê aprenderá os controles e mecânicas básicas do jogo";
                _lowerText.text = "Vamos começar com a movimentação\r\n\r\nUse o </color=#FFEC00>joystick</color> no canto indferior esquerdo para mover o barco.";
                break;

            case 1:
                _upperText.text = "Agora vamos aprender a lançar a linha";
                _lowerText.text = "Clique em qualquer ponto da tela para lançar o anzol neste ponto";
                break;

            case 2:
                _upperText.text = "Recolher";
                _lowerText.text = "Agora tente puxar a linha de volta para o barco, clicando na tela novamente";
                break;

            case 3:
                _upperText.text = "Hora da Pesca";
                _lowerText.text = "Mova o barco e procure peixes! QUando achar um, jogue o anzol bem perto dele!";
                if (!_fishSpawner.activeInHierarchy) _fishSpawner.SetActive(true);
                break;

            case 4:
                _upperText.text = "Hora da Pesca";
                _lowerText.text = "Espere o peixe morder a isca. Quando ele morder, toque ma tela APENAS quando o aro branco estiver </color=yellow>dentro</color> do círculo! Repita até puxar o peixe para o barco";
                break;

            case 5:
                _upperText.text = "Coletando Gemas";
                _lowerText.text = "Ótimo! agora sabemos pescar. Mas há mais coisas na água...\nProcure por gemas e colete uma\n";
                if (!_treasureSpawner.activeInHierarchy) _treasureSpawner.SetActive(true);
                break;

            case 6:
                _upperText.text = "Caça ao Tesouro";
                _lowerText.text = "Procure por um marcador de tesouro\nDica: Caminhos de gemas podem indicar um tesouro!\nEles aparecem como pontosa escuros na água";
                break;

            case 7:
                _upperText.text = "Fisgue o Tesouro";
                _lowerText.text = "Jogue o anzol no círculo do tesouro e toque várias vezes na tela, rapidamente, antes que o círculo vermelho encolha totalmente!";
                break;

            case 8:
                _upperText.text = "Tutorial Completado!";
                _lowerText.text = "parabéns! Você completou o tutorial. Agora pode voltar ao menu e jogar normalmente\nDivirta-se! ^-^";
                break;
        }
    }

    private void UpdateInputLocks()
    {
        // movement locked only on cast / recall steps
        InputLock.movementLocked = (_index == 1 || _index == 2);

        // click locked during movement, gem collection, and treasure-nearby search
        InputLock.clickLocked = (_index == 0 || _index == 5 || _index == 6);
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
        _tutCanvasGroup.interactable = true;
        _tutCanvasGroup.blocksRaycasts = true;
        _tutCanvasGroup.DOFade(1, _fadeTime).SetUpdate(true);
    }

    public void FadeOutPopUp()
    {
        _tutCanvasGroup.interactable = false;
        _tutCanvasGroup.DOFade(0, _fadeTime).SetUpdate(true).OnComplete(() =>
        {
            _tutCanvasGroup.blocksRaycasts = false;
            Time.timeScale = 1f;
        });

        if (_index == 5 && _firstGemCollected)
        {
            AdvanceIndex();
        }
    }
    #endregion

    #region Detections
    private void DetectMovement()
    {
        _instructionsTxt.SetText(_tutorialQuest.objectives[0].description);
        float distanceMoved = Vector3.Distance(_boatTransform.position, _initialPos);
        _anim.SetBool("MoveTut", true);

        if (distanceMoved > _moveAmmount)
        {
            _anim.SetBool("MoveTut", false);
            _tutorialQuest.objectives[0].currentAmount = 1;
            AdvanceIndex();
        }
    }

    private void DetectCastAndRecall()
    {
        int objIndex = _index;
        _instructionsTxt.SetText(_tutorialQuest.objectives[objIndex].description);
        _anim.SetBool("HeelTut", true);

        if (_index == 1)
        {
            if (_rodController.IsHookInWater())
            {
                _anim.SetBool("HeelTut", false);
                _tutorialQuest.objectives[1].currentAmount = 1;
                AdvanceIndex();
            }
        }
        else if (_index == 2)
        {
            if (!_hasCast && _rodController.IsHookInWater()) _hasCast = true;

            if (_hasCast && !_rodController.IsHookInWater())
            {
                _tutorialQuest.objectives[2].currentAmount = 1;
                _hasCast = false;
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
        _instructionsTxt.SetText(_tutorialQuest.objectives[4].description);
    }

    private void OnCaughtFish()
    {
        if (_index == 4)
        {
            _tutorialQuest.objectives[4].currentAmount += 1;
            if (_tutorialQuest.objectives[4].isCompleted)
            {
                AdvanceIndex();
            }
        }
    }

    private void DetectGemCollection()
    {
        _instructionsTxt.SetText(_tutorialQuest.objectives[5].description);
        // progression happens through OnUpdateMoneyUI event -> PlayMoneyAnim()
    }

    private void PlayMoneyAnim()
    {
        // Only handle the first gem event
        if (_firstGemCollected) return;

        _firstGemCollected = true;
        _upperText.text = "Estas são suas gemas\r\n\r\nCom elas, você pode comprar itens e upgrades";
        _lowerText.text = "Você pode obtê-las ao vender seus peixes, ou coleta-las navegando pelo mapa";

        InputLock.movementLocked = true;
        InputLock.clickLocked = true;
        _anim.SetTrigger("MoneyTut");

        FadeInPopUp();

        // Advance tutorial if we are on the gem-collection step
        if (_index == 5)
        {
            _tutorialQuest.objectives[5].currentAmount = 1;
        }
    }

    private void DetectTresureNearby()
    {
        _instructionsTxt.SetText(_tutorialQuest.objectives[6].description);
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

        if (_index == 7)
        {
            _tutorialQuest.objectives[7].currentAmount += 1;
            StopFishingAnimClue();

            if (_tutorialQuest.objectives[7].isCompleted)
            {
                AdvanceIndex();
            }
        }
    }
    #endregion

    private void ResetTutorialSteps()
    {
        foreach (var obj in _tutorialQuest.objectives)
            obj.currentAmount = 0;

        _firstGemCollected = false;
    }

    private void TutorialCompleted()
    {
        _instructionsTxt.SetText("Tutorial Completado!");
        _tutorialStatus.tutorialCompleted = true;
        PlayerPrefs.SetInt("TutorialCompleted", 1);
    }

    public void ReturnToMenuButton()
    {
        if (_index == 8)
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