using System.Collections;
using System.Collections.Generic;
using UnityEditor;

//using UnityEditor.MemoryProfiler;
using UnityEngine;
using XNode;
//using static UnityEngine.GraphicsBuffer;
//using static XNodeEditor.NodeEditor;

//using UnityEngine.Serialization;
//using UnityEngine.Events;
#if UNITY_EDITOR
using XNodeEditor;
#endif

[NodeTint(0.7f, 0.6f, 0f)]
public class ActionNode : Node
{
    [Input(backingValue = ShowBackingValue.Never)]
    [SerializeField] private ActionNode _origin;

    [SerializeField] private bool _callEvent;
    [SerializeField] private string _eventName;

    [SerializeField] private bool _boolValue;
    [SerializeField] private int _intValue;
    [SerializeField] private float _floatValue;
    [SerializeField] private string _stringValue;

    [SerializeField] private bool _changeGlobalFlag;
    [SerializeField] private string _flagName;
    [SerializeField] private int _flagValue;

    public bool CallEvent { get => _callEvent; }
    public string EventName { get => _eventName; }
    public bool ChangeGlobalFlag { get => _changeGlobalFlag; }
    public int FlagValue { get => _flagValue; }

    public override object GetValue(NodePort port)
    {
        return this;
    }

    public void Act()
    {
        if (_callEvent)
        {
            EventManager.TriggerEvent(_eventName);
        }
        if (_changeGlobalFlag)
        {
            GlobalFlagsManager.SetFlag(_flagName, _flagValue);
        }
    }
}

#if UNITY_EDITOR

[CustomNodeEditor(typeof(ActionNode))]
public class ActionNodeEditor : NodeEditor
{
    private ActionNode actionNode;

    public override void OnBodyGUI()
    {
        if (actionNode == null) actionNode = target as ActionNode;

        // Update serialized object's representation
        serializedObject.Update();

        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("_origin"));
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("_callEvent"));
        if (actionNode.CallEvent)
        {
            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("_eventName"));
        }
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("_changeGlobalFlag"));
        if (actionNode.ChangeGlobalFlag)
        {
            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("_flagName"));
            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("_flagValue"));
        }

        // Apply property modifications
        serializedObject.ApplyModifiedProperties();
    }
}

#endif