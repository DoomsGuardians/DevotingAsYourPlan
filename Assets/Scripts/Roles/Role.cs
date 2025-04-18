using System.Collections.Generic;

public class Role
{
    public RoleType type;
    private Dictionary<string, float> stats = new();
    private Dictionary<string, List<float>> statHistory = new(); // 用来记录每个属性的历史变化
    public Role(RoleType type) => this.type = type;

    public void SetStat(string key, float value)
    {
        if (!statHistory.ContainsKey(key))
            statHistory[key] = new List<float>();
        statHistory[key].Add(value);  // 添加新的回合数据

        stats[key] = value;
    }
    public float GetStat(string key) => stats.TryGetValue(key, out var v) ? v : 0;
    public List<float> GetStatHistory(string key)
    {
        return statHistory.TryGetValue(key, out var history) ? history : new List<float>();
    }
    public void AddStat(string key, float delta) => stats[key] = GetStat(key) + delta;
    public Dictionary<string, float> GetAllStats() => new(stats);
}