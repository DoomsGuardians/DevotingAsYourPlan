using System.Collections.Generic;
using QFSW.QC;
using UnityEngine;
using System.Linq;
using System.Text;

public static class StaticCheatDebugCommand
{
    private static bool isCheating = false;

    [Command("Debug")]
    [Command("debug")]
    public static void ToggleCheatMode()
    {
        isCheating = !isCheating;
        Debug.Log($"作弊模式已{(isCheating ? "开启" : "关闭")}。");
    }

    [Command("RoleStat")]
    [Command("rs")]
    [Command("RS")]
    [Command("rolestat")]
    public static void GetRoleStat(RoleType roleType)
    {
        if (!isCheating)
        {
            Debug.Log("尚未开启作弊模式，无法使用指令");
            return;
        }

        PrintRoleStat(roleType);
    }

    [Command("RoleStat.Name")]
    [Command("rs.name")]
    public static void GetRoleStatByName(string name)
    {
        if (!isCheating)
        {
            Debug.Log("尚未开启作弊模式，无法使用指令");
            return;
        }

        if (System.Enum.TryParse<RoleType>(name, true, out var roleType))
        {
            PrintRoleStat(roleType);
        }
        else
        {
            Debug.LogWarning($"找不到名为 {name} 的角色类型！");
        }
    }

    [Command("RoleStat.All")]
    [Command("rs.all")]
    [Command("RS.All")]
    [Command("rolestat.all")]
    public static void GetAllRoleStats()
    {
        if (!isCheating)
        {
            Debug.Log("尚未开启作弊模式，无法使用指令");
            return;
        }

        foreach (RoleType roleType in System.Enum.GetValues(typeof(RoleType)))
        {
            PrintRoleStat(roleType);
        }
    }

    private static void PrintRoleStat(RoleType roleType)
    {
        var role = GameManager.Instance.GetRole(roleType);
        if (role == null)
        {
            Debug.LogWarning($"未找到角色：{roleType}");
            return;
        }

        Debug.Log($"【{roleType}】的属性：");
        PrintDictionary(role.GetAllStats());
    }

    private static void PrintDictionary<TKey, TValue>(Dictionary<TKey, TValue> dict)
    {
        if (dict == null || dict.Count == 0)
        {
            Debug.Log("属性为空。");
            return;
        }

        var sb = new StringBuilder();
        int keyWidth = dict.Keys.Max(k => k.ToString().Length);
        int valWidth = dict.Values.Max(v => v?.ToString().Length ?? 0);

        keyWidth = Mathf.Max(keyWidth, 4);
        valWidth = Mathf.Max(valWidth, 5);

        sb.AppendLine();
        sb.AppendLine($"{"属性名".PadRight(keyWidth)} | {"属性值".PadRight(valWidth)}");
        sb.AppendLine(new string('-', keyWidth + valWidth + 3));

        foreach (var kvp in dict)
        {
            string key = kvp.Key.ToString().PadRight(keyWidth);
            string val = kvp.Value?.ToString().PadRight(valWidth);
            sb.AppendLine($"{key} | {val}");
        }

        Debug.Log(sb.ToString());
    }

    [Command("GiveCard")]
    [Command("givecard")]
    [Command("gc")]
    [Command("GC")]
    public static void GiveCard(string cardName)
    {
        if (!isCheating)
        {
            Debug.Log("尚未开启作弊模式，无法使用指令");
            return;
        }

        var card = GameManager.Instance.CardManager.GetCardByExactName(cardName);
        if (card == null)
        {
            Debug.LogWarning($"未找到名为【{cardName}】的卡牌！");
            return;
        }

        var runtime = GameManager.Instance.CardManager.CreateCard(card);
        if (runtime == null)
        {
            Debug.LogWarning($"卡牌【{cardName}】无法创建（可能是唯一卡）");
            return;
        }

        GameManager.Instance.CardManager.playerCardHolder.AddCard(runtime);
        Debug.Log($"已发放卡牌：{card.cardName}");
    }
}
