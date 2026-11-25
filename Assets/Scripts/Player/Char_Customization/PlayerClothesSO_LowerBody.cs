using UnityEngine;

[CreateAssetMenu(fileName = "PlayerClothesSO_LowerBody", menuName = "Scriptable Objects/PlayerClothesSO_LowerBody")]
public class PlayerClothesSO_LowerBody : ScriptableObject
{
    /// <summary>
    /// Cada opção de roupa será um scriptable que guarda cada sprite de cada pedaço de roupa
    /// 
    /// O sistema de customização referenciará o scriptable da roupa guardada e fará com que as 
    /// sprites do jogador mudem para que sejam as mesmas sprites listadas no scriptable 
    /// da roupa atual
    /// </summary>

    public string legsClothingName;

    [Header("PERNAS")]
    [Header("Base")]
    public Sprite shortBase;

    [Header("Perna DIREITA")]
    public Sprite shortD;
    public Sprite pernaD;
    public Sprite peD;

    [Header("Perna ESQUERDA")]
    public Sprite shortE;
    public Sprite peE;
    public Sprite pernaE;
}
