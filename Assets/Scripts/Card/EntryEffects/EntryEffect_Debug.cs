using UnityEngine;
public class EntryEffect_Debug : ICardEntryEffect
{
    public void Apply(CardRuntime card)
    {
        Debug.Log($"卡牌 {card.data.cardName} 触发被动效果！");
        // 在此添加实际逻辑，比如加资源、触发事件等
    }
}
