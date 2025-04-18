using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/Conditions/Trigger Conditions/Role Stat")]
public class RoleStatTriggerCondition : EventTriggerConditionSO
{
    public string statKey;
    public float threshold;
    public ComparisonType comparison;

    public override bool Evaluate(EventNodeData context)
    {
        float value = GameManager.Instance.RoleManager.GetRole(context.sourceRole).GetStat(statKey);
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

