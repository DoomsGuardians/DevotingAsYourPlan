using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(menuName = "Events/Condition Groups/Trigger Condition Group")]
public class TriggerConditionGroup : ScriptableObject
{
    public string label;
    [Tooltip("触发事件条件组判断模式")]
    public ConditionMode mode;
    [Tooltip("触发事件条件组")]
    public List<EventTriggerConditionSO> conditions;

    public bool EvaluateAll(EventNodeData context)
    {
        return mode switch
        {
            ConditionMode.All => conditions.All(c => c.Evaluate(context)),
            ConditionMode.Any => conditions.Any(c => c.Evaluate(context)),
            _ => false
        };
    }
}