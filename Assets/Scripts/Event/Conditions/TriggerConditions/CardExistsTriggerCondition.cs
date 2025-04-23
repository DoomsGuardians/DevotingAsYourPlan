using UnityEngine;

[CreateAssetMenu(menuName = "Events/Conditions/Trigger Conditions/特定卡牌存在")]
public class SpecificCardExistsTriggerCondition : EventTriggerConditionSO
{
    [Tooltip("需要存在的目标卡牌（CardData）")]
    public CardData targetCard;

    [Tooltip("是否为反向判断（即卡牌不存在时触发）")]
    public bool invert = false;

    public override bool Evaluate(EventNodeData context)
    {
        if (targetCard == null)
        {
            Debug.LogWarning("[卡牌检测] 没有配置目标卡牌");
            return false;
        }

        var hand = GameManager.Instance.playerCardHolder.cards;
        bool exists = hand.Exists(c => c.runtimeData.data == targetCard);

        Debug.Log($"[🃏 卡牌判断] {targetCard.cardName} {(exists ? "存在" : "不存在")} → {(invert ? "反向判断" : "正常判断")}");

        return invert ? !exists : exists;
    }

    public override string Description => $"手牌中包含卡牌：{targetCard?.cardName ?? "（未设置）"}";
}
