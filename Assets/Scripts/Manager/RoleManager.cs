using System.Collections.Generic;
using UnityEngine;

public class RoleManager
{
    private Dictionary<RoleType, Role> roles = new();
    private RoleStatDefinitionTable definitionTable;

    private readonly Dictionary<RoleType, IRoleLogicModule> logicModules = new()
    {
        { RoleType.Player, new PlayerLogicModule() },
        { RoleType.People, new PeopleLogicModule() },
        { RoleType.World, new WorldLogicModule() }
    };
    
    public void Initialize(List<RoleData> configs, RoleStatDefinitionTable table)
    {
        roles.Clear();
        definitionTable = table;
        foreach (var config in configs)
        {
            var role = new Role(config.type, definitionTable);
            foreach (var stat in config.initialStats)
                role.SetStat(stat.key, stat.value);
            roles[config.type] = role;
        }
    }

    public Role GetRole(RoleType type) => roles[type];
    public string GetStatDisplayName(string key) => definitionTable.GetStat(key)?.displayName ?? key;
    public string GetStatDescription(string key) => definitionTable.GetStat(key)?.description ?? "";
    
    public void SettleAllRoles()
    {
        foreach (var role in roles.Values)
        {
            if (logicModules.TryGetValue(role.type, out var module))
            {
                module.Settle(role, GameManager.Instance.turnStateMachine.TurnNum);
            }
            else
            {
                Debug.LogWarning($"未注册 {role.type} 的结算逻辑模块！");
            }
        }
    }
}