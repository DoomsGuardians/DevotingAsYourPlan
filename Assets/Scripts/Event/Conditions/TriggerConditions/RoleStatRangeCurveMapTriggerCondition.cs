using UnityEngine;

[CreateAssetMenu(menuName = "Events/Conditions/Trigger Conditions/角色属性范围曲线映射")]
public class RoleStatRangeCurveMapTriggerCondition : EventTriggerConditionSO
{
    [Tooltip("角色类型")]
    public RoleType role;

    [Tooltip("属性名称（必须是定义表中的 key）")]
    [RoleStatKey]
    public string statKey;

    [Tooltip("最小值（包含）")]
    public float minValue;

    [Tooltip("最大值（包含）")]
    public float maxValue;

    [Tooltip("概率曲线，X: 属性值映射 0~1, Y: 概率（0~1）")]
    public AnimationCurve probabilityCurve = AnimationCurve.Linear(0, 1, 1, 1);

    public override bool Evaluate(EventNodeData context)
    {
        float value = GameManager.Instance.RoleManager.GetRole(role).GetStat(statKey);

        if (value < minValue || value > maxValue)
        {
            Debug.Log($"[❌ 跳过] {role} 的 {statKey}={value} 不在范围 [{minValue}, {maxValue}]");
            return false;
        }

        float t = Mathf.InverseLerp(minValue, maxValue, value); // 将值映射到 0~1
        float probability = probabilityCurve.Evaluate(t);
        float roll = Random.value;

        Debug.Log($"[🎲 概率判断] {role} 的 {statKey}={value} ∈ [{minValue},{maxValue}] ➤ 映射 {t:F2}, 概率 {probability:F2}, 随机 {roll:F2}");

        return roll <= probability;
    }

    public override string Description => $"{role} 的 {statKey} ∈ [{minValue}, {maxValue}] → 曲线概率触发";
}
