using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ConditionMode
{
    All, // 所有条件都满足
    Any  // 任意一个条件满足
}

public abstract class EventResolveConditionSO : ScriptableObject
{
    public abstract bool Evaluate(EventInstance context);
    public abstract string Description { get; }
}

public abstract class EventTriggerConditionSO : ScriptableObject
{
    public abstract bool Evaluate(EventNodeData context);
    public abstract string Description { get; }
}

