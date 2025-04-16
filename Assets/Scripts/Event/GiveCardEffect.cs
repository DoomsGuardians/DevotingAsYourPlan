using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/Effects/GiveCard")]
public class GiveCardEffect : EventEffectSO
{
    public CardType cardType;
    public int amount = 1;

    public override void Apply()
    {
        for (int i = 0; i < amount; i++)
        {
            GameManager.Instance.CardManager.DrawCard(cardType);
        }

        Debug.Log($"[事件效果] 发放 {amount} 张 {cardType} 卡牌");
    }

    public override string Description => $"发放{amount}张{cardType}卡牌";
}

