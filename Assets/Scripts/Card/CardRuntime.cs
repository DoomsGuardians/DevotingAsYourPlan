using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CardRuntime
{
    public CardData data;
    public int remainingLife;

    public List<CardEntry> entries;  // 不再直接引用 CardData.entries

    public CardRuntime(CardData data)
    {
        this.data = data;
        this.remainingLife = data.maxLife;
        // 通过复制构造函数创建一个新的列表，而不是引用原来的列表
        entries = new List<CardEntry>(data.entries);  // 创建副本
    }

    public void TickLife()
    { 
            remainingLife--;
    }

    public bool IsExpired() => remainingLife <= 0;

    public void ApplyEntryEffects()
    {
        foreach (var entry in entries)
        {
            if (EntryEffectRegistry.TryGet(entry.entryName, out var effect))
            {
                effect.Apply(this);
            }
        }
    }
}
