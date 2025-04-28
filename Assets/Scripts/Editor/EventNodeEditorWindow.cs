using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;
using Naninovel.UI;

public class EventNodeEditorWindow : EditorWindow
{
    private string eventName = "事件名称";
    private string eventID = "事件ID";
    private RoleType roleType;
    private string description = "事件描述...";
    private Sprite icon;
    private int duration = 2;
    private bool isUnique = false;
    private int cooldownTurns = 0;
    private float decreaseFactor = 1f;

    private TriggerConditionGroup triggerGroup;
    private List<EventOutcomeBranch> branches = new();
    private List<EventEffectSO> expiredEffects = new();

    private Vector2 scroll;
    private Dictionary<string, bool> foldoutStates = new();

    private EventNodeData loadedEventNode;

    private int highlightEffectIndex = -1;
    private double highlightStartTime = 0;
    private const double highlightDuration = 1.0; // 秒数

    [MenuItem("LevityTools/Event Node Editor")]
    public static void ShowWindow()
    {
        var window = GetWindow<EventNodeEditorWindow>("Levity事件编辑器");
        window.minSize = new Vector2(700, 800); // 设置最小窗口尺寸
    }

    private void OnEnable()
    {
        foldoutStates = LoadFoldoutStates();
    }

    private void OnDisable()
    {
        SaveFoldoutStates(foldoutStates);
    }

    private void OnGUI()
    {
        scroll = EditorGUILayout.BeginScrollView(scroll);

        GUILayout.Label("加载已有事件数据", EditorStyles.boldLabel);
        loadedEventNode =
            (EventNodeData)EditorGUILayout.ObjectField("事件数据", loadedEventNode, typeof(EventNodeData), false);
        if (loadedEventNode != null && GUILayout.Button("加载该事件数据", GUILayout.Height(28)))
        {
            LoadEventNodeData(loadedEventNode);
        }

        GUILayout.Space(20);
        GUILayout.BeginVertical("box");
        bool eventExpanded = GetFoldout("事件基本信息", false);
        eventExpanded = EditorGUILayout.Foldout(eventExpanded, $"事件基本信息", true);
        SetFoldout("事件基本信息", eventExpanded);
        if (eventExpanded)
        {
            GUILayout.Label("事件基本信息", EditorStyles.boldLabel);
            eventName = EditorGUILayout.TextField("事件名称", eventName);
            eventID = EditorGUILayout.TextField("事件ID", eventID);
            roleType = (RoleType)EditorGUILayout.EnumPopup("来源角色", roleType);
            icon = (Sprite)EditorGUILayout.ObjectField("图标", icon, typeof(Sprite), false);
            duration = EditorGUILayout.IntField("持续时间", duration);
            description = EditorGUILayout.TextArea(description, GUILayout.Height(60));

            GUILayout.Space(5);
            GUILayout.Label("高级选项", EditorStyles.boldLabel);
            isUnique = EditorGUILayout.Toggle("是否为唯一事件", isUnique);
            cooldownTurns = EditorGUILayout.IntSlider("冷却回合数", cooldownTurns, 0, 10);
            decreaseFactor = EditorGUILayout.FloatField("损耗系数", decreaseFactor);
        }
        GUILayout.EndVertical();
        GUILayout.Space(5);
        GUILayout.BeginVertical("box");
        bool triggerExpanded = GetFoldout("触发条件", false);
        triggerExpanded = EditorGUILayout.Foldout(triggerExpanded, $"触发条件", true);
        SetFoldout("触发条件", triggerExpanded);
        if (triggerExpanded)
        {
            DrawTriggerConditionGroup();
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(5);
        GUILayout.BeginVertical("box");
        bool expiredEExpanded = GetFoldout("过期效果", false);
        expiredEExpanded = EditorGUILayout.Foldout(expiredEExpanded, $"过期效果", true);
        SetFoldout("过期效果", expiredEExpanded);
        if (expiredEExpanded)
        {
            DrawEffectList(expiredEffects, "ExpiredEffect", 1);
        }
        GUILayout.EndVertical();
        GUILayout.Space(5);
        GUILayout.BeginVertical("box");
        bool branchExpanded = GetFoldout("分支设置", false);
        branchExpanded = EditorGUILayout.Foldout(branchExpanded, $"分支设置", true);
        SetFoldout("分支设置", branchExpanded);
        if (branchExpanded)
        {

            int branchToRemove = -1;

            for (int i = 0; i < branches.Count; i++)
            {
                var branch = branches[i];
                string key = $"EventEditor_Foldout_Branch_{i}";
                bool expanded = GetFoldout(key, false);
                GUILayout.BeginVertical("box");
                GUILayout.BeginHorizontal();
                expanded = EditorGUILayout.Foldout(expanded, $"分支 {i + 1}：{branch.label}", true);
                if (GUILayout.Button("↑", GUILayout.Width(30), GUILayout.Height(28)) && i > 0)
                {
                    (branches[i], branches[i - 1]) = (branches[i - 1], branches[i]);
                }

                if (GUILayout.Button("↓", GUILayout.Width(30), GUILayout.Height(28)) && i < branches.Count - 1)
                {
                    (branches[i], branches[i + 1]) = (branches[i + 1], branches[i]);
                }

                if (GUILayout.Button("X", GUILayout.Width(30), GUILayout.Height(28)))
                {
                    if (EditorUtility.DisplayDialog("确认删除", "您确定要删除这个分支吗?", "删除", "取消"))
                    {
                        branchToRemove = i;
                    }
                }
                GUILayout.EndHorizontal();
                SetFoldout(key, expanded);

                // 捕获右键点击事件
                Rect lastRect = GUILayoutUtility.GetLastRect(); // 获取该分支最后渲染的位置
                if (Event.current.type == EventType.ContextClick && lastRect.Contains(Event.current.mousePosition))
                {
                    // 当右键点击时，显示右键菜单
                    OnBranchRightClick(i); // 右键点击时显示菜单
                    Event.current.Use(); // 防止事件被传递到其他地方
                }

                if (expanded)
                {
                    EditorGUI.indentLevel++;
                    branch.label = EditorGUILayout.TextField("分支名称", branch.label);

                    EditorGUI.indentLevel++;
                    DrawResolveConditionGroup(branch, i, 2);

                    DrawEffectList(branch.effects ??= new List<EventEffectSO>(), $"Branch{i}_Effect", 2);

                    GUILayout.BeginHorizontal();

                    GUILayout.Space(5);

                    GUILayout.EndHorizontal();

                    EditorGUI.indentLevel--;

                    EditorGUI.indentLevel--;
                }
                GUILayout.EndVertical();
            }
            if (GUILayout.Button("添加分支", GUILayout.Height(28))) branches.Add(new EventOutcomeBranch());
            if (branchToRemove >= 0)
            {
                var branch = branches[branchToRemove];
                TryDeleteAsset(branch.matchConditions);
                if (branch.effects != null)
                    foreach (var e in branch.effects)
                        TryDeleteAsset(e);
                branches.RemoveAt(branchToRemove);
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(20);
        if (loadedEventNode != null && GUILayout.Button("基于当前事件克隆为新事件", GUILayout.Height(28)))
        {
            CloneEventNodeAsNew();
        }

        GUILayout.Space(20);
        if (GUILayout.Button("保存事件数据", GUILayout.Height(28))) SaveEventNodeData();

        EditorGUILayout.EndScrollView();
    }

    private void DrawTriggerConditionGroup()
    {
        GUILayout.Label("触发条件组", EditorStyles.boldLabel);
        if (triggerGroup == null && GUILayout.Button("新建 Trigger 条件组", GUILayout.Height(28)))
        {
            triggerGroup = CreateAndSaveSO<TriggerConditionGroup>($"{eventID}_TriggerGroup");
        }
        GUILayout.BeginHorizontal();
        triggerGroup =
            (TriggerConditionGroup)EditorGUILayout.ObjectField("条件组资源", triggerGroup, typeof(TriggerConditionGroup),
                false);
        if (triggerGroup != null)
        {
            GUIStyle bigButtonStyle = new GUIStyle(GUI.skin.button);
            bigButtonStyle.fontSize = 20;
            bigButtonStyle.fontStyle = FontStyle.Bold;
            bigButtonStyle.normal.textColor = Color.white;
            if (GUILayout.Button("+", bigButtonStyle, GUILayout.Width(40), GUILayout.Height(30)))
            {
                TriggerConditionSelectorWindow.Show(type =>
                {
                    var cond = (EventTriggerConditionSO)CreateAndSaveSO(type, $"TriggerCond_{eventID}_{triggerGroup.name}_{triggerGroup.conditions.Count}");
                    triggerGroup.conditions.Add(cond);
                    EditorUtility.SetDirty(triggerGroup);
                    AssetDatabase.SaveAssets();
                });
            }
        }
        GUILayout.EndHorizontal();

        if (triggerGroup != null)
        {
            int removeIndex = -1;
            for (int i = 0; i < triggerGroup.conditions.Count; i++)
            {
                string key = $"EventEditor_Foldout_TriggerCondition_{i}";
                bool expanded = GetFoldout(key, false);
                expanded = EditorGUILayout.Foldout(expanded, $"Trigger 条件 {i + 1}", true);
                SetFoldout(key, expanded);

                if (expanded)
                {
                    EditorGUI.indentLevel++;
                    GUILayout.BeginHorizontal();
                    triggerGroup.conditions[i] =
                        (EventTriggerConditionSO)EditorGUILayout.ObjectField(triggerGroup.conditions[i],
                            typeof(EventTriggerConditionSO), false);
                    if (GUILayout.Button("↑", GUILayout.Width(30), GUILayout.Height(28)) && i > 0)
                    {
                        (triggerGroup.conditions[i], triggerGroup.conditions[i - 1]) = (triggerGroup.conditions[i - 1],
                            triggerGroup.conditions[i]);
                    }

                    if (GUILayout.Button("↓", GUILayout.Width(30), GUILayout.Height(28)) &&
                        i < triggerGroup.conditions.Count - 1)
                    {
                        (triggerGroup.conditions[i], triggerGroup.conditions[i + 1]) = (triggerGroup.conditions[i + 1],
                            triggerGroup.conditions[i]);
                    }

                    if (GUILayout.Button("X", GUILayout.Width(30), GUILayout.Height(28)))
                    {
                        if (EditorUtility.DisplayDialog("确认删除", "您确定要删除这个条件吗?", "删除", "取消"))
                        {
                            removeIndex = i;
                        }
                    }
                    GUILayout.EndHorizontal();
                    if (triggerGroup.conditions[i] != null)
                    {
                        var editor = Editor.CreateEditor(triggerGroup.conditions[i]);
                        editor?.OnInspectorGUI();
                    }
                    EditorGUI.indentLevel--;
                }
            }

            if (removeIndex >= 0)
            {
                TryDeleteAsset(triggerGroup.conditions[removeIndex]);
                triggerGroup.conditions.RemoveAt(removeIndex);
                EditorUtility.SetDirty(triggerGroup);
                AssetDatabase.SaveAssets();
            }


        }

    }


    private void DrawResolveConditionGroup(EventOutcomeBranch branch, int index, int indent)
    {
        GUILayout.Label("结算条件组", EditorStyles.boldLabel);
        GUILayout.Space(5);
        if (branch.matchConditions == null && GUILayout.Button("新建 Resolve 条件组", GUILayout.Height(28)))
        {
            branch.matchConditions = CreateAndSaveSO<ResolveConditionGroup>($"{eventID}_ResolveGroup_{index}");
        }

        GUILayout.BeginHorizontal();
        branch.matchConditions = (ResolveConditionGroup)EditorGUILayout.ObjectField("条件组资源", branch.matchConditions,
            typeof(ResolveConditionGroup), false);
        GUIStyle bigButtonStyle = new GUIStyle(GUI.skin.button);
        bigButtonStyle.fontSize = 20; // 字号大一点
        bigButtonStyle.fontStyle = FontStyle.Bold;
        bigButtonStyle.normal.textColor = Color.white;

        if (GUILayout.Button("+", bigButtonStyle, GUILayout.Width(30), GUILayout.Height(28)))
        {
            ResolveConditionSelectorWindow.Show(type =>
            {
                var cond = CreateAndSaveSO(type, $"ResolveCond_{eventID}_{branch.label}_{branch.matchConditions.conditions.Count}");
                branch.matchConditions.conditions.Add((EventResolveConditionSO)cond);
                EditorUtility.SetDirty(branch.matchConditions);
                AssetDatabase.SaveAssets();
            });
        }
        GUILayout.EndHorizontal();

        if (branch.matchConditions != null)
        {
            int removeIndex = -1;
            for (int i = 0; i < branch.matchConditions.conditions.Count; i++)
            {
                string key = $"EventEditor_Foldout_ResolveCondition_{index}_{i}";
                bool expanded = GetFoldout(key, false);
                GUILayout.BeginVertical("box");
                GUILayout.BeginHorizontal();
                expanded = EditorGUILayout.Foldout(expanded, $"結算条件{i + 1} {branch.matchConditions.label}", true);
                if (GUILayout.Button("↑", GUILayout.Width(30), GUILayout.Height(28)) && i > 0)
                {
                    (branch.matchConditions.conditions[i], branch.matchConditions.conditions[i - 1]) =
                        (branch.matchConditions.conditions[i - 1], branch.matchConditions.conditions[i]);
                }

                GUILayout.Space(5);
                if (GUILayout.Button("↓", GUILayout.Width(30), GUILayout.Height(28)) &&
                    i < branch.matchConditions.conditions.Count - 1)
                {
                    (branch.matchConditions.conditions[i], branch.matchConditions.conditions[i + 1]) =
                        (branch.matchConditions.conditions[i + 1], branch.matchConditions.conditions[i]);
                }

                GUILayout.Space(5);
                if (GUILayout.Button("X", GUILayout.Width(30), GUILayout.Height(28)))
                {
                    if (EditorUtility.DisplayDialog("确认删除", "您确定要删除这个条件吗?", "删除", "取消"))
                    {
                        removeIndex = i;
                    }
                }
                GUILayout.EndHorizontal();
                SetFoldout(key, expanded);

                if (expanded)
                {
                    branch.matchConditions.label = EditorGUILayout.TextField("結算條件組名称", branch.matchConditions.label);
                    EditorGUI.indentLevel++;
                    branch.matchConditions.conditions[i] =
                        (EventResolveConditionSO)EditorGUILayout.ObjectField(branch.matchConditions.conditions[i],
                            typeof(EventResolveConditionSO), false);
                    if (branch.matchConditions.conditions[i] != null)
                    {
                        var editor = Editor.CreateEditor(branch.matchConditions.conditions[i]);
                        editor?.OnInspectorGUI();
                    }
                    EditorGUI.indentLevel--;
                }
                GUILayout.EndVertical();
            }

            if (removeIndex >= 0)
            {
                TryDeleteAsset(branch.matchConditions.conditions[removeIndex]);
                branch.matchConditions.conditions.RemoveAt(removeIndex);
                EditorUtility.SetDirty(branch.matchConditions);
                AssetDatabase.SaveAssets();
            }
        }

    }
    private void DrawEffectList(List<EventEffectSO> list, string prefix, int indent)
    {
        int removeIndex = -1;
        int swapFrom = -1;
        int swapTo = -1;

        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        GUILayout.Label("效果组", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        GUIStyle bigButtonStyle = new GUIStyle(GUI.skin.button);
        bigButtonStyle.fontSize = 20;
        bigButtonStyle.fontStyle = FontStyle.Bold;
        bigButtonStyle.normal.textColor = Color.white;

        if (GUILayout.Button("+", bigButtonStyle, GUILayout.Width(40), GUILayout.Height(30)))
        {
            EffectsSelectorWindow.Show(type =>
            {
                var effect = (EventEffectSO)CreateAndSaveSO(type, $"{eventName}_{prefix}_Effect_{list.Count}");
                list.Add(effect);
                EditorUtility.SetDirty(effect);
                AssetDatabase.SaveAssets();
            });
        }
        GUILayout.EndHorizontal();
        // 循环遍历效果
        for (int i = 0; i < list.Count; i++)
        {
            string key = $"EventEditor_Foldout_Effect_{prefix}_{i}";
            bool expanded = GetFoldout(key, false);
            // 顺序调整按钮开始
            GUILayout.BeginVertical("box");
            GUILayout.BeginHorizontal("box");
            expanded = EditorGUILayout.Foldout(expanded, $"效果{i + 1} {list[i].label}", true);
            if (GUILayout.Button("↑", GUILayout.Width(30), GUILayout.Height(28)) && i > 0)
            {
                swapFrom = i;
                swapTo = i - 1;
            }

            if (GUILayout.Button("↓", GUILayout.Width(30), GUILayout.Height(28)) && i < list.Count - 1)
            {
                swapFrom = i;
                swapTo = i + 1;
            }

            if (GUILayout.Button("X", GUILayout.Width(30), GUILayout.Height(28)))
            {
                if (EditorUtility.DisplayDialog("确认删除", "您确定要删除这个效果吗?", "删除", "取消"))
                {
                    removeIndex = i;
                }
            }
            GUILayout.EndHorizontal();
            SetFoldout(key, expanded);

            if (expanded)
            {
                EditorGUI.indentLevel++;
                list[i] = (EventEffectSO)EditorGUILayout.ObjectField(list[i], typeof(EventEffectSO), false);
                if (list[i] != null)
                {
                    var editor = Editor.CreateEditor(list[i]);
                    editor?.OnInspectorGUI();
                }

                EditorGUI.indentLevel--;
            }
            GUILayout.EndVertical();
        }


        // 按钮排布优化：使用垂直布局并添加适当间距
        GUILayout.EndVertical(); // 结束外部垂直布局

        // 循环结束后统一处理交换
        if (swapFrom >= 0 && swapTo >= 0 &&
            swapFrom < list.Count && swapTo < list.Count)
        {
            (list[swapFrom], list[swapTo]) = (list[swapTo], list[swapFrom]);

            highlightEffectIndex = swapTo;
            highlightStartTime = EditorApplication.timeSinceStartup;

            if (list[highlightEffectIndex] != null)
                EditorGUIUtility.PingObject(list[highlightEffectIndex]);
        }

        // 移除效果
        if (removeIndex >= 0)
        {
            TryDeleteAsset(list[removeIndex]);
            list.RemoveAt(removeIndex);
        }


    }


    private void SaveEventNodeData()
    {
        string path = $"Assets/SO/EventData/EventContainer/{eventName}";
        Directory.CreateDirectory(path);

        EventNodeData node = loadedEventNode != null
            ? loadedEventNode
            : ScriptableObject.CreateInstance<EventNodeData>();
        node.eventName = eventName;
        node.eventID = eventID;
        node.sourceRole = roleType;
        node.description = description;
        node.icon = icon;
        node.duration = duration;
        node.isUnique = isUnique;
        node.cooldownTurns = cooldownTurns;
        node.decreaseFactor = decreaseFactor;
        node.triggerConditions = triggerGroup;
        node.expiredEffects = expiredEffects;
        node.outcomeBranches = branches;

        if (loadedEventNode == null)
        {
            AssetDatabase.CreateAsset(node, $"{path}/{eventID}_EventNode.asset");
        }

        EditorUtility.SetDirty(node);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("保存成功", $"事件数据已保存到：{path}", "OK");
    }

    private T CreateAndSaveSO<T>(string baseFileName) where T : ScriptableObject
    {
        string dir = $"Assets/SO/EventData/EventContainer/EventConditions&Effects/{eventName}";
        Directory.CreateDirectory(dir);

        string uniqueID = Guid.NewGuid().ToString("N");
        string fileName = $"{baseFileName}_{uniqueID}.asset";
        string path = Path.Combine(dir, fileName);

        T so = ScriptableObject.CreateInstance<T>();

        if (so is TriggerConditionGroup trigger)
            trigger.conditions = new List<EventTriggerConditionSO>();
        else if (so is ResolveConditionGroup resolve)
            resolve.conditions = new List<EventResolveConditionSO>();

        AssetDatabase.CreateAsset(so, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return so;
    }

    private ScriptableObject CreateAndSaveSO(Type type, string baseFileName)
    {
        string dir = $"Assets/SO/EventData/EventContainer/EventConditions&Effects/{eventName}";
        Directory.CreateDirectory(dir);

        string uniqueID = Guid.NewGuid().ToString("N");
        string fileName = $"{baseFileName}_{uniqueID}.asset";
        string path = Path.Combine(dir, fileName);

        var so = ScriptableObject.CreateInstance(type);
        so.name = Path.GetFileNameWithoutExtension(fileName);
        AssetDatabase.CreateAsset(so, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return so;
    }


    private void TryDeleteAsset(UnityEngine.Object obj)
    {
        if (obj == null) return;
        string path = AssetDatabase.GetAssetPath(obj);
        if (!string.IsNullOrEmpty(path))
        {
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.Refresh();
        }
    }

    private bool GetFoldout(string key, bool defaultValue)
    {
        if (foldoutStates.TryGetValue(key, out var value))
            return value;
        var val = EditorPrefs.GetBool(key, defaultValue);
        foldoutStates[key] = val;
        return val;
    }

    private void SetFoldout(string key, bool value)
    {
        foldoutStates[key] = value;
        EditorPrefs.SetBool(key, value);
    }

    private Dictionary<string, bool> LoadFoldoutStates()
    {
        return new Dictionary<string, bool>(); // 实际状态靠 EditorPrefs 读取
    }

    private void SaveFoldoutStates(Dictionary<string, bool> states)
    {
        foreach (var kvp in states)
            EditorPrefs.SetBool(kvp.Key, kvp.Value);
    }

    private void LoadEventNodeData(EventNodeData node)
    {
        eventName = node.eventName;
        eventID = node.eventID;
        roleType = node.sourceRole;
        description = node.description;
        icon = node.icon;
        duration = node.duration;
        isUnique = node.isUnique;
        cooldownTurns = node.cooldownTurns;
        decreaseFactor = node.decreaseFactor;
        triggerGroup = node.triggerConditions;
        expiredEffects = node.expiredEffects != null
            ? new List<EventEffectSO>(node.expiredEffects)
            : new List<EventEffectSO>();
        branches = node.outcomeBranches != null
            ? new List<EventOutcomeBranch>(node.outcomeBranches)
            : new List<EventOutcomeBranch>();
    }

    private void CloneEventNodeAsNew()
    {
        if (loadedEventNode == null)
        {
            Debug.LogError("没有加载的事件可供克隆");
            return;
        }

        // ✅ 使用当前窗口中填写的 eventName 和 eventID（而不是 loadedEventNode 的值）
        string newEventName = eventName;
        string newEventID = eventID;
        string newFolder = $"Assets/SO/EventData/EventContainer/{newEventName}";
        string newFolderForConditionEffects =
            $"Assets/SO/EventData/EventContainer/EventConditions&Effects/{newEventName}";
        Directory.CreateDirectory(newFolder);
        Directory.CreateDirectory(newFolderForConditionEffects);
        EventNodeData newNode = Instantiate(loadedEventNode);
        newNode.name = $"{newEventID}_EventNode";
        newNode.eventName = newEventName;
        newNode.eventID = newEventID;


        // 克隆 Trigger 条件组及内部 Trigger 条件
        if (loadedEventNode.triggerConditions != null)
        {
            var oldGroup = loadedEventNode.triggerConditions;
            string groupAssetName = $"{eventID}_TriggerGroup";
            var newGroup = CloneSOAsset(oldGroup, $"{newFolderForConditionEffects}/{groupAssetName}.asset");
            newGroup.name = groupAssetName;
            newGroup.conditions = new List<EventTriggerConditionSO>();

            for (int i = 0; i < oldGroup.conditions.Count; i++)
            {
                var cond = oldGroup.conditions[i];
                if (cond != null)
                {
                    string typeName = cond.GetType().Name;
                    string condName = $"TriggerCond_{eventID}_{newGroup.name}_{typeName}_{i}";
                    var clonedCond = CloneSOAsset(cond, $"{newFolderForConditionEffects}/{condName}.asset");
                    clonedCond.name = condName;
                    newGroup.conditions.Add(clonedCond);
                }
            }

            newNode.triggerConditions = newGroup;
        }


        newNode.expiredEffects = new List<EventEffectSO>();
        for (int i = 0; i < loadedEventNode.expiredEffects.Count; i++)
        {
            var effect = loadedEventNode.expiredEffects[i];
            if (effect != null)
            {
                string typeName = effect.GetType().Name;
                string effName = $"{eventName}_ExpiredEffect_{typeName}_{i}";
                var clonedEff = CloneSOAsset(effect, $"{newFolderForConditionEffects}/{effName}.asset");
                clonedEff.name = effName;
                newNode.expiredEffects.Add(clonedEff);
            }
        }


        newNode.outcomeBranches = new List<EventOutcomeBranch>();
        for (int i = 0; i < loadedEventNode.outcomeBranches.Count; i++)
        {
            var branch = loadedEventNode.outcomeBranches[i];
            var newBranch = new EventOutcomeBranch
            {
                label = branch.label,
                effects = new List<EventEffectSO>(),
            };

            // 克隆 Resolve 条件组
            if (branch.matchConditions != null)
            {
                string groupName = $"{eventID}_ResolveGroup_{i}";
                var newResolveGroup = CloneSOAsset(branch.matchConditions,
                    $"{newFolderForConditionEffects}/{groupName}.asset");
                newResolveGroup.name = groupName;
                newResolveGroup.conditions = new List<EventResolveConditionSO>();

                for (int j = 0; j < branch.matchConditions.conditions.Count; j++)
                {
                    var cond = branch.matchConditions.conditions[j];
                    if (cond != null)
                    {
                        string typeName = cond.GetType().Name;
                        string condName = $"ResolveCond_{eventID}_{branch.label}_{typeName}_{j}";
                        var clonedCond = CloneSOAsset(cond, $"{newFolderForConditionEffects}/{condName}.asset");
                        clonedCond.name = condName;
                        newResolveGroup.conditions.Add(clonedCond);
                    }
                }

                newBranch.matchConditions = newResolveGroup;
            }

            // 克隆 Effect
            for (int j = 0; j < branch.effects.Count; j++)
            {
                var effect = branch.effects[j];
                if (effect != null)
                {
                    string typeName = effect.GetType().Name;
                    string effectName = $"{eventName}_Branch{i}_Effect_{typeName}_{j}";
                    var clonedEffect = CloneSOAsset(effect, $"{newFolderForConditionEffects}/{effectName}.asset");
                    clonedEffect.name = effectName;
                    newBranch.effects.Add(clonedEffect);
                }
            }

            newNode.outcomeBranches.Add(newBranch);
        }


        // 保存主 EventNodeData
        string newNodePath = $"{newFolder}/{eventID}_EventNode.asset";
        AssetDatabase.CreateAsset(newNode, newNodePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("克隆成功", $"新事件克隆已保存至：{newNodePath}", "OK");

        // 自动加载新克隆的事件
        loadedEventNode = newNode;
        LoadEventNodeData(newNode);

        // 关键：同步引用到 UI 当前状态
        triggerGroup = newNode.triggerConditions;
        expiredEffects = newNode.expiredEffects;
        branches = newNode.outcomeBranches;
    }


    private T CloneSOAsset<T>(T original, string newPath) where T : ScriptableObject
    {
        T newSO = Instantiate(original);

        // 设置 name 字段
        string fileName = Path.GetFileNameWithoutExtension(newPath);
        newSO.name = fileName;

        AssetDatabase.CreateAsset(newSO, newPath);
        return newSO;
    }

    private EventOutcomeBranch copiedBranch = null; // 用来存储复制的分支
    private bool canPaste = false; // 是否允许粘贴操作

    private void OnBranchRightClick(int branchIndex)
    {
        GenericMenu menu = new GenericMenu();

        // 复制分支的操作
        menu.AddItem(new GUIContent("复制分支"), false, () => CopyBranch(branchIndex));

        // 如果可以粘贴，显示粘贴分支的操作
        if (canPaste)
        {
            menu.AddItem(new GUIContent("粘贴分支"), false, () => PasteBranch(branchIndex));
            menu.AddItem(new GUIContent("深拷貝分支"), false, () => PasteBranchDeep(branchIndex));
        }
        else
        {
            menu.AddDisabledItem(new GUIContent("粘贴分支"));
        }

        // 显示菜单
        menu.ShowAsContext();
    }

    private void CopyBranch(int branchIndex)
    {
        // 复制指定的分支数据
        copiedBranch = branches[branchIndex]; // 这里复制整个分支对象
        canPaste = true; // 设置粘贴状态为可用
        Debug.Log("分支已复制");
    }

    private void PasteBranch(int branchIndex)
    {
        if (copiedBranch != null)
        {
            // 创建一个新的分支并复制数据
            var newBranch = new EventOutcomeBranch();

            // 手动复制分支的属性
            newBranch.label = copiedBranch.label;

            // 复制 ResolveConditionGroup（条件组）,不拷貝
            if (copiedBranch.matchConditions != null)
            {
                // 创建新的 ResolveConditionGroup
                newBranch.matchConditions = copiedBranch.matchConditions;
            }

            // 复制 Effects（效果），不拷貝
            if (copiedBranch.effects != null)
            {
                newBranch.effects = copiedBranch.effects;
            }

            // 将复制的分支数据赋值给目标分支
            branches[branchIndex] = newBranch;

            EditorUtility.SetDirty(this); // 标记为脏，保存修改
            AssetDatabase.SaveAssets(); // 保存更改
            Debug.Log("分支已粘贴");
        }
    }

    private void PasteBranchDeep(int branchIndex)
    {
        string newFolderForConditionEffects =
        $"Assets/SO/EventData/EventContainer/EventConditions&Effects/{eventName}";
        if (copiedBranch != null)
        {
            string uniqueID = Guid.NewGuid().ToString("N");
            // 创建一个新的分支并复制数据
            var newBranch = new EventOutcomeBranch();

            // 手动复制分支的属性
            newBranch.label = $"{copiedBranch.label}_Copied";

            // 复制 ResolveConditionGroup（条件组）, 深拷贝
            if (copiedBranch.matchConditions != null)
            {
                // 创建新的 ResolveConditionGroup
                newBranch.matchConditions = CloneSOAsset(copiedBranch.matchConditions,
                    $"{newFolderForConditionEffects}/{copiedBranch.matchConditions.name}_Copied_{uniqueID}.asset");
                newBranch.matchConditions.name = $"{copiedBranch.matchConditions.name}_Copied";
                newBranch.matchConditions.conditions = new List<EventResolveConditionSO>();
                foreach (var condition in copiedBranch.matchConditions.conditions)
                {
                    // 克隆每个条件
                    var newCondition = CloneSOAsset(condition, $"{newFolderForConditionEffects}/{condition.name}_Copied_{uniqueID}.asset");
                    newBranch.matchConditions.conditions.Add(newCondition);
                }
            }

            // 复制 Effects（效果）, 深拷贝
            if (copiedBranch.effects != null)
            {
                newBranch.effects = new List<EventEffectSO>();
                foreach (var effect in copiedBranch.effects)
                {
                    // 克隆每个效果
                    var newEffect = CloneSOAsset(effect, $"{newFolderForConditionEffects}/{effect.name}_Copied_{uniqueID}.asset");
                    newBranch.effects.Add(newEffect);
                }
            }

            // 将复制的分支数据赋值给目标分支
            branches[branchIndex] = newBranch;

            EditorUtility.SetDirty(this); // 标记为脏，保存修改
            AssetDatabase.SaveAssets(); // 保存更改
            Debug.Log("分支已粘贴");
        }
    }



}