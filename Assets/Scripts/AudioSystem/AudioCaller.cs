using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioCaller : MonoBehaviour
{
    [SerializeField] private string callOnEnable;

    private void OnEnable()
    {
        if (callOnEnable != "")
        {
            AudioSystem.Instance.PlaySFX(callOnEnable);
        }
    }
    public void CallSFX(string name)
    {
        AudioSystem.Instance.PlaySFX(name);
    }

    public void CallLoopSfx(string name)
    {
        AudioSystem.Instance.PlayLoopingSFX(name);
    }

    public void StopLoopingSfx(string name)
    {
        AudioSystem.Instance.StopLoopingSFX(name);
    }

    public void StopAllLoopingSFX()
    {
        AudioSystem.Instance.StopAllLoopingSFX();
    }
}
