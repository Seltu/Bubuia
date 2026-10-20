using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class MeshVegetationPainterWindow : EditorWindow
{
    private const string WINDOW_TITLE = "Mesh Vegetation Painter";
    private const string VEGETATION_ROOT_NAME = "PaintedVegetation";
    private const string UNDO_NAME = "Paint Mesh Vegetation";
    private const float RAYCAST_MARGIN = 10f;

    private readonly System.Random _random = new System.Random();

    private MeshCollider _surface;
    private GameObject _prefab;
    private float _brushRadius = 3f;
    private float _minimumSpacing = 1f;
    private int _objectsPerDab = 5;
    private float _minimumScale = 0.8f;
    private float _maximumScale = 1.2f;
    private float _maximumSlope = 45f;
    private float _surfaceOffset;
    private bool _alignToSurface;
    private bool _isPainting;
    private Vector3 _lastStrokePosition;
    private bool _hasStrokePosition;
    private int _activeControlId;
    private int _undoGroup = -1;
    private Vector2 _scrollPosition;

    [MenuItem("Tools/Level Design/Mesh Vegetation Painter")]
    private static void OpenWindow()
    {
        MeshVegetationPainterWindow Window =
            GetWindow<MeshVegetationPainterWindow>(WINDOW_TITLE);
        Window.minSize = new Vector2(360f, 440f);
        Window.Show();
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
        Undo.undoRedoPerformed += OnUndoRedo;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        Undo.undoRedoPerformed -= OnUndoRedo;
        EndStroke();
        SceneView.RepaintAll();
    }

    private void OnUndoRedo()
    {
        Repaint();
        SceneView.RepaintAll();
    }

    private void OnGUI()
    {
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
        EditorGUILayout.HelpBox(
            "Assign the island's Land object to Surface and a vegetation prefab " +
            "from Project to Prefab. Enable painting, then drag over the mesh in Scene view. " +
            "Works in a scene or in Prefab Mode.", MessageType.Info);

        EditorGUI.BeginChangeCheck();
        _surface = (MeshCollider)EditorGUILayout.ObjectField(
            "Surface", _surface, typeof(MeshCollider), true);
        _prefab = (GameObject)EditorGUILayout.ObjectField(
            "Prefab", _prefab, typeof(GameObject), false);

        EditorGUILayout.Space();
        _brushRadius = Mathf.Max(0.1f, EditorGUILayout.FloatField("Brush Radius", _brushRadius));
        _objectsPerDab = EditorGUILayout.IntSlider("Objects Per Dab", _objectsPerDab, 1, 50);
        _minimumSpacing = Mathf.Max(0.05f,
            EditorGUILayout.FloatField("Minimum Spacing", _minimumSpacing));
        _minimumScale = Mathf.Max(0.01f,
            EditorGUILayout.FloatField("Minimum Scale", _minimumScale));
        _maximumScale = Mathf.Max(_minimumScale,
            EditorGUILayout.FloatField("Maximum Scale", _maximumScale));
        _maximumSlope = EditorGUILayout.Slider("Maximum Slope", _maximumSlope, 0f, 90f);
        _surfaceOffset = EditorGUILayout.FloatField("Surface Offset", _surfaceOffset);
        _alignToSurface = EditorGUILayout.Toggle("Align To Surface", _alignToSurface);

        if (EditorGUI.EndChangeCheck())
        {
            EndStroke();
            SceneView.RepaintAll();
        }

        string ValidationMessage = GetValidationMessage();
        bool CanPaint = string.IsNullOrEmpty(ValidationMessage);
        if (!CanPaint)
        {
            _isPainting = false;
            EndStroke();
            EditorGUILayout.HelpBox(ValidationMessage, MessageType.Warning);
        }

        EditorGUILayout.Space();
        using (new EditorGUI.DisabledScope(!CanPaint))
        {
            bool EnablePainting = GUILayout.Toggle(_isPainting, "Enable Painting", "Button");
            if (EnablePainting != _isPainting)
            {
                _isPainting = EnablePainting;
                EndStroke();
                SceneView.RepaintAll();
            }
        }

        EditorGUILayout.HelpBox(
            "Left drag: paint. Shift + left drag: erase the selected prefab. " +
            "Ctrl+Z: undo a stroke. Escape: stop painting. Alt: orbit the camera.\n" +
            "Vegetation is grouped under Surface/PaintedVegetation. " +
            "Save the scene or prefab after painting.", MessageType.Info);
        EditorGUILayout.EndScrollView();
    }

    private string GetValidationMessage()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return "Exit Play Mode before painting.";
        }

        if (_surface == null)
        {
            return "Assign the MeshCollider on the island's Land object to Surface.";
        }

        if (EditorUtility.IsPersistent(_surface) || !_surface.gameObject.scene.IsValid())
        {
            return "Use a surface from the Hierarchy. Open the island in Prefab Mode to edit its prefab.";
        }

        if (!StageUtility.GetStageHandle(_surface.gameObject).Equals(StageUtility.GetCurrentStageHandle()))
        {
            return "Assign a surface from the currently open scene or Prefab Mode.";
        }

        if (!_surface.enabled || !_surface.gameObject.activeInHierarchy || _surface.sharedMesh == null)
        {
            return "The surface needs an active MeshCollider with a mesh assigned.";
        }

        if (_surface.convex)
        {
            return "Disable Convex on the surface MeshCollider so painting follows the island mesh.";
        }

        if (_prefab == null || !PrefabUtility.IsPartOfPrefabAsset(_prefab) || _prefab.transform.parent != null)
        {
            return "Assign a vegetation prefab root from the Project window.";
        }

        if (AssetDatabase.GetAssetPath(_prefab) == PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(_surface))
        {
            return "Choose a vegetation prefab instead of the surface's own prefab.";
        }

        PrefabStage CurrentPrefabStage = PrefabStageUtility.GetCurrentPrefabStage();
        if (CurrentPrefabStage != null && CurrentPrefabStage.assetPath == AssetDatabase.GetAssetPath(_prefab))
        {
            return "A prefab cannot be painted into itself.";
        }

        return string.Empty;
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        Event CurrentEvent = Event.current;
        if (CurrentEvent.rawType == UnityEngine.EventType.MouseUp && _activeControlId != 0)
        {
            EndStroke();
        }

        if (!_isPainting)
        {
            return;
        }

        if (CurrentEvent.type == UnityEngine.EventType.KeyDown && CurrentEvent.keyCode == KeyCode.Escape)
        {
            _isPainting = false;
            EndStroke();
            CurrentEvent.Use();
            Repaint();
            sceneView.Repaint();
            return;
        }

        if (!string.IsNullOrEmpty(GetValidationMessage()))
        {
            EndStroke();
            return;
        }

        if (CurrentEvent.alt || Tools.viewToolActive)
        {
            return;
        }

        int ControlId = GUIUtility.GetControlID(FocusType.Passive);
        if (CurrentEvent.type == UnityEngine.EventType.Layout)
        {
            HandleUtility.AddDefaultControl(ControlId);
        }

        Ray PointerRay = HandleUtility.GUIPointToWorldRay(CurrentEvent.mousePosition);
        bool HasSurfaceHit = _surface.Raycast(PointerRay, out RaycastHit SurfaceHit, Mathf.Infinity);
        if (HasSurfaceHit && CurrentEvent.type == UnityEngine.EventType.Repaint)
        {
            Color BrushColor = Color.green;
            if (CurrentEvent.shift)
            {
                BrushColor = Color.red;
            }
            else if (Vector3.Angle(Vector3.up, SurfaceHit.normal) > _maximumSlope)
            {
                BrushColor = Color.yellow;
            }

            using (new Handles.DrawingScope(BrushColor))
            {
                Handles.DrawWireDisc(SurfaceHit.point, Vector3.up, _brushRadius);
                Handles.DrawLine(SurfaceHit.point, SurfaceHit.point + SurfaceHit.normal);
            }
        }

        if (CurrentEvent.type == UnityEngine.EventType.MouseMove)
        {
            sceneView.Repaint();
        }

        if (CurrentEvent.button != 0)
        {
            return;
        }

        if (CurrentEvent.type == UnityEngine.EventType.MouseDown && HasSurfaceHit && HandleUtility.nearestControl == ControlId)
        {
            Undo.IncrementCurrentGroup();
            _undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName(UNDO_NAME);
            _activeControlId = ControlId;
            GUIUtility.hotControl = ControlId;
            _hasStrokePosition = false;
            ApplyBrush(SurfaceHit, CurrentEvent.shift);
            CurrentEvent.Use();
        }
        else if (CurrentEvent.type == UnityEngine.EventType.MouseDrag &&
                 _activeControlId != 0 && GUIUtility.hotControl == _activeControlId)
        {
            if (HasSurfaceHit)
            {
                ApplyBrush(SurfaceHit, CurrentEvent.shift);
            }

            CurrentEvent.Use();
        }
    }

    private void ApplyBrush(RaycastHit surfaceHit, bool erase)
    {
        float StrokeSpacing = Mathf.Max(0.05f, Mathf.Min(_brushRadius * 0.2f, _minimumSpacing));
        if (_hasStrokePosition &&
            (surfaceHit.point - _lastStrokePosition).sqrMagnitude < StrokeSpacing * StrokeSpacing)
        {
            return;
        }

        _lastStrokePosition = surfaceHit.point;
        _hasStrokePosition = true;

        if (erase)
        {
            EraseVegetation(surfaceHit.point);
        }
        else
        {
            PaintVegetation(surfaceHit);
        }

        SceneView.RepaintAll();
    }

    private void PaintVegetation(RaycastHit surfaceHit)
    {
        Transform VegetationRoot = _surface.transform.Find(VEGETATION_ROOT_NAME);
        Bounds SurfaceBounds = _surface.bounds;
        float MinimumUpwardNormal = Mathf.Cos(_maximumSlope * Mathf.Deg2Rad);

        for (int i = 0; i < _objectsPerDab; i++)
        {
            Vector3 CandidatePosition = surfaceHit.point;
            if (i > 0)
            {
                float Angle = (float)_random.NextDouble() * Mathf.PI * 2f;
                float Radius = Mathf.Sqrt((float)_random.NextDouble()) * _brushRadius;
                CandidatePosition.x += Mathf.Cos(Angle) * Radius;
                CandidatePosition.z += Mathf.Sin(Angle) * Radius;
            }

            Vector3 RayOrigin = new Vector3(
                CandidatePosition.x, SurfaceBounds.max.y + RAYCAST_MARGIN, CandidatePosition.z);
            Ray PlacementRay = new Ray(RayOrigin, Vector3.down);
            if (!_surface.Raycast(PlacementRay, out RaycastHit PlacementHit,
                    SurfaceBounds.size.y + RAYCAST_MARGIN * 2f))
            {
                continue;
            }

            if (Vector3.Dot(Vector3.up, PlacementHit.normal) < MinimumUpwardNormal ||
                (PlacementHit.point - surfaceHit.point).sqrMagnitude > _brushRadius * _brushRadius)
            {
                continue;
            }

            Vector3 PlantPosition = PlacementHit.point + PlacementHit.normal * _surfaceOffset;
            if (!HasEnoughSpacing(VegetationRoot, PlantPosition))
            {
                continue;
            }

            if (VegetationRoot == null)
            {
                GameObject RootObject = new GameObject(VEGETATION_ROOT_NAME);
                SceneManager.MoveGameObjectToScene(RootObject, _surface.gameObject.scene);
                Undo.RegisterCreatedObjectUndo(RootObject, UNDO_NAME);
                VegetationRoot = RootObject.transform;
                Undo.SetTransformParent(VegetationRoot, _surface.transform, UNDO_NAME);
                Undo.RecordObject(VegetationRoot, UNDO_NAME);
                VegetationRoot.localPosition = Vector3.zero;
                VegetationRoot.localRotation = Quaternion.identity;
                VegetationRoot.localScale = Vector3.one;
            }

            GameObject Instance = (GameObject)PrefabUtility.InstantiatePrefab(
                _prefab, _surface.gameObject.scene);
            Undo.RegisterCreatedObjectUndo(Instance, UNDO_NAME);
            Transform InstanceTransform = Instance.transform;
            Undo.SetTransformParent(InstanceTransform, VegetationRoot, UNDO_NAME);
            Undo.RecordObject(InstanceTransform, UNDO_NAME);

            Quaternion Alignment = Quaternion.identity;
            if (_alignToSurface)
            {
                Alignment = Quaternion.FromToRotation(Vector3.up, PlacementHit.normal);
            }

            float RotationAngle = (float)_random.NextDouble() * 360f;
            Quaternion PlantRotation = Alignment * Quaternion.AngleAxis(RotationAngle, Vector3.up) *
                _prefab.transform.localRotation;
            InstanceTransform.SetPositionAndRotation(PlantPosition, PlantRotation);
            float ScaleMultiplier = Mathf.Lerp(_minimumScale, _maximumScale, (float)_random.NextDouble());
            InstanceTransform.localScale *= ScaleMultiplier;
            PrefabUtility.RecordPrefabInstancePropertyModifications(InstanceTransform);
            EditorSceneManager.MarkSceneDirty(_surface.gameObject.scene);
        }
    }

    private bool HasEnoughSpacing(Transform vegetationRoot, Vector3 plantPosition)
    {
        if (vegetationRoot == null)
        {
            return true;
        }

        float MinimumSquaredSpacing = _minimumSpacing * _minimumSpacing;
        for (int i = 0; i < vegetationRoot.childCount; i++)
        {
            Transform ExistingPlant = vegetationRoot.GetChild(i);
            if (PrefabUtility.GetCorrespondingObjectFromSource(ExistingPlant.gameObject) != _prefab)
            {
                continue;
            }

            if ((ExistingPlant.position - plantPosition).sqrMagnitude < MinimumSquaredSpacing)
            {
                return false;
            }
        }

        return true;
    }

    private void EraseVegetation(Vector3 brushPosition)
    {
        Transform VegetationRoot = _surface.transform.Find(VEGETATION_ROOT_NAME);
        if (VegetationRoot == null)
        {
            return;
        }

        float SquaredBrushRadius = _brushRadius * _brushRadius;
        for (int i = VegetationRoot.childCount - 1; i >= 0; i--)
        {
            Transform ExistingPlant = VegetationRoot.GetChild(i);
            if (PrefabUtility.GetCorrespondingObjectFromSource(ExistingPlant.gameObject) == _prefab &&
                (ExistingPlant.position - brushPosition).sqrMagnitude <= SquaredBrushRadius)
            {
                Undo.DestroyObjectImmediate(ExistingPlant.gameObject);
                EditorSceneManager.MarkSceneDirty(_surface.gameObject.scene);
            }
        }
    }

    private void EndStroke()
    {
        if (_activeControlId != 0 && GUIUtility.hotControl == _activeControlId)
        {
            GUIUtility.hotControl = 0;
        }

        _activeControlId = 0;
        _hasStrokePosition = false;
        if (_undoGroup >= 0)
        {
            Undo.FlushUndoRecordObjects();
            Undo.CollapseUndoOperations(_undoGroup);
            _undoGroup = -1;
        }
    }
}
