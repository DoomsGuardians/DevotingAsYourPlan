using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(menuName = "Events/Conditions/Resolve Conditions/Card Type")]
public class CardTypeResolveCondition : EventResolveConditionSO
{
    [Tooltip("需要检测数量的卡牌种类")]
    public CardType requiredType;
    public int minCount = 1;

    public override bool Evaluate(EventInstance context)
    {
        return context.cardHolder.cards.Count(c => c.runtimeData.data.cardType == requiredType) >= minCount;
    }

    public override string Description => $"至少 {minCount} 张 {requiredType}";
}


