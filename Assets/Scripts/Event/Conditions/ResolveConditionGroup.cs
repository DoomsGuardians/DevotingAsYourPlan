using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(menuName = "Events/Condition Groups/Resolve Condition Group")]
public class ResolveConditionGroup : ScriptableObject
{
    [Tooltip("结算事件条件组判断模式")]
    public ConditionMode mode;
    [Tooltip("结算事件条件组")]
    public List<EventResolveConditionSO> conditions;

    public bool EvaluateAll(EventInstance context)
    {
        return mode switch
        {
            ConditionMode.All => conditions.All(c => c.Evaluate(context)),
            ConditionMode.Any => conditions.Any(c => c.Evaluate(context)),
            _ => false
        };
    }
}

