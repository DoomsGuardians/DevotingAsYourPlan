using System.Collections.Generic;

public class Role
{
    public RoleType type;
    private Dictionary<string, float> stats = new();

    public Role(RoleType type) => this.type = type;

    public void SetStat(string key, float value) => stats[key] = value;
    public float GetStat(string key) => stats.TryGetValue(key, out var v) ? v : 0;
    public void AddStat(string key, float delta) => stats[key] = GetStat(key) + delta;
    public Dictionary<string, float> GetAllStats() => new(stats);
}