using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ComparisonType { GreaterThan, LessThan, Equal }

[CreateAssetMenu(menuName = "Events/Conditions/Role Stat")]
public class RoleStatResolveCondition : EventResolveConditionSO
{
    public string statKey;
    public float threshold;
    public ComparisonType comparison;

    public override bool Evaluate(EventInstance context)
    {
        float value = context.sourceRole.GetStat(statKey);
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

