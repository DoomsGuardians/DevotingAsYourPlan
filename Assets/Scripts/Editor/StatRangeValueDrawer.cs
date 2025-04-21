#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(StatRangeValueAttribute))]
public class StatRangeValueDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty parent = GetParent(property);

        if (parent == null)
        {
            EditorGUI.PropertyField(position, property, label);
            return;
        }

        var valueTypeProp = parent.FindPropertyRelative("valueType");
        var presetProp = parent.FindPropertyRelative("rangePreset");
        var minProp = parent.FindPropertyRelative("min");
        var maxProp = parent.FindPropertyRelative("max");

        if (presetProp != null)
        {
            switch ((RangePreset)presetProp.enumValueIndex)
            {
                case RangePreset.Minus100To100:
                    minProp.floatValue = -100;
                    maxProp.floatValue = 100;
                    break;
                case RangePreset.ZeroTo100:
                    minProp.floatValue = 0;
                    maxProp.floatValue = 100;
                    break;
                case RangePreset.Custom:
                    // 保持当前值
                    break;
            }
        }

        float min = minProp != null ? minProp.floatValue : 0f;
        float max = maxProp != null ? maxProp.floatValue : 100f;

        if (valueTypeProp.enumValueIndex == (int)ValueType.Int)
        {
            property.floatValue = EditorGUI.IntSlider(position, label, Mathf.RoundToInt(property.floatValue), Mathf.RoundToInt(min), Mathf.RoundToInt(max));
        }
        else
        {
            property.floatValue = EditorGUI.Slider(position, label, property.floatValue, min, max);
        }
    }

    private SerializedProperty GetParent(SerializedProperty property)
    {
        string path = property.propertyPath;
        string[] parts = path.Split('.');
        if (parts.Length < 2) return null;

        string parentPath = string.Join(".", parts, 0, parts.Length - 1);
        return property.serializedObject.FindProperty(parentPath);
    }
}
#endif
