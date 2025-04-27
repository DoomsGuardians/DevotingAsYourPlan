using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/Effects/特定卡设置")]
public class SetSpecificCardEffect : EventEffectSO
{
    [Header("筛选条件")] [Tooltip("要给予的卡牌种类")] public bool filterByType = false;
    public CardType cardType;

    [Tooltip("要给予的卡牌词条")] public bool filterByEntry = false;
    public List<CardEntry> requiredEntries;

    [Tooltip("要给予的卡牌名称（需与 CardData.cardName 完全匹配）")]
    public bool filterByName = false;

    public string cardName;

    [Tooltip("如果同名，是所有卡还是随机1张")] public bool isFirstFound;

    [Tooltip("增加词条")] public bool isAddEntry;

    [Tooltip("增加什么词条")] public CardEntry addEntry;

    [Tooltip("除掉词条")] public bool isSubEntry;

    [Tooltip("去除什么词条")] public CardEntry subEntry;

    [Tooltip("改变寿命")] public bool isChangeLife;

    [Tooltip("改变多少")] public int lifeDelta;

    public override void Apply(EventInstance instance)
    {
        List<CardRuntime> cards = new List<CardRuntime>();
        if (filterByName)
        {
            // 直接获取所有匹配的卡牌
            cards = GameManager.Instance.playerCardHolder.cards
                .Where(c => c.runtimeData.data.cardName == cardName)
                .Select(c => c.runtimeData)
                .ToList();
        }

        if (filterByType)
            cards = cards.Where(c => c.data.cardType == cardType).ToList();

        if (filterByEntry && requiredEntries != null && requiredEntries.Count > 0)
            cards = cards.Where(c => requiredEntries.All(e => c.entries.Contains(e))).ToList();


        // 如果是只操作第一个找到的卡牌
        if (isFirstFound && cards.Any())
        {
            var card = cards.First(); // 获取第一个匹配卡片的 runtimeData
            ApplyChanges(card);
        }
        else // 否则对所有匹配的卡牌进行操作
        {
            foreach (var card in cards)
            {
                ApplyChanges(card);
            }
        }
    }

    private void ApplyChanges(CardRuntime card)
    {
        if (card != null)
        {
            if (isAddEntry)
            {
                if (!card.entries.Contains(addEntry)) // 检查是否已添加，避免重复添加
                    card.entries.Add(addEntry);
            }

            if (isSubEntry)
            {
                card.entries.Remove(subEntry); // 从列表中移除条目
            }

            if (isChangeLife)
            {
                card.remainingLife += lifeDelta; // 修改卡片剩余寿命
            }
        }
    }


    public override string Description =>
        $"设置玩家卡牌【{cardName}】";
}