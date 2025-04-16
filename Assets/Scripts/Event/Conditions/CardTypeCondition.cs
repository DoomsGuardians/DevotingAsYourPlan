using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(menuName = "Events/Conditions/Card Type")]
public class CardTypeCondition : EventConditionSO
{
    public CardType requiredType;
    public int minCount = 1;

    public override bool Evaluate(EventContext context)
    {
        return context.eventInstance.cardHolder.cards.Count(c => c.runtimeData.data.cardType == requiredType) >= minCount;
    }

    public override string Description => $"至少 {minCount} 张 {requiredType}";
}


