using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/Conditions/Resolve Conditions/卡牌匹配范围")]
public class CardMatchRangeResolveCondition : EventResolveConditionSO
{
    [Header("数量区间")]
    public int minCount = 1;
    public int maxCount = 999;

    [Header("筛选条件")]
    public bool filterByName = false;
    public string nameFilter;

    public bool filterByType = false;
    public CardType typeFilter;

    public bool filterByEntry = false;
    public CardEntry entryFilter;

    public override bool Evaluate(EventInstance context)
    {
        var matchedCards = context.originalCards.Where(card =>
        {
            var data = card.data;

            if (filterByName && data.cardName != nameFilter)
                return false;

            if (filterByType && data.cardType != typeFilter)
                return false;

            if (filterByEntry && !data.entries.Contains(entryFilter))
                return false;

            return true;
        });

        int count = matchedCards.Count();

        Debug.Log($"[分支判断] 筛选后卡牌数 = {count}，要求属于 [{minCount}, {maxCount}]");

        return count >= minCount && count <= maxCount;
    }

    public override string Description
    {
        get
        {
            List<string> filters = new();

            if (filterByName) filters.Add($"名字 = {nameFilter}");
            if (filterByType) filters.Add($"类型 = {typeFilter}");
            if (filterByEntry) filters.Add($"包含词条 = {entryFilter?.name}");

            string conditionStr = filters.Count > 0 ? string.Join(" 且 ", filters) : "任意卡";
            return $"满足 [{conditionStr}] 的卡牌数量 ∈ [{minCount}, {maxCount}]";
        }
    }
}
