using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/Conditions/Trigger Conditions/高级卡牌范围")]
public class CardCountRangeFilterTriggerCondition : EventTriggerConditionSO
{
    [Header("基础范围")]
    public int minCount = 0;
    public int maxCount = 99;

    [Header("筛选条件")]
    public bool filterByType = false;
    public CardType cardTypeFilter;

    public bool excludeSpecificCards = false;
    public List<string> excludedCardNames;

    public bool filterByEntry = false;
    public List<CardEntry> requiredEntries;

    public override bool Evaluate(EventNodeData context)
    {
        var hand = GameManager.Instance.playerCardHolder.cards;
        int count = 0;

        foreach (var runtime in hand)
        {
            CardRuntime data = runtime.runtimeData;

            // 筛选：卡牌类型
            if (filterByType && data.data.cardType != cardTypeFilter)
                continue;

            // 筛选：排除某些卡牌名
            if (excludeSpecificCards && excludedCardNames.Contains(data.data.cardName))
                continue;

            // 筛选：必须包含指定词条
            if (filterByEntry && !requiredEntries.All(e => data.entries.Contains(e)))
                continue;

            count++;
        }

        Debug.Log($"[手牌检查] 过滤后卡牌数 = {count}，目标范围 [{minCount}, {maxCount}]");

        return count >= minCount && count <= maxCount;
    }

    public override string Description
    {
        get
        {
            string desc = $"满足条件的手牌数属于[{minCount}, {maxCount}]";
            if (filterByType) desc += $"，类型为 {cardTypeFilter}";
            if (excludeSpecificCards) desc += $"，排除 {string.Join(",", excludedCardNames)}";
            if (filterByEntry) desc += $"，包含词条: {string.Join(",", requiredEntries.Select(e => e.name))}";
            return desc;
        }
    }
}
