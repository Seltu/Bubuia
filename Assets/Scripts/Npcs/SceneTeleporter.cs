using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTeleporter : NpcDetection
{
    [SerializeField] private string _sceneName;
    private bool _isTeleporting = false;

    protected virtual void OnInteract()
    {
        if (!_isTeleporting && _isInReach)
        {
            _isTeleporting = true;
            StartCoroutine(FadeOutTimer());
        }
    }

    private IEnumerator FadeOutTimer()
    {
        gameObject.GetComponent<AudioCaller>().CallSFX("Footsteps");
        EventManager.TriggerEvent("ChangeScene", _sceneName);
        yield return new WaitForSeconds(0.7f);
        SceneManager.LoadScene(_sceneName);
    }
}
