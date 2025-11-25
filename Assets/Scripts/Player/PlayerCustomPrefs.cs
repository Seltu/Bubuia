using UnityEngine;

public static class PlayerCustomPrefs
{
    private const string SKIN_COLOR = "PC_SKIN";
    private const string HAIR_COLOR = "PC_HAIR";
    private const string HAIR_CODE = "PC_HAIR_CODE";
    private const string DRESS_CODE = "PC_DRESS_CODE";

    // Save the current values from your SO
    public static void Save(PlayerCustomSO so)
    {
        PlayerPrefs.SetString(SKIN_COLOR, ColorToHex(so.skinColor));
        PlayerPrefs.SetString(HAIR_COLOR, ColorToHex(so.hairColor));
        PlayerPrefs.SetInt(HAIR_CODE, so.hairCode);
        PlayerPrefs.SetInt(DRESS_CODE, so.dressCode);
        PlayerPrefs.Save();
    }

    // Load into your SO (keeps existing SO values as defaults if no keys yet)
    public static void LoadInto(PlayerCustomSO so)
    {
        if (PlayerPrefs.HasKey(SKIN_COLOR))
            so.skinColor = HexToColor(PlayerPrefs.GetString(SKIN_COLOR));
        if (PlayerPrefs.HasKey(HAIR_COLOR))
            so.hairColor = HexToColor(PlayerPrefs.GetString(HAIR_COLOR));
        if (PlayerPrefs.HasKey(HAIR_CODE))
            so.hairCode = PlayerPrefs.GetInt(HAIR_CODE);
        if (PlayerPrefs.HasKey(DRESS_CODE))
            so.dressCode = PlayerPrefs.GetInt(DRESS_CODE);
    }

    public static void ClearAll()
    {
        PlayerPrefs.DeleteKey(SKIN_COLOR);
        PlayerPrefs.DeleteKey(HAIR_COLOR);
        PlayerPrefs.DeleteKey(HAIR_CODE);
        PlayerPrefs.DeleteKey(DRESS_CODE);
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
