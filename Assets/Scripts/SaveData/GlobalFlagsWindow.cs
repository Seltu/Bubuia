using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
public class GlobalFlagsWindow : EditorWindow
{
    [MenuItem("Window/Global Flags")]
    public static void ShowWindow()
    {
        GetWindow<GlobalFlagsWindow>("Global Flags");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Global Flag List", EditorStyles.boldLabel);

        foreach (GlobalFlag flag in GlobalFlagsManager.GetAllFlags())
        {
            bool newValue = EditorGUILayout.Toggle(flag.id, flag.value);

            if (newValue != flag.value)
            {
                GlobalFlagsManager.SetFlag(flag.id, newValue);
            }
        }

        if (GUILayout.Button("Clear Global Flags"))
        {
            GlobalFlagsManager.DeleteSave();
        }
    }
}

#endif