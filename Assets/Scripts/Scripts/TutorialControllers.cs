using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialControllers : MonoBehaviour
{
    [SerializeField] private GameObject[] _tutorialsPanels;
    [SerializeField] private TutorialStatus _tutorialStatus;
    [SerializeField] private AlmanacSO _almanacSO;

    private void Awake()
    {
        EventManager.AddListener<string>("CallTutorial", ShowPanel);
        EventManager.AddListener<string>("HideTutorial", HidePanel);
        EventManager.AddListener("SetTutorialConcluded", SetTutorialConcluded);
    }

    private void OnDestroy()
    {
        EventManager.RemoveListener<string>("CallTutorial", ShowPanel);
        EventManager.RemoveListener<string>("HideTutorial", HidePanel);
    }

    private void HideAllPanels (string panel)
    {
        if (!System.Array.Exists(_tutorialsPanels, p => p.name == panel)) return;

        for (int i =0; i < _tutorialsPanels.Length; i++)
        {
            Debug.Log("hide " +  _tutorialsPanels[i].name);
            _tutorialsPanels[i].SetActive(false);
        }
    }

    private void ShowPanel(string panelName)
    {
        if (_tutorialStatus._tutorialIsConcluded) return;
        

        GameObject panelToShow = System.Array.Find(_tutorialsPanels, panel => panel.name == panelName);
        if(panelToShow != null) Debug.Log(panelToShow.name);

        if (panelToShow != null)
            panelToShow.SetActive(true);
    }

    private void HidePanel(string panelName)
    {
        if (_tutorialStatus._tutorialIsConcluded) return;

        GameObject panelToShow = System.Array.Find(_tutorialsPanels, panel => panel.name == panelName);

        if (panelToShow != null)
            panelToShow.SetActive(false);
    }

    private void SetTutorialConcluded()
    {
        _tutorialStatus._tutorialIsConcluded = true;
    }
}
