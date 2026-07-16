using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void TurnOffControls()
    {
        EventManager.TriggerEvent("TurnOffControls");
    }

    public void ResetGlobalVars()
    {
        GlobalFlagsManager.DeleteSave();
        Debug.Log("Flags Cleared");
        InputLock.movementLocked = false;
        InputLock.clickLocked = false;
    }
}
