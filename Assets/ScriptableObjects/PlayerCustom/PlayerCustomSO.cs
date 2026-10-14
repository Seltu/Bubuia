using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Player Custom Data", menuName = "ScriptableObjects/PlayerCustomData")]
public class PlayerCustomSO: ScriptableObject
{
    /// <summary>
    /// Este scriptable guardará os códigos da roupa do player durante a sessão 
    /// 
    /// Toda vez que o jogo inicia, o scriptable deverá ler as informações salvas no player prefs
    /// e guardar os códigos de lá aqui.
    /// </summary>

    // marcador para outros scripts do jogo verificarem se a roupa já foi lida da memória ou não
    public bool playerCustomizationLoaded = false;

    public string playerName;

    public Color skinColor;

    public Color eyesColor;

    public Color hairColor;
    public string hairCode;

    public string dressCode;
    public string upperDressCode;
    public string lowerDressCode;

    private void OnEnable()
    {
        hideFlags = HideFlags.DontUnloadUnusedAsset;
    }
}
