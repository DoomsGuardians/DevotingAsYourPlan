using UnityEngine;

[CreateAssetMenu(menuName = "Events/Effects/给定名字卡")]
public class GiveSpecificCardEffect : EventEffectSO
{
    [Tooltip("要给予的卡牌名称（需与 CardData.cardName 完全匹配）")]
    public string cardName;

    public override void Apply()
    {
        GameManager.Instance.CardManager.DrawCard(cardName);
    }

    public override string Description =>
        $"给予玩家卡牌【{cardName}】";
}
