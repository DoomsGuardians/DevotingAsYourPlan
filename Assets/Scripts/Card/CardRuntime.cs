using System.Collections.Generic;
public class CardRuntime
{
    public CardData data;
    public int remainingLife;

    public List<CardEntry> entries;

    public CardRuntime(CardData data)
    {
        this.data = data;
        this.remainingLife = data.maxLife;
        this.entries = data.entries;
    }

    public void TickLife()
    { 
            remainingLife--;
    }

    public bool IsExpired() => remainingLife <= 0;

    public void ApplyPassiveEffects()
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
