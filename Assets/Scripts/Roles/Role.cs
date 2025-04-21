using System.Collections.Generic;
using UnityEngine;

public class Role
{
    public RoleType type;
    private Dictionary<string, float> stats = new();
    private Dictionary<string, List<float>> statHistory = new(); // 用来记录每个属性的历史变化
    
    private RoleStatDefinitionTable definitionTable;

    public Role(RoleType type, RoleStatDefinitionTable definitionTable)
    {
        this.type = type;
        this.definitionTable = definitionTable;
        
    }

    public void SetStat(string key, float value)
    {
        // 1. 获取 stat 定义
        var def = definitionTable?.GetStat(key);

        if (def != null)
        {
            // 2. Clamp 到 min/max
            value = Mathf.Clamp(value, def.min, def.max);

            // 3. 如果是 Int 类型，强制为整数
            if (def.valueType == ValueType.Int)
                value = Mathf.Round(value);
        }

        // 4. 记录历史（保留的是合法后的值）
        if (!statHistory.ContainsKey(key))
            statHistory[key] = new List<float>();
        statHistory[key].Add(value);

        stats[key] = value;
    }
    
    public float GetStat(string key) => stats.TryGetValue(key, out var v) ? v : 0;
    public List<float> GetStatHistory(string key)
    {
        return statHistory.TryGetValue(key, out var history) ? history : new List<float>();
    }
    
    public void AddStat(string key, float delta)
    {
        float current = GetStat(key);
        float result = current + delta;

        var def = definitionTable?.GetStat(key);
        if (def != null)
        {
            result = Mathf.Clamp(result, def.min, def.max);
            if (def.valueType == ValueType.Int)
                result = Mathf.Round(result);
        }

        SetStat(key, result); // 自动记录历史 & 覆盖值
    }
    
    public Dictionary<string, float> GetAllStats() => new(stats);
}