using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Bait Type", menuName = "ScriptableObjects/BaitType")]
public class BaitTypeSO : ScriptableObject
{
    public string baitName;
    public Sprite baitSprite;
    public string descricao;
    public int valor;
    public float baitPower;

    private void OnEnable()
    {
        hideFlags = HideFlags.DontUnloadUnusedAsset;
    }
}
