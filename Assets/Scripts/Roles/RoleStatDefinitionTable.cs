using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Roles/Stat Definition Table")]
public class RoleStatDefinitionTable : ScriptableObject
{
    public List<RoleStatDefinition> stats;
    public RoleStatDefinition GetStat(string key) => stats.FirstOrDefault(s => s.key == key);
}

[System.Serializable]
public class RoleStatDefinition
{
    public string key;
    public string displayName;
    public string description;
    public float defaultValue;
}
