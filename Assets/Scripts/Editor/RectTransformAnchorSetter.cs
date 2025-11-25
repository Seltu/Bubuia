using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR
public class UGUIAnchorAroundObjectEditorWindow : EditorWindow
{
    private RectTransform selectedRectTransform;

    [MenuItem("Window/Anchor Around Object")]
    public static void ShowWindow()
    {
        GetWindow<UGUIAnchorAroundObjectEditorWindow>("Anchor Around Object");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Select a RectTransform", EditorStyles.boldLabel);
        selectedRectTransform = EditorGUILayout.ObjectField("RectTransform", selectedRectTransform, typeof(RectTransform), true) as RectTransform;

        if (selectedRectTransform == null)
        {
            EditorGUILayout.HelpBox("Please select a RectTransform to anchor.", MessageType.Warning);
        }

        if (GUILayout.Button("Anchor Around Object") && selectedRectTransform != null)
        {
            AnchorAroundObject(selectedRectTransform);
        }
    }

    private void AnchorAroundObject(RectTransform r)
    {
        if (r != null)
        {
            var p = r.parent.GetComponent<RectTransform>();

            var offsetMin = r.offsetMin;
            var offsetMax = r.offsetMax;
            var _anchorMin = r.anchorMin;
            var _anchorMax = r.anchorMax;

            var parent_width = p.rect.width;
            var parent_height = p.rect.height;

            var anchorMin = new Vector2(_anchorMin.x + (offsetMin.x / parent_width),
                                        _anchorMin.y + (offsetMin.y / parent_height));
            var anchorMax = new Vector2(_anchorMax.x + (offsetMax.x / parent_width),
                                        _anchorMax.y + (offsetMax.y / parent_height));

            r.anchorMin = anchorMin;
            r.anchorMax = anchorMax;

            r.offsetMin = Vector2.zero;
            r.offsetMax = Vector2.zero;
            r.pivot = new Vector2(0.5f, 0.5f);
        }
    }
}
#endif