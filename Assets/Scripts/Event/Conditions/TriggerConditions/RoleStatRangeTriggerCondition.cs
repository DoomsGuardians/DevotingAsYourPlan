using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/Conditions/Trigger Conditions/角色属性范围")]
public class RoleStatRangeTriggerCondition : EventTriggerConditionSO
{
    [Tooltip("是哪个角色的属性？")]
    public RoleType role;

    [Tooltip("是哪个属性？")]
    [RoleStatKey]
    public string statKey;

    [Tooltip("最小值（包含）")]
    public float minValue;

    [Tooltip("最大值（包含）")]
    public float maxValue;

    [Tooltip("是否为反向判断（即 不在 区间范围内）")]
    public bool invert = false;

    public override bool Evaluate(EventNodeData context)
    {
        float value = GameManager.Instance.RoleManager.GetRole(role).GetStat(statKey);
        bool inRange = value >= minValue && value <= maxValue;
        return invert ? !inRange : inRange;
    }

    public override string Description =>
        invert 
        ? $"{role} 的 {statKey} ∉ [{minValue}, {maxValue}]"
        : $"{role} 的 {statKey} ∈ [{minValue}, {maxValue}]";
}
