using UnityEngine;
using UnityEditor;

public class StatDefinitionRangeAttribute : PropertyAttribute
{
    public string definitionTableFieldName;

    public StatDefinitionRangeAttribute(string definitionTableFieldName)
    {
        this.definitionTableFieldName = definitionTableFieldName;
    }
}

#if UNITY_EDITOR


[CustomPropertyDrawer(typeof(StatDefinitionRangeAttribute))]
public class StatDefinitionRangeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.PropertyField(position, property, label);
    }
}
#endif