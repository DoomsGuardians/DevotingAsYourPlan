using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/Conditions/Resolve Conditions/随机数条件")]
public class RandomNumResolveCondition : EventResolveConditionSO
{
    [Tooltip("随机值最小值（闭区间）")]
    public float min = 0f;

    [Tooltip("随机值最大值（闭区间）")]
    public float max = 100f;

    [Tooltip("与目标值比较方式")]
    public ComparisonType comparison = ComparisonType.GreaterThan;
    
    [Tooltip("比较目标值")]
    public float targetValue = 50f;
    
    public override bool Evaluate(EventInstance context)
    {
        float randomValue = Random.Range(min, max);
        Debug.Log($"[随机判定] 随机值为 {randomValue:F2}，目标为 {targetValue}，比较方式为 {comparison}");

        switch (comparison)
        {
            case ComparisonType.GreaterThan: return randomValue > targetValue;
            case ComparisonType.LessThan: return randomValue < targetValue;
            case ComparisonType.Equal: return Mathf.Approximately(randomValue, targetValue);
            default: return false;
        }
    }
    
    public override string Description
    {
        get
        {
            return $"随机数范围{min}~{max}，比较数值{targetValue}";
        }
    }
}
