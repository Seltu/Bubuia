using UnityEngine;

public class CheckTutorial : MonoBehaviour
{
    [SerializeField] private TutorialStatus _tutStatusSO;
    [SerializeField] private SceneChanger _changer;
    [SerializeField] private GameObject _tutorialPanel;

    private void Awake()
    {
        _tutStatusSO.tutorialCompleted = PlayerPrefs.GetInt("TutorialCompleted") != 0;
    }

    public void PlayButton()
    {
        if (_tutStatusSO.tutorialCompleted)
        {
            _changer.ChangeScene("FishingScene");
        }
        else
        {
            _tutorialPanel.SetActive(true);
        }
    }

    public void TutorialPopUpYes()
    {
        _changer.ChangeScene("TutorialScene");
    }
    public void TutorialPopUpNo()
    {
        _changer.ChangeScene("FishingScene");
        _tutStatusSO.tutorialCompleted = true;
        PlayerPrefs.SetInt("TutorialCompleted", _tutStatusSO.tutorialCompleted ? 1 : 0);
    }
}
