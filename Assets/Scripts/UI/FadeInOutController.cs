using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeInOutController : MonoBehaviour
{
    [SerializeField] BoolVariable canPause;
    [SerializeField] private Animator _anim;

    void Start()
    {
        EventManager.AddListener("OnSceneStarts", PlayFadeOut);
        EventManager.AddListener("OnChangeScene", PlayFadeIn);
    }

    private void OnDestroy()
    {
        EventManager.RemoveListener("OnSceneStarts", PlayFadeOut);
        EventManager.RemoveListener("OnChangeScene", PlayFadeIn);
    }

    private void PlayFadeIn()
    {
        canPause.value = false;
        _anim.SetTrigger("FadeIn");
    }

    private void PlayFadeOut()
    {
        canPause.value = false;
        _anim.SetTrigger("FadeOut");
    }

    public void OnFadeOutFinish()
    {
        canPause.value = true;
    }
}
