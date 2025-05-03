using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(AudioKeyAttribute))]
public class AudioKeyDrawer : PropertyDrawer
{
    private const string ConfigSearchPath = "Assets/SO/AudioData/";

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // 找到第一个 AudioConfig 文件
        string[] guids = AssetDatabase.FindAssets("t:AudioConfig", new[] { ConfigSearchPath });

        if (guids.Length == 0)
        {
            EditorGUI.LabelField(position, label.text, "未在 Assets/SO/AudioData 找到 AudioConfig");
            return;
        }

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        AudioConfig config = AssetDatabase.LoadAssetAtPath<AudioConfig>(path);

        if (config == null)
        {
            EditorGUI.LabelField(position, label.text, "加载 AudioConfig 失败");
            return;
        }

        var keys = config.audioClips
            .Where(c => !string.IsNullOrEmpty(c.key))
            .Select(c => c.key)
            .Distinct()
            .ToArray();

        if (keys.Length == 0)
        {
            EditorGUI.LabelField(position, label.text, "AudioConfig 中没有音效 Key");
            return;
        }

        int currentIndex = Mathf.Max(0, System.Array.IndexOf(keys, property.stringValue));
        int selectedIndex = EditorGUI.Popup(position, label.text, currentIndex, keys);

        if (selectedIndex >= 0 && selectedIndex < keys.Length)
        {
            property.stringValue = keys[selectedIndex];
        }
    }
}

#endif 