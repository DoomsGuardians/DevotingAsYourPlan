using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class SOReferenceCleaner
{
    [MenuItem("Tools/Clean Unused Event ScriptableObjects")]
    public static void CleanUnusedSO()
    {
        string basePath = "Assets/SO/EventData/EventContainer";
        string effectPath = $"{basePath}/EventConditions&Effects";
        var allAssets = AssetDatabase.FindAssets("t:ScriptableObject", new[] { effectPath });
        var allPaths = new HashSet<string>(allAssets.Select(AssetDatabase.GUIDToAssetPath));

        var usedPaths = new HashSet<string>();

        var eventAssets = AssetDatabase.FindAssets("t:EventNodeData", new[] { basePath });
        foreach (var guid in eventAssets)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var eventNode = AssetDatabase.LoadAssetAtPath<EventNodeData>(path);
            if (eventNode == null) continue;

            void AddSO(UnityEngine.Object so)
            {
                if (so == null) return;
                string soPath = AssetDatabase.GetAssetPath(so);
                if (!string.IsNullOrEmpty(soPath))
                    usedPaths.Add(soPath);
            }

            AddSO(eventNode.triggerConditions);
            if (eventNode.triggerConditions != null)
                foreach (var cond in eventNode.triggerConditions.conditions)
                    AddSO(cond);

            if (eventNode.expiredEffects != null)
                foreach (var effect in eventNode.expiredEffects)
                    AddSO(effect);

            if (eventNode.branchGroups != null)
            {
                foreach (var group in eventNode.branchGroups)
                {
                    AddSO(group);
                    if (group?.branches != null)
                    {
                        foreach (var branch in group.branches)
                        {
                            AddSO(branch);
                            if (branch.effects != null)
                                foreach (var eff in branch.effects)
                                    AddSO(eff);

                            if (branch.matchConditions != null)
                            {
                                AddSO(branch.matchConditions);
                                foreach (var cond in branch.matchConditions.conditions)
                                    AddSO(cond);
                            }
                        }
                    }
                }
            }
        }

        var unusedPaths = allPaths.Except(usedPaths).ToList();
        if (unusedPaths.Count == 0)
        {
            Debug.Log("没有未被引用的 ScriptableObject 可清理。");
            return;
        }

        if (!EditorUtility.DisplayDialog("确认删除",
                $"将要删除 {unusedPaths.Count} 个未引用的资源，是否继续？", "确认", "取消"))
        {
            return;
        }

        foreach (var path in unusedPaths)
        {
            AssetDatabase.DeleteAsset(path);
        }

        AssetDatabase.Refresh();
        Debug.Log($"完成清理。已删除 {unusedPaths.Count} 个未被引用的资源。");
    }
}
