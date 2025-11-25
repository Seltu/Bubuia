using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGMSetter : MonoBehaviour
{
    [SerializeField] private string BGMName;
    private void Start()
    {
        AudioSystem.Instance.PlayBGM(BGMName);
    }
}
