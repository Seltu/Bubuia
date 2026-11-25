using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CityNpc))]
public class CityNpcEditor : Editor
{
    private static readonly Color RouteColor = new Color(1f, 0.8f, 0.15f);

    private SerializedProperty _path;

    private void OnEnable()
    {
        _path = serializedObject.FindProperty("_homePath");
    }

    private void OnSceneGUI()
    {
        if (Application.isPlaying)
            return;

        serializedObject.Update();

        Vector3 origin = ((CityNpc)target).transform.position;
        bool removing = Event.current.shift;

        DrawPoints(origin, removing);
        DrawRoute(origin);

        if (!removing)
            DrawAddButtons(origin);

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawRoute(Vector3 origin)
    {
        using (new Handles.DrawingScope(RouteColor))
        {
            float labelOffset = HandleUtility.GetHandleSize(origin) * 0.12f;
            Handles.Label(origin + Vector3.up * labelOffset, "Trabalho");

            Vector3 previous = origin;

            for (int i = 0; i < _path.arraySize; i++)
            {
                Vector3 current = GetWorldPoint(i, origin);
                Handles.DrawAAPolyLine(3f, previous, current);
                previous = current;
            }
        }
    }

    private void DrawPoints(Vector3 origin, bool removing)
    {
        using (new Handles.DrawingScope(removing ? Color.red : Color.cyan))
        {
            for (int i = 0; i < _path.arraySize; i++)
            {
                SerializedProperty point = _path.GetArrayElementAtIndex(i);
                Vector3 position = GetWorldPoint(i, origin);
                float size = HandleUtility.GetHandleSize(position) * 0.08f;

                if (removing)
                {
                    if (SphereButton(position, size))
                    {
                        _path.DeleteArrayElementAtIndex(i);
                        ApplyAndExitGUI();
                        return;
                    }
                }
                else
                {
                    EditorGUI.BeginChangeCheck();

                    Vector3 newPosition = Handles.Slider2D(
                        position,
                        Vector3.up,       // Normal do plano
                        Vector3.right,    // Eixo X
                        Vector3.forward,  // Eixo Z
                        size,
                        Handles.SphereHandleCap,
                        Vector2.zero,
                        false
                    );

                    if (EditorGUI.EndChangeCheck())
                    {
                        Vector3 offset = newPosition - origin;
                        offset.y = 0f;
                        point.vector3Value = offset;
                        position = origin + offset;
                    }
                }

                string label = i == _path.arraySize - 1 ? "Casa" : $"{i + 1}";
                Handles.Label(position + Vector3.up * size * 2f, label);
            }
        }
    }

    private void DrawAddButtons(Vector3 origin)
    {
        using (new Handles.DrawingScope(Color.green))
        {
            if (_path.arraySize == 0)
            {
                Vector3 position = origin + Vector3.forward * 2f;
                float size = HandleUtility.GetHandleSize(position) * 0.08f;

                Handles.Label(position + Vector3.up * size * 2f, "Criar rota");

                if (SphereButton(position, size))
                    InsertPoint(0, position - origin);

                return;
            }

            Vector3 previous = origin;

            for (int i = 0; i < _path.arraySize; i++)
            {
                Vector3 current = GetWorldPoint(i, origin);
                Vector3 midpoint = (previous + current) * 0.5f;
                float size = HandleUtility.GetHandleSize(midpoint) * 0.06f;

                if (SphereButton(midpoint, size))
                {
                    InsertPoint(i, midpoint - origin);
                    return;
                }

                previous = current;
            }
        }
    }

    private Vector3 GetWorldPoint(int index, Vector3 origin)
    {
        Vector3 offset = _path.GetArrayElementAtIndex(index).vector3Value;
        offset.y = 0f;
        return origin + offset;
    }

    private static bool SphereButton(Vector3 position, float size)
    {
        return Handles.Button(
            position,
            Quaternion.identity,
            size,
            size,
            Handles.SphereHandleCap
        );
    }

    private void InsertPoint(int index, Vector3 offset)
    {
        if (index == _path.arraySize)
            _path.arraySize++;
        else
            _path.InsertArrayElementAtIndex(index);

        _path.GetArrayElementAtIndex(index).vector3Value = offset;
        ApplyAndExitGUI();
    }

    private void ApplyAndExitGUI()
    {
        serializedObject.ApplyModifiedProperties();
        SceneView.RepaintAll();
        GUIUtility.ExitGUI();
    }
}