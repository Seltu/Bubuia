using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public sealed class SpriteShadowSetupTool : EditorWindow
{
    private GameObject _root;
    private Material _material;

    [MenuItem("Tools/Apply Sprite Shadow")]
    private static void Open()
    {
        var window = GetWindow<SpriteShadowSetupTool>("Sombras dos Sprites");
        window._root = Selection.activeGameObject;
        window.Show();
    }

    private void OnGUI()
    {
        _root = (GameObject)EditorGUILayout.ObjectField(
            "Raiz do personagem", _root, typeof(GameObject), true);

        _material = (Material)EditorGUILayout.ObjectField(
            "Material novo", _material, typeof(Material), false);

        bool isPrefabAsset = _root != null && EditorUtility.IsPersistent(_root);

        if (isPrefabAsset)
            EditorGUILayout.HelpBox(
                "Abra o prefab em Prefab Mode para editar o asset.",
                MessageType.Info);

        using (new EditorGUI.DisabledScope(
            _root == null || _material == null || isPrefabAsset ||
            EditorApplication.isPlayingOrWillChangePlaymode))
        {
            if (GUILayout.Button("Aplicar material e sombras"))
                Apply();
        }
    }

    private void Apply()
    {
        var sprites = _root.GetComponentsInChildren<SpriteRenderer>(true);
        if (sprites.Length == 0)
        {
            ShowNotification(new GUIContent("Nenhum SpriteRenderer encontrado."));
            return;
        }

        Undo.RecordObjects(sprites, "Configurar Sombras dos Sprites");

        foreach (var sprite in sprites)
        {
            sprite.sharedMaterial = _material;
            sprite.shadowCastingMode = ShadowCastingMode.TwoSided;

            if (PrefabUtility.IsPartOfPrefabInstance(sprite))
                PrefabUtility.RecordPrefabInstancePropertyModifications(sprite);
        }

        ShowNotification(new GUIContent($"{sprites.Length} SpriteRenderer(s) configurados."));
    }
}