using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeSceneOnDisable : MonoBehaviour
{
    [SerializeField] private string _sceneName;

    private void OnDisable()
    {
        ChangeScene(_sceneName);
    }

    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
