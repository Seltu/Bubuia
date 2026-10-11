using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public sealed class EventManagerBrowserWindow : EditorWindow
{
    private const string WINDOW_TITLE = "Event Manager Browser";

    private static readonly Regex EVENT_CALL_REGEX = new Regex(
        "\\bEventManager\\s*\\.\\s*" +
        "(?<method>AddListener|RemoveListener|TriggerEvent)\\s*" +
        "(?:<(?<generic>(?:[^<>\\r\\n]+|<[^<>\\r\\n]*>)+)>)?\\s*" +
        "\\(\\s*" +
        "(?<event>" +
        "@\"(?:\"\"|[^\"])*\"" +
        "|" +
        "\"(?:\\\\.|[^\"\\\\])*\"" +
        "|" +
        "nameof\\s*\\(\\s*[A-Za-z_][A-Za-z0-9_\\.]*\\s*\\)" +
        "|" +
        "[A-Za-z_][A-Za-z0-9_\\.]*" +
        ")",
        RegexOptions.Compiled | RegexOptions.Multiline
    );

    private static readonly Regex CONSTANT_STRING_REGEX = new Regex(
        "\\bconst\\s+string\\s+" +
        "(?<name>[A-Za-z_][A-Za-z0-9_]*)\\s*=\\s*" +
        "(?<value>" +
        "@\"(?:\"\"|[^\"])*\"" +
        "|" +
        "\"(?:\\\\.|[^\"\\\\])*\"" +
        ")\\s*;",
        RegexOptions.Compiled | RegexOptions.Multiline
    );

    private readonly Dictionary<string, EventGroup> _eventGroups =
        new Dictionary<string, EventGroup>();

    private readonly Dictionary<string, HashSet<string>> _constantLookup =
        new Dictionary<string, HashSet<string>>();

    private readonly List<EventGroup> _orderedGroups =
        new List<EventGroup>();

    private Vector2 _scrollPosition;
    private string _searchText = string.Empty;

    [MenuItem("Tools/Event Manager/Event Browser")]
    private static void OpenWindow()
    {
        EventManagerBrowserWindow Window =
            GetWindow<EventManagerBrowserWindow>(WINDOW_TITLE);

        Window.minSize = new Vector2(700f, 400f);
        Window.Show();
    }

    private void OnEnable()
    {
        ScanProject();
    }

    private void OnGUI()
    {
        DrawToolbar();

        EditorGUILayout.Space(4f);

        EditorGUILayout.HelpBox(
            "A janela procura chamadas ao EventManager nos scripts dentro da pasta Assets. " +
            "Eventos construídos dinamicamente podem aparecer como expressões não resolvidas.",
            MessageType.Info
        );

        EditorGUILayout.Space(4f);

        if (_orderedGroups.Count == 0)
        {
            EditorGUILayout.HelpBox(
                "Nenhum evento foi encontrado.",
                MessageType.Warning
            );

            return;
        }

        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

        foreach (EventGroup Group in _orderedGroups)
        {
            if (!MatchesSearch(Group))
            {
                continue;
            }

            DrawEventGroup(Group);
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        GUILayout.Label(
            $"Eventos encontrados: {_orderedGroups.Count}",
            EditorStyles.miniLabel,
            GUILayout.Width(150f)
        );

        _searchText = GUILayout.TextField(
            _searchText,
            EditorStyles.toolbarSearchField
        );

        if (GUILayout.Button(
                "Atualizar",
                EditorStyles.toolbarButton,
                GUILayout.Width(80f)))
        {
            ScanProject();
        }

        EditorGUILayout.EndHorizontal();
    }

    private void DrawEventGroup(EventGroup Group)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        EditorGUILayout.BeginHorizontal();

        string DisplayName = Group.EventName;

        if (!Group.IsResolved)
        {
            DisplayName += "  [expressão não resolvida]";
        }

        Group.IsExpanded = EditorGUILayout.Foldout(
            Group.IsExpanded,
            DisplayName,
            true
        );

        GUILayout.FlexibleSpace();

        int AddCount = Group.Usages.Count(
            Usage => Usage.Method == EventMethod.AddListener
        );

        int TriggerCount = Group.Usages.Count(
            Usage => Usage.Method == EventMethod.TriggerEvent
        );

        int RemoveCount = Group.Usages.Count(
            Usage => Usage.Method == EventMethod.RemoveListener
        );

        GUILayout.Label(
            $"Listeners: {AddCount}",
            EditorStyles.miniLabel,
            GUILayout.Width(85f)
        );

        GUILayout.Label(
            $"Triggers: {TriggerCount}",
            EditorStyles.miniLabel,
            GUILayout.Width(80f)
        );

        GUILayout.Label(
            $"Remoções: {RemoveCount}",
            EditorStyles.miniLabel,
            GUILayout.Width(85f)
        );

        EditorGUILayout.EndHorizontal();

        if (Group.IsExpanded)
        {
            DrawEventStatus(AddCount, TriggerCount);

            EditorGUILayout.Space(3f);

            IEnumerable<EventUsage> OrderedUsages = Group.Usages
                .OrderBy(Usage => Usage.AssetPath)
                .ThenBy(Usage => Usage.LineNumber);

            foreach (EventUsage Usage in OrderedUsages)
            {
                DrawUsage(Usage);
            }
        }

        EditorGUILayout.EndVertical();
    }

    private static void DrawEventStatus(int AddCount, int TriggerCount)
    {
        if (AddCount == 0)
        {
            EditorGUILayout.HelpBox(
                "Nenhum AddListener foi encontrado para este evento.",
                MessageType.Warning
            );
        }

        if (TriggerCount == 0)
        {
            EditorGUILayout.HelpBox(
                "Nenhum TriggerEvent foi encontrado para este evento.",
                MessageType.Warning
            );
        }
    }

    private static void DrawUsage(EventUsage Usage)
    {
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField(
            Usage.Method.ToString(),
            GUILayout.Width(110f)
        );

        string GenericDescription = "sem tipo explícito";

        if (!string.IsNullOrWhiteSpace(Usage.GenericArguments))
        {
            GenericDescription = $"<{Usage.GenericArguments}>";
        }

        EditorGUILayout.LabelField(
            GenericDescription,
            GUILayout.Width(180f)
        );

        string Location =
            $"{Usage.AssetPath}:{Usage.LineNumber}";

        EditorGUILayout.LabelField(
            new GUIContent(Location, Usage.EventExpression),
            EditorStyles.miniLabel
        );

        if (GUILayout.Button("Abrir", GUILayout.Width(55f)))
        {
            OpenUsage(Usage);
        }

        EditorGUILayout.EndHorizontal();
    }

    private void ScanProject()
    {
        _eventGroups.Clear();
        _orderedGroups.Clear();
        _constantLookup.Clear();

        List<ScriptSource> Scripts = LoadProjectScripts();

        BuildConstantLookup(Scripts);

        foreach (ScriptSource Script in Scripts)
        {
            ScanScript(Script);
        }

        IEnumerable<EventGroup> OrderedGroups = _eventGroups.Values
            .OrderBy(Group => Group.EventName);

        _orderedGroups.AddRange(OrderedGroups);

        Repaint();
    }

    private static List<ScriptSource> LoadProjectScripts()
    {
        List<ScriptSource> Scripts = new List<ScriptSource>();

        string[] Guids = AssetDatabase.FindAssets(
            "t:MonoScript",
            new[] { "Assets" }
        );

        foreach (string Guid in Guids)
        {
            string AssetPath = AssetDatabase.GUIDToAssetPath(Guid);

            if (!AssetPath.EndsWith(
                    ".cs",
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            try
            {
                string Source = File.ReadAllText(AssetPath);
                string SanitizedSource =
                    RemoveCommentsPreservingLayout(Source);

                ScriptSource Script = new ScriptSource
                {
                    AssetPath = AssetPath,
                    Source = Source,
                    SanitizedSource = SanitizedSource
                };

                Scripts.Add(Script);
            }
            catch (Exception Exception)
            {
                Debug.LogWarning(
                    $"Não foi possível ler o script {AssetPath}.\n" +
                    Exception.Message
                );
            }
        }

        return Scripts;
    }

    private void BuildConstantLookup(IEnumerable<ScriptSource> Scripts)
    {
        foreach (ScriptSource Script in Scripts)
        {
            string ClassName =
                Path.GetFileNameWithoutExtension(Script.AssetPath);

            MatchCollection Matches =
                CONSTANT_STRING_REGEX.Matches(
                    Script.SanitizedSource
                );

            foreach (Match Match in Matches)
            {
                string ConstantName =
                    Match.Groups["name"].Value;

                string Literal =
                    Match.Groups["value"].Value;

                string ConstantValue;

                if (!TryParseStringLiteral(
                        Literal,
                        out ConstantValue))
                {
                    continue;
                }

                AddConstantReference(
                    ConstantName,
                    ConstantValue
                );

                AddConstantReference(
                    $"{ClassName}.{ConstantName}",
                    ConstantValue
                );
            }
        }
    }

    private void AddConstantReference(
        string Reference,
        string Value)
    {
        HashSet<string> Values;

        if (!_constantLookup.TryGetValue(
                Reference,
                out Values))
        {
            Values = new HashSet<string>();
            _constantLookup.Add(Reference, Values);
        }

        Values.Add(Value);
    }

    private void ScanScript(ScriptSource Script)
    {
        MatchCollection Matches =
            EVENT_CALL_REGEX.Matches(
                Script.SanitizedSource
            );

        int CurrentLine = 1;
        int CurrentIndex = 0;

        foreach (Match Match in Matches)
        {
            while (CurrentIndex < Match.Index)
            {
                if (Script.SanitizedSource[CurrentIndex] == '\n')
                {
                    CurrentLine++;
                }

                CurrentIndex++;
            }

            string MethodName =
                Match.Groups["method"].Value;

            string GenericArguments =
                Match.Groups["generic"].Value.Trim();

            string EventExpression =
                Match.Groups["event"].Value.Trim();

            bool IsResolved;

            string EventName = ResolveEventName(
                EventExpression,
                out IsResolved
            );

            string GroupKey = EventName;

            if (!IsResolved)
            {
                GroupKey = $"unresolved:{EventExpression}";
            }

            EventGroup Group;

            if (!_eventGroups.TryGetValue(
                    GroupKey,
                    out Group))
            {
                Group = new EventGroup
                {
                    EventName = EventName,
                    EventExpression = EventExpression,
                    IsResolved = IsResolved
                };

                _eventGroups.Add(GroupKey, Group);
            }

            EventUsage Usage = new EventUsage
            {
                Method = ParseEventMethod(MethodName),
                GenericArguments = GenericArguments,
                EventExpression = EventExpression,
                AssetPath = Script.AssetPath,
                LineNumber = CurrentLine
            };

            Group.Usages.Add(Usage);
        }
    }

    private string ResolveEventName(
        string Expression,
        out bool IsResolved)
    {
        string LiteralValue;

        if (TryParseStringLiteral(
                Expression,
                out LiteralValue))
        {
            IsResolved = true;
            return LiteralValue;
        }

        if (Expression.StartsWith(
                "nameof",
                StringComparison.Ordinal))
        {
            int OpeningParenthesis =
                Expression.IndexOf('(');

            int ClosingParenthesis =
                Expression.LastIndexOf(')');

            if (OpeningParenthesis >= 0 &&
                ClosingParenthesis > OpeningParenthesis)
            {
                string NameofExpression = Expression.Substring(
                    OpeningParenthesis + 1,
                    ClosingParenthesis - OpeningParenthesis - 1
                );

                string[] Parts =
                    NameofExpression.Split('.');

                IsResolved = true;
                return Parts[Parts.Length - 1].Trim();
            }
        }

        HashSet<string> ConstantValues;

        if (_constantLookup.TryGetValue(
                Expression,
                out ConstantValues))
        {
            if (ConstantValues.Count == 1)
            {
                IsResolved = true;
                return ConstantValues.First();
            }
        }

        int LastDot = Expression.LastIndexOf('.');

        if (LastDot >= 0 &&
            LastDot < Expression.Length - 1)
        {
            string SimpleName =
                Expression.Substring(LastDot + 1);

            if (_constantLookup.TryGetValue(
                    SimpleName,
                    out ConstantValues))
            {
                if (ConstantValues.Count == 1)
                {
                    IsResolved = true;
                    return ConstantValues.First();
                }
            }
        }

        IsResolved = false;
        return Expression;
    }

    private static bool TryParseStringLiteral(
        string Expression,
        out string Value)
    {
        Value = string.Empty;

        if (Expression.StartsWith(
                "@\"",
                StringComparison.Ordinal) &&
            Expression.EndsWith(
                "\"",
                StringComparison.Ordinal))
        {
            Value = Expression.Substring(
                2,
                Expression.Length - 3
            );

            Value = Value.Replace(
                "\"\"",
                "\""
            );

            return true;
        }

        if (Expression.StartsWith(
                "\"",
                StringComparison.Ordinal) &&
            Expression.EndsWith(
                "\"",
                StringComparison.Ordinal))
        {
            string Content = Expression.Substring(
                1,
                Expression.Length - 2
            );

            try
            {
                Value = Regex.Unescape(Content);
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        return false;
    }

    private static EventMethod ParseEventMethod(
        string MethodName)
    {
        EventMethod ParsedMethod;

        if (Enum.TryParse(
                MethodName,
                out ParsedMethod))
        {
            return ParsedMethod;
        }

        return EventMethod.Unknown;
    }

    private bool MatchesSearch(EventGroup Group)
    {
        if (string.IsNullOrWhiteSpace(_searchText))
        {
            return true;
        }

        if (Group.EventName.IndexOf(
                _searchText,
                StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return true;
        }

        foreach (EventUsage Usage in Group.Usages)
        {
            if (Usage.AssetPath.IndexOf(
                    _searchText,
                    StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
        }

        return false;
    }

    private static void OpenUsage(EventUsage Usage)
    {
        MonoScript Script =
            AssetDatabase.LoadAssetAtPath<MonoScript>(
                Usage.AssetPath
            );

        if (Script == null)
        {
            Debug.LogWarning(
                $"Script não encontrado: {Usage.AssetPath}"
            );

            return;
        }

        EditorGUIUtility.PingObject(Script);

        AssetDatabase.OpenAsset(
            Script,
            Usage.LineNumber
        );
    }

    private static string RemoveCommentsPreservingLayout(
        string Source)
    {
        StringBuilder Builder =
            new StringBuilder(Source.Length);

        ParseState State = ParseState.Code;

        for (int Index = 0;
             Index < Source.Length;
             Index++)
        {
            char Current = Source[Index];
            char Next = '\0';

            if (Index + 1 < Source.Length)
            {
                Next = Source[Index + 1];
            }

            switch (State)
            {
                case ParseState.Code:
                    {
                        if (Current == '/' && Next == '/')
                        {
                            Builder.Append(' ');
                            Builder.Append(' ');
                            Index++;
                            State = ParseState.SingleLineComment;
                            break;
                        }

                        if (Current == '/' && Next == '*')
                        {
                            Builder.Append(' ');
                            Builder.Append(' ');
                            Index++;
                            State = ParseState.MultiLineComment;
                            break;
                        }

                        if (Current == '@' && Next == '"')
                        {
                            Builder.Append(Current);
                            Builder.Append(Next);
                            Index++;
                            State = ParseState.VerbatimString;
                            break;
                        }

                        if (Current == '"')
                        {
                            Builder.Append(Current);
                            State = ParseState.String;
                            break;
                        }

                        if (Current == '\'')
                        {
                            Builder.Append(Current);
                            State = ParseState.Character;
                            break;
                        }

                        Builder.Append(Current);
                        break;
                    }

                case ParseState.SingleLineComment:
                    {
                        if (Current == '\n')
                        {
                            Builder.Append(Current);
                            State = ParseState.Code;
                        }
                        else
                        {
                            Builder.Append(' ');
                        }

                        break;
                    }

                case ParseState.MultiLineComment:
                    {
                        if (Current == '*' && Next == '/')
                        {
                            Builder.Append(' ');
                            Builder.Append(' ');
                            Index++;
                            State = ParseState.Code;
                            break;
                        }

                        if (Current == '\n')
                        {
                            Builder.Append(Current);
                        }
                        else
                        {
                            Builder.Append(' ');
                        }

                        break;
                    }

                case ParseState.String:
                    {
                        Builder.Append(Current);

                        if (Current == '\\' && Next != '\0')
                        {
                            Builder.Append(Next);
                            Index++;
                            break;
                        }

                        if (Current == '"')
                        {
                            State = ParseState.Code;
                        }

                        break;
                    }

                case ParseState.VerbatimString:
                    {
                        Builder.Append(Current);

                        if (Current != '"')
                        {
                            break;
                        }

                        if (Next == '"')
                        {
                            Builder.Append(Next);
                            Index++;
                            break;
                        }

                        State = ParseState.Code;
                        break;
                    }

                case ParseState.Character:
                    {
                        Builder.Append(Current);

                        if (Current == '\\' && Next != '\0')
                        {
                            Builder.Append(Next);
                            Index++;
                            break;
                        }

                        if (Current == '\'')
                        {
                            State = ParseState.Code;
                        }

                        break;
                    }
            }
        }

        return Builder.ToString();
    }

    private sealed class ScriptSource
    {
        public string AssetPath;
        public string Source;
        public string SanitizedSource;
    }

    private sealed class EventGroup
    {
        public string EventName;
        public string EventExpression;
        public bool IsResolved;
        public bool IsExpanded;
        public readonly List<EventUsage> Usages =
            new List<EventUsage>();
    }

    private sealed class EventUsage
    {
        public EventMethod Method;
        public string GenericArguments;
        public string EventExpression;
        public string AssetPath;
        public int LineNumber;
    }

    private enum EventMethod
    {
        Unknown,
        AddListener,
        RemoveListener,
        TriggerEvent
    }

    private enum ParseState
    {
        Code,
        SingleLineComment,
        MultiLineComment,
        String,
        VerbatimString,
        Character
    }
}