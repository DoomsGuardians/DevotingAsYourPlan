using UnityEditor;
using UnityEngine;
using System.Linq;

[CustomPropertyDrawer(typeof(RoleStatKeyAttribute))]
public class RoleStatKeyDrawer : PropertyDrawer
{
    private const string AssetPath = "Assets/SO/StatDefinitionTable/RoleStatDefinitionTable.asset";

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        RoleStatDefinitionTable table = AssetDatabase.LoadAssetAtPath<RoleStatDefinitionTable>(AssetPath);
        if (table == null)
        {
            EditorGUI.LabelField(position, $"未找到 StatDefinitionTable：{AssetPath}");
            return;
        }

        var keys = table.stats.Select(s => s.key).ToList();
        if (keys.Count == 0)
        {
            EditorGUI.LabelField(position, "无可用属性定义");
            return;
        }

        int index = Mathf.Max(0, keys.IndexOf(property.stringValue));
        index = EditorGUI.Popup(position, label.text, index, keys.ToArray()); // ✅ 最简写法
        property.stringValue = keys[index];
    }
}
