using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventContext
{
    public EventInstance eventInstance;
    public bool success;
    public Role role;

    public EventContext(EventInstance evt, bool success, Role role)
    {
        this.eventInstance = evt;
        this.success = success;
        this.role = role;
    }
}

