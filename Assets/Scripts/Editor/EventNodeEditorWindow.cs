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

    private TriggerConditionGroup triggerGroup;
    private List<EventOutcomeBranch> branches = new();
    private List<EventEffectSO> expiredEffects = new();

    private Vector2 scroll;
    private Dictionary<string, bool> foldoutStates = new();

    [MenuItem("LevityTools/Event Node Editor")]
    public static void ShowWindow()
    {
        GetWindow<EventNodeEditorWindow>("Levity事件编辑器");
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

        DrawTriggerConditionGroup();

        GUILayout.Space(20);
        GUILayout.Label("过期效果", EditorStyles.boldLabel);
        DrawEffectList(expiredEffects, "ExpiredEffect", 1);

        GUILayout.Space(20);
        GUILayout.Label("分支设置", EditorStyles.boldLabel);
        if (GUILayout.Button("添加分支")) branches.Add(new EventOutcomeBranch());

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
                EditorGUI.indentLevel++;
                DrawResolveConditionGroup(branch, i, 2);
                DrawEffectList(branch.effects ??= new List<EventEffectSO>(), $"Branch{i}_Effect", 2);
                EditorGUI.indentLevel--;
                if (GUILayout.Button("移除该分支")) branchToRemove = i;
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
        if (GUILayout.Button("保存事件数据")) SaveEventNodeData();

        EditorGUILayout.EndScrollView();
    }

    private void DrawTriggerConditionGroup()
{
    GUILayout.Space(20);
    GUILayout.Label("触发条件组", EditorStyles.boldLabel);
    if (triggerGroup == null && GUILayout.Button("新建 Trigger 条件组"))
    {
        triggerGroup = CreateAndSaveSO<TriggerConditionGroup>($"{eventID}_TriggerGroup");
    }

    triggerGroup = (TriggerConditionGroup)EditorGUILayout.ObjectField("条件组资源", triggerGroup, typeof(TriggerConditionGroup), false);

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
                triggerGroup.conditions[i] = (EventTriggerConditionSO)EditorGUILayout.ObjectField(triggerGroup.conditions[i], typeof(EventTriggerConditionSO), false);
                if (triggerGroup.conditions[i] != null)
                {
                    var editor = Editor.CreateEditor(triggerGroup.conditions[i]);
                    editor?.OnInspectorGUI();
                }
                if (GUILayout.Button("移除条件")) removeIndex = i;
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
        
        if (GUILayout.Button("添加 手牌数量范围 触发条件"))
        {
            var cond = CreateAndSaveSO<HandCardCountTriggerCondition>($"TriggerCond_{eventID}_{triggerGroup.conditions.Count}");
            triggerGroup.conditions.Add(cond);
            EditorUtility.SetDirty(triggerGroup);
            AssetDatabase.SaveAssets();
        }

        GUILayout.Space(10); // 按钮间距

        if (GUILayout.Button("添加 卡牌范围过滤 触发条件"))
        {
            var cond = CreateAndSaveSO<CardCountRangeFilterTriggerCondition>($"TriggerCond_{eventID}_{triggerGroup.conditions.Count}");
            triggerGroup.conditions.Add(cond);
            EditorUtility.SetDirty(triggerGroup);
            AssetDatabase.SaveAssets();
        }

        GUILayout.Space(10); // 按钮间距

        if (GUILayout.Button("添加 特定卡牌存在 触发条件"))
        {
            var cond = CreateAndSaveSO<SpecificCardExistsTriggerCondition>($"TriggerCond_{eventID}_{triggerGroup.conditions.Count}");
            triggerGroup.conditions.Add(cond);
            EditorUtility.SetDirty(triggerGroup);
            AssetDatabase.SaveAssets();
        }

        GUILayout.Space(10); // 按钮间距

        if (GUILayout.Button("添加 角色属性范围 触发条件"))
        {
            var cond = CreateAndSaveSO<RoleStatRangeTriggerCondition>($"TriggerCond_{eventID}_{triggerGroup.conditions.Count}");
            triggerGroup.conditions.Add(cond);
            EditorUtility.SetDirty(triggerGroup);
            AssetDatabase.SaveAssets();
        }

        GUILayout.Space(10); // 按钮间距

        if (GUILayout.Button("添加 角色属性范围曲线映射 触发条件"))
        {
            var cond = CreateAndSaveSO<RoleStatRangeCurveMapTriggerCondition>($"TriggerCond_{eventID}_{triggerGroup.conditions.Count}");
            triggerGroup.conditions.Add(cond);
            EditorUtility.SetDirty(triggerGroup);
            AssetDatabase.SaveAssets();
        }

        GUILayout.Space(10); // 按钮间距

        if (GUILayout.Button("添加 回合数范围 触发条件"))
        {
            var cond = CreateAndSaveSO<TurnCountTriggerCondition>($"TriggerCond_{eventID}_{triggerGroup.conditions.Count}");
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
    if (branch.matchConditions == null && GUILayout.Button("新建 Resolve 条件组"))
    {
        branch.matchConditions = CreateAndSaveSO<ResolveConditionGroup>($"{eventID}_ResolveGroup_{index}");
    }

    branch.matchConditions = (ResolveConditionGroup)EditorGUILayout.ObjectField("条件组资源", branch.matchConditions, typeof(ResolveConditionGroup), false);

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
                branch.matchConditions.conditions[i] = (EventResolveConditionSO)EditorGUILayout.ObjectField(branch.matchConditions.conditions[i], typeof(EventResolveConditionSO), false);
                if (branch.matchConditions.conditions[i] != null)
                {
                    var editor = Editor.CreateEditor(branch.matchConditions.conditions[i]);
                    editor?.OnInspectorGUI();
                }
                if (GUILayout.Button("移除条件")) removeIndex = i;
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

        if (GUILayout.Button("添加 卡牌匹配范围 结算条件"))
        {
            var cond = CreateAndSaveSO<CardMatchRangeResolveCondition>($"ResolveCond_{eventID}_{branch.matchConditions.conditions.Count}");
            branch.matchConditions.conditions.Add(cond);
            EditorUtility.SetDirty(branch.matchConditions);
            AssetDatabase.SaveAssets();
        }

        GUILayout.Space(10); // 按钮间距

        if (GUILayout.Button("添加 角色属性范围 结算条件"))
        {
            var cond = CreateAndSaveSO<RoleStatRangeResolveCondition>($"ResolveCond_{eventID}_{branch.matchConditions.conditions.Count}");
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
            if (GUILayout.Button("移除效果")) removeIndex = i;
            EditorGUI.indentLevel--;
        }
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
    if (GUILayout.Button("添加 GiveCard 效果"))
    {
        var effect = CreateAndSaveSO<GiveFilteredRandomCardEffect>($"{eventName}_{prefix}_GiveCard_{list.Count}");
        list.Add(effect);
    }

    GUILayout.Space(10); // 按钮间距

    if (GUILayout.Button("添加 TriggerEvent 效果"))
    {
        var effect = CreateAndSaveSO<TriggerEventEffect>($"{eventName}_{prefix}_TriggerEvent_{list.Count}");
        list.Add(effect);
    }

    GUILayout.Space(10); // 按钮间距

    if (GUILayout.Button("添加 ChangeStat 效果"))
    {
        var effect = CreateAndSaveSO<ChangeStatEffect>($"{eventName}_{prefix}_ChangeStat_{list.Count}");
        list.Add(effect);
    }

    GUILayout.Space(10); // 按钮间距

    if (GUILayout.Button("添加 PlayScenario 效果"))
    {
        var effect = CreateAndSaveSO<PlayScenarioEffect>($"{eventName}_{prefix}_PlayScenario_{list.Count}");
        list.Add(effect);
    }

    GUILayout.Space(10); // 按钮间距

    if (GUILayout.Button("添加 RemoveFilteredCard 效果"))
    {
        var effect = CreateAndSaveSO<RemoveFilteredCardEffect>($"{eventName}_{prefix}_RemoveFilteredCard_{list.Count}");
        list.Add(effect);
    }

    GUILayout.Space(10); // 按钮间距

    if (GUILayout.Button("添加 Debug 效果"))
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

        var node = ScriptableObject.CreateInstance<EventNodeData>();
        node.eventName = eventName;
        node.eventID = eventID;
        node.sourceRole = roleType;
        node.description = description;
        node.icon = icon;
        node.duration = duration;
        node.isUnique = isUnique;
        node.cooldownTurns = cooldownTurns;
        node.triggerConditions = triggerGroup;
        node.expiredEffects = expiredEffects;
        node.outcomeBranches = branches;

        AssetDatabase.CreateAsset(node, $"{path}/{eventID}_EventNode.asset");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("保存成功", $"事件及资源保存到：{path}", "OK");
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
}
