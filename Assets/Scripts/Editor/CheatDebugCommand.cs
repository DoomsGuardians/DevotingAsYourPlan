using System.Collections;
using System.Collections.Generic;
using QFSW.QC;
using Unity.Burst.CompilerServices;
using UnityEngine;
using System.Linq;
using System.Text;
using Mono.CSharp;

public static class StaticCheatDebugCommand
{
    private static bool isCheating = false;

    [Command("Debug")]
    [Command("debug")]
    public static void ToggleCheatMode()
    {
        isCheating = !isCheating;
    }

    [Command("RoleStat")]
    [Command("rs")]
    [Command("RS")]
    [Command("rolestat")]
    public static void GetRoleStat(RoleType roleType)
    {
        if(!isCheating) 
        {
            Debug.Log("尚未开启作弊模式，无法使用指令");
            return;
        }
        PrintDictionary(GameManager.Instance.GetRole(roleType).GetAllStats());
    }
    [Command("RoleStat.All")]
    [Command("rs.all")]
    [Command("RS.All")]
    [Command("rolestat.all")]
    public static void GetRoleStat()
    {
        if(!isCheating) 
        {
            Debug.Log("尚未开启作弊模式，无法使用指令");
            return;
        }
        foreach (RoleType roleType in System.Enum.GetValues(typeof(RoleType)))
        {
            Debug.Log($"{roleType}的属性");
            PrintDictionary(GameManager.Instance.GetRole(roleType).GetAllStats());
        }
    }
    
    private static void PrintDictionary<TKey, TValue>(Dictionary<TKey, TValue> dict)
    {
        if (dict == null || dict.Count == 0)
        {
            Debug.Log("Dictionary is empty.");
            return;
        }

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("\n属性名\t\t属性值");
        sb.AppendLine("-------------------------------");

        foreach (var kvp in dict)
        {
            sb.AppendLine($"{kvp.Key}\t\t{kvp.Value}");
        }

        Debug.Log(sb.ToString());
    }
}