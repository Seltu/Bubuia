using UnityEngine;

[CreateAssetMenu(fileName = "PlayerClothesSO_WholeBody", menuName = "Scriptable Objects/PlayerClothesSO_WholeBody")]
public class PlayerClothesSO_WholeBody : ScriptableObject
{
    /// <summary>
    /// Cada opção de roupa será um scriptable que guarda cada sprite de cada pedaço de roupa
    /// 
    /// O sistema de customização referenciará o scriptable da roupa guardada e fará com que as 
    /// sprites do jogador mudem para que sejam as mesmas sprites listadas no scriptable 
    /// da roupa atual
    /// </summary>

    public string clothingName;

    [Header("CORPO")]
    [Header("Braço ESQUERDO")]
    public Sprite bracoE;
    public Sprite mangaE;
    public Sprite maoE;

    [Header("Braço DIREITO")]
    public Sprite maoD;
    public Sprite bracoD;
    public Sprite golaD;

    [Header("Blusa")]
    public Sprite blusa;
    public Sprite roupa2;
    public Sprite tronco;

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
