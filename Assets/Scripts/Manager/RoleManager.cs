using System.Collections.Generic;
using UnityEngine;

public class RoleManager
{
    private Dictionary<RoleType, Role> roles = new();
    private RoleStatDefinitionTable definitionTable;

    public void Initialize(List<RoleData> configs, RoleStatDefinitionTable table)
    {
        roles.Clear();
        definitionTable = table;
        foreach (var config in configs)
        {
            var role = new Role(config.type);
            foreach (var stat in config.initialStats)
                role.SetStat(stat.key, stat.value);
            roles[config.type] = role;
        }
    }

    public Role GetRole(RoleType type) => roles[type];
    public string GetStatDisplayName(string key) => definitionTable.GetStat(key)?.displayName ?? key;
    public string GetStatDescription(string key) => definitionTable.GetStat(key)?.description ?? "";
}