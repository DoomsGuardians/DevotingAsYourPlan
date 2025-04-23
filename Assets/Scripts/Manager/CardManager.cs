using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Cysharp.Threading.Tasks;

public class CardManager : Singleton<CardManager>
{
    public List<CardData> allCards; 

    public CardData GetCardByExactName(string cardName)
    {
        return allCards.FirstOrDefault(c => 
        string.Equals(c.cardName, cardName, System.StringComparison.OrdinalIgnoreCase));
    }

    private readonly HashSet<CardData> generatedUniques = new();//用于处理唯一卡

    public void ResetUniques() => generatedUniques.Clear();

    public bool HasAlreadyCreated(CardData card) =>
    card.isUnique && generatedUniques.Contains(card);

    public CardRuntime CreateCard(CardData data)
    {
        if (data.isUnique && generatedUniques.Contains(data))
        {
            Debug.LogWarning($"唯一卡 {data.cardName} 已生成，无法再次生成");
            return null;
        }

        if (data.isUnique)
            generatedUniques.Add(data);

        return new CardRuntime(data);
    }

    public void LoadCardsFromPool(CardPool pool)
    {
        allCards = pool.cards.Distinct().ToList();
    }

    private readonly Dictionary<int, int> rarityWeights = new()
    {
        { 0, 60 },
        { 1, 30 },
        { 2, 10 },
        { 3, 1 }
    };

    public CardData GetCard(CardQuery query)
    {
        var filtered = allCards.Where(card =>
        {
            if (!string.IsNullOrEmpty(query.nameContains) && !card.cardName.Contains(query.nameContains))
                return false;

            if (query.typeFilter.HasValue && card.cardType != query.typeFilter.Value)
                return false;

            if (query.rarityMin.HasValue && card.rarity < query.rarityMin.Value)
                return false;

            if (query.rarityMax.HasValue && card.rarity > query.rarityMax.Value)
                return false;

            if (query.excludeUniques && card.isUnique)
                return false;

            if (query.requiredEntries != null && query.requiredEntries.Count > 0)
            {
                var entryNames = card.entries.Select(e => e.entryName).ToHashSet();
                if (query.matchAllEntries)
                {
                    if (!query.requiredEntries.All(entryNames.Contains))
                        return false;
                }
                else
                {
                    if (!query.requiredEntries.Any(entryNames.Contains))
                        return false;
                }
            }

            return true;
        }).ToList();

        if (filtered.Count == 0)
            return null;

        if (query.randomPick)
        {
            if (query.weightedByRarity)
            {
                int totalWeight = filtered.Sum(c => rarityWeights.TryGetValue(c.rarity, out var w) ? w : 1);
                int roll = UnityEngine.Random.Range(0, totalWeight);
                int acc = 0;

                foreach (var card in filtered)
                {
                    int weight = rarityWeights.TryGetValue(card.rarity, out var w) ? w : 1;
                    acc += weight;
                    if (roll < acc)
                        return card;
                }

                return filtered.Last();
            }
            else
            {
                return filtered[UnityEngine.Random.Range(0, filtered.Count)];
            }
        }

        return filtered.First();
    }

    public HorizontalCardHolder playerCardHolder;

    public void Initialize(CardPool cardPool,HorizontalCardHolder playerCardHolder)
    {
        LoadCardsFromPool(cardPool);
        this.playerCardHolder = playerCardHolder;
        Debug.Log($"卡片管理器初始化了");
    }

    public async UniTask DrawCardsAsync()
    {
        var drawSequence = new List<Func<UniTask>>
        {
            () => DrawAndDelay(CardType.Labor),
            () => DrawAndDelay("Kevin"),
            () => DrawAndDelay(CardType.Tribute)
        };

        foreach (var task in drawSequence)
        {
            await task();
        }
    }

    private async UniTask DrawAndDelay(CardType type)
    {
        DrawCard(type);
        await UniTask.Delay(200);
    }

    private async UniTask DrawAndDelay(string name)
    {
        DrawCard(name);
        await UniTask.Delay(200);
    }



    /// <summary>
    /// 这个重载用于根据卡牌冲类抽取一张随机的卡牌
    /// </summary>
    /// <param name="type"></param>
    public void DrawCard (CardType type)
    {
        var query = CardQueryBuilder.New()
                    .WithType(type)
                    .ExcludeUniques()
                    .Random(weighted: true)
                    .Build();
        CardData selected = GetCard(query);
        if (selected == null)
        {
            Debug.LogWarning($"未找到【{type}】类型的卡牌！");
            return;
        }
        if (HasAlreadyCreated(selected))
        {
            Debug.LogWarning($"唯一卡【{selected.name}】已经抽取过，无法再次生成！");
            return;
        }
        CardRuntime runtime = CreateCard(selected);
        if (runtime == null)
        {
            Debug.LogWarning($"创建卡牌【{selected.name}】失败！");
            return;
        }
        playerCardHolder.AddCard(runtime);
    }

    /// <summary>
    /// 这个重载用于根据卡牌类型和名抽取特定卡牌
    /// </summary>
    public void DrawCard(string name)
    {
        CardData selected = GetCardByExactName(name);
        if (selected == null)
        {
            Debug.LogWarning($"未找到名为【{name}】的卡牌！");
            return;
        }
        if (HasAlreadyCreated(selected))
        {
            //Debug.LogWarning($"唯一卡【{name}】已经抽取过，无法再次生成！");
            return;
        }
        CardRuntime runtime = CreateCard(selected);
        if (runtime == null)
        {
            Debug.LogWarning($"创建卡牌【{name}】失败！");
            return;
        }
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
