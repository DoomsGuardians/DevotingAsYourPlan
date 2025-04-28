using UnityEditor;
using UnityEngine;
using System;

public class ConditionSelectorWindow : EditorWindow
{
    public Action<Type> onSelect;

    private Vector2 scroll;

    public static void Show(Action<Type> onSelectCallback)
    {
        var window = CreateInstance<ConditionSelectorWindow>();
        window.titleContent = new GUIContent("选择结算条件类型");
        window.onSelect = onSelectCallback;
        window.position = new Rect(Screen.width / 2, Screen.height / 2, 200, 300);
        window.ShowUtility(); // 模态窗口
    }

    private void OnGUI()
    {
        GUILayout.Label("选择要添加的条件类型：", EditorStyles.boldLabel);
        scroll = GUILayout.BeginScrollView(scroll);

        DrawTypeButton<CardMatchRangeResolveCondition>("卡牌匹配范围");
        DrawTypeButton<RoleStatRangeResolveCondition>("角色属性范围");
        DrawTypeButton<RoleStatRangeCurveMapTriggerCondition>("角色属性曲线映射");
        DrawTypeButton<HandCardCountTriggerCondition>("手牌数量范围");
        DrawTypeButton<SpecificCardExistsTriggerCondition>("特定卡牌存在");

        GUILayout.EndScrollView();
    }

    private void DrawTypeButton<T>(string displayName) where T : ScriptableObject
    {
        if (GUILayout.Button(displayName, GUILayout.Width(200), GUILayout.Height(30)))
        {
            onSelect?.Invoke(typeof(T));
            Close();
        }
        GUILayout.Space(10);
    }
}
