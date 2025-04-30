using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class UntrackedSOInspector : EditorWindow
{
    private List<ScriptableObject> inMemoryObjects = new();
    private List<ScriptableObject> dirtyAssets = new();
    private List<bool> dirtySelection = new(); // checkbox 状态

    [MenuItem("LevityTools/检查未保存的 ScriptableObject")]
    public static void ShowWindow()
    {
        GetWindow<UntrackedSOInspector>("SO 未保存检查");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("扫描未保存的 ScriptableObject", GUILayout.Height(30)))
        {
            ScanSOIssues();
        }

        GUILayout.Space(20);

        GUILayout.Label("未保存为 .asset 的 ScriptableObject（内存中）", EditorStyles.boldLabel);
        foreach (var so in inMemoryObjects)
        {
            EditorGUILayout.ObjectField(so.name + " (未保存)", so, typeof(ScriptableObject), false);
        }

        GUILayout.Space(10);

        GUILayout.Label("被修改但未保存（dirty）的 ScriptableObject", EditorStyles.boldLabel);
        if (dirtyAssets.Count > 0)
        {
            GUILayout.BeginVertical("box");
            for (int i = 0; i < dirtyAssets.Count; i++)
            {
                GUILayout.BeginHorizontal();
                dirtySelection[i] = EditorGUILayout.Toggle(dirtySelection[i], GUILayout.Width(20));
                EditorGUILayout.ObjectField(dirtyAssets[i], typeof(ScriptableObject), false);
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();

            GUILayout.Space(10);
            if (GUILayout.Button("保存选中的 ScriptableObject", GUILayout.Height(30)))
            {
                SaveSelectedDirtyAssets();
            }
        }
        else
        {
            GUILayout.Label("没有 dirty 的 SO 需要保存");
        }
    }

    private void ScanSOIssues()
    {
        inMemoryObjects.Clear();
        dirtyAssets.Clear();
        dirtySelection.Clear();

        // 获取所有 ScriptableObject .asset 文件路径
        var allSOPaths = AssetDatabase.FindAssets("t:ScriptableObject")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Distinct();

        HashSet<ScriptableObject> seenAssets = new();

        foreach (string path in allSOPaths)
        {
            var obj = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            if (obj != null)
            {
                seenAssets.Add(obj);

                if (EditorUtility.IsDirty(obj))
                {
                    dirtyAssets.Add(obj);
                    dirtySelection.Add(true); // 默认勾选
                }
            }
        }

        // 找出内存中未保存的 ScriptableObject
        var allInMemory = Resources.FindObjectsOfTypeAll<ScriptableObject>();
        foreach (var so in allInMemory)
        {
            if (EditorUtility.IsPersistent(so)) continue; // 已保存的跳过
            if (seenAssets.Contains(so)) continue;       // 已记录的跳过
            if (so.hideFlags.HasFlag(HideFlags.NotEditable)) continue; // 内部对象跳过

            inMemoryObjects.Add(so);
        }

        Debug.Log($"扫描完成：{inMemoryObjects.Count} 个未保存的，{dirtyAssets.Count} 个 dirty");
    }

    private void SaveSelectedDirtyAssets()
    {
        int savedCount = 0;
        for (int i = 0; i < dirtyAssets.Count; i++)
        {
            if (dirtySelection[i])
            {
                EditorUtility.SetDirty(dirtyAssets[i]);
                savedCount++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"已保存 {savedCount} 个 ScriptableObject 资源。");
    }
}
