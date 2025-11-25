using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Sprite List", menuName = "ScriptableObjects/SpriteList")]
public class SpriteList : ScriptableObject
{
    public Sprite[] sprites;
    private void OnEnable()
    {
        hideFlags = HideFlags.DontUnloadUnusedAsset;
    }
}

public struct PlayerCustomArray
{
    public Sprite sprite;
    public Animator _animation;
}