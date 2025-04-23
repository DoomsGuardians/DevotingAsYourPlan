using UnityEngine;

[CreateAssetMenu(menuName = "Events/Conditions/Resolve Conditions/角色属性范围")]
public class RoleStatRangeResolveCondition : EventResolveConditionSO
{
    [Tooltip("目标角色")]
    public RoleType role;

    [Tooltip("属性 key（来自定义表）")]
    [RoleStatKey]
    public string statKey;

    [Tooltip("区间最小值（包含）")]
    public float minValue;

    [Tooltip("区间最大值（包含）")]
    public float maxValue;

    [Tooltip("是否取反（不在该区间时为 true）")]
    public bool invert = false;

    public override bool Evaluate(EventInstance context)
    {
        float value = GameManager.Instance.RoleManager.GetRole(role).GetStat(statKey);
        bool inRange = value >= minValue && value <= maxValue;

        bool result = invert ? !inRange : inRange;

        Debug.Log($"[角色属性判断] {role} 的 {statKey} = {value} ➤ {(invert ? "反向判断" : "正常判断")} → {(result ? "✅ 满足" : "❌ 不满足")}");

        return result;
    }

    public override string Description =>
        invert
        ? $"{role} 的 {statKey} ∉ [{minValue}, {maxValue}]"
        : $"{role} 的 {statKey} ∈ [{minValue}, {maxValue}]";
}
