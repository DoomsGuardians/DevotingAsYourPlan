using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;
using Naninovel.UI;
using Unity.VisualScripting;

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
    private List<BranchGroup> branchGroups = new List<BranchGroup>(); // Now using List<BranchGroup>
    private List<EventEffectSO> expiredEffects = new();
    private BranchGroup group;
    //private EventOutcomeBranch branch;

    private Vector2 scroll;
    private Dictionary<string, bool> foldoutStates = new();

    private EventNodeData loadedEventNode;

    private int highlightEffectIndex = -1;
    private double highlightStartTime = 0;

    [MenuItem("LevityTools/Event Node Editor 2.0")]
    public static void ShowWindow()
    {
        var window = GetWindow<EventNodeEditorWindow>("Levity事件编辑器2.0");
        window.minSize = new Vector2(800, 800); // 设置最小窗口尺寸
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

        // Event Node Data Section
        GUILayout.Label("加载已有事件数据", EditorStyles.boldLabel);
        loadedEventNode =
            (EventNodeData)EditorGUILayout.ObjectField("事件数据", loadedEventNode, typeof(EventNodeData), false);

        if (loadedEventNode != null && GUILayout.Button("加载该事件数据", GUILayout.Height(28)))
        {
            LoadEventNodeData(loadedEventNode);
        }

        GUILayout.Space(20);
        DrawBasicInfoSection();

        GUILayout.Space(5);
        DrawTriggerSection();

        GUILayout.Space(5);
        DrawExpiredEffectSection();

        GUILayout.Space(5);
        DrawBranchGroupSection();

        GUILayout.Space(20);
        if (loadedEventNode != null && GUILayout.Button("基于当前事件克隆为新事件", GUILayout.Height(28)))
        {
            CloneEventNodeAsNew();
        }

        GUILayout.Space(20);
        if (GUILayout.Button("保存事件数据", GUILayout.Height(28))) SaveEventNodeData();

        GUILayout.Space(20);
        DrawSummarySection();

        EditorGUILayout.EndScrollView();
    }

    // Draws the basic information section
    private void DrawBasicInfoSection()
    {
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
            // 创建一个新的GUIStyle以确保启用自动换行
            GUIStyle wordWrapStyle = new GUIStyle(GUI.skin.textArea)
            {
                wordWrap = true  // 启用自动换行
            };
            // 设置文本框的高度，并将自定义的样式应用于文本区域
            description = EditorGUILayout.TextArea(description, wordWrapStyle, GUILayout.Height(60));
            GUILayout.Space(5);
            GUILayout.Label("高级选项", EditorStyles.boldLabel);
            isUnique = EditorGUILayout.Toggle("是否为唯一事件", isUnique);
            cooldownTurns = EditorGUILayout.IntSlider("冷却回合数", cooldownTurns, 0, 10);
            decreaseFactor = EditorGUILayout.FloatField("损耗系数", decreaseFactor);
        }

        GUILayout.EndVertical();
    }

    // Draws the trigger conditions section
    private void DrawTriggerSection()
    {
        GUILayout.BeginVertical("box");
        bool triggerExpanded = GetFoldout("触发条件", false);
        triggerExpanded = EditorGUILayout.Foldout(triggerExpanded, $"触发条件", true);
        SetFoldout("触发条件", triggerExpanded);
        if (triggerExpanded)
        {
            DrawTriggerConditionGroup();
        }

        GUILayout.EndVertical();
    }

    // Draws the expired effects section
    private void DrawExpiredEffectSection()
    {
        GUILayout.BeginVertical("box");
        bool expiredEExpanded = GetFoldout("过期效果", false);
        expiredEExpanded = EditorGUILayout.Foldout(expiredEExpanded, $"过期效果", true);
        SetFoldout("过期效果", expiredEExpanded);
        if (expiredEExpanded)
        {
            DrawEffectList(expiredEffects, "ExpiredEffect", 1);
        }

        GUILayout.EndVertical();
    }

    // Draws the branch group section
    private int toRemoveGroup = -1;
    private (BranchGroup, int) toRemoveBranch = (null, -1);

    private void DrawBranchGroupSection()
    {
        if (branchGroups == null)
            return;
        GUILayout.BeginVertical("box");

        // 顶部标题栏
        bool branchGroupExpanded = GetFoldout("分支组设置", false);
        GUILayout.BeginHorizontal();
        branchGroupExpanded = EditorGUILayout.Foldout(branchGroupExpanded, "分支组设置", true);

        GUIStyle bigButtonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 20,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white }
        };

        if (GUILayout.Button("+", bigButtonStyle, GUILayout.Width(30), GUILayout.Height(28)))
        {
            var newGroup = CreateAndSaveSO<BranchGroup>($"{eventID}_分支组_{branchGroups.Count + 1}");
            newGroup.label = $"分支组_{branchGroups.Count + 1}";
            newGroup.branches = new List<EventOutcomeBranch>();
            branchGroups.Add(newGroup);
        }

        GUILayout.EndHorizontal();
        SetFoldout("分支组设置", branchGroupExpanded);

        if (branchGroupExpanded)
        {
            for (int i = 0; i < branchGroups.Count; i++)
            {
                string key = $"BranchGroup_{i}";
                bool expanded = GetFoldout(key, false);
                GUILayout.BeginVertical("box");

                // 分支组标题 + 操作按钮
                GUILayout.BeginHorizontal();
                expanded = EditorGUILayout.Foldout(expanded, $"分支组 {i + 1}", true);

                if (GUILayout.Button("↑", GUILayout.Width(30)) && i > 0)
                    (branchGroups[i], branchGroups[i - 1]) = (branchGroups[i - 1], branchGroups[i]);

                if (GUILayout.Button("↓", GUILayout.Width(30)) && i < branchGroups.Count - 1)
                    (branchGroups[i], branchGroups[i + 1]) = (branchGroups[i + 1], branchGroups[i]);

                if (GUILayout.Button("X", GUILayout.Width(30)))
                    toRemoveGroup = i;

                GUILayout.EndHorizontal();
                SetFoldout(key, expanded);

                if (expanded)
                {
                    branchGroups[i] =
                        (BranchGroup)EditorGUILayout.ObjectField("分支组资源", branchGroups[i], typeof(BranchGroup), false);

                    if (branchGroups[i] == null)
                    {
                        EditorGUILayout.HelpBox("分支组资源不能为空！请重新指定或删除。", MessageType.Error);

                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("删除这个空的分支组", GUILayout.Width(150)))
                        {
                            branchGroups.RemoveAt(i);
                            return;
                        }

                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();

                        GUILayout.Space(10); // 加点空隙，界面美观
                    }
                    else
                    {
                        for (int j = 0; j < branchGroups[i].branches.Count; j++)
                        {
                            DrawBranch(branchGroups[i], j);
                        }

                        if (GUILayout.Button("添加分支", GUILayout.Height(24)))
                        {
                            var newBranch = CreateAndSaveSO<EventOutcomeBranch>(
                                $"{eventID}_{branchGroups[i].name}_分支_{branchGroups[i].branches.Count}");
                            newBranch.label = $"新分支_{branchGroups[i].branches.Count + 1}";
                            newBranch.effects = new List<EventEffectSO>();
                            branchGroups[i].branches.Add(newBranch);
                        }
                    }
                }

                GUILayout.EndVertical();
            }
        }

        GUILayout.EndVertical();

        // 删除缓存处理
        if (toRemoveGroup >= 0)
        {
            TryDeleteAsset(branchGroups[toRemoveGroup]);
            branchGroups.RemoveAt(toRemoveGroup);
            toRemoveGroup = -1;
            return;
        }

        if (toRemoveBranch.Item1 != null)
        {
            var group = toRemoveBranch.Item1;
            int idx = toRemoveBranch.Item2;
            TryDeleteAsset(group.branches[idx]);
            group.branches.RemoveAt(idx);
            toRemoveBranch = (null, -1);
            return;
        }
    }


    // Draws the summary section at the bottom of the window
    private void DrawSummarySection()
    {
        GUILayout.BeginVertical("box");

        bool summaryExpanded = GetFoldout("总览", false);
        summaryExpanded = EditorGUILayout.Foldout(summaryExpanded, $"总览", true);
        SetFoldout("总览", summaryExpanded);
        if (summaryExpanded)
        {
            GUILayout.Label("事件信息", EditorStyles.boldLabel);
            GUILayout.Label($"事件名称: {eventName}");
            GUILayout.Label($"事件ID: {eventID}");
            GUILayout.Label($"事件描述: {description}");
            GUILayout.Label($"来源角色: {roleType}");
            GUILayout.Label($"持续时间: {duration} 回合");
            GUILayout.Label($"是否为唯一事件: {(isUnique ? "是" : "否")}");
            GUILayout.Label($"冷却回合数: {cooldownTurns}");
            GUILayout.Label($"损耗系数: {decreaseFactor}");

            GUILayout.Space(10);
            GUILayout.Label("触发条件描述", EditorStyles.boldLabel);

            if (triggerGroup != null)
            {
                foreach (var condition in triggerGroup.conditions)
                {
                    GUILayout.Label($"条件描述: {condition.Description}");
                }
            }
            else
            {
                GUILayout.Label("没有触发条件");
            }

            GUILayout.Space(10);
            GUILayout.Label("分支设置描述", EditorStyles.boldLabel);

            if (branchGroups != null && branchGroups.Count > 0)
            {
                foreach (var branchGroup in branchGroups)
                {
                    if (branchGroup == null)
                    {
                        GUILayout.Label("（空的分支组）", EditorStyles.helpBox);
                        continue; // 遇到空分支组就跳过
                    }

                    GUILayout.Label($"分支组名称: {branchGroup.label}");

                    if (branchGroup.branches != null)
                    {
                        foreach (var branch in branchGroup.branches)
                        {
                            GUILayout.BeginVertical("box");

                            GUILayout.Label($"分支名称: {branch.label}");

                            if (branch.matchConditions != null)
                            {
                                GUILayout.Label("结算条件", EditorStyles.boldLabel);
                                foreach (var condition in branch.matchConditions.conditions)
                                {
                                    GUILayout.Label($"需满足: {condition.Description}");
                                }
                            }

                            if (branch.effects != null)
                            {
                                GUILayout.Label("效果", EditorStyles.boldLabel);
                                foreach (var effect in branch.effects)
                                {
                                    GUILayout.Label($"将导致：{effect.Description}");
                                }
                            }

                            GUILayout.EndVertical();
                        }
                    }
                }
            }

            else
            {
                GUILayout.Label("没有分支设置");
            }
        }


        GUILayout.EndVertical();
    }


    private void DrawTriggerConditionGroup()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("触发条件组", EditorStyles.boldLabel);
        GUIStyle bigButtonStyle = new GUIStyle(GUI.skin.button);
        bigButtonStyle.fontSize = 20; // 字号大一点
        bigButtonStyle.fontStyle = FontStyle.Bold;
        bigButtonStyle.normal.textColor = Color.white;
        if (triggerGroup == null &&
            GUILayout.Button("+", bigButtonStyle, GUILayout.Width(30), GUILayout.Height(28)))
        {
            triggerGroup = CreateAndSaveSO<TriggerConditionGroup>($"{eventID}_触发条件组");
        }

        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        triggerGroup =
            (TriggerConditionGroup)EditorGUILayout.ObjectField("条件组资源", triggerGroup, typeof(TriggerConditionGroup),
                false);
        if (triggerGroup != null)
        {
            if (GUILayout.Button("+", bigButtonStyle, GUILayout.Width(30), GUILayout.Height(30)))
            {
                TriggerConditionSelectorWindow.Show(type =>
                {
                    var cond = (EventTriggerConditionSO)CreateAndSaveSO(type,
                        $"{eventID}_触发条件_{triggerGroup.name}_{triggerGroup.conditions.Count}");
                    triggerGroup.conditions.Add(cond);
                    EditorUtility.SetDirty(triggerGroup);
                    AssetDatabase.SaveAssets();
                });
            }

            // 删除按钮（删除整个 triggerGroup）
            if (GUILayout.Button("X", GUILayout.Width(30), GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("确认删除", "您确定要删除这个条件组吗?", "删除", "取消"))
                {
                    // 删除 triggerGroup
                    TryDeleteAsset(triggerGroup);
                    triggerGroup = null; // 清空 triggerGroup
                    EditorUtility.SetDirty(this);
                    AssetDatabase.SaveAssets();
                    Debug.Log("触发条件组已删除");
                }
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
                        (triggerGroup.conditions[i], triggerGroup.conditions[i - 1]) = (
                            triggerGroup.conditions[i - 1],
                            triggerGroup.conditions[i]);
                    }

                    if (GUILayout.Button("↓", GUILayout.Width(30), GUILayout.Height(28)) &&
                        i < triggerGroup.conditions.Count - 1)
                    {
                        (triggerGroup.conditions[i], triggerGroup.conditions[i + 1]) = (
                            triggerGroup.conditions[i + 1],
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
        if (branch == null) return;
        GUILayout.BeginHorizontal();
        GUILayout.Label("结算条件组", EditorStyles.boldLabel);
        GUIStyle bigButtonStyle = new GUIStyle(GUI.skin.button);
        bigButtonStyle.fontSize = 20; // 字号大一点
        bigButtonStyle.fontStyle = FontStyle.Bold;
        bigButtonStyle.normal.textColor = Color.white;
        if (branch.matchConditions == null &&
            GUILayout.Button("+", bigButtonStyle, GUILayout.Width(30), GUILayout.Height(28)))
        {
            branch.matchConditions = CreateAndSaveSO<ResolveConditionGroup>($"{eventID}_结算条件组_{index}");
        }

        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        branch.matchConditions = (ResolveConditionGroup)EditorGUILayout.ObjectField("条件组资源", branch.matchConditions,
            typeof(ResolveConditionGroup), false);
        if (branch.matchConditions != null)
        {
            if (GUILayout.Button("+", bigButtonStyle, GUILayout.Width(30), GUILayout.Height(28)))
            {
                ResolveConditionSelectorWindow.Show(type =>
                {
                    var cond = CreateAndSaveSO(type,
                        $"{eventID}_结算条件_{branch.label}_{branch.matchConditions.conditions.Count}");
                    branch.matchConditions.conditions.Add((EventResolveConditionSO)cond);
                    EditorUtility.SetDirty(branch.matchConditions);
                    AssetDatabase.SaveAssets();
                });
            }
        }

        GUILayout.EndHorizontal();

        if (branch.matchConditions != null)
        {
            branch.matchConditions.label = EditorGUILayout.TextField("结算条件组名称", branch.matchConditions.label);
            int removeIndex = -1;
            for (int i = 0; i < branch.matchConditions.conditions.Count; i++)
            {
                string key = $"EventEditor_Foldout_ResolveCondition_{index}_{i}";
                bool expanded = GetFoldout(key, false);
                GUILayout.BeginVertical("box");
                GUILayout.BeginHorizontal();
                expanded = EditorGUILayout.Foldout(expanded, $"{branch.matchConditions.label} 条件{i + 1}", true);
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
        if (list == null) return;
        int removeIndex = -1;
        int swapFrom = -1;
        int swapTo = -1;

        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        GUILayout.Label($"效果组", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        GUIStyle bigButtonStyle = new GUIStyle(GUI.skin.button);
        bigButtonStyle.fontSize = 20;
        bigButtonStyle.fontStyle = FontStyle.Bold;
        bigButtonStyle.normal.textColor = Color.white;

        if (GUILayout.Button("+", bigButtonStyle, GUILayout.Width(30), GUILayout.Height(30)))
        {
            EffectsSelectorWindow.Show(type =>
            {
                var effect = (EventEffectSO)CreateAndSaveSO(type, $"{eventID}_{prefix}_Effect_{list.Count}");
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

    private void DrawBranch(BranchGroup group, int j)
    {
        string key = $"Branch_{group.name}_{j}";
        bool expanded = GetFoldout(key, false);
        GUILayout.BeginVertical("box");

        GUILayout.BeginHorizontal();
        expanded = EditorGUILayout.Foldout(expanded, $"分支: {group.branches[j].label}", true);

        if (GUILayout.Button("↑", GUILayout.Width(30)) && j > 0)
            (group.branches[j], group.branches[j - 1]) = (group.branches[j - 1], group.branches[j]);

        if (GUILayout.Button("↓", GUILayout.Width(30)) && j < group.branches.Count - 1)
            (group.branches[j], group.branches[j + 1]) = (group.branches[j + 1], group.branches[j]);

        if (GUILayout.Button("X", GUILayout.Width(30)))
            toRemoveBranch = (group, j);

        GUILayout.EndHorizontal();
        SetFoldout(key, expanded);

        if (expanded)
        {
            group.branches[j] =
                (EventOutcomeBranch)EditorGUILayout.ObjectField("分支资源", group.branches[j],
                    typeof(EventOutcomeBranch),
                    false);
            group.branches[j].label = EditorGUILayout.TextField("分支名称", group.branches[j].label);
            GUILayout.Space(5);
            DrawResolveConditionGroup(group.branches[j], j, 2);
            GUILayout.Space(5);
            DrawEffectList(group.branches[j].effects ??= new List<EventEffectSO>(), $"{group.name}_Effect_{j}", 2);
        }

        GUILayout.EndVertical();
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

        // 保存每个分支组（BranchGroup）为独立的资产
        List<BranchGroup> validBranchGroups = new List<BranchGroup>();

        foreach (var branchGroup in branchGroups)
        {
            if (branchGroup == null) continue;
            string branchGroupPath = $"{path}/{branchGroup.name}_BranchGroup.asset";

            // 如果这个 branchGroup 本身没有保存过资产，需要保存
            if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(branchGroup)))
            {
                AssetDatabase.CreateAsset(branchGroup, branchGroupPath);
            }

            validBranchGroups.Add(branchGroup);
        }

        node.branchGroups = validBranchGroups;

        // 如果是新创建的 EventNodeData，则保存它
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
        else if (so is BranchGroup branchGroup)
        {
            branchGroup.branches = new List<EventOutcomeBranch>();
        }

        AssetDatabase.CreateAsset(so, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return AssetDatabase.LoadAssetAtPath<T>(path);
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
        branchGroups = node.branchGroups != null
            ? new List<BranchGroup>(node.branchGroups)
            : new List<BranchGroup>();
    }

    private void CloneEventNodeAsNew()
    {
        if (loadedEventNode == null)
        {
            Debug.LogError("没有加载的事件可供克隆");
            return;
        }

        // 使用当前窗口中填写的 eventName 和 eventID（而不是 loadedEventNode 的值）
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

        // 克隆过期效果
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

        // 克隆分支组及内部分支
        newNode.branchGroups = new List<BranchGroup>();
        for (int i = 0; i < loadedEventNode.branchGroups.Count; i++)
        {
            var branchGroup = loadedEventNode.branchGroups[i];
            var newBranchGroup = ScriptableObject.CreateInstance<BranchGroup>();
            newBranchGroup.name = $"{eventID}_BranchGroup_{i}";

            // 克隆每个 EventOutcomeBranch 并添加到新分支组中
            newBranchGroup.branches = new List<EventOutcomeBranch>();
            foreach (var branch in branchGroup.branches)
            {
                var newBranch = CreateAndSaveSO<EventOutcomeBranch>($"{eventID}_Branch_{Guid.NewGuid().ToString("N")}");

                newBranch.label = branch.label;
                newBranch.effects = new List<EventEffectSO>();

                if (branch.matchConditions != null)
                {
                    var newResolveGroup = CloneSOAsset(branch.matchConditions,
                        $"{newFolderForConditionEffects}/{branch.matchConditions.name}_Copy.asset");
                    newBranch.matchConditions = newResolveGroup;
                }

                foreach (var effect in branch.effects)
                {
                    var newEffect = CloneSOAsset(effect,
                        $"{newFolderForConditionEffects}/{effect.name}_Copy.asset");
                    newBranch.effects.Add(newEffect);
                }

                newBranchGroup.branches.Add(newBranch);
            }

            // 保存新的 BranchGroup 资产
            string branchGroupPath = $"{newFolderForConditionEffects}/{newBranchGroup.name}.asset";
            AssetDatabase.CreateAsset(newBranchGroup, branchGroupPath);
            newNode.branchGroups.Add(newBranchGroup);
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

        // 同步引用到 UI 当前状态
        triggerGroup = newNode.triggerConditions;
        expiredEffects = newNode.expiredEffects;
        branchGroups = newNode.branchGroups;
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
        // 获取分支组中的特定分支（EventOutcomeBranch）
        if (branchIndex >= 0 && branchIndex < branchGroups.Count)
        {
            var branchGroup = branchGroups[branchIndex];
            if (branchGroup.branches.Count > 0)
            {
                copiedBranch = branchGroup.branches[0]; // 假设复制的是第一个分支
                canPaste = true; // 设置粘贴状态为可用
                Debug.Log("分支已复制");
            }
        }
    }

    private void PasteBranch(int branchIndex)
    {
        if (copiedBranch != null && branchIndex >= 0 && branchIndex < branchGroups.Count)
        {
            var branchGroup = branchGroups[branchIndex];

            // 创建一个新的 EventOutcomeBranch
            var newBranch = new EventOutcomeBranch
            {
                label = copiedBranch.label,
                matchConditions = copiedBranch.matchConditions,
                effects = copiedBranch.effects
            };

            // 将复制的分支添加到目标分支组中
            branchGroup.branches.Add(newBranch);

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

            // 获取分支组
            if (branchIndex >= 0 && branchIndex < branchGroups.Count)
            {
                var branchGroup = branchGroups[branchIndex];

                // 创建一个新的 EventOutcomeBranch
                var newBranch = new EventOutcomeBranch
                {
                    label = $"{copiedBranch.label}_Copied"
                };

                // 深拷贝 ResolveConditionGroup（条件组）
                if (copiedBranch.matchConditions != null)
                {
                    newBranch.matchConditions = CloneSOAsset(copiedBranch.matchConditions,
                        $"{newFolderForConditionEffects}/{copiedBranch.matchConditions.name}_Copied_{uniqueID}.asset");
                    newBranch.matchConditions.name = $"{copiedBranch.matchConditions.name}_Copied";
                    newBranch.matchConditions.conditions = new List<EventResolveConditionSO>();

                    // 深拷贝每个条件
                    foreach (var condition in copiedBranch.matchConditions.conditions)
                    {
                        var newCondition = CloneSOAsset(condition,
                            $"{newFolderForConditionEffects}/{condition.name}_Copied_{uniqueID}.asset");
                        newBranch.matchConditions.conditions.Add(newCondition);
                    }
                }

                // 深拷贝 Effects（效果）
                if (copiedBranch.effects != null)
                {
                    newBranch.effects = new List<EventEffectSO>();
                    foreach (var effect in copiedBranch.effects)
                    {
                        var newEffect = CloneSOAsset(effect,
                            $"{newFolderForConditionEffects}/{effect.name}_Copied_{uniqueID}.asset");
                        newBranch.effects.Add(newEffect);
                    }
                }

                // 将深拷贝后的分支添加到目标分支组中
                branchGroup.branches.Add(newBranch);

                EditorUtility.SetDirty(this); // 标记为脏，保存修改
                AssetDatabase.SaveAssets(); // 保存更改
                Debug.Log("分支已深拷贝并粘贴");
            }
        }
    }
}