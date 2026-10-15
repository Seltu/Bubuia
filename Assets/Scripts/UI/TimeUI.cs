using TMPro;
using UnityEngine;

public class TimeUI : MonoBehaviour
{
    public TMP_Text timeText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        EventManager.AddListener<int, int>("TimePassed", UpdateUI);
    }

    private void OnDestroy()
    {
        EventManager.RemoveListener<int, int>("TimePassed", UpdateUI);
    }

    void UpdateUI(int Hours, int Minutes)
    {
        string niceTime = string.Format("{0:00}:{1:00}", Hours, Minutes);
        timeText.text = niceTime;
    }
}
