using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/Effects/移除手牌")]
public class RemoveFilteredCardEffect : EventEffectSO
{
    [Header("删除数量限制")]
    [Tooltip("最多删除几张（设为 0 表示全部）")]
    public int amount = 1;

    [Header("筛选条件")]
    public bool filterByType = false;
    public CardType cardType;

    public bool filterByEntry = false;
    public List<CardEntry> requiredEntries;

    public bool filterByName = false;
    public string cardName;

    public override void Apply(EventInstance instance)
    {
        amount = (int)(1 + rarityFactor * instance.RaritySum) * amount;
        var hand = GameManager.Instance.playerCardHolder.cards;

        var candidates = hand.Where(c =>
        {
            var data = c.runtimeData;

            if (filterByType && data.data.cardType != cardType)
                return false;

            if (filterByEntry && !requiredEntries.All(e => data.entries.Contains(e)))
                return false;

            if (filterByName && data.data.cardName != cardName)
                return false;

            return true;
        }).ToList();

        int toRemove = (amount <= 0) ? candidates.Count : Mathf.Min(amount, candidates.Count);

        for (int i = 0; i < toRemove; i++)
        {
            var card = candidates[i];
            GameManager.Instance.playerCardHolder.RemoveCard(card);
            Debug.Log($"[事件效果] 移除手牌：{card.runtimeData.data.cardName}");
        }

        if (toRemove == 0)
        {
            Debug.Log($"[事件效果] 没有找到符合条件的卡牌可删除");
        }
    }

    public override string Description
    {
        get
        {
            List<string> filters = new();
            if (filterByType) filters.Add($"类型 = {cardType}");
            if (filterByEntry) filters.Add($"包含词条 = {string.Join(",", requiredEntries.Select(e => e.name))}");
            if (filterByName) filters.Add($"名称 = {cardName}");

            string condition = filters.Count > 0 ? string.Join(" 且 ", filters) : "任意卡";
            return $"移除手牌中满足 [{condition}] 的卡牌，最多 {amount} 张";
        }
    }
}
