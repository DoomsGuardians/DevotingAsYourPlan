using UnityEngine;
using System.Linq;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Events/Effects/事件卡牌效果")]
public class SettleCardsInEventEffect : EventEffectSO
{
    public bool IsAddRemainingLife = true;
    public int extraLifeForBeliever = 0;
    public int extraLifeForLabor = 0;
    public int extraLifeForTribute = 0;
    public override void Apply(EventInstance evt)
    {
        if (IsAddRemainingLife)
        {
            foreach (var card in evt.originalCards)
            {
                if (card.data.cardName != "破旧的还原机")
                {
                    if (card.data.cardType == CardType.Believer)
                    {
                        card.remainingLife += extraLifeForBeliever;
                        Debug.Log($"[延寿效果] 卡【{card.data.cardName}】 +{extraLifeForBeliever} 寿命");
                    }
                    if (card.data.cardType == CardType.Tribute)
                    {
                        card.remainingLife += extraLifeForTribute;
                        Debug.Log($"[延寿效果] 卡【{card.data.cardName}】 +{extraLifeForTribute} 寿命");
                    }
                    if (card.data.cardType == CardType.Labor)
                    {
                        card.remainingLife += extraLifeForLabor;
                        Debug.Log($"[延寿效果] 卡【{card.data.cardName}】 +{extraLifeForLabor} 寿命");
                    }
                }
            }
        }
    }

    public override string Description => $"给所有用于该事件的信徒卡增加{extraLifeForBeliever} 寿命，贡品卡增加{extraLifeForTribute} 寿命， 劳力卡增加{extraLifeForLabor} 寿命";
}
