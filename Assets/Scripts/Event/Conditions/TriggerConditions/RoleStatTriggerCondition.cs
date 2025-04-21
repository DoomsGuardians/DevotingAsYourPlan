using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/Conditions/Trigger Conditions/Role Stat")]
public class RoleStatTriggerCondition : EventTriggerConditionSO
{
    [Tooltip("是哪个角色的属性呢？")]
    public RoleType role;
    [Tooltip("是哪个属性呢？")]
    public string statKey;
    [Tooltip("和什么值比较呢？")]
    public float threshold;
    [Tooltip("怎么比较呢？")]
    public ComparisonType comparison;

    public override bool Evaluate(EventNodeData context)
    {
        float value = GameManager.Instance.RoleManager.GetRole(role).GetStat(statKey);
        return comparison switch
        {
            ComparisonType.GreaterThan => value > threshold,
            ComparisonType.LessThan => value < threshold,
            ComparisonType.Equal => Mathf.Approximately(value, threshold),
            _ => false
        };
    }

    public override string Description => $"{statKey} {comparison} {threshold}";
}

