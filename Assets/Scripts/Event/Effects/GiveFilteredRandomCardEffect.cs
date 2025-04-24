using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/Effects/抽卡")]
public class GiveFilteredRandomCardEffect : EventEffectSO
{
    [Header("发放数量")]
    public int amount = 1;

    [Header("筛选条件")]
    public bool filterByType = false;
    public CardType cardType;

    public bool filterByEntry = false;
    public List<CardEntry> requiredEntries;

    [Tooltip("是否排除所有唯一卡牌")]
    public bool excludeUniqueCard = true;

    [Tooltip("指定卡池（为空则使用全局卡池）")]
    public CardPool cardPoolOverride;

    [Header("稀有度权重")]
    public bool filterByRarity = false;

    [Tooltip("稀有度 0~3 的权重，分别对应 普通/稀有/史诗/传说")]
    public List<int> rarityWeights = new() { 60, 30, 10, 1 };

    public override void Apply(EventInstance instance)
    {
        List<CardData> pool = cardPoolOverride != null
            ? new List<CardData>(cardPoolOverride.cards)
            : GameManager.Instance.CardManager.allCards;

        if (filterByType)
            pool = pool.Where(c => c.cardType == cardType).ToList();

        if (filterByEntry && requiredEntries != null && requiredEntries.Count > 0)
            pool = pool.Where(c => requiredEntries.All(e => c.entries.Contains(e))).ToList();

        if (excludeUniqueCard)
            pool = pool.Where(c => !c.isUnique).ToList();

        for (int i = 0; i < amount; i++)
        {
            CardData selected = filterByRarity
                ? WeightedRandom(pool, c => GetRarityWeight(c.rarity))
                : pool.OrderBy(_ => Random.value).FirstOrDefault();

            if (selected != null)
            {
                var runtime = GameManager.Instance.CardManager.CreateCard(selected);
                GameManager.Instance.playerCardHolder.AddCard(runtime);
                Debug.Log($"[事件效果] 发放卡牌：{selected.cardName}");
            }
            else
            {
                Debug.LogWarning("[事件效果] 没有符合条件的卡牌可抽取！");
            }
        }
    }

    private int GetRarityWeight(int rarity)
    {
        if (rarity < 0 || rarity >= rarityWeights.Count) return 1;
        return rarityWeights[rarity];
    }

    private CardData WeightedRandom(List<CardData> list, System.Func<CardData, int> weightSelector)
    {
        int total = list.Sum(weightSelector);
        if (total == 0) return null;

        int roll = Random.Range(0, total);
        int cumulative = 0;

        foreach (var item in list)
        {
            cumulative += weightSelector(item);
            if (roll < cumulative)
                return item;
        }

        return list.LastOrDefault();
    }

    public override string Description =>
        $"给予 {amount} 张卡牌（" +
        $"{(filterByType ? $"类型={cardType}" : "任意类型")}, " +
        $"{(filterByEntry ? $"词条={string.Join(",", requiredEntries.Select(e => e.name))}" : "任意词条")}, " +
        $"{(excludeUniqueCard ? "排除唯一卡" : "")}" +
        $"{(filterByRarity ? ", 稀有度加权" : ", 等概率")}" +
        $"{(cardPoolOverride ? $"，卡池={cardPoolOverride.name}" : "")}）";
}
