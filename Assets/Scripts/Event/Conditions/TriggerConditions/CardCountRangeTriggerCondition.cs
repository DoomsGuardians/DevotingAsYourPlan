using UnityEngine;

[CreateAssetMenu(menuName = "Events/Conditions/Trigger Conditions/手牌数量范围")]
public class HandCardCountTriggerCondition : EventTriggerConditionSO
{
    [Tooltip("最小手牌数（包含）")]
    public int minCount = 0;

    [Tooltip("最大手牌数（包含）")]
    public int maxCount = 99;

    public override bool Evaluate(EventNodeData context)
    {
        int currentHand = GameManager.Instance.playerCardHolder.cards.Count;

        bool result = currentHand >= minCount && currentHand <= maxCount;

        Debug.Log($"[手牌判断] 当前手牌数 = {currentHand}，要求范围 [{minCount}, {maxCount}] -> {(result ? "满足" : "不满足")}");

        return result;
    }

    public override string Description => $"玩家手牌数 ∈ [{minCount}, {maxCount}]";
}
