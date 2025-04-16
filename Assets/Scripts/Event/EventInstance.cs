using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventInstance
{
    public EventNodeData data;
    public int remainingLife;
    public HorizontalCardHolder cardHolder;
    public bool resolved;
    public Role sourceRole;

    public EventInstance(EventNodeData data)
    {
        this.data = data;
        this.remainingLife = data.duration;
        this.cardHolder = EventSlotFactory.CreateCardHolder();
    }

    public void TickLife() => remainingLife--;
    public bool IsExpired() => remainingLife <= 0;
}

