using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;

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

    [MenuItem("LevityTools/Event Node Editor")]
    public static void ShowWindow()
    {
        var window = GetWindow<EventNodeEditorWindow>("Levity事件编辑器");
        window.minSize = new Vector2(800, 600); // 设置最小窗口尺寸
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

        GUILayout.Label("事件基本信息", EditorStyles.boldLabel);
        eventName = EditorGUILayout.TextField("事件名称", eventName);
        eventID = EditorGUILayout.TextField("事件ID", eventID);
        roleType = (RoleType)EditorGUILayout.EnumPopup("来源角色", roleType);
        icon = (Sprite)EditorGUILayout.ObjectField("图标", icon, typeof(Sprite), false);
        duration = EditorGUILayout.IntField("持续时间", duration);
        description = EditorGUILayout.TextArea(description, GUILayout.Height(60));

        GUILayout.Space(10);
        GUILayout.Label("高级选项", EditorStyles.boldLabel);
        isUnique = EditorGUILayout.Toggle("是否为唯一事件", isUnique);
        cooldownTurns = EditorGUILayout.IntSlider("冷却回合数", cooldownTurns, 0, 10);
        decreaseFactor = EditorGUILayout.FloatField("损耗系数", decreaseFactor);

        DrawTriggerConditionGroup();

        GUILayout.Space(20);
        GUILayout.Label("过期效果", EditorStyles.boldLabel);
        DrawEffectList(expiredEffects, "ExpiredEffect", 1);

        GUILayout.Space(20);
        GUILayout.Label("分支设置", EditorStyles.boldLabel);
        if (GUILayout.Button("添加分支", GUILayout.Height(28))) branches.Add(new EventOutcomeBranch());

        int branchToRemove = -1;

        for (int i = 0; i < branches.Count; i++)
        {
            var branch = branches[i];
            string key = $"EventEditor_Foldout_Branch_{i}";
            bool expanded = GetFoldout(key, false);
            expanded = EditorGUILayout.Foldout(expanded, $"分支 {i + 1}：{branch.label}", true);
            SetFoldout(key, expanded);

            if (expanded)
            {
                EditorGUI.indentLevel++;
                branch.label = EditorGUILayout.TextField("分支名称", branch.label);
                GUILayout.BeginHorizontal();

                GUILayout.Space(5);
                if (GUILayout.Button("↑", GUILayout.Width(30), GUILayout.Height(28)) && i > 0)
                {
                    (branches[i], branches[i - 1]) = (branches[i - 1], branches[i]);
                }

                if (GUILayout.Button("↓", GUILayout.Width(30), GUILayout.Height(28)) && i < branches.Count - 1)
                {
                    (branches[i], branches[i + 1]) = (branches[i + 1], branches[i]);
                }

                if (GUILayout.Button("移除该分支", GUILayout.Height(28)))
                {
                    branchToRemove = i;
                }

                GUILayout.EndHorizontal();

                EditorGUI.indentLevel++;
                DrawResolveConditionGroup(branch, i, 2);
                DrawEffectList(branch.effects ??= new List<EventEffectSO>(), $"Branch{i}_Effect", 2);
                EditorGUI.indentLevel--;
                GUILayout.Space(5); // 增加按钮左边间距
                if (GUILayout.Button("移除该分支", GUILayout.Height(28))) branchToRemove = i;
                EditorGUI.indentLevel--;
            }
        }

        if (branchToRemove >= 0)
        {
            var branch = branches[branchToRemove];
            TryDeleteAsset(branch.matchConditions);
            if (branch.effects != null)
                foreach (var e in branch.effects)
                    TryDeleteAsset(e);
            branches.RemoveAt(branchToRemove);
        }

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
        GUILayout.Space(20);
        GUILayout.Label("触发条件组", EditorStyles.boldLabel);
        if (triggerGroup == null && GUILayout.Button("新建 Trigger 条件组", GUILayout.Height(28)))
        {
            triggerGroup = CreateAndSaveSO<TriggerConditionGroup>($"{eventID}_TriggerGroup");
        }

        triggerGroup =
            (TriggerConditionGroup)EditorGUILayout.ObjectField("条件组资源", triggerGroup, typeof(TriggerConditionGroup),
                false);

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
                    triggerGroup.conditions[i] =
                        (EventTriggerConditionSO)EditorGUILayout.ObjectField(triggerGroup.conditions[i],
                            typeof(EventTriggerConditionSO), false);
                    if (triggerGroup.conditions[i] != null)
                    {
                        var editor = Editor.CreateEditor(triggerGroup.conditions[i]);
                        editor?.OnInspectorGUI();
                    }

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(5);
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

                    if (GUILayout.Button("移除条件", GUILayout.Height(28))) removeIndex = i;

                    GUILayout.EndHorizontal();
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

            GUILayout.Space(20); // 增加垂直间距

            // 按钮排布优化：使用垂直布局并添加适当间距
            GUILayout.BeginVertical("box");
            GUILayout.Space(5);
            if (GUILayout.Button("添加 手牌数量范围 触发条件", GUILayout.Height(28)))
            {
                var cond = CreateAndSaveSO<HandCardCountTriggerCondition>(
                    $"TriggerCond_{eventID}_{triggerGroup.name}_手牌数量范围_{triggerGroup.conditions.Count}");
                triggerGroup.conditions.Add(cond);
                EditorUtility.SetDirty(triggerGroup);
                AssetDatabase.SaveAssets();
            }

            GUILayout.Space(10); // 按钮间距

            if (GUILayout.Button("添加 卡牌范围过滤 触发条件", GUILayout.Height(28)))
            {
                var cond = CreateAndSaveSO<CardCountRangeFilterTriggerCondition>(
                    $"TriggerCond_{eventID}_{triggerGroup.name}_卡牌范围过滤_{triggerGroup.conditions.Count}");
                triggerGroup.conditions.Add(cond);
                EditorUtility.SetDirty(triggerGroup);
                AssetDatabase.SaveAssets();
            }

            GUILayout.Space(10); // 按钮间距

            if (GUILayout.Button("添加 特定卡牌存在 触发条件", GUILayout.Height(28)))
            {
                var cond = CreateAndSaveSO<SpecificCardExistsTriggerCondition>(
                    $"TriggerCond_{eventID}_{triggerGroup.name}_特定卡牌存在_{triggerGroup.conditions.Count}");
                triggerGroup.conditions.Add(cond);
                EditorUtility.SetDirty(triggerGroup);
                AssetDatabase.SaveAssets();
            }

            GUILayout.Space(10); // 按钮间距

            if (GUILayout.Button("添加 角色属性范围 触发条件", GUILayout.Height(28)))
            {
                var cond = CreateAndSaveSO<RoleStatRangeTriggerCondition>(
                    $"TriggerCond_{eventID}_{triggerGroup.name}_角色属性范围_{triggerGroup.conditions.Count}");
                triggerGroup.conditions.Add(cond);
                EditorUtility.SetDirty(triggerGroup);
                AssetDatabase.SaveAssets();
            }

            GUILayout.Space(10); // 按钮间距

            if (GUILayout.Button("添加 角色属性范围曲线映射 触发条件", GUILayout.Height(28)))
            {
                var cond = CreateAndSaveSO<RoleStatRangeCurveMapTriggerCondition>(
                    $"TriggerCond_{eventID}_{triggerGroup.name}_角色属性范围曲线映射_{triggerGroup.conditions.Count}");
                triggerGroup.conditions.Add(cond);
                EditorUtility.SetDirty(triggerGroup);
                AssetDatabase.SaveAssets();
            }

            GUILayout.Space(10); // 按钮间距

            if (GUILayout.Button("添加 回合数范围 触发条件", GUILayout.Height(28)))
            {
                var cond = CreateAndSaveSO<TurnCountTriggerCondition>(
                    $"TriggerCond_{eventID}_{triggerGroup.name}_回合数范围_{triggerGroup.conditions.Count}");
                triggerGroup.conditions.Add(cond);
                EditorUtility.SetDirty(triggerGroup);
                AssetDatabase.SaveAssets();
            }

            GUILayout.EndVertical(); // 结束垂直布局
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

        branch.matchConditions = (ResolveConditionGroup)EditorGUILayout.ObjectField("条件组资源", branch.matchConditions,
            typeof(ResolveConditionGroup), false);

        if (branch.matchConditions != null)
        {
            int removeIndex = -1;
            for (int i = 0; i < branch.matchConditions.conditions.Count; i++)
            {
                string key = $"EventEditor_Foldout_ResolveCondition_{index}_{i}";
                bool expanded = GetFoldout(key, false);
                expanded = EditorGUILayout.Foldout(expanded, $"Resolve 条件 {i + 1}", true);
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

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(5);
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
                    if (GUILayout.Button("移除条件", GUILayout.Height(28))) removeIndex = i;

                    GUILayout.EndHorizontal();
                    EditorGUI.indentLevel--;
                }
            }

            if (removeIndex >= 0)
            {
                TryDeleteAsset(branch.matchConditions.conditions[removeIndex]);
                branch.matchConditions.conditions.RemoveAt(removeIndex);
                EditorUtility.SetDirty(branch.matchConditions);
                AssetDatabase.SaveAssets();
            }

            GUILayout.Space(20); // 增加一些间距

            // 结算条件按钮（垂直排列）
            GUILayout.BeginVertical("box");

            if (GUILayout.Button("添加 卡牌匹配范围 结算条件", GUILayout.Height(28)))
            {
                var cond = CreateAndSaveSO<CardMatchRangeResolveCondition>(
                    $"ResolveCond_{eventID}_{branch.label}_卡牌匹配范围_{branch.matchConditions.conditions.Count}");
                branch.matchConditions.conditions.Add(cond);
                EditorUtility.SetDirty(branch.matchConditions);
                AssetDatabase.SaveAssets();
            }

            GUILayout.Space(10); // 按钮间距

            if (GUILayout.Button("添加 角色属性范围 结算条件", GUILayout.Height(28)))
            {
                var cond = CreateAndSaveSO<RoleStatRangeResolveCondition>(
                    $"ResolveCond_{eventID}_{branch.label}_角色属性范围_{branch.matchConditions.conditions.Count}");
                branch.matchConditions.conditions.Add(cond);
                EditorUtility.SetDirty(branch.matchConditions);
                AssetDatabase.SaveAssets();
            }

            GUILayout.EndVertical(); // 结束垂直布局
        }
    }


    private void DrawEffectList(List<EventEffectSO> list, string prefix, int indent)
    {
        int removeIndex = -1;
        int swapFrom = -1;
        int swapTo = -1;

        GUILayout.BeginVertical("box");

        // 循环遍历效果
        for (int i = 0; i < list.Count; i++)
        {
            string key = $"EventEditor_Foldout_Effect_{prefix}_{i}";
            bool expanded = GetFoldout(key, false);
            expanded = EditorGUILayout.Foldout(expanded, $"效果 {i + 1}", true);
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

                // 顺序调整按钮开始
                GUILayout.BeginHorizontal();
                GUILayout.Space(5);
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

                if (GUILayout.Button("移除效果", GUILayout.Height(28)))
                {
                    removeIndex = i;
                }

                GUILayout.EndHorizontal();

                if (GUILayout.Button("移除效果", GUILayout.Height(28))) removeIndex = i;
                EditorGUI.indentLevel--;
            }
        }

        // 循环结束后统一处理交换
        if (swapFrom >= 0 && swapTo >= 0)
        {
            (list[swapFrom], list[swapTo]) = (list[swapTo], list[swapFrom]);
        }

        // 移除效果
        if (removeIndex >= 0)
        {
            TryDeleteAsset(list[removeIndex]);
            list.RemoveAt(removeIndex);
        }

        // 按钮排布优化：使用垂直布局并添加适当间距
        GUILayout.Space(10); // 增加间距，使按钮之间不拥挤

        GUILayout.BeginVertical("box");

        // 添加不同类型的效果按钮
        if (GUILayout.Button("添加 给予随机卡牌 效果", GUILayout.Height(28)))
        {
            var effect = CreateAndSaveSO<GiveFilteredRandomCardEffect>($"{eventName}_{prefix}_给予随机卡牌_{list.Count}");
            list.Add(effect);
        }

        GUILayout.Space(10); // 按钮间距

        // 添加不同类型的效果按钮
        if (GUILayout.Button("添加 给予卡牌 效果", GUILayout.Height(28)))
        {
            var effect = CreateAndSaveSO<GiveSpecificCardEffect>($"{eventName}_{prefix}_给予卡牌_{list.Count}");
            list.Add(effect);
        }

        GUILayout.Space(10); // 按钮间距

        if (GUILayout.Button("添加 后续事件 效果", GUILayout.Height(28)))
        {
            var effect = CreateAndSaveSO<TriggerEventEffect>($"{eventName}_{prefix}_后续事件_{list.Count}");
            list.Add(effect);
        }

        GUILayout.Space(10); // 按钮间距

        if (GUILayout.Button("添加 角色属性 效果", GUILayout.Height(28)))
        {
            var effect = CreateAndSaveSO<ChangeStatEffect>($"{eventName}_{prefix}_角色属性_{list.Count}");
            list.Add(effect);
        }

        GUILayout.Space(10); // 按钮间距

        if (GUILayout.Button("添加 角色历史属性 效果", GUILayout.Height(28)))
        {
            var effect = CreateAndSaveSO<ChangeStatHistorialEffect>($"{eventName}_{prefix}_角色历史属性_{list.Count}");
            list.Add(effect);
        }

        GUILayout.Space(10); // 按钮间距

        if (GUILayout.Button("添加 播放台本 效果", GUILayout.Height(28)))
        {
            var effect = CreateAndSaveSO<PlayScenarioEffect>($"{eventName}_{prefix}_播放台本_{list.Count}");
            list.Add(effect);
        }

        GUILayout.Space(10); // 按钮间距

        if (GUILayout.Button("添加 去除卡牌 效果", GUILayout.Height(28)))
        {
            var effect = CreateAndSaveSO<RemoveFilteredCardEffect>($"{eventName}_{prefix}_去除卡牌_{list.Count}");
            list.Add(effect);
        }

        GUILayout.Space(10); // 按钮间距

        if (GUILayout.Button("添加 在事件上展示结果 效果", GUILayout.Height(28)))
        {
            var effect = CreateAndSaveSO<ShowEndEffect>($"{eventName}_{prefix}_在事件上展示结果_{list.Count}");
            list.Add(effect);
        }

        GUILayout.Space(10); // 按钮间距

        if (GUILayout.Button("添加 阻止自身再次触发 效果", GUILayout.Height(28)))
        {
            var effect = CreateAndSaveSO<PreventSelfTriggerEffect>($"{eventName}_{prefix}_阻止自身再次触发_{list.Count}");
            list.Add(effect);
        }

        GUILayout.Space(10); // 按钮间距

        if (GUILayout.Button("添加 Debug 效果", GUILayout.Height(28)))
        {
            var effect = CreateAndSaveSO<DebugEffect>($"{eventName}_{prefix}_Debug_{list.Count}");
            list.Add(effect);
        }


        GUILayout.EndVertical(); // 结束垂直布局
        GUILayout.EndVertical(); // 结束外部垂直布局
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

    private T CreateAndSaveSO<T>(string fileName) where T : ScriptableObject
    {
        string path = $"Assets/SO/EventData/EventContainer/EventConditions&Effects/{eventName}";
        Directory.CreateDirectory(path);
        string assetPath = $"{path}/{fileName}.asset";

        T so = ScriptableObject.CreateInstance<T>();

        if (so is TriggerConditionGroup trigger)
            trigger.conditions = new List<EventTriggerConditionSO>();
        else if (so is ResolveConditionGroup resolve)
            resolve.conditions = new List<EventResolveConditionSO>();

        AssetDatabase.CreateAsset(so, assetPath);
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
}