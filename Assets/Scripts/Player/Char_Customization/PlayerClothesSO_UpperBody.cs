using UnityEngine;

[CreateAssetMenu(fileName = "PlayerClothesSO_UpperBody", menuName = "Scriptable Objects/PlayerClothesSO_UpperBody")]
public class PlayerClothesSO_UpperBody : ScriptableObject
{
    /// <summary>
    /// Cada opção de roupa será um scriptable que guarda cada sprite de cada pedaço de roupa
    /// 
    /// O sistema de customização referenciará o scriptable da roupa guardada e fará com que as 
    /// sprites do jogador mudem para que sejam as mesmas sprites listadas no scriptable 
    /// da roupa atual
    /// </summary>

    public string torsoClothingName;

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
}
