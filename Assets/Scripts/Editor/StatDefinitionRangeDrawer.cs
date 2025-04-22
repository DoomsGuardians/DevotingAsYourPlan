#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Reflection;

[CustomPropertyDrawer(typeof(StatDefinitionRangeAttribute))]
public class StatDefinitionRangeDrawer : PropertyDrawer
{
    
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        StatDefinitionRangeAttribute rangeAttr = (StatDefinitionRangeAttribute)attribute;
        SerializedProperty parent = property.serializedObject.FindProperty(property.propertyPath.Replace(".value", ""));

        if (parent == null)
        {
            EditorGUI.PropertyField(position, property, label);
            return;
        }

        SerializedProperty keyProp = parent.FindPropertyRelative("key");
        string statKey = keyProp?.stringValue;

        // 查找 RoleData 中的字段（RoleStatDefinitionTable）
        Object targetObj = property.serializedObject.targetObject;
        FieldInfo tableField = targetObj.GetType().GetField(rangeAttr.definitionTableFieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        RoleStatDefinitionTable table = tableField?.GetValue(targetObj) as RoleStatDefinitionTable;

        RoleStatDefinition def = table?.GetStat(statKey);

        if (def != null)
        {
            float min = def.min;
            float max = def.max;

            if (def.valueType == ValueType.Int)
            {
                int current = Mathf.RoundToInt(property.floatValue);
                current = EditorGUI.IntSlider(position, label, current, Mathf.RoundToInt(min), Mathf.RoundToInt(max));
                property.floatValue = current;
            }
            else
            {
                float current = property.floatValue;
                current = EditorGUI.Slider(position, label, current, min, max);
                property.floatValue = current;
            }
        }
        else
        {
            EditorGUI.PropertyField(position, property, label);
            EditorGUI.HelpBox(position, $"无法找到 Stat 定义：{statKey}", MessageType.Warning);
        }
    }
}
#endif
