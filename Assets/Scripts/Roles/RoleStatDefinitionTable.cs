using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Roles/Stat Definition Table")]
public class RoleStatDefinitionTable : ScriptableObject
{
    public List<RoleStatDefinition> stats;
    public RoleStatDefinition GetStat(string key) => stats.FirstOrDefault(s => s.key == key);
}

public enum ValueType { Int, Float }
public enum RangePreset { Minus100To100, ZeroTo100, Custom }

[System.Serializable]
public class RoleStatDefinition
{
    public string key;
    public string displayName;
    public string description;

    public ValueType valueType = ValueType.Float;
    public RangePreset rangePreset = RangePreset.Custom;
    public float min = 0;
    public float max = 100;

    [StatRangeValue]
    public float defaultValue;
}
