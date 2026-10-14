using UnityEngine;

public static class PlayerCustomPrefs
{
    private const string PLAYER_NAME = "PC_PLAYER_NAME";

    private const string SKIN_COLOR = "PC_SKIN";

    private const string EYES_COLOR = "PC_EYES_COLOR";

    private const string HAIR_COLOR = "PC_HAIR";
    private const string HAIR_CODE = "PC_HAIR_CODE";

    private const string DRESS_CODE = "PC_DRESS_CODE"; // for whole body clothing
    private const string UPPERBODY_DRESS_CODE = "PC_UPPERBODY_DRESS_CODE"; // for upper body clothing
    private const string LOWERBODY_DRESS_CODE = "PC_LOWERBODY_DRESS_CODE"; // for lower body clothing

    // Save the current values from your SO
    public static void Save(PlayerCustomSO so)
    {
        PlayerPrefs.SetString(PLAYER_NAME, so.playerName);

        PlayerPrefs.SetString(SKIN_COLOR, ColorToHex(so.skinColor));

        PlayerPrefs.SetString(EYES_COLOR, ColorToHex(so.eyesColor));

        PlayerPrefs.SetString(HAIR_COLOR, ColorToHex(so.hairColor));
        PlayerPrefs.SetString(HAIR_CODE, so.hairCode);

        PlayerPrefs.SetString(DRESS_CODE, so.dressCode);
        PlayerPrefs.SetString(UPPERBODY_DRESS_CODE, so.upperDressCode);
        PlayerPrefs.SetString(LOWERBODY_DRESS_CODE, so.lowerDressCode);
        PlayerPrefs.Save();
    }

    // Load into your SO (keeps existing SO values as defaults if no keys yet)
    public static void LoadInto(PlayerCustomSO so)
    {
        if (PlayerPrefs.HasKey(PLAYER_NAME))
            so.playerName = PlayerPrefs.GetString(PLAYER_NAME);

        if (PlayerPrefs.HasKey(SKIN_COLOR))
            so.skinColor = HexToColor(PlayerPrefs.GetString(SKIN_COLOR));

        if (PlayerPrefs.HasKey(EYES_COLOR))
            so.eyesColor = HexToColor(PlayerPrefs.GetString(EYES_COLOR));

        if (PlayerPrefs.HasKey(HAIR_COLOR))
            so.hairColor = HexToColor(PlayerPrefs.GetString(HAIR_COLOR));
        if (PlayerPrefs.HasKey(HAIR_CODE))
            so.hairCode = PlayerPrefs.GetString(HAIR_CODE);
        
        if (PlayerPrefs.HasKey(DRESS_CODE))
            so.dressCode = PlayerPrefs.GetString(DRESS_CODE);
        if (PlayerPrefs.HasKey(UPPERBODY_DRESS_CODE))
            so.upperDressCode = PlayerPrefs.GetString(UPPERBODY_DRESS_CODE);
        if (PlayerPrefs.HasKey(LOWERBODY_DRESS_CODE))
            so.lowerDressCode = PlayerPrefs.GetString(LOWERBODY_DRESS_CODE);
    }

    public static void ClearAll()
    {
        PlayerPrefs.DeleteKey(PLAYER_NAME);

        PlayerPrefs.DeleteKey(SKIN_COLOR);

        PlayerPrefs.DeleteKey(EYES_COLOR);

        PlayerPrefs.DeleteKey(HAIR_COLOR);
        PlayerPrefs.DeleteKey(HAIR_CODE);
        
        PlayerPrefs.DeleteKey(DRESS_CODE);
        PlayerPrefs.DeleteKey(UPPERBODY_DRESS_CODE);
        PlayerPrefs.DeleteKey(LOWERBODY_DRESS_CODE);
        PlayerPrefs.Save();
    }

    // Helpers ---------------------------------------------------------
    private static string ColorToHex(Color c)
    {
        Color32 c32 = c;
        return "#" + ColorUtility.ToHtmlStringRGBA(c32);
    }

    private static Color HexToColor(string hex)
    {
        if (ColorUtility.TryParseHtmlString(hex, out var col))
            return col;
        return Color.white;
    }
}
