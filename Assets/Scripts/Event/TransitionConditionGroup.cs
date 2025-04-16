using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(menuName = "Events/Condition Group")]
public class TransitionConditionGroup : ScriptableObject
{
    public ConditionMode mode;
    public List<EventConditionSO> conditions;

    public bool EvaluateAll(EventContext context)
    {
        return mode switch
        {
            ConditionMode.All => conditions.All(c => c.Evaluate(context)),
            ConditionMode.Any => conditions.Any(c => c.Evaluate(context)),
            _ => false
        };
    }
}

