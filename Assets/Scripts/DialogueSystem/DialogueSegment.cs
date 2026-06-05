


using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.MemoryProfiler;

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

[NodeTint(0f, 0.5f, 0.5f)]
public class DialogueSegment : Node
{
    [Input(backingValue = ShowBackingValue.Never)]
    [SerializeField] private DialogueSegment _origin;

    [SerializeField] private string _name;

    [TextArea]
    [SerializeField] private string _sentence;

    [SerializeField] private bool _hasChoices;

    [Output(dynamicPortList = true, connectionType = ConnectionType.Override)]
    [TextArea]
    [SerializeField] private List<string> _choices;

    [Output(connectionType = ConnectionType.Override)]
    [SerializeField] private DialogueSegment _next;

    [Output(connectionType = ConnectionType.Multiple)]
    [SerializeField] private ActionNode _actions;

    public bool HasChoices { get => _hasChoices;}

    public override object GetValue(NodePort port)
    {
        return this;
    }

    public string GetSentence()
    {
        return _sentence;
    }

    public bool HasOrigin()
    {
        return GetInputPort("_origin").ConnectionCount > 0;
    }

    public Node GetNextDialogue(int option)
    {
        if(HasNextDialogue())
            if (HasChoices)
            {
                var choiceString = "_choices " + option.ToString();
                return (Node)GetOutputPort(choiceString).Connection.node.GetValue(GetOutputPort(choiceString).Connection);
            }
            else
            {
                return (Node)GetOutputPort("_next").Connection.node.GetValue(GetOutputPort("_next").Connection);
            }
        else
            return null;
    }

    public List<string> GetChoices()
    {
        return _choices;
    }

    public List<ActionNode> GetActions()
    {
        List<NodePort> actionConnections = GetOutputPort("_actions").GetConnections();
        List<ActionNode> actions = new List<ActionNode>();
        foreach (var connection in actionConnections)
        {
            actions.Add((ActionNode)connection.node.GetValue(GetOutputPort("_actions").Connection));
        }
        return actions;
    }

    public bool HasNextDialogue()
    {
        if (_hasChoices)
        {
            for (int i = 0; i < _choices.Count; i++)
            {
                if (GetOutputPort("_choices " + i.ToString()).ConnectionCount > 0)
                    return true;
            }
            return false;
        }
        else
            return GetOutputPort("_next").ConnectionCount > 0;
    }

    private void OnValidate()
    {
        name = _name;
    }
}

#if UNITY_EDITOR

[CustomNodeEditor(typeof(DialogueSegment))]
public class DialogueNodeEditor : NodeEditor
{
    private DialogueSegment dialogueSegment;

    public override void OnBodyGUI()
    {
        if (dialogueSegment == null) dialogueSegment = target as DialogueSegment;

        // Update serialized object's representation
        serializedObject.Update();

        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("_origin"));
        List<GUILayoutOption> layoutOptions = new List<GUILayoutOption> { GUILayout.Width(200), GUILayout.Height(50)};
        if (!dialogueSegment.HasOrigin())
            EditorGUILayout.LabelField("No origin seguments.\nThis segment will begin dialogue.\nMake sure there's only one", options: layoutOptions.ToArray());
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("_name"));
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("_sentence"));
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("_hasChoices"));
        if (dialogueSegment.HasChoices)
        {
            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("_choices"));
        }
        else
            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("_next"));
        if (!dialogueSegment.HasNextDialogue())
            EditorGUILayout.LabelField("No connected segument.\nThis segment will end dialogue.", options: layoutOptions.ToArray());
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("_actions"));
        // Apply property modifications
        serializedObject.ApplyModifiedProperties();
    }
}

#endif