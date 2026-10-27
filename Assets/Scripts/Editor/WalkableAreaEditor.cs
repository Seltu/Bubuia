using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[CustomEditor(typeof(WalkableArea))]
public class WalkableAreaEditor : Editor
{
    private WalkableArea _area;

    private void OnEnable()
    {
        _area = (WalkableArea)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();

        // Live Edit
        bool liveEdit = WalkableAreaLiveEdit.Enabled;

        Color previousColor = GUI.backgroundColor;

        GUI.backgroundColor = liveEdit
            ? new Color(0.4f, 1f, 0.5f)
            : Color.white;

        if (GUILayout.Button(
            liveEdit ? "Live Edit: ON" : "Live Edit: OFF",
            GUILayout.Height(30)))
        {
            WalkableAreaLiveEdit.Enabled = !liveEdit;
        }

        GUI.backgroundColor = previousColor;

        EditorGUILayout.Space();

        if (GUILayout.Button("Add Point"))
        {
            Undo.RecordObject(_area, "Add Walkable Point");

            Vector2 point = _area.Points.Count > 0
                ? _area.Points[_area.Points.Count - 1] + Vector2.right
                : Vector2.zero;

            _area.Points.Add(point);

            EditorUtility.SetDirty(_area);
            SceneView.RepaintAll();
        }

        if (GUILayout.Button("Remove Last Point"))
        {
            if (_area.Points.Count > 3)
            {
                Undo.RecordObject(_area, "Remove Walkable Point");

                _area.Points.RemoveAt(_area.Points.Count - 1);

                EditorUtility.SetDirty(_area);
                SceneView.RepaintAll();
            }
        }

        EditorGUILayout.HelpBox(
            "Drag the blue handles to move points.\n" +
            "Click the green handles to add points.\n" +
            "Shift + Click a blue handle to remove it.",
            MessageType.Info
        );

        if (EditorApplication.isPlaying && liveEdit)
        {
            EditorGUILayout.HelpBox(
                "Live Edit enabled. Changes will be preserved when Play Mode ends.",
                MessageType.Info
            );
        }
    }

    private void OnSceneGUI()
    {
        if (_area.Points == null || _area.Points.Count < 3)
            return;

        DrawEdges();
        DrawPoints();
        DrawAddButtons();
    }

    private void DrawEdges()
    {
        Handles.color = Color.cyan;

        for (int i = 0; i < _area.Points.Count; i++)
        {
            Vector3 a = _area.PointToWorld(_area.Points[i]);

            Vector3 b = _area.PointToWorld(
                _area.Points[(i + 1) % _area.Points.Count]
            );

            Handles.DrawAAPolyLine(3f, a, b);
        }
    }

    private void DrawPoints()
    {
        for (int i = 0; i < _area.Points.Count; i++)
        {
            Vector3 position = _area.PointToWorld(_area.Points[i]);

            float size = HandleUtility.GetHandleSize(position) * 0.08f;

            Handles.color = Color.cyan;

            if (Event.current.shift)
            {
                if (Handles.Button(
                    position,
                    Quaternion.identity,
                    size,
                    size,
                    Handles.SphereHandleCap))
                {
                    if (_area.Points.Count > 3)
                    {
                        Undo.RecordObject(_area, "Remove Walkable Point");

                        _area.Points.RemoveAt(i);

                        EditorUtility.SetDirty(_area);
                        GUIUtility.ExitGUI();
                    }
                }

                continue;
            }

            EditorGUI.BeginChangeCheck();

            Vector3 newPosition = Handles.FreeMoveHandle(
                position,
                size,
                Vector3.zero,
                Handles.SphereHandleCap
            );

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_area, "Move Walkable Point");

                _area.Points[i] = _area.WorldToPoint(newPosition);

                EditorUtility.SetDirty(_area);
            }

            Handles.Label(
                position + Vector3.up * size * 2f,
                i.ToString()
            );
        }
    }

    private void DrawAddButtons()
    {
        if (Event.current.shift)
            return;

        for (int i = 0; i < _area.Points.Count; i++)
        {
            int next = (i + 1) % _area.Points.Count;

            Vector2 midpoint = (
                _area.Points[i] + _area.Points[next]
            ) * 0.5f;

            Vector3 position = _area.PointToWorld(midpoint);

            float size = HandleUtility.GetHandleSize(position) * 0.06f;

            Handles.color = Color.green;

            if (Handles.Button(
                position,
                Quaternion.identity,
                size,
                size,
                Handles.SphereHandleCap))
            {
                Undo.RecordObject(_area, "Insert Walkable Point");

                _area.Points.Insert(next, midpoint);

                EditorUtility.SetDirty(_area);
                GUIUtility.ExitGUI();
            }
        }
    }
}

[InitializeOnLoad]
public static class WalkableAreaLiveEdit
{
    private const string ENABLED_KEY = "WalkableArea_LiveEdit";
    private const string DATA_KEY = "WalkableArea_LiveEdit_Data";

    public static bool Enabled
    {
        get => EditorPrefs.GetBool(ENABLED_KEY, false);
        set => EditorPrefs.SetBool(ENABLED_KEY, value);
    }

    [Serializable]
    private class AreaData
    {
        public string globalId;
        public List<Vector2> points;
    }

    [Serializable]
    private class SaveData
    {
        public List<AreaData> areas = new();
    }

    static WalkableAreaLiveEdit()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode && Enabled)
        {
            SaveChanges();
        }
        else if (state == PlayModeStateChange.EnteredEditMode)
        {
            RestoreChanges();
        }
    }

    private static void SaveChanges()
    {
        SaveData data = new SaveData();

        WalkableArea[] areas =
            Resources.FindObjectsOfTypeAll<WalkableArea>();

        foreach (WalkableArea area in areas)
        {
            if (!area.gameObject.scene.IsValid())
                continue;

            // Ignora objetos criados somente durante o Play Mode
            // e objetos que não pertencem a uma cena salva.
            if (string.IsNullOrEmpty(area.gameObject.scene.path))
                continue;

            GlobalObjectId id = GlobalObjectId.GetGlobalObjectIdSlow(area);

            AreaData areaData = new AreaData
            {
                globalId = id.ToString(),
                points = new List<Vector2>(area.Points)
            };

            data.areas.Add(areaData);
        }

        SessionState.SetString(DATA_KEY, JsonUtility.ToJson(data));
    }

    private static void RestoreChanges()
    {
        string json = SessionState.GetString(DATA_KEY, "");

        if (string.IsNullOrEmpty(json))
            return;

        SessionState.SetString(DATA_KEY, "");

        SaveData data = JsonUtility.FromJson<SaveData>(json);

        if (data == null || data.areas == null)
            return;

        foreach (AreaData areaData in data.areas)
        {
            if (!GlobalObjectId.TryParse(
                areaData.globalId,
                out GlobalObjectId id))
                continue;

            WalkableArea area =
                GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id)
                as WalkableArea;

            if (area == null)
                continue;

            Undo.RecordObject(area, "Apply Walkable Area Live Edit");

            area.Points.Clear();
            area.Points.AddRange(areaData.points);

            EditorUtility.SetDirty(area);

            EditorSceneManager.MarkSceneDirty(area.gameObject.scene);
        }

        SceneView.RepaintAll();
    }
}