using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CrowdSpeechConfigSO))]
public class CrowdSpeechConfigEditor : Editor
{
    private SerializedProperty rules;

    void OnEnable() => rules = serializedObject.FindProperty("rules");

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        for (int i = 0; i < rules.arraySize; i++)
        {
            var rule = rules.GetArrayElementAtIndex(i);
            var sourceRole = rule.FindPropertyRelative("sourceRole");
            var key = rule.FindPropertyRelative("statKey");
            var range = rule.FindPropertyRelative("range");
            var speeches = rule.FindPropertyRelative("speeches");

            var minProp = range.FindPropertyRelative("min");
            var maxProp = range.FindPropertyRelative("max");

            float min = minProp.floatValue;
            float max = maxProp.floatValue;

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.BeginHorizontal();
            sourceRole.enumValueIndex = (int)(RoleType)EditorGUILayout.EnumPopup("所属角色", (RoleType)sourceRole.enumValueIndex);
            if (GUILayout.Button("删除", GUILayout.Width(60)))
            {
                rules.DeleteArrayElementAtIndex(i);
                break;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(key, new GUIContent("属性字段"));

            EditorGUILayout.BeginHorizontal();
            min = EditorGUILayout.FloatField("最小值", min);
            max = EditorGUILayout.FloatField("最大值", max);
            EditorGUILayout.EndHorizontal();


            minProp.floatValue = min;
            maxProp.floatValue = max;

            EditorGUILayout.PropertyField(speeches, new GUIContent("语料"), true);

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        if (GUILayout.Button("添加规则"))
            rules.InsertArrayElementAtIndex(rules.arraySize);

        serializedObject.ApplyModifiedProperties();
    }
}