using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FadeInOutController : MonoBehaviour
{
    [SerializeField] BoolVariable canPause;
    [SerializeField] private Animator _anim;

    void Start()
    {
        EventManager.AddListener("OnSceneStarts", PlayFadeOut);
        EventManager.AddListener<string>("ChangeScene", PlayFadeIn);
    }

    private void OnDestroy()
    {
        EventManager.RemoveListener("OnSceneStarts", PlayFadeOut);
        EventManager.RemoveListener<string>("ChangeScene", PlayFadeIn);
    }

    private void PlayFadeIn(string newScene)
    {
        StartCoroutine(FadeIn(newScene));
    }

    private IEnumerator FadeIn(string newScene)
    {
        canPause.value = false;
        _anim.SetTrigger("FadeIn");
        yield return new WaitForSeconds(0.7f);
        SceneManager.LoadScene(newScene);
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
