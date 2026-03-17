#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public class RockVariantCreator : EditorWindow
{
    [Header("Inputs")]
    [SerializeField] private GameObject baseRockPrefab;     // Project prefab (Rock)
    [SerializeField] private Transform rockPresetRoot;      // Scene object (Rock_preset)

    [Header("Output")]
    [SerializeField] private DefaultAsset outputFolder;     // Folder in Project view

    [Header("Options")]
    [SerializeField] private string modelRootName = "Model";     // Where the mesh will be placed
    [SerializeField] private bool replaceExistingModelRoot = true;
    [SerializeField] private bool unpackPrefabChildren = true;   // If preset children are prefabs, unpack them
    [SerializeField] private bool copyWorldTransforms = false;   // Usually false for prefab content

    [MenuItem("Tools/Rocks/Create Rock Prefab Variants...")]
    public static void Open()
    {
        var w = GetWindow<RockVariantCreator>("Rock Variant Creator");
        w.minSize = new Vector2(420, 260);
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Rock Variant Generator", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Creates prefab variants from Base Rock prefab and attaches each child of Rock_preset as the model.",
            MessageType.Info);

        EditorGUILayout.Space(8);

        baseRockPrefab = (GameObject)EditorGUILayout.ObjectField("Base Rock Prefab", baseRockPrefab, typeof(GameObject), false);
        rockPresetRoot = (Transform)EditorGUILayout.ObjectField("Rock_preset (Scene)", rockPresetRoot, typeof(Transform), true);
        outputFolder = (DefaultAsset)EditorGUILayout.ObjectField("Output Folder", outputFolder, typeof(DefaultAsset), false);

        EditorGUILayout.Space(6);
        modelRootName = EditorGUILayout.TextField("Model Root Name", modelRootName);
        replaceExistingModelRoot = EditorGUILayout.Toggle("Replace Existing Model Root", replaceExistingModelRoot);
        unpackPrefabChildren = EditorGUILayout.Toggle("Unpack Preset Child Prefabs", unpackPrefabChildren);
        copyWorldTransforms = EditorGUILayout.Toggle("Copy World Transforms", copyWorldTransforms);

        EditorGUILayout.Space(12);

        GUI.enabled = baseRockPrefab != null && rockPresetRoot != null && outputFolder != null;
        if (GUILayout.Button("Generate Variants", GUILayout.Height(32)))
        {
            Generate();
        }
        GUI.enabled = true;
    }

    private void Generate()
    {
        string outputPath = AssetDatabase.GetAssetPath(outputFolder);
        if (string.IsNullOrEmpty(outputPath) || !AssetDatabase.IsValidFolder(outputPath))
        {
            Debug.LogError("Output folder is invalid.");
            return;
        }

        if (!PrefabUtility.IsPartOfPrefabAsset(baseRockPrefab))
        {
            Debug.LogError("Base Rock Prefab must be a prefab asset from the Project, not a scene object.");
            return;
        }

        if (rockPresetRoot.childCount == 0)
        {
            Debug.LogWarning("Rock_preset has no children.");
            return;
        }

        int created = 0;

        try
        {
            AssetDatabase.StartAssetEditing();

            for (int i = 0; i < rockPresetRoot.childCount; i++)
            {
                Transform presetChild = rockPresetRoot.GetChild(i);
                if (presetChild == null) continue;

                // Instantiate base prefab in a temp scene context
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(baseRockPrefab);
                instance.name = $"Rock_{i.ToString()}";

                // Find/Create model root inside instance
                Transform modelRoot = instance.transform.Find(modelRootName);

                if (modelRoot != null && replaceExistingModelRoot)
                {
                    DestroyImmediate(modelRoot.gameObject);
                    modelRoot = null;
                }

                if (modelRoot == null)
                {
                    var mr = new GameObject(modelRootName);
                    mr.transform.SetParent(instance.transform, false);
                    modelRoot = mr.transform;
                }

                // Duplicate preset child under model root
                GameObject duplicatedModel = Instantiate(presetChild.gameObject);
                duplicatedModel.name = presetChild.name;

                // Optional: if the preset child is itself a prefab instance, you may want to unpack
                if (unpackPrefabChildren)
                {
                    var status = PrefabUtility.GetPrefabInstanceStatus(duplicatedModel);
                    if (status == PrefabInstanceStatus.Connected || status == PrefabInstanceStatus.Disconnected)
                    {
                        PrefabUtility.UnpackPrefabInstance(duplicatedModel, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                    }
                }

                // Parent it
                duplicatedModel.transform.SetParent(modelRoot, false);

                if (copyWorldTransforms)
                {
                    // preserve world pose (rarely needed for prefab content)
                    duplicatedModel.transform.position = presetChild.position;
                    duplicatedModel.transform.rotation = presetChild.rotation;
                    duplicatedModel.transform.localScale = presetChild.lossyScale;
                }
                else
                {
                    // typical: keep local pose relative to parent
                    duplicatedModel.transform.localPosition = Vector3.zero;
                    duplicatedModel.transform.localRotation = presetChild.localRotation;
                    duplicatedModel.transform.localScale = presetChild.localScale;
                }

                // Save as a *prefab variant* (so it stays linked to the base prefab)
                string safeName = MakeFileSafe(instance.name);
                string prefabPath = Path.Combine(outputPath, $"{safeName}.prefab").Replace("\\", "/");

                // Ensure unique if already exists
                prefabPath = AssetDatabase.GenerateUniqueAssetPath(prefabPath);

                PrefabUtility.SaveAsPrefabAssetAndConnect(instance, prefabPath, InteractionMode.AutomatedAction);

                // Cleanup temp instance from scene
                DestroyImmediate(instance);

                created++;
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        Debug.Log($"✅ Rock variants created: {created} (saved to {AssetDatabase.GetAssetPath(outputFolder)})");
    }

    private static string MakeFileSafe(string name)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
            name = name.Replace(c.ToString(), "_");
        return name.Trim();
    }
}
#endif