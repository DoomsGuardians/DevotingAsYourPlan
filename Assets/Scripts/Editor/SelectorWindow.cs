using UnityEditor;
using UnityEngine;
using System;

public class ResolveConditionSelectorWindow : EditorWindow
{
    public Action<Type> onSelect;

    private Vector2 scroll;

    public static void Show(Action<Type> onSelectCallback)
    {
        var window = CreateInstance<ResolveConditionSelectorWindow>();
        window.titleContent = new GUIContent("选择结算条件类型");
        window.onSelect = onSelectCallback;
        window.position = new Rect(Screen.width / 2, Screen.height / 2, 150, 100);
        window.ShowUtility(); // 模态窗口
    }

    private void OnGUI()
    {
        GUILayout.Label("选择要添加的条件类型：", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();

        DrawTypeButton<CardMatchRangeResolveCondition>("卡牌匹配范围");
        DrawTypeButton<RoleStatRangeResolveCondition>("角色属性范围");
        DrawTypeButton<RandomNumResolveCondition>("随机数条件");

        GUILayout.EndHorizontal();
    }

    private void DrawTypeButton<T>(string displayName) where T : ScriptableObject
    {
        if (GUILayout.Button(displayName, GUILayout.Width(150), GUILayout.Height(100)))
        {
            onSelect?.Invoke(typeof(T));
            Close();
        }
        GUILayout.Space(10);
    }
}


public class TriggerConditionSelectorWindow : EditorWindow
{
    public Action<Type> onSelect;

    private Vector2 scroll;

    public static void Show(Action<Type> onSelectCallback)
    {
        var window = CreateInstance<TriggerConditionSelectorWindow>();
        window.titleContent = new GUIContent("选择觸發条件类型");
        window.onSelect = onSelectCallback;
        window.position = new Rect(Screen.width / 2, Screen.height / 2, 420, 100);
        window.ShowUtility(); // 模态窗口
    }

    private void OnGUI()
    {
        GUILayout.Label("选择要添加的条件类型：", EditorStyles.boldLabel);
        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        DrawTypeButton<HandCardCountTriggerCondition>("添加 手牌数量范围 触发条件");
        DrawTypeButton<CardCountRangeFilterTriggerCondition>("添加 卡牌范围过滤 触发条件");
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        DrawTypeButton<SpecificCardExistsTriggerCondition>("添加 特定卡牌存在 触发条件");
        DrawTypeButton<RoleStatRangeTriggerCondition>("添加 角色属性范围 触发条件");
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        DrawTypeButton<RoleStatRangeCurveMapTriggerCondition>("添加 角色属性范围曲线映射 触发条件");
        DrawTypeButton<TurnCountTriggerCondition>("添加 回合数范围 触发条件");
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }

    private void DrawTypeButton<T>(string displayName) where T : ScriptableObject
    {
        if (GUILayout.Button(displayName, GUILayout.Width(200), GUILayout.Height(50)))
        {
            onSelect?.Invoke(typeof(T));
            Close();
        }
        GUILayout.Space(10);
    }
}


public class EffectsSelectorWindow : EditorWindow
{
    public Action<Type> onSelect;

    private Vector2 scroll;

    public static void Show(Action<Type> onSelectCallback)
    {
        var window = CreateInstance<EffectsSelectorWindow>();
        window.titleContent = new GUIContent("选择效果类型");
        window.onSelect = onSelectCallback;
        window.position = new Rect(Screen.width / 2, Screen.height / 2, 400, 400);
        window.ShowUtility(); // 模态窗口
    }

    private void OnGUI()
    {
        GUILayout.Label("选择要添加的条件类型：", EditorStyles.boldLabel);
        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        DrawTypeButton<GiveFilteredRandomCardEffect>("添加 给予随机卡牌 效果");
        DrawTypeButton<GiveSpecificCardEffect>("添加 给予卡牌 效果");
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        DrawTypeButton<SetSpecificCardEffect>("添加 修改特定卡牌 效果");
        DrawTypeButton<TriggerEventEffect>("添加 后续事件 效果");
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        DrawTypeButton<SettleCardsInEventEffect>("添加 用于事件的卡牌 效果");
        DrawTypeButton<ChangeStatEffect>("添加 角色属性 效果");
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        DrawTypeButton<ChangeStatHistorialEffect>("添加 角色历史属性 效果");
        DrawTypeButton<PlayScenarioEffect>("添加 播放台本 效果");
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        DrawTypeButton<RemoveFilteredCardEffect>("添加 去除卡牌 效果");
        DrawTypeButton<ShowEndEffect>("添加 在事件上展示结果 效果");
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        DrawTypeButton<PreventSelfTriggerEffect>("添加 阻止自身再次触发 效果");
        DrawTypeButton<DebugEffect>("添加 Debug 效果");
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }

    private void DrawTypeButton<T>(string displayName) where T : ScriptableObject
    {
        if (GUILayout.Button(displayName, GUILayout.Width(200), GUILayout.Height(50)))
        {
            onSelect?.Invoke(typeof(T));
            Close();
        }
        GUILayout.Space(10);
    }
}