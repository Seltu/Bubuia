using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class GlobalFlagsManager
{
    private static Dictionary<string, bool> globalFlags;
    private static string SavePath => Path.Combine(Application.persistentDataPath, "global_flags.json");

    public static bool GetFlag(string id)
    {
        if(globalFlags.ContainsKey(id))
            return globalFlags[id]; 
        else
            return false;
    }

    public static void SetFlag(string id, bool value)
    {
        if (globalFlags.ContainsKey(id))
            globalFlags[id] = value;
        else
            globalFlags.Add(id, value);
        SaveFlags();
    }

    public static void SaveFlags()
    {
        GlobalFlagsSaveData data = new GlobalFlagsSaveData();

        foreach (var pair in globalFlags)
        {
            GlobalFlag flag = new GlobalFlag();
            flag.id = pair.Key;
            flag.value = pair.Value;
            data.entries.Add(flag);
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
    }

    public static void LoadFlags()
    {
        if (!File.Exists(SavePath)) return;

        string json = File.ReadAllText(SavePath);
        GlobalFlagsSaveData data = JsonUtility.FromJson<GlobalFlagsSaveData>(json);

        if (data == null || data.entries == null) return;

        foreach (var entry in data.entries)
        {
            if (string.IsNullOrWhiteSpace(entry.id))
                continue;

            if (globalFlags.ContainsKey(entry.id))
                globalFlags[entry.id] = entry.value;
            else
                globalFlags.Add(entry.id, entry.value);
        }
    }

    public static void DeleteSave()
    {
        if (File.Exists(SavePath))
            File.Delete(SavePath);
    }
}
