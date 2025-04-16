using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardManager : Singleton<CardManager>
{
    public List<CardData> cardDatabase; // 由美术or策划配置

    public HorizontalCardHolder playerCardHolder;

    // 可以按类型分类池子
    private Dictionary<CardType, List<CardData>> cardPools;

    public void Initialize(List<CardData> cardDatabase,HorizontalCardHolder playerCardHolder)
    {
        this.cardDatabase = cardDatabase;
        // 分类卡池
        this.playerCardHolder = playerCardHolder;
        cardPools = new Dictionary<CardType, List<CardData>>();
        foreach (CardType type in System.Enum.GetValues(typeof(CardType)))
            cardPools[type] = new List<CardData>();

        foreach (var card in cardDatabase)
            cardPools[card.cardType].Add(card);
        Debug.Log($"卡片管理器初始化了");
    }

    public void DrawCard (CardType type)
    {
        var pool = cardPools[type];
        if (pool.Count == 0)
        {
            Debug.Log($"{type.ToString()}类的卡数据库中没有");
            return;
        }

        CardData selected = pool[Random.Range(0, pool.Count)];
        CardRuntime runtime = new CardRuntime(selected);
        playerCardHolder.AddCard(runtime);
        
    }
    

    public void TickCards()
    {
        foreach (var holder in CardHolderManager.Holders)
        {
            for (int i = holder.cards.Count - 1; i >= 0; i--)
            {
                var card = holder.cards[i];
                card.runtimeData.TickLife();
                if (card.runtimeData.IsExpired())
                {
                    holder.DestroyCard(card);
                }
            }
        }
        
    }

    public void RefreshCards()
    {
        foreach (var holder in CardHolderManager.Holders)
        {
            holder.RefreshCardsInfo();
        }
    }
}
